namespace Sandbox.Components.Stargate
{
	public class EventHorizonTrigger : ModelCollider, Component.ITriggerListener
	{
		[Property]
		public EventHorizon EventHorizon =>
			GameObject.Components.Get<EventHorizon>(
				IsMainTrigger ? FindMode.InSelf : FindMode.InParent
			);

		[Property]
		public bool IsMainTrigger = false;

		public static int GetNumberOfObjectCollidersTouchingTrigger(
			GameObject obj,
			Collider trigger
		)
		{
			var body = obj.Components.Get<Rigidbody>();
			if (!body.IsValid() || !body.PhysicsBody.IsValid())
				return 0;

			var shapes = body.PhysicsBody.Shapes;
			// Log.Info( $"this body has {shapes.Count()} shapes" );
			var numTouching = 0;
			foreach (var shape in shapes)
			{
				var collider = shape.Collider as Collider;
				if (collider.IsValid())
				{
					if (trigger.Touching.Contains(collider))
					{
						numTouching++;
					}
				}
			}

			// Log.Info( $"{numTouching} shapes are touching EH trigger" );
			return numTouching;
		}

		// Trigger
		public new void OnTriggerEnter(GameObject other)
		{
			if (!other.IsValid())
				return;

			// only call 'enter' stuff if we entered the first time (aka only one collider is touching us when we started touching)
			if (EventHorizon.IsValid())
			{
				if (IsMainTrigger)
				{
					// if ( GetNumberOfObjectCollidersTouchingTrigger( other.GameObject, this ) == 0 )
					// {
					// Log.Info( "entered EH trigger" );
					EventHorizon.StartTouch(other);
					// }
				}
				else
					EventHorizon.OnEntityTriggerStartTouch(this, other);
			}
		}

		public new void OnTriggerExit(GameObject other)
		{
			Log.Info($"Called OnTriggerExit on trigger {this}");
			if (!other.IsValid())
				return;

			// only call 'exit' stuff if no colliders are touching us anymore
			if (EventHorizon.IsValid())
			{
				if (IsMainTrigger)
				{
					// if ( GetNumberOfObjectCollidersTouchingTrigger( other.GameObject, this ) == 0 )
					// {
					// Log.Info( "exited EH trigger" );
					EventHorizon.EndTouch(other);
					// }
				}
				else
					EventHorizon.OnEntityTriggerEndTouch(this, other);
			}
		}

		// TODO: debug triggers, check why Touching is not working as expected

		protected override void OnDestroy()
		{
			var gameObjects = Touching.Select(t => t.GameObject).ToHashSet();
			foreach (var other in gameObjects)
			{
				OnTriggerExit(other);
				Log.Info($"OnDestroy called on {this}, calling OnTriggerExit for {other}");
			}

			base.OnDestroy();
		}
	}
}

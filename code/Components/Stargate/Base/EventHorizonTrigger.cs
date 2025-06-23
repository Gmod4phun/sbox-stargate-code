namespace Sandbox.Components.Stargate
{
	public class EventHorizonTrigger : ModelCollider, Component.ITriggerListener
	{
		[Property]
		public EventHorizon EventHorizon { get; set; }

		[Property]
		public bool IsMainTrigger = false;

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
					EventHorizon.StartTouch(other);
				}
				else
					EventHorizon.OnEntityTriggerStartTouch(this, other);
			}
		}

		public new void OnTriggerExit(GameObject other)
		{
			if (!other.IsValid())
				return;

			// only call 'exit' stuff if no colliders are touching us anymore
			if (EventHorizon.IsValid())
			{
				if (IsMainTrigger)
				{
					EventHorizon.EndTouch(other);
				}
				else
				{
					EventHorizon.OnEntityTriggerEndTouch(this, other);
				}
			}
		}

		public void PreDestroyCleanup()
		{
			var gameObjects = Touching.Select(t => t.GameObject).ToHashSet();
			foreach (var other in gameObjects)
			{
				OnTriggerExit(other);
			}

			base.OnDestroy();
		}
	}
}

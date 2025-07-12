public class TouchingDebug : Component
{
	[Property]
	public Rigidbody Body => Components.Get<Rigidbody>(FindMode.InSelf);

	[Property]
	public Collider Trigger { get; set; }

	public static int GetNumberOfObjectCollidersTouchingTrigger(GameObject obj, Collider trigger)
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

	int curNumber = -1;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (Trigger.IsValid())
		{
			var num = GetNumberOfObjectCollidersTouchingTrigger(GameObject, Trigger);

			if (num != curNumber)
			{
				curNumber = num;
				// Log.Info( $"{curNumber} shapes are touching the trigger" );
			}
		}
	}
}

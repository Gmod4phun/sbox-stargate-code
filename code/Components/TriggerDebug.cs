public class TriggerDebug : Component, Component.ITriggerListener
{
	[Property]
	public Collider TargetCollider { get; set; }

	protected override void OnUpdate()
	{
		base.OnUpdate();
	}

	public void OnTriggerEnter(Collider other)
	{
		Log.Info(
			$"something entered the trigger, we are being touched by {TargetCollider.Touching.Count()} colliders"
		);
	}

	public void OnTriggerExit(Collider other)
	{
		Log.Info(
			$"something exited the trigger, we are being touched by {TargetCollider.Touching.Count()} colliders"
		);
	}

	protected override void OnDestroy()
	{
		Log.Info($"TriggerDebug {GameObject} is being destroyed");

		Log.Info(
			$"We are being touched by {TargetCollider.Touching.Count()} colliders before destruction"
		);

		base.OnDestroy();
	}
}

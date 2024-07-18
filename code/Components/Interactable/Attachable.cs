public class Attachable : Component, IUse
{
	[Property]
	public Rigidbody Body => Components.Get<Rigidbody>();

	[Property]
	public AttachPoint AttachedTo { get; private set; }

	[Property]
	public bool IsAttached => AttachedTo.IsValid();

	[Property]
	public Type AttachableType { get; set; }

	public void AttachTo(AttachPoint attachPoint)
	{
		if (IsAttached || !Body.IsValid())
			return;

		Body.MotionEnabled = false;

		GameObject.SetParent(attachPoint.GameObject, true);
		GameObject.Transform.Position = attachPoint.Transform.Position;
		GameObject.Transform.Rotation = attachPoint.Transform.Rotation;
		GameObject.Transform.ClearInterpolation();

		AttachedTo = attachPoint;
		AttachedTo.CurrentAttachable = this;
	}

	public void Detach()
	{
		if (!IsAttached || !Body.IsValid())
			return;

		Body.MotionEnabled = true;
		GameObject.ClearParent();
		AttachedTo.CurrentAttachable = null;
		AttachedTo.TimeSinceLastDetach = 0;
		AttachedTo = null;
	}

	public bool OnUse(GameObject user)
	{
		Detach();
		return true;
	}

	public bool IsUsable(GameObject user)
	{
		return true;
	}
}

public static class AttachableExtensions
{
	public static bool IsAttached(this Rigidbody rigidbody)
	{
		return rigidbody.Components.TryGet<Attachable>(out var attachable) && attachable.IsAttached;
	}
}

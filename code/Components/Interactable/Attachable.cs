public class Attachable : Component, IUse
{
	[Property]
	public Rigidbody Body => Components.Get<Rigidbody>();

	[Property]
	public AttachPoint AttachedTo { get; private set; }

	[Property]
	public Type AttachableType { get; set; }

	[Property]
	public bool ContinuousUse { get; set; } = false;

	public bool IsAttached => AttachedTo.IsValid();

	public Action UseAction { get; set; }

	public Func<AttachPoint, bool> TryAttachAction { get; set; }

	public void AttachTo(AttachPoint attachPoint, bool force = false)
	{
		if (IsAttached || !Body.IsValid())
			return;

		if (!force && TryAttachAction != null && !TryAttachAction.Invoke(attachPoint))
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
		UseAction?.Invoke();
		return ContinuousUse;
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
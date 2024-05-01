using Sandbox;
using Sandbox.VR;

public sealed class WebSwinging : Component
{
	[Property] VRHand.HandSources HandSide { get; set; }

	public Transform handTransform; // The hand (controller) from which the web is attached
	[Property] public float webForce { get; set; } = 10f;    // Force applied to the rigidbody when swinging
	float maxSwingDistance = 10000f; // Maximum distance the player can swing

	private bool isSwinging = false;
	private Rigidbody rb;

	protected override void OnStart()
	{
		rb = GameObject.Parent.Components.Get<Rigidbody>();
	}

	public Sandbox.VR.VRController TranslateHandSide()
	{
		switch ( HandSide )
		{
			case VRHand.HandSources.Left:
				return Input.VR.LeftHand;
			case VRHand.HandSources.Right:
				return Input.VR.RightHand;
			default:
				return Input.VR.RightHand;
		}
	}

	protected override void OnUpdate()
	{
		handTransform = Transform.World;

		if ( WebAttachPosition == Vector3.Zero )
		{
			Gizmo.Draw.Line( handTransform.Position, handTransform.Position + (handTransform.Rotation.Forward + handTransform.Rotation.Down) * maxSwingDistance );
		}
		else
		{
			Gizmo.Draw.Line( handTransform.Position, WebAttachPosition );
		}


		if ( TranslateHandSide().Trigger.Value < 0.5f )
		{
			StopSwinging();
		}

		if ( TranslateHandSide().Trigger.Value > 0.5f )
		{
			StartSwinging();
		}

	}

	Vector3 WebAttachPosition;

	void StartSwinging()
	{
		var hit = Scene.Trace.Ray( handTransform.Position, handTransform.Position + (handTransform.Rotation.Forward + handTransform.Rotation.Down) * maxSwingDistance ).Run();

		if ( hit.Hit && !isSwinging )
		{
			WebAttachPosition = hit.HitPosition;

			isSwinging = true;
		}

		if ( isSwinging )
		{
			Vector3 swingDirection = WebAttachPosition - handTransform.Position;

			rb.Velocity += (swingDirection.Normal * webForce);
		}
	}

	void StopSwinging()
	{
		WebAttachPosition = Vector3.Zero;
		isSwinging = false;
	}
}

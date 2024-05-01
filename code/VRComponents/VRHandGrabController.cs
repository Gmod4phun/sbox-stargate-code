using Sandbox;
using Sandbox.Engine;
using Sandbox.VR;

public sealed class VRHandGrabController : Component
{
	[Property] VRHand.HandSources HandSide { get; set; }

	[Property] GameObject HandOffset { get; set; }

	VRHandAnimationController anim { get; set; }

	GameObject HoverObject { get; set; }

	public GameObject HoldingObject { get; set; }

	VRHandGrabController OtherHand { get; set; }


	protected override void OnAwake()
	{
		anim = Components.Get<VRHandAnimationController>( FindMode.EverythingInSelfAndChildren );

		OtherHand = GameObject.Parent.Components.GetAll<VRHandGrabController>( FindMode.EverythingInSelfAndChildren ).Where( X => X != this ).First();
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
		var grabtr = Scene.Trace.Ray( Transform.Position, Transform.Position ).Radius( 8f ).WithTag( "grabbable" ).Run();

		if ( grabtr.Hit && grabtr.Body != null && grabtr.GameObject != null )
		{
			HoverObject = grabtr.GameObject;
		}
		else
		{
			HoverObject = null;
		}

		//Gizmo.Draw.Line( Transform.Position, Transform.Position + (TranslateHandSide().Velocity * Rotation.FromRoll( 90 ) * Rotation.FromYaw( 270 ) * VRLegs.instance.GameObject.Transform.Rotation) * 5f );

		if ( HoverObject != null )
		{
			if ( HoldingObject == null && TranslateHandSide().Grip.Value > 0.5f && OtherHand.HoldingObject != HoverObject )
			{
				HoldingObject = HoverObject;

				var grabobj = HoldingObject.Components.Get<VRGrabbableObject>( FindMode.EverythingInSelfAndChildren );

				if ( grabobj != null )
				{
					anim.SetCurlClamps( grabobj.curlClamps );
				}

				var physobj = HoldingObject.Components.Get<Rigidbody>( FindMode.EverythingInSelfAndChildren );

				if ( physobj != null )
				{
					physobj.Gravity = false;
					physobj.Velocity = Vector3.Zero;
					physobj.AngularVelocity = Vector3.Zero;
				}
			}
		}

		if ( HoldingObject != null )
		{
			if ( TranslateHandSide().Grip.Value < 0.5f )
			{
				HoldingObject.Tags.Remove( "player" );
				var physobj = HoldingObject.Components.Get<Rigidbody>( FindMode.EverythingInSelfAndChildren );

				if ( physobj != null )
				{
					physobj.Gravity = true;
					physobj.Velocity = TranslateHandSide().Velocity * Rotation.FromRoll( 90 ) * Rotation.FromYaw( 270 ) * VRLegs.instance.GameObject.Transform.Rotation;
					//physobj.AngularVelocity = TranslateHandSide().AngularVelocity.AsVector3();// * VRLegs.instance.GameObject.Transform.Rotation * Rotation.FromYaw( -90f ) * Rotation.FromPitch( 90f );
				}

				HoldingObject = null;

				anim.ResetCurlClamps();
			}
			else
			{
				HoldingObject.Tags.Add( "player" );

				var grabobj = HoldingObject.Components.Get<VRGrabbableObject>( FindMode.EverythingInSelfAndChildren );

				GameObject referenceObject = HandOffset;

				if ( grabobj != null )
				{
					anim.SetCurlClamps( grabobj.curlClamps );
					if ( grabobj.HandposeObject.IsValid() )
					{
						referenceObject = grabobj.HandposeObject;
					}
				}

				Vector3 deltapos = grabobj.Transform.Position - referenceObject.Transform.Position;

				HoldingObject.Transform.Position = anim.Transform.World.Position + deltapos;// ( referenceObject.Transform.LocalPosition * Transform.World.Rotation);
				HoldingObject.Transform.Rotation = anim.Transform.World.Rotation * referenceObject.Transform.LocalRotation.Inverse;// * (HandOffset.Transform.LocalRotation.Inverse * Transform.World.Rotation);
			}
		}
	}
}

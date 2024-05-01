using Sandbox;
using Sandbox.Engine;
using Sandbox.VR;

public sealed class VRGrabbableObject : Component
{
	[Property] public GameObject HandposeObject { get; set; }

	//0=Thumb, 1=Index, 2=Middle, 3=Ring, 4=Pinky

	public float[] curlClamps = new float[5] { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };

	protected override void OnAwake()
	{
		if ( HandposeObject.IsValid() )
		{
			HandposeObject.Components.Get<SkinnedModelRenderer>( FindMode.EverythingInSelfAndChildren ).Enabled = false;
			var handpose = HandposeObject.Components.Get<VRHandPoseController>( false );
			if ( handpose != null )
			{
				curlClamps[0] = handpose.ThumbClamp;
				curlClamps[1] = handpose.IndexClamp;
				curlClamps[2] = handpose.MiddleClamp;
				curlClamps[3] = handpose.RingClamp;
				curlClamps[4] = handpose.PinkyClamp;
			}
		}
	}

	public Sandbox.VR.VRController TranslateHandSide( VRHand.HandSources HandSide )
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

	}
}

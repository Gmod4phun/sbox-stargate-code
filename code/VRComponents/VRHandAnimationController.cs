using Sandbox;
using Sandbox.Engine;
using Sandbox.VR;

public sealed class VRHandAnimationController : Component
{
	[Property] VRHand.HandSources HandSide { get; set; }

	SkinnedModelRenderer anim { get; set; }

	//0=Thumb, 1=Index, 2=Middle, 3=Ring, 4=Pinky

	public float[] curlClamps = new float[5] { 1f, 1f, 1f, 1f, 1f };

	protected override void OnAwake()
	{
		anim = Components.Get<SkinnedModelRenderer>();
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

	public float ClampedFingerCurl( FingerValue finger )
	{
		float rawCurl = TranslateHandSide().GetFingerCurl( (int)finger );
		return MathX.Clamp( rawCurl, 0f, curlClamps[(int)finger] );
	}

	public void SetCurlClamp( FingerValue finger, float clamp )
	{
		curlClamps[(int)finger] = clamp;
	}

	public void ResetCurlClamps()
	{
		curlClamps = new float[5] { 1f, 1f, 1f, 1f, 1f };
	}

	public void SetCurlClamps( float[] clamps )
	{
		if ( curlClamps.Length == clamps.Length )
		{
			curlClamps = clamps;
		}
	}

	protected override void OnUpdate()
	{
		anim.Set( "Thumb", ClampedFingerCurl( FingerValue.ThumbCurl ) );
		anim.Set( "Index", ClampedFingerCurl( FingerValue.IndexCurl ) );
		anim.Set( "Middle", ClampedFingerCurl( FingerValue.MiddleCurl ) );
		anim.Set( "Ring", ClampedFingerCurl( FingerValue.RingCurl ) );
		anim.Set( "Pinky", ClampedFingerCurl( FingerValue.PinkyCurl ) );
	}
}

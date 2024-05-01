using Sandbox;

public sealed class VRHandPoseController : Component
{
	//0=Thumb, 1=Index, 2=Middle, 3=Ring, 4=Pinky
	[Property] public float ThumbClamp { get; set; } = 1f;
	[Property] public float IndexClamp { get; set; } = 1f;
	[Property] public float MiddleClamp { get; set; } = 1f;
	[Property] public float RingClamp { get; set; } = 1f;
	[Property] public float PinkyClamp { get; set; } = 1f;

	SkinnedModelRenderer model;

	protected override void OnUpdate()
	{
		if ( model == null )
		{
			model = Components.Get<SkinnedModelRenderer>();
			if ( model == null )
			{
				return;
			}
		}

		if ( !model.Enabled )
		{
			return;
		}

		model.SceneModel.Update( 0.1f );

		model.SceneModel.SetAnimParameter( "thumb", ThumbClamp );
		model.SceneModel.SetAnimParameter( "index", IndexClamp );
		model.SceneModel.SetAnimParameter( "middle", MiddleClamp );
		model.SceneModel.SetAnimParameter( "ring", RingClamp );
		model.SceneModel.SetAnimParameter( "pinky", PinkyClamp );
	}
}

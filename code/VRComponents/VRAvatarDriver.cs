using Sandbox;
using Sandbox.Citizen;

public sealed class VRAvatarDriver : Component
{
	[Property] public GameObject SourcePlayer { get; set; }

	[Property] public bool IsLocal { get; set; }

	CitizenAnimationHelper anim { get; set; }
	SkinnedModelRenderer animModel { get; set; }

	GameObject Head;
	GameObject LeftHand;
	GameObject RightHand;

	GameObject LeftBone;
	GameObject RightBone;

	bool Initialized;

	protected override void OnStart()
	{
		Head = SourcePlayer.Children.Where( C => C.Name == "Head" ).First();

		LeftHand = SourcePlayer.Components.GetAll<VRHandAnimationController>().Where( C => C.GameObject.Name == "Left Hand Model" ).First().GameObject;
		RightHand = SourcePlayer.Components.GetAll<VRHandAnimationController>().Where( C => C.GameObject.Name == "Right Hand Model" ).First().GameObject;

		anim = Components.Get<CitizenAnimationHelper>( FindMode.EverythingInSelfAndChildren );

		animModel = anim.Target.Components.Get<SkinnedModelRenderer>( FindMode.EverythingInSelfAndChildren );

		if ( Head.IsValid() && LeftHand.IsValid() && RightHand.IsValid() )
		{
			LeftBone = new GameObject();
			RightBone = new GameObject();
			Initialized = true;
		}

		Log.Info( "Head:" + Head.IsValid() + " LeftHand:" + LeftHand.IsValid() + " RightHand:" + RightHand.IsValid() );
	}

	Vector3 PositionDelta;

	Vector3 LastPosition;

	SceneTraceResult heighttr;

	public Transform GetBoneTransform( SkinnedModelRenderer model, BoneCollection.Bone bone )
	{
		Transform trans;
		model.TryGetBoneTransform( bone, out trans );
		return trans;
	}

	protected override void OnUpdate()
	{
		if ( !Initialized ) return;

		anim.WithLook( Head.Transform.Rotation.Forward );

		anim.IkLeftHand = LeftBone;

		anim.IkRightHand = RightBone;

		LeftBone.Transform.World = GetBoneTransform( LeftHand.Components.Get<SkinnedModelRenderer>(), LeftHand.Components.Get<SkinnedModelRenderer>().Model.Bones.GetBone( "hand_R" ) );

		LeftBone.Transform.LocalRotation *= Rotation.FromRoll( 180 );

		RightBone.Transform.World = GetBoneTransform( RightHand.Components.Get<SkinnedModelRenderer>(), RightHand.Components.Get<SkinnedModelRenderer>().Model.Bones.GetBone( "hand_R" ) );

		float ScaleOffset = 1f - (1f - (animModel.GetFloat( "scale_height" ) - 0.1f));


		heighttr = Scene.Trace.Ray( Head.Transform.Position, Head.Transform.Position.WithZ( SourcePlayer.Transform.Position.z ) ).WithoutTags( "player" ).Run();


		float LocalEyeHeight = heighttr.Distance / 64f;

		animModel.Set( "duck", (ScaleOffset - LocalEyeHeight) * 3f );
		if ( IsLocal )
		{
			animModel.SceneModel.SetBodyGroup( "Hands", 1 );
			animModel.SceneModel.SetBodyGroup( "Head", 1 );
		}

		Transform.Position = Head.Transform.Position.WithZ( heighttr.EndPosition.z ) - Head.Transform.Rotation.Forward.WithZ( 0 ) * animModel.GetFloat( "duck" ) * 20f;
		Transform.Rotation = Rotation.LookAt( Head.Transform.Rotation.Forward.WithZ( 0 ) );
	}

	protected override void OnFixedUpdate()
	{
		if ( !Initialized ) return;

		Transform.Position = Head.Transform.Position.WithZ( heighttr.EndPosition.z ) - Head.Transform.Rotation.Forward.WithZ( 0 ) * animModel.GetFloat( "duck" ) * 20f;
		Transform.Rotation = Rotation.LookAt( Head.Transform.Rotation.Forward.WithZ( 0 ) );

		PositionDelta = Head.Transform.Position - LastPosition;

		anim.WithVelocity( PositionDelta * Transform.Rotation * Rotation.FromYaw( 180 ) * 40f );
		anim.WithWishVelocity( PositionDelta * Transform.Rotation * Rotation.FromYaw( 180 ) * 40f );

		LastPosition = Head.Transform.Position;

		anim.IsGrounded = heighttr.Hit;
	}
}

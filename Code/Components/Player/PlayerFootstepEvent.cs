public sealed class PlayerFootstepEvent : Component
{
	[Property]
	SkinnedModelRenderer Source { get; set; }

	[Property]
	PlayerController Player => Components.Get<PlayerController>(FindMode.InSelf);

	TimeSince timeSinceStep;

	protected override void OnEnabled()
	{
		if (Source is null)
			return;

		Source.OnFootstepEvent += OnEvent;
	}

	protected override void OnDisabled()
	{
		if (Source is null)
			return;

		Source.OnFootstepEvent -= OnEvent;
	}

	private void OnEvent(SceneModel.FootstepEvent e)
	{
		if (timeSinceStep < 0.2f)
			return;

		var playerWorld = Player.GetMultiWorld();

		var tr = Scene
			.Trace.Ray(
				e.Transform.Position + Vector3.Up * 20,
				e.Transform.Position + Vector3.Up * -20
			)
			.WithWorld(playerWorld)
			.Run();

		if (!tr.Hit)
			return;

		if (tr.Surface is null)
			return;

		timeSinceStep = 0;

		var sound =
			e.FootId == 0
				? tr.Surface.SoundCollection.FootLeft
				: tr.Surface.SoundCollection.FootRight;
		if (sound is null)
			return;

		var multiWorldSound = MultiWorldSound.Play(
			sound.ResourceName,
			tr.HitPosition + tr.Normal * 5,
			playerWorld.WorldIndex
		);
		multiWorldSound.Volume = 0.5f;
	}

	public static void PlayJumpLandSound(
		PlayerController player,
		Vector3 position,
		bool landSound = false
	)
	{
		var playerWorld = player.GetMultiWorld();

		var tr = player
			.Scene.Trace.Ray(position + Vector3.Up * 20, position + Vector3.Up * -20)
			.WithWorld(playerWorld)
			.Run();

		if (!tr.Hit)
			return;

		if (tr.Surface is null)
			return;

		var sound = landSound
			? tr.Surface.SoundCollection.FootLand
			: tr.Surface.SoundCollection.FootLaunch;
		if (sound is null)
			return;

		var multiWorldSound = MultiWorldSound.Play(
			sound.ResourceName,
			tr.HitPosition + tr.Normal * 5,
			playerWorld.WorldIndex
		);
		multiWorldSound.Volume = 1;
	}
}

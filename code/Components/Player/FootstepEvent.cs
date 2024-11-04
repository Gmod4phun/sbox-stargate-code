using PlayerController = Scenegate.PlayerController;

public sealed class FootstepEvent : Component
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
		if (!Player.PlayerAlive)
			return;

		if (timeSinceStep < 0.2f)
			return;

		var currentWorldTag = MultiWorldSystem.GetWorldTag(Player.CurrentWorldIndex);

		var tr = Scene
			.Trace.Ray(
				e.Transform.Position + Vector3.Up * 20,
				e.Transform.Position + Vector3.Up * -20
			)
			.WithTag(currentWorldTag)
			.Run();

		if (!tr.Hit)
			return;

		if (tr.Surface is null)
			return;

		timeSinceStep = 0;

		var sound = e.FootId == 0 ? tr.Surface.Sounds.FootLeft : tr.Surface.Sounds.FootRight;
		if (sound is null)
			return;

		var multiWorldSound = MultiWorldSound.Play(
			sound,
			tr.HitPosition + tr.Normal * 5,
			Player.CurrentWorldIndex
		);
		multiWorldSound.Volume = 0.5f;
	}

	public static void PlayJumpLandSound(
		PlayerController player,
		Vector3 position,
		bool landSound = false
	)
	{
		if (!player.PlayerAlive)
			return;

		var currentWorldTag = MultiWorldSystem.GetWorldTag(player.CurrentWorldIndex);

		var tr = player
			.Scene.Trace.Ray(position + Vector3.Up * 20, position + Vector3.Up * -20)
			.WithTag(currentWorldTag)
			.Run();

		if (!tr.Hit)
			return;

		if (tr.Surface is null)
			return;

		var sound = landSound ? tr.Surface.Sounds.FootLand : tr.Surface.Sounds.FootLaunch;
		if (sound is null)
			return;

		var multiWorldSound = MultiWorldSound.Play(
			sound,
			tr.HitPosition + tr.Normal * 5,
			player.CurrentWorldIndex
		);
		multiWorldSound.Volume = 1;
	}
}

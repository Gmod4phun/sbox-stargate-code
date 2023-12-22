public sealed class FootstepEvent : Component
{
	[Property] SkinnedModelRenderer Source { get; set; }

	protected override void OnEnabled()
	{
		if ( Source is null )
			return;

		Source.OnFootstepEvent += OnEvent;
	}

	protected override void OnDisabled()
	{
		if ( Source is null )
			return;

		Source.OnFootstepEvent -= OnEvent;
	}

	private void OnEvent( SceneModel.FootstepEvent e )
	{
        var tr = Scene.Trace
			.Ray( e.Transform.Position + Vector3.Up * 20, e.Transform.Position + Vector3.Up * -20 )
			.Run();

		if ( !tr.Hit )
			return;

		var sound = Sound.Play("footstep-concrete", e.Transform.Position);
		sound.Volume = e.Volume;
	}
}

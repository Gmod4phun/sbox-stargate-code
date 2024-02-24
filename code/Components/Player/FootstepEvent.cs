public sealed class FootstepEvent : Component
{
	[Property] SkinnedModelRenderer Source { get; set; }
	[Property] PlayerController Player => Components.Get<PlayerController>( FindMode.InSelf );

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
		var currentWorldTag = MultiWorldSystem.GetWorldTag( Player.CurrentWorldIndex );

		var tr = Scene.Trace
			.Ray( e.Transform.Position + Vector3.Up * 20, e.Transform.Position + Vector3.Up * -20 )
			.WithTag( currentWorldTag )
			.Run();

		if ( !tr.Hit )
			return;

		var sound = MultiWorldSound.Play( "footstep-concrete", e.Transform.Position, Player.CurrentWorldIndex );
		sound.Handle.Volume = e.Volume;
	}
}

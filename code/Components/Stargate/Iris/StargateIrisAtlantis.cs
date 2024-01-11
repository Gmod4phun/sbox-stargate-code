namespace Sandbox.Components.Stargate
{
	public class StargateIrisAtlantis : StargateIris
	{
		protected override float _openCloseDelay => 1f;
		private SoundHandle IrisLoop;
		private float _currentAlpha = 0;

		public override async void Close()
		{
			if ( Busy || Closed ) return;

			Busy = true;
			Closed = true;
			IrisModel.SceneModel.RenderingEnabled = true;
			IrisCollider.Enabled = true;

			Sound.Play( "stargate.iris.atlantis.close", Transform.Position );

			await Task.DelaySeconds( _openCloseDelay );

			Busy = false;

			await Task.DelaySeconds( 0.5f );

			if ( Closed )
			{
				IrisLoop?.Stop( 0.1f );
				IrisLoop = Sound.Play( "stargate.iris.atlantis.loop", Transform.Position );
			}
		}

		public override async void Open()
		{
			if ( Busy || !Closed ) return;
			IrisLoop?.Stop( 0.1f );

			Busy = true;
			Closed = false;
			IrisCollider.Enabled = false;
			Sound.Play( "stargate.iris.atlantis.open", Transform.Position );

			await Task.DelaySeconds( _openCloseDelay );

			IrisModel.SceneModel.RenderingEnabled = false;
			Busy = false;
		}

		public override void PlayHitSound()
		{
			Sound.Play( "stargate.iris.atlantis.hit", Transform.Position );
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			IrisLoop?.Stop();

			if ( Closed )
			{
				Sound.Play( "stargate.iris.atlantis.open", Transform.Position );
			}
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			_currentAlpha = _currentAlpha.LerpTo( Closed ? 1 : 0, Time.Delta * 6 );

			if ( IrisModel.IsValid() && IrisModel.SceneModel.IsValid() )
			{
				var sm = IrisModel.SceneModel;
				sm.Flags.IsOpaque = false;
				sm.Flags.IsTranslucent = true;
				sm.ColorTint = sm.ColorTint.WithAlpha( _currentAlpha );
			}
		}
	}
}

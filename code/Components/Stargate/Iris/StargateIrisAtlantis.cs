namespace Sandbox.Components.Stargate
{
	public class StargateIrisAtlantis : StargateIris
	{
		protected override float _openCloseDelay => 1f;
		private SoundHandle IrisLoop;

		public override async void Close()
		{
			if ( Busy || Closed ) return;

			Busy = true;
			Closed = true;

			IrisModel.SceneModel.RenderingEnabled = true;
			IrisCollider.Enabled = true;

			Log.Info( IrisModel.SceneModel );

			Sound.Play( "stargate.iris.atlantis.close", Transform.Position );

			await Task.DelayRealtimeSeconds( _openCloseDelay );

			Busy = false;

			await Task.DelayRealtimeSeconds( 0.6f );

			if ( Closed )
			{
				IrisLoop?.Stop();
				IrisLoop = Sound.Play( "stargate.iris.atlantis.loop", Transform.Position );
			}
		}

		public override async void Open()
		{
			if ( Busy || !Closed ) return;

			IrisLoop?.Stop();

			Busy = true;
			Closed = false;

			IrisModel.SceneModel.RenderingEnabled = false;
			IrisCollider.Enabled = false;

			Sound.Play( "stargate.iris.atlantis.open", Transform.Position );

			await Task.DelayRealtimeSeconds( _openCloseDelay );

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
	}
}

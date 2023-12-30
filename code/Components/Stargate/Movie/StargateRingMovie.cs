namespace Sandbox.Components.Stargate
{
	public partial class StargateRingMovie : StargateRingMilkyWay
	{
		public StargateRingMovie() : base()
		{
			StartSoundName = "stargate.movie.ring_roll";
		}

		public override void StopStopSound() { }
		public override void PlayStopSound() { }

		public override void StopStartSound()
		{
			StartSoundInstance?.Stop( 1.2f );
		}

		protected override void OnDestroy()
		{
			StartSoundInstance?.Stop();

			base.OnDestroy();
		}

		public override void OnStarting()
		{
			PlayStartSound();
		}

		public override void OnStopping()
		{
			StopStartSound();
		}
	}
}

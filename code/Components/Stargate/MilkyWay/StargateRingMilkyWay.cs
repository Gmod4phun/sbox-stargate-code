namespace Sandbox.Components.Stargate
{
	public partial class StargateRingMilkyWay : StargateRing
	{
		public string StartSoundName { get; set; } = "stargate.milkyway.ring_start_long";
		public string StopSoundName { get; set; } = "stargate.milkyway.ring_stop";

		[Property]
		public ModelRenderer RingModel { get; set; }

		protected MultiWorldSound StartSoundInstance { get; set; }
		protected MultiWorldSound StopSoundInstance { get; set; }

		public virtual void StopStartSound()
		{
			StartSoundInstance?.Stop();
		}

		public virtual void PlayStartSound()
		{
			StopStartSound();
			StartSoundInstance = Stargate.PlayFollowingSound( GameObject, StartSoundName );
		}

		public virtual void StopStopSound()
		{
			StopSoundInstance?.Stop();
		}

		public virtual void PlayStopSound()
		{
			StopStopSound();
			StopSoundInstance = Stargate.PlayFollowingSound( GameObject, StopSoundName );
		}

		protected override void OnDestroy()
		{
			StartSoundInstance?.Stop();
			StopSoundInstance?.Stop();

			base.OnDestroy();
		}

		public override void OnStarting()
		{
			PlayStartSound();
		}

		public override void OnStopped()
		{
			PlayStopSound();
			StopStartSound();
		}
	}
}

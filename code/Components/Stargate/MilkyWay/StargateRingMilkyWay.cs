﻿namespace Sandbox.Components.Stargate
{
	public partial class StargateRingMilkyWay : StargateRing
	{
		public string StartSoundName { get; set; } = "stargate.milkyway.ring_start_long";
		public string StopSoundName { get; set; } = "stargate.milkyway.ring_stop";

		[Property]
		public ModelRenderer RingModel { get; set; }

		protected SoundHandle StartSoundInstance { get; set; }
		protected SoundHandle StopSoundInstance { get; set; }

		public virtual void StopStartSound()
		{
			StartSoundInstance?.Stop();
		}

		public virtual void PlayStartSound()
		{
			StopStartSound();
			StartSoundInstance = Sound.Play( StartSoundName, Transform.Position );
		}

		public virtual void StopStopSound()
		{
			StopSoundInstance?.Stop();
		}

		public virtual void PlayStopSound()
		{
			StopStopSound();
			StopSoundInstance = Sound.Play( StopSoundName, Transform.Position );
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

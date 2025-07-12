namespace Sandbox.Components.Stargate
{
	public partial class StargateRingMilkyWay : StargateRing
	{
		public StargateRingMilkyWay()
			: base()
		{
			SpinUpTime = 1.15f;
			SpinDownTime = 1.25f;
		}

		public string StartSoundName { get; set; } = "stargate.milkyway.ring_start";
		public string StopSoundName { get; set; } = "stargate.milkyway.ring_stop";
		public string LoopSoundName { get; set; } = "stargate.milkyway.ring_loop";

		[Property]
		public ModelRenderer RingModel { get; set; }

		protected MultiWorldSound StartSoundInstance { get; set; }
		protected MultiWorldSound StopSoundInstance { get; set; }
		protected MultiWorldSound LoopSoundInstance { get; set; }

		public virtual void StopStartSound()
		{
			StartSoundInstance?.Stop();
		}

		public virtual void PlayStartSound()
		{
			StopStartSound();
			StartSoundInstance = Stargate.PlayFollowingSound(GameObject, StartSoundName);
		}

		public virtual void StopStopSound()
		{
			StopSoundInstance?.Stop();
		}

		public virtual void PlayStopSound()
		{
			StopStopSound();
			StopSoundInstance = Stargate.PlayFollowingSound(GameObject, StopSoundName);
		}

		protected override void OnDestroy()
		{
			StartSoundInstance?.Stop();
			StopSoundInstance?.Stop();

			base.OnDestroy();
		}

		async void PlayLoopSoundAfter(float time)
		{
			await Task.DelaySeconds(time);
			LoopSoundInstance = Stargate.PlayFollowingSound(GameObject, LoopSoundName);
		}

		async void StopStartSoundAfter(float time)
		{
			await Task.DelaySeconds(time);
			StartSoundInstance?.Stop();
		}

		public override void OnStarting()
		{
			PlayStartSound();
			PlayLoopSoundAfter(1.2f);
			StopStartSoundAfter(3.6f);
		}

		public override void OnStopped()
		{
			PlayStopSound();
			StopStartSound();
			LoopSoundInstance?.Stop();
		}
	}
}

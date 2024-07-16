namespace Sandbox.Components.Stargate
{
	public partial class StargateRingUniverse : StargateRingMilkyWay
	{
		public StargateRingUniverse()
			: base()
		{
			SpinUpTime = 1f;
			SpinDownTime = 1f;
			StartSoundName = "stargate.universe.roll_long";
			StopSoundName = "stargate.universe.roll_stop";
			RingSymbols = " ZB9J QNLM@VKO6 DCWY #RTS 8APU F7H5X4IG0 12E3";
		}

		public SkinnedModelRenderer SymbolsPart1 =>
			GameObject
				.Components.GetAll<SkinnedModelRenderer>()
				.Where(c =>
					c.Model.Name
					== "models/sbox_stargate/gate_universe/gate_universe_symbols_1_18.vmdl"
				)
				.FirstOrDefault();

		public SkinnedModelRenderer SymbolsPart2 =>
			GameObject
				.Components.GetAll<SkinnedModelRenderer>()
				.Where(c =>
					c.Model.Name
					== "models/sbox_stargate/gate_universe/gate_universe_symbols_19_36.vmdl"
				)
				.FirstOrDefault();

		public override float GetSymbolAngle(char sym)
		{
			return sym == ' ' ? 0 : base.GetSymbolAngle(sym);
		}

		public int GetSymbolNumber(char sym)
		{
			if (!RingSymbols.Contains(sym))
				return -1;

			var syms = RingSymbols;
			syms = syms.Replace(" ", "");
			syms = syms.Replace("@", "");
			syms = syms.Replace("X", "");

			return syms.ToString().IndexOf(sym);
		}

		public async void SetSymbolState(int num, bool state, float delay = 0)
		{
			if (delay > 0)
			{
				await GameTask.DelaySeconds(delay);
				if (this.IsValid())
					return;
			}

			num = num.UnsignedMod(36);
			var isPart1 = num < 18;
			(isPart1 ? SymbolsPart1 : SymbolsPart2).SetBodyGroup(
				(isPart1 ? num : num - 18),
				state ? 1 : 0
			);
		}

		public void SetSymbolState(char sym, bool state)
		{
			var symNum = GetSymbolNumber(sym);
			if (symNum >= 0)
				SetSymbolState(symNum, state);
		}

		public void ResetSymbols()
		{
			for (int i = 0; i <= 35; i++)
				SetSymbolState(i, false);
		}

		// public override void StopStopSound() { }
		// public override void PlayStopSound() { }

		// public override void StopStartSound()
		// {
		// 	StartSoundInstance?.Stop( 1.2f );
		// }

		public override void OnStarting()
		{
			PlayStartSound();
		}

		public override void OnStopped()
		{
			PlayStopSound();
			StopStartSound();
		}

		protected override void OnDestroy()
		{
			StartSoundInstance?.Stop();

			SymbolsPart1?.Destroy();
			SymbolsPart2?.Destroy();

			base.OnDestroy();
		}
	}
}

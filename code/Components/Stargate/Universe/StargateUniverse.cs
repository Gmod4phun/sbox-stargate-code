namespace Sandbox.Components.Stargate
{
	public class StargateUniverse : Stargate
	{
		public StargateUniverse()
		{
			SoundDict = new()
			{
				{ "gate_open", "stargate.universe.open" },
				{ "gate_close", "stargate.universe.close" },
				{ "gate_roll_fast", "stargate.universe.roll_long" },
				{ "gate_roll_slow", "stargate.universe.roll_long" },
				{ "gate_activate", "stargate.universe.activate" },
				{ "symbol", "stargate.universe.symbol_encode" },
				{ "chevron_dhd", "stargate.universe.symbol_encode" },
				{ "dial_fail", "stargate.universe.dial_fail" }
			};

			GateGlyphType = GlyphType.UNIVERSE;
			GateGroup = "U@";
			GateAddress = GenerateGateAddress(GateGroup);
			EventHorizonMaterialGroup = "universe";
		}

		public override float RingSpeedPerSecond => 50f;

		public StargateRingUniverse Ring =>
			Components.Get<StargateRingUniverse>(FindMode.EnabledInSelfAndDescendants);

		public Chevron Chevron => Components.Get<Chevron>(FindMode.EnabledInSelfAndDescendants);

		public async Task<bool> RotateRingToSymbol(char sym, int angOffset = 0)
		{
			return await Ring.RotateToSymbol(sym, angOffset);
		}

		// DIALING

		public override void SetChevronsGlowState(bool state, float delay = 0)
		{
			if (state)
				Chevron.TurnOn(delay);
			else
				Chevron.TurnOff(delay);
		}

		public override void OnStopDialingBegin()
		{
			base.OnStopDialingBegin();

			PlaySound(this, GetSound("dial_fail"), 0.5f);
		}

		public override void OnStopDialingFinish()
		{
			base.OnStopDialingFinish();

			SetChevronsGlowState(false);
			Ring?.ResetSymbols();
			Bearing?.TurnOff();

			Ring.SpinDown();

			AddTask(
				Time.Now + 2.5f,
				() =>
				{
					if (!IsGateUpright())
						DoResetGateRoll();
				},
				TimedTaskCategory.GENERIC
			);
		}

		public override void OnStargateBeginOpen()
		{
			base.OnStargateBeginOpen();

			PlaySound(this, GetSound("gate_open"));
			Bearing?.TurnOn();
		}

		public override void OnStargateOpened()
		{
			base.OnStargateOpened();
		}

		public override void OnStargateBeginClose()
		{
			base.OnStargateBeginClose();

			PlaySound(this, GetSound("gate_close"));
		}

		public override void OnStargateClosed()
		{
			base.OnStargateClosed();

			SetChevronsGlowState(false);
			Ring?.ResetSymbols();
			Bearing?.TurnOff();

			AddTask(
				Time.Now + 2.5f,
				() =>
				{
					if (!IsGateUpright())
						DoResetGateRoll();
				},
				TimedTaskCategory.GENERIC
			);
		}

		public override void DoStargateReset()
		{
			if (Dialing)
				ShouldStopDialing = true;

			base.DoStargateReset();

			SetChevronsGlowState(false);
			Ring?.ResetSymbols();
			Bearing?.TurnOff();
		}

		public void DoPreRoll()
		{
			SetChevronsGlowState(true, 0.2f);
			PlaySound(this, GetSound("gate_activate"));
		}

		public async Task DoPreRollAsync()
		{
			DoPreRoll();
			await Task.DelaySeconds(1.5f);
		}

		public bool IsGateUpright(float tolerance = 1f)
		{
			return MathF.Abs(0 - (Ring.RingAngle.UnsignedMod(360f))) < tolerance;
		}

		public void DoResetGateRoll()
		{
			if (Idle)
				_ = Ring.RotateToSymbol(' ');
		}

		public void SymbolOn(char sym, bool nosound = false)
		{
			Ring.SetSymbolState(sym, true);
			if (!nosound)
				PlaySound(this, GetSound("symbol"));
		}

		public void SymbolOff(char sym)
		{
			Ring.SetSymbolState(sym, false);
		}

		// INDIVIDUAL DIAL TYPES

		// FAST DIAL
		public override void BeginDialFast(string address)
		{
			base.BeginDialFast(address);

			if (!CanStargateStartDial())
				return;

			// Event.Run( StargateEvent.DialBegin, this, address );

			try
			{
				CurGateState = GateState.DIALING;
				CurDialType = DialType.FAST;

				if (!IsValidFullAddress(address))
				{
					StopDialing();
					return;
				}

				DoPreRoll();

				var target = FindDestinationGateByDialingAddress(this, address);
				var wasTargetReadyOnStart = false; // if target gate was not available on dial start, dont bother doing anything at the end

				if (target.IsValid() && target != this && target.IsStargateReadyForInboundFast())
				{
					wasTargetReadyOnStart = true;
					target.BeginInboundFast(
						GetSelfAddressBasedOnOtherAddress(this, address).Length
					);
					OtherGate = target; // this is needed so that the gate can stop dialing if we cancel the dial
					OtherGate.OtherGate = this;
				}

				var startTime = Time.Now;
				var addrLen = address.Length;

				bool gateValidCheck()
				{
					return wasTargetReadyOnStart
						&& target.IsValid()
						&& target != this
						&& target.IsStargateReadyForInboundFastEnd();
				}

				var rollStartTime = startTime + 0.5f;
				var rollEndTime = rollStartTime + 4.8f;

				AddTask(rollStartTime, () => Ring.SpinUp(), TimedTaskCategory.DIALING);
				AddTask(rollEndTime, () => Ring.SpinDown(), TimedTaskCategory.DIALING);

				var symbolStartDelay = 0.5f;
				var symbolDelay = 5f / addrLen;

				// lets encode each chevron but the last
				for (var i = 0; i < addrLen; i++)
				{
					var i_copy = i;
					var symTime = rollStartTime + symbolStartDelay + (symbolDelay * i_copy);

					AddTask(
						symTime,
						() =>
						{
							SymbolOn(address[i_copy]);

							CurDialingSymbol = address[i_copy];

							var isLastChev = i_copy == addrLen - 1;
							if (!isLastChev)
							{
								// Event.Run( StargateEvent.ChevronEncoded, this, i_copy + 1 );
							}
							else
							{
								var isValid = gateValidCheck();

								IsLocked = true;
								IsLockedInvalid = !isValid;

								// Event.Run( StargateEvent.ChevronLocked, this, i_copy + 1, isValid );
							}
						},
						TimedTaskCategory.DIALING
					);
				}

				async void openOrStop()
				{
					if (gateValidCheck()) // if valid, open both gates
					{
						EstablishWormholeTo(target);
					}
					else
					{
						await Task.DelaySeconds(0.25f); // otherwise wait a bit, fail and stop dialing
						StopDialing();
					}
				}

				AddTask(startTime + 7, openOrStop, TimedTaskCategory.DIALING);
			}
			catch (Exception)
			{
				if (this.IsValid())
					StopDialing();
			}
		}

		// FAST INBOUND
		public async override void BeginInboundFast(int numChevs)
		{
			base.BeginInboundFast(numChevs);

			if (!IsStargateReadyForInboundFast())
				return;

			// Event.Run( StargateEvent.InboundBegin, this );

			try
			{
				if (Dialing)
				{
					StopDialing(true);
					ShouldStopDialing = true;
					await Task.DelaySeconds(0.05f);
					ShouldStopDialing = false;
				}

				CurGateState = GateState.ACTIVE;
				Inbound = true;

				DoPreRoll();

				var rollStartTime = Time.Now + 0.5f;
				var rollEndTime = rollStartTime + 4.8f;

				AddTask(rollStartTime, () => Ring.SpinUp(), TimedTaskCategory.DIALING);
				AddTask(rollEndTime, () => Ring.SpinDown(), TimedTaskCategory.DIALING);
			}
			catch (Exception)
			{
				if (this.IsValid())
					StopDialing();
			}
		}

		// SLOW DIAL
		public override async void BeginDialSlow(string address, float initialDelay = 0)
		{
			base.BeginDialSlow(address, initialDelay);

			if (!CanStargateStartDial())
				return;

			// Event.Run( StargateEvent.DialBegin, this, address );

			try
			{
				CurGateState = GateState.DIALING;
				CurDialType = DialType.SLOW;

				if (!IsValidFullAddress(address))
				{
					StopDialing();
					return;
				}

				if (initialDelay > 0)
					await Task.DelaySeconds(initialDelay);

				if (ShouldStopDialing || !Dialing)
					return;

				await DoPreRollAsync();

				//await Task.DelaySeconds( 1.5f );

				Stargate target = null;
				var readyForOpen = false;

				bool gateValidCheck(bool noBeginInbound = false)
				{
					target = FindDestinationGateByDialingAddress(this, address); // if its last chevron, try to find the target gate
					if (
						target.IsValid()
						&& target != this
						&& target.IsStargateReadyForInboundInstantSlow()
					)
					{
						if (!noBeginInbound)
							target.BeginInboundSlow(address.Length);

						return true;
					}

					return false;
				}

				if (ShouldStopDialing || !Dialing)
					return;

				foreach (var sym in address)
				{
					var isLastChev = sym == address.Last();

					if (ShouldStopDialing)
					{
						StopDialing();
						return;
					}

					var success = await RotateRingToSymbol(sym); // wait for ring to rotate to the target symbol
					if (!success || ShouldStopDialing)
					{
						StopDialing();
						return;
					}

					void symbolAction()
					{
						SymbolOn(sym);
						Bearing?.TurnOn(0.1f);

						CurDialingSymbol = sym;

						if (!isLastChev)
						{
							Bearing?.TurnOff(0.6f);
							// Event.Run( StargateEvent.ChevronEncoded, this, address.IndexOf( sym ) + 1 );
						}
						else
						{
							var isValid = gateValidCheck(true);

							IsLocked = true;
							IsLockedInvalid = !isValid;

							// Event.Run( StargateEvent.ChevronLocked, this, address.IndexOf( sym ) + 1, isValid );
						}
					}

					AddTask(Time.Now + 0.65f, symbolAction, TimedTaskCategory.DIALING);

					await Task.DelaySeconds(1.25f);

					if (isLastChev)
						readyForOpen = gateValidCheck();
				}

				void openOrStop()
				{
					if (readyForOpen) // if valid, open both gates
					{
						EstablishWormholeTo(target);
					}
					else
					{
						StopDialing();
					}
				}

				AddTask(Time.Now + 1f, openOrStop, TimedTaskCategory.DIALING);
			}
			catch (Exception)
			{
				if (this.IsValid())
					StopDialing();
			}
		}

		// SLOW INBOUND
		public override void BeginInboundSlow(int numChevs)
		{
			base.BeginInboundSlow(numChevs);

			if (!IsStargateReadyForInboundInstantSlow())
				return;

			// Event.Run( StargateEvent.InboundBegin, this );

			try
			{
				if (Dialing)
					DoStargateReset();

				CurGateState = GateState.ACTIVE;
				Inbound = true;

				DoPreRoll();

				ActiveChevrons = numChevs;
			}
			catch (Exception)
			{
				if (this.IsValid())
					StopDialing();
			}
		}

		public override void BeginDialInstant(string address)
		{
			base.BeginDialInstant(address);

			if (!CanStargateStartDial())
				return;

			// Event.Run( StargateEvent.DialBegin, this, address );

			try
			{
				CurGateState = GateState.DIALING;
				CurDialType = DialType.INSTANT;

				if (!IsValidFullAddress(address))
				{
					StopDialing();
					return;
				}

				var otherGate = FindDestinationGateByDialingAddress(this, address);
				if (
					!otherGate.IsValid()
					|| otherGate == this
					|| !otherGate.IsStargateReadyForInboundInstantSlow()
				)
				{
					StopDialing();
					return;
				}

				DoPreRoll();
				otherGate.BeginInboundSlow(address.Length);

				void activateSymbols()
				{
					foreach (var sym in address)
					{
						SymbolOn(sym, sym != address.Last());
					}
				}

				AddTask(Time.Now + 0.2f, activateSymbols, TimedTaskCategory.DIALING);

				AddTask(
					Time.Now + 0.5f,
					() => EstablishWormholeTo(otherGate),
					TimedTaskCategory.DIALING
				);
			}
			catch (Exception)
			{
				if (this.IsValid())
					StopDialing();
			}
		}

		// DHD DIAL

		public override async void BeginOpenByDHD(string address)
		{
			base.BeginOpenByDHD(address);

			if (!CanStargateStartDial())
				return;

			try
			{
				CurGateState = GateState.DIALING;
				CurDialType = DialType.DHD;

				await Task.DelaySeconds(0.35f);

				var otherGate = FindDestinationGateByDialingAddress(this, address);
				if (
					otherGate.IsValid()
					&& otherGate != this
					&& otherGate.IsStargateReadyForInboundDHD()
				)
				{
					otherGate.BeginInboundDHD(address.Length);
				}
				else
				{
					StopDialing();
					return;
				}

				await Task.DelaySeconds(0.15f);

				EstablishWormholeTo(otherGate);
			}
			catch (Exception)
			{
				if (this.IsValid())
					StopDialing();
			}
		}

		public override async void BeginInboundDHD(int numChevs)
		{
			base.BeginInboundDHD(numChevs);

			if (!IsStargateReadyForInboundDHD())
				return;

			// Event.Run( StargateEvent.InboundBegin, this );

			try
			{
				if (Dialing)
					DoStargateReset();

				CurGateState = GateState.ACTIVE;
				Inbound = true;

				DoPreRoll();
			}
			catch (Exception)
			{
				if (this.IsValid())
					StopDialing();
			}
		}

		// CHEVRON STUFF - DHD DIALING
		public override void DoDHDChevronEncode(char sym)
		{
			base.DoDHDChevronEncode(sym);

			if (DialingAddress.Length == 1)
				DoPreRoll();

			AddTask(
				Time.Now + 0.25f,
				() => SymbolOn(sym, DialingAddress.Length == 1),
				TimedTaskCategory.DIALING
			);
		}

		public override void DoDHDChevronLock(char sym) // only the top chevron locks, always
		{
			base.DoDHDChevronLock(sym);

			AddTask(Time.Now + 0.25f, () => SymbolOn(sym), TimedTaskCategory.DIALING);
		}

		public override async Task<bool> DoManualChevronEncode(char sym)
		{
			if (!await base.DoManualChevronEncode(sym))
				return false;

			IsManualDialInProgress = true;

			var chevNum = DialingAddress.Length + 1;

			if (chevNum == 1)
			{
				await DoPreRollAsync();
			}

			var success = await RotateRingToSymbol(sym); // wait for ring to rotate to the target symbol
			if (!success || ShouldStopDialing)
			{
				StopDialing();
				return false;
			}

			void symbolAction()
			{
				SymbolOn(sym);
				Bearing?.TurnOn(0.1f);

				CurDialingSymbol = sym;

				DialingAddress += sym;
				ActiveChevrons++;

				Bearing?.TurnOff(0.6f);
				// Event.Run( StargateEvent.ChevronEncoded, this, chevNum );
			}

			AddTask(Time.Now + 0.65f, symbolAction, TimedTaskCategory.DIALING);

			await Task.DelaySeconds(1.25f);

			IsManualDialInProgress = false;

			return true;
		}

		public override async Task<bool> DoManualChevronLock(char sym)
		{
			if (!await base.DoManualChevronLock(sym))
				return false;

			IsManualDialInProgress = true;

			var chevNum = DialingAddress.Length + 1;

			var success = await RotateRingToSymbol(sym); // wait for ring to rotate to the target symbol
			if (!success || ShouldStopDialing)
			{
				StopDialing();
				return false;
			}

			bool gateValidCheck(bool noBeginInbound = false)
			{
				var target = FindDestinationGateByDialingAddress(this, DialingAddress); // if its last chevron, try to find the target gate
				if (
					target.IsValid()
					&& target != this
					&& target.IsStargateReadyForInboundInstantSlow()
				)
				{
					if (!noBeginInbound)
						target.BeginInboundSlow(DialingAddress.Length);

					return true;
				}

				return false;
			}

			void symbolAction()
			{
				SymbolOn(sym);
				Bearing?.TurnOn(0.1f);

				CurDialingSymbol = sym;

				DialingAddress += sym;
				ActiveChevrons++;

				var isValid = gateValidCheck(true);

				IsLocked = true;
				IsLockedInvalid = !isValid;

				// Event.Run( StargateEvent.ChevronLocked, this, DialingAddress.Length, isValid );
			}

			AddTask(Time.Now + 0.65f, symbolAction, TimedTaskCategory.DIALING);

			await Task.DelaySeconds(1.25f);

			BeginManualOpen(DialingAddress);

			return true;
		}

		public override async void BeginManualOpen(string address)
		{
			try
			{
				var otherGate = FindDestinationGateByDialingAddress(this, address);
				if (
					otherGate.IsValid()
					&& otherGate != this
					&& otherGate.IsStargateReadyForInboundInstantSlow()
				)
				{
					otherGate.BeginInboundSlow(address.Length);
					IsManualDialInProgress = false;

					await Task.DelaySeconds(0.5f);

					EstablishWormholeTo(otherGate);
				}
				else
				{
					StopDialing();
					return;
				}
			}
			catch (Exception)
			{
				if (this.IsValid())
					StopDialing();
			}
		}
	}
}

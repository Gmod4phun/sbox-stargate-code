namespace Sandbox.Components.Stargate
{
	public class StargateRingPegasus : Component
	{
		// ring variables

		[Property]
		public StargatePegasus Gate => GameObject.Parent.Components.Get<StargatePegasus>();

		public static string RingSymbols => "@E2LMQYB3OIWT8UG967KVZNR0#F1JSHXPDCA";

		[Property]
		public IEnumerable<Superglyph> Glyphs =>
			GameObject.Components.GetAll<Superglyph>(FindMode.InSelf);

		[Property]
		public ModelRenderer RingModel { get; set; }

		// [Property]
		// public List<SkinnedModelRenderer> SymbolParts { get; set; } = new();

		public List<int> DialSequenceActiveSymbols { get; private set; } = new();
		private MultiWorldSound RollSound { get; set; }

		public void CreateGlyphs()
		{
			for (var i = 0; i < 36; i++)
			{
				var glyph = Components.Create<Superglyph>();
				glyph.Model = Model.Load("models/sbox_stargate/sg_peg/sg_peg_glyph_flipbook.vmdl");
				glyph.PositionOnRing = i;
				glyph.GlyphNumber = i;
			}
		}

		// create symbols
		// symbol models

		public int GetSymbolNum(int num)
		{
			return num.UnsignedMod(36);
		}

		public int GetSymbolNumFromChevron(int chevNum)
		{
			return GetSymbolNum((4 * chevNum) - 1);
		}

		public async void SetSymbolState(int num, int displayedGlyph, bool state, float delay = 0)
		{
			if (delay > 0)
			{
				await Task.DelaySeconds(delay);
			}

			num = (num + 1).UnsignedMod(36);
			var glyph = Glyphs.First(g => g.PositionOnRing == num);
			glyph.GlyphNumber = displayedGlyph;
			glyph.GlyphEnabled = state;
		}

		public async void SetRingState(bool state, float delay = 0)
		{
			if (delay > 0)
			{
				await Task.DelaySeconds(delay);
			}

			if (RingModel.IsValid())
			{
				RingModel.SetBodyGroup("glyphs", 0);
			}
		}

		public void RollSymbol(
			int displayedGlyph,
			int start,
			int count,
			bool counterclockwise = false,
			float time = 2.0f
		)
		{
			if (start < 0 || start > 35)
				return;

			var startTime = Time.Now;
			var delay = time / (count + 1);

			try
			{
				for (var i = 0; i <= count; i++)
				{
					var i_copy = i;
					var taskTime = startTime + (delay * i_copy);

					void rollSym()
					{
						if (Gate.ShouldStopDialing)
							return;

						var symIndex = counterclockwise ? (start - i_copy) : start + i_copy;
						var symPrevIndex = counterclockwise ? (symIndex + 1) : symIndex - 1;

						if (!DialSequenceActiveSymbols.Contains(symIndex.UnsignedMod(36)))
						{
							SetSymbolState(symIndex, displayedGlyph, true);
						}

						if (!DialSequenceActiveSymbols.Contains(symPrevIndex.UnsignedMod(36)))
						{
							SetSymbolState(symPrevIndex, displayedGlyph, false);
						}

						if (i_copy == count)
						{
							DialSequenceActiveSymbols.Add(symIndex.UnsignedMod(36));
						}
					}

					Gate.AddTask(taskTime, rollSym, Stargate.TimedTaskCategory.DIALING);
				}
			}
			catch (Exception) { }
		}

		public void ResetSymbols(bool clearDialActive = true)
		{
			for (int i = 0; i <= 35; i++)
				SetSymbolState(i, i, false);
			if (clearDialActive)
				DialSequenceActiveSymbols.Clear();
		}

		public void ResetSymbol(int num, bool clearDialActive = true)
		{
			SetSymbolState(num, num, false);
			if (clearDialActive)
				DialSequenceActiveSymbols.Remove(num);
		}

		public void LightupSymbols()
		{
			for (int i = 0; i <= 35; i++)
				SetSymbolState(i, i, true);
		}

		public void PlayRollSound(bool fast = false)
		{
			StopRollSound();
			RollSound = Stargate.PlayFollowingSound(
				GameObject,
				Gate.GetSound(fast ? "gate_roll_fast" : "gate_roll_slow")
			);
		}

		public void StopRollSound()
		{
			RollSound?.Stop();
		}

		// INBOUND
		public void RollSymbolsInbound(float time, float startDelay = 0, int chevCount = 7)
		{
			// try
			{
				var startTime = Time.Now;

				void firstRun()
				{
					ResetSymbols();
					SetSymbolState(0, 0, true);
				}

				var pegasusSymbolChevrons = new Dictionary<int, int>()
				{
					{ 3, 1 },
					{ 7, 2 },
					{ 11, 3 },
					{ 15, 8 },
					{ 19, 9 },
					{ 23, 4 },
					{ 27, 5 },
					{ 31, 6 },
					{ 35, 7 }
				};

				var delay = time / 35f;
				for (int i = 0; i <= 35; i++)
				{
					var i_copy = i;
					var taskTime = startTime + startDelay + (delay * i_copy);

					Gate.AddTask(
						taskTime,
						i_copy == 0 ? firstRun : () => SetSymbolState(i_copy, i_copy, true),
						Stargate.TimedTaskCategory.DIALING
					);

					if ((i + 1) % 4 == 0)
					{
						var chev = Gate.GetChevron(pegasusSymbolChevrons[i]);
						void chevAction() =>
							Gate.ChevronActivate(
								chev,
								delay * 0.5f,
								true,
								i_copy == 35,
								i_copy == 11 && chevCount == 7,
								i_copy == 31
							);

						if (
							(chevCount == 7 && i != 15 && i != 19)
							|| (chevCount == 8 && i != 19)
							|| (chevCount == 9)
						)
						{
							Gate.AddTask(taskTime, chevAction, Stargate.TimedTaskCategory.DIALING);
						}
					}
				}
			}
			// catch ( Exception ) { }
		}

		public void DoSymbolsInboundInstant()
		{
			for (int i = 0; i <= 35; i++)
				SetSymbolState(i, i, true);
		}

		public async Task<bool> RollSymbolSlow(char symbol, int chevNum, bool isLast = false)
		{
			// try
			{
				//ResetSymbols();

				var dataSymbols7 = new int[7, 2]
				{
					{ 35, 32 },
					{ 3, 40 },
					{ 7, 32 },
					{ 11, 48 },
					{ 23, 32 },
					{ 27, 40 },
					{ 31, 32 }
				};
				var dataSymbols8 = new int[8, 2]
				{
					{ 35, 32 },
					{ 3, 40 },
					{ 7, 32 },
					{ 11, 48 },
					{ 23, 32 },
					{ 27, 40 },
					{ 31, 52 },
					{ 15, 56 }
				};
				var dataSymbols9 = new int[9, 2]
				{
					{ 35, 32 },
					{ 3, 40 },
					{ 7, 32 },
					{ 11, 48 },
					{ 23, 32 },
					{ 27, 40 },
					{ 31, 52 },
					{ 15, 40 },
					{ 19, 56 }
				};

				//var data = (chevNum == 9) ? dataSymbols9 : ((chevNum == 8) ? dataSymbols8 : dataSymbols7);
				var data = dataSymbols7;
				if (chevNum == 8 || chevNum == 7 && !isLast)
				{
					data = dataSymbols8;
				}
				if (chevNum == 9 || (chevNum == 8 && !isLast))
				{
					data = dataSymbols9;
				}

				var rollStartDelay = 0.75f;
				var startTime = Time.Now;

				var i_copy = chevNum - 1;
				var startPos = data[i_copy, 0];
				var symSteps = data[i_copy, 1];
				var symRollTime = symSteps * 0.05f;

				var rollSoundTaskTime = startTime;
				var symTaskTime = startTime + rollStartDelay;
				var finishTime = startTime + rollStartDelay + symRollTime;

				Gate.AddTask(
					rollSoundTaskTime,
					() => PlayRollSound(),
					Stargate.TimedTaskCategory.DIALING
				);
				Gate.AddTask(
					symTaskTime,
					() =>
						RollSymbol(
							RingSymbols.IndexOf(symbol),
							startPos,
							symSteps,
							i_copy % 2 == 0,
							symRollTime
						),
					Stargate.TimedTaskCategory.DIALING
				);
				Gate.AddTask(finishTime, () => StopRollSound(), Stargate.TimedTaskCategory.DIALING);

				if (i_copy == 0)
					Gate.AddTask(
						symTaskTime,
						() => SetRingState(false),
						Stargate.TimedTaskCategory.DIALING
					);

				await Task.DelaySeconds(finishTime - startTime);

				return true;

				/*
				void chevTask()
				{
				    StopRollSound();

				    Gate.CurDialingSymbol = symbol;

				    var chev = Gate.GetChevronBasedOnAddressLength( chevNum, chevNum );
				    if ( i_copy < chevCount - 1 )
				    {
				        Gate.ChevronActivateDHD( chev, 0, true );
				        Event.Run( StargateEvent.ChevronEncoded, Gate, i_copy + 1 );
				    }
				    else
				    {
				        var isValid = validCheck();
				        Gate.IsLocked = true;
				        Gate.IsLockedInvalid = !isValid;

				        Gate.ChevronActivate( chev, 0, isValid, true );
				        Event.Run( StargateEvent.ChevronLocked, Gate, i_copy + 1, isValid );
				    }
				}

				Gate.AddTask( chevTaskTime, chevTask, Stargate.TimedTaskCategory.DIALING );
				*/
			}
			// catch ( Exception )
			// {
			//     return false;
			// }
		}

		// SLOWDIAL
		public void RollSymbolsDialSlow(string address, Func<bool> validCheck)
		{
			// try
			{
				ResetSymbols();

				var chevCount = address.Length;

				var dataSymbols7 = new int[7, 2]
				{
					{ 35, 32 },
					{ 3, 40 },
					{ 7, 32 },
					{ 11, 48 },
					{ 23, 32 },
					{ 27, 40 },
					{ 31, 32 }
				};
				var dataSymbols8 = new int[8, 2]
				{
					{ 35, 32 },
					{ 3, 40 },
					{ 7, 32 },
					{ 11, 48 },
					{ 23, 32 },
					{ 27, 40 },
					{ 31, 52 },
					{ 15, 56 }
				};
				var dataSymbols9 = new int[9, 2]
				{
					{ 35, 32 },
					{ 3, 40 },
					{ 7, 32 },
					{ 11, 48 },
					{ 23, 32 },
					{ 27, 40 },
					{ 31, 52 },
					{ 15, 40 },
					{ 19, 56 }
				};

				var data =
					(chevCount == 9)
						? dataSymbols9
						: ((chevCount == 8) ? dataSymbols8 : dataSymbols7);

				var rollStartDelay = 0.75f;
				var delayBetweenSymbols = 1.25f;
				var startTime = Time.Now;

				var elapsedTime = 0f;
				for (int i = 0; i < chevCount; i++)
				{
					var i_copy = i;
					var startPos = data[i_copy, 0];
					var symSteps = data[i_copy, 1];
					var symRollTime = symSteps * 0.05f;

					var rollSoundTaskTime = startTime + elapsedTime;
					var symTaskTime = startTime + elapsedTime + rollStartDelay;
					var chevTaskTime = startTime + elapsedTime + rollStartDelay + symRollTime;

					elapsedTime += rollStartDelay + symRollTime + delayBetweenSymbols;

					var symbol = address[i_copy];

					Gate.AddTask(
						rollSoundTaskTime,
						() => PlayRollSound(),
						Stargate.TimedTaskCategory.DIALING
					);
					Gate.AddTask(
						symTaskTime,
						() =>
							RollSymbol(
								RingSymbols.IndexOf(symbol),
								startPos,
								symSteps,
								i_copy % 2 == 0,
								symRollTime
							),
						Stargate.TimedTaskCategory.DIALING
					);

					if (i_copy == 0)
						Gate.AddTask(
							symTaskTime,
							() => SetRingState(false),
							Stargate.TimedTaskCategory.DIALING
						);

					void chevTask()
					{
						StopRollSound();

						Gate.CurDialingSymbol = address[i_copy];

						var chev = Gate.GetChevronBasedOnAddressLength(i_copy + 1, chevCount);
						if (i_copy < chevCount - 1)
						{
							Gate.ChevronActivateDHD(chev, 0, true);
							// Event.Run( StargateEvent.ChevronEncoded, Gate, i_copy + 1 );
						}
						else
						{
							var isValid = validCheck();
							Gate.IsLocked = true;
							Gate.IsLockedInvalid = !isValid;

							Gate.ChevronActivate(chev, 0, isValid, true);
							// Event.Run( StargateEvent.ChevronLocked, Gate, i_copy + 1, isValid );
						}
					}

					Gate.AddTask(chevTaskTime, chevTask, Stargate.TimedTaskCategory.DIALING);
				}
			}
			// catch ( Exception ) { }
		}

		// FASTDIAL
		public void RollSymbolsDialFast(string address, Func<bool> validCheck)
		{
			// try
			{
				ResetSymbols();

				SetRingState(false);

				PlayRollSound(true);

				var chevCount = address.Length;

				var symRollTime = 5f / chevCount;
				var delayBetweenSymbols = 1.5f / (chevCount - 1);

				var dataSymbols7 = new List<int>() { 27, 19, 35, 35, 15, 7, 23 };
				var dataSymbols8 = new List<int>() { 27, 19, 35, 35, 15, 7, 3, 11 };
				var dataSymbols9 = new List<int>() { 27, 19, 35, 35, 15, 7, 3, 31, 23 };

				var data =
					(chevCount == 9)
						? dataSymbols9
						: ((chevCount == 8) ? dataSymbols8 : dataSymbols7);

				var startTime = Time.Now;
				for (int i = 0; i < chevCount; i++)
				{
					var i_copy = i;
					var symTaskTime = startTime + (symRollTime + delayBetweenSymbols) * (i_copy);
					var symbol = address[i_copy];

					Gate.AddTask(
						symTaskTime,
						() =>
							RollSymbol(
								RingSymbols.IndexOf(symbol),
								data[i_copy],
								12,
								i_copy % 2 == 1,
								symRollTime
							),
						Stargate.TimedTaskCategory.DIALING
					);

					var chevTaskTime =
						startTime
						+ (symRollTime + delayBetweenSymbols) * (i_copy + 1)
						- delayBetweenSymbols;
					Gate.AddTask(
						chevTaskTime,
						() =>
						{
							Gate.CurDialingSymbol = address[i_copy];

							var isLastChev = i_copy == chevCount - 1;
							Gate.ChevronActivate(
								Gate.GetChevronBasedOnAddressLength(i_copy + 1, chevCount),
								0,
								isLastChev ? validCheck() : true,
								isLastChev
							);
							if (!isLastChev)
							{
								// Event.Run( StargateEvent.ChevronEncoded, Gate, i_copy + 1 );
							}
							else
							{
								Gate.IsLocked = true;
								Gate.IsLockedInvalid = !validCheck();

								// Event.Run( StargateEvent.ChevronLocked, Gate, i_copy + 1, validCheck() );
							}
						},
						Stargate.TimedTaskCategory.DIALING
					);
				}
			}
			// catch ( Exception ) { }
		}

		public void RollSymbolDHDFast(
			char symbol,
			int chevCount,
			Func<bool> validCheck,
			int chevNum,
			float symRollTime
		)
		{
			// try
			{
				SetRingState(false);

				var dataSymbols7 = new List<int>() { 27, 19, 35, 35, 15, 7, 23 };
				var dataSymbols8 = new List<int>() { 27, 19, 35, 35, 15, 7, 3, 11 };
				var dataSymbols9 = new List<int>() { 27, 19, 35, 35, 15, 7, 3, 31, 23 };

				var data =
					(chevCount == 9)
						? dataSymbols9
						: ((chevCount == 8) ? dataSymbols8 : dataSymbols7);

				var isLast = chevNum == chevCount;

				var startTime = Time.Now;
				var symTaskTime = startTime;
				Gate.AddTask(
					symTaskTime,
					() =>
						RollSymbol(
							RingSymbols.IndexOf(symbol),
							data[chevNum - 1],
							12,
							(chevNum - 1) % 2 == 1,
							symRollTime
						),
					Stargate.TimedTaskCategory.SYMBOL_ROLL_PEGASUS_DHD
				);

				var chevTaskTime = startTime + symRollTime;
				Gate.AddTask(
					chevTaskTime,
					() =>
						Gate.ChevronActivateDHD(
							Gate.GetChevronBasedOnAddressLength(chevNum, chevCount),
							0,
							isLast ? validCheck() : true
						),
					Stargate.TimedTaskCategory.SYMBOL_ROLL_PEGASUS_DHD
				);
			}
			// catch ( Exception ) { }
		}

		// DEBUG
		public void DrawSymbols()
		{
			var deg = 10;
			var ang = Transform.Rotation.Angles();
			for (int i = 0; i < 36; i++)
			{
				var rotAng = ang.WithRoll(ang.roll - (i * deg) - deg);
				var newRot = rotAng.ToRotation();
				var pos = Transform.Position + newRot.Forward * 4 + newRot.Up * 117.5f;
				// DebugOverlay.Text( i.ToString(), pos, Color.Yellow );
			}
		}

		/*
		[GameEvent.Client.Frame]
		public void RingSymbolsDebug()
		{
		    //DrawSymbols();
		}
		*/
	}
}

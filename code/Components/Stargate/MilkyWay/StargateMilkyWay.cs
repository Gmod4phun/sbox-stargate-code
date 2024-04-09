namespace Sandbox.Components.Stargate
{
	public class StargateMilkyWay : Stargate
	{
		public StargateMilkyWay()
		{
			SoundDict = new Dictionary<string, string>
			{
				{ "gate_open", "stargate.milkyway.open" },
				{ "gate_close", "stargate.milkyway.close" },
				{ "chevron_open", "stargate.milkyway.chevron_open" },
				{ "chevron_close", "stargate.milkyway.chevron_close" },
				{ "dial_fail", "stargate.milkyway.dial_fail_noclose" },
				{ "dial_fail_noclose", "stargate.milkyway.dial_fail_noclose" },
				{ "dial_begin_9chev", "stargate.universe.dial_begin_9chev" },
				{ "dial_fail_9chev", "stargate.universe.dial_fail_9chev" }
			};

			GateGlyphType = GlyphType.MILKYWAY;
			GateGroup = "M@";
			GateAddress = GenerateGateAddress( GateGroup );
		}

		public override float RingRotationStepSize => AcceleratedDialup ? 1f : 0.2f;

		[Sync]
		public NetList<Chevron> EncodedChevronsOrdered { get; set; } = new();

		public StargateRingMilkyWay Ring => Components.Get<StargateRingMilkyWay>( FindMode.EnabledInSelfAndDescendants );

		[Property, Sync]
		public bool MovieDialingType { get; set; } = false; // when enabled, encodes the symbol under each chevron like in the movie

		[Property, Sync]
		public bool ChevronLightup { get; set; } = true;

		[Property, Sync]
		public bool AcceleratedDialup { get; set; } = false;

		public static void DrawGizmos( EditorContext context )
		{
			Gizmo.Draw.Model( "models/sbox_stargate/sg_mw/sg_mw_ring.vmdl" );

			for ( var i = 0; i < 9; i++ )
			{
				Gizmo.Draw.Model( "models/sbox_stargate/sg_mw/sg_mw_chevron.vmdl", new Transform( Vector3.Zero, Rotation.FromRoll( i * 40 ) ) );
			}
		}

		public override void ResetGateVariablesToIdle()
		{
			base.ResetGateVariablesToIdle();

			EncodedChevronsOrdered.Clear();
		}

		public async Task<bool> RotateRingToSymbol( char sym, int angOffset = 0 )
		{
			return await Ring.RotateToSymbol( sym, angOffset );
		}

		// DIALING

		public override void OnStopDialingBegin()
		{
			base.OnStopDialingBegin();

			PlaySound( this, (ActiveChevrons > 0 || CurDialType is DialType.DHD) ? GetSound( "dial_fail" ) : GetSound( "dial_fail_noclose" ) );

			if ( Ring.IsValid() && Ring.IsMoving ) Ring.SpinDown();
		}

		public override void OnStopDialingFinish()
		{
			base.OnStopDialingFinish();

			SetChevronsGlowState( false );
			ChevronAnimUnlockAll();
		}

		public override void OnStargateBeginOpen()
		{
			base.OnStargateBeginOpen();

			PlaySound( this, GetSound( "gate_open" ) );
		}

		public override void OnStargateOpened()
		{
			base.OnStargateOpened();
		}

		public override void OnStargateBeginClose()
		{
			base.OnStargateBeginClose();

			PlaySound( this, GetSound( "gate_close" ) );
		}

		public override void OnStargateClosed()
		{
			base.OnStargateClosed();

			SetChevronsGlowState( false );
			ChevronAnimUnlockAll();
		}

		public override async void DoStargateReset()
		{
			if ( Dialing )
			{
				ShouldStopDialing = true;
				// await Task.DelaySeconds( Game.Tick * 4 ); // give the ring logic a chance to catch up
				await Task.DelaySeconds( 0.05f );
			}

			base.DoStargateReset();
			SetChevronsGlowState( false );
		}

		// CHEVRON ANIMS & SOUNDS

		public void ChevronAnimLockUnlock( Chevron chev, bool lightup = true, bool keeplit = false )
		{
			if ( chev.IsValid() )
			{
				chev.Open = true;
				chev.ChevronClose( 0.75f );

				if ( lightup )
				{
					chev.TurnOn( 0.5f );
					if ( !keeplit ) chev.TurnOff( 1.5f );
				}
			}
		}

		public void ChevronAnimLock( Chevron chev, float delay = 0, bool turnon = false )
		{
			if ( chev.IsValid() )
			{
				chev.ChevronOpen( delay );
				if ( turnon ) chev.TurnOn( delay );
			}
		}

		public void ChevronAnimUnlock( Chevron chev, float delay = 0, bool turnoff = false )
		{
			if ( chev.IsValid() )
			{
				chev.ChevronClose( delay );
				if ( turnoff ) chev.TurnOff( delay );
			}
		}

		public void ChevronActivate( Chevron chev, float delay = 0, bool turnon = false, bool noSound = false )
		{
			if ( chev.IsValid() )
			{
				if ( !noSound )
					Stargate.PlaySound( chev, GetSound( "chevron_open" ), delay );

				if ( turnon )
					chev.TurnOn( delay );
			}
		}

		public void ChevronDeactivate( Chevron chev, float delay = 0, bool turnoff = false, bool noSound = false )
		{
			if ( chev.IsValid() )
			{
				if ( !noSound )
					Stargate.PlaySound( chev, GetSound( "chevron_close" ), delay );

				if ( turnoff )
					chev.TurnOff( delay );
			}
		}

		public void ChevronAnimLockAll( int num, float delay = 0, bool turnon = false )
		{
			for ( int i = 1; i <= num; i++ )
			{
				ChevronAnimLock( GetChevronBasedOnAddressLength( i, num ), delay, turnon );
			}
		}

		public void ChevronAnimUnlockAll( float delay = 0, bool turnoff = false )
		{
			foreach ( var chev in Chevrons )
			{
				if ( chev.Open ) ChevronAnimUnlock( chev, delay, turnoff );
			}
		}

		// INDIVIDUAL DIAL TYPES

		// FAST DIAL
		public override async void BeginDialFast( string address )
		{
			base.BeginDialFast( address );

			if ( !CanStargateStartDial() ) return;

			// Event.Run( StargateEvent.DialBegin, this, address );

			try
			{
				CurGateState = GateState.DIALING;
				CurDialType = DialType.FAST;

				if ( !IsValidFullAddress( address ) )
				{
					StopDialing();
					return;
				}

				var target = FindDestinationGateByDialingAddress( this, address );
				var wasTargetReadyOnStart = false; // if target gate was not available on dial start, dont bother doing anything at the end

				if ( target.IsValid() && target != this && target.IsStargateReadyForInboundFast() )
				{
					wasTargetReadyOnStart = true;

					target.BeginInboundFast( GetSelfAddressBasedOnOtherAddress( this, address ).Length );

					OtherGate = target; // this is needed so that the gate can stop dialing if we cancel the dial
					OtherGate.OtherGate = this;
				}

				_ = Ring.SpinUp(); // start rotating ring

				var addrLen = address.Length;

				// duration of the dial until the gate starts opening - let's stick to 7 seconds for total (just like GMod stargates)
				// default values are for 7 chevron sequence
				var chevronsStartDelay = (addrLen == 9) ? 0.60f : ((addrLen == 8) ? 0.70f : 0.70f);
				var chevronsLoopDuration = (addrLen == 9) ? 4.40f : ((addrLen == 8) ? 4.25f : 3.90f);
				var chevronBeforeLastDelay = (addrLen == 9) ? 0.75f : ((addrLen == 8) ? 0.80f : 1.05f);
				var chevronAfterLastDelay = (addrLen == 9) ? 1.25f : ((addrLen == 8) ? 1.25f : 1.35f);
				var chevronDelay = chevronsLoopDuration / (addrLen - 1);

				await Task.DelaySeconds( chevronsStartDelay ); // wait 0.5 sec and start locking chevrons

				// lets encode each chevron but the last
				for ( var i = 1; i < addrLen; i++ )
				{
					if ( ShouldStopDialing )
					{
						StopDialing();
						return;
					} // check if we should stop dialing

					var chev = GetChevronBasedOnAddressLength( i, addrLen );
					if ( chev.IsValid() )
					{
						if ( MovieDialingType )
						{
							ChevronAnimLock( chev, 0, ChevronLightup );
						}
						else
						{
							ChevronActivate( chev, 0, ChevronLightup );
						}

						ActiveChevrons++;
						CurDialingSymbol = address[i - 1];
						// Event.Run( StargateEvent.ChevronEncoded, this, i );
					}

					if ( i == addrLen - 1 ) Ring.SpinDown(); // stop rotating ring when the last looped chevron locks

					await Task.DelaySeconds( chevronDelay );
				}

				if ( ShouldStopDialing )
				{
					StopDialing();
					return;
				} // check if we should stop dialing

				await Task.DelaySeconds( chevronBeforeLastDelay ); // wait before locking the last chevron

				if ( ShouldStopDialing )
				{
					StopDialing();
					return;
				} // check if we should stop dialing

				Busy = true; // gate has to lock last chevron, lets go busy so we cant stop the dialing at this point

				var topChev = GetChevron( 7 ); // lock last (top) chevron
				if ( topChev.IsValid() )
				{
					var readyForOpen = wasTargetReadyOnStart && target.IsValid() && target != this && target.IsStargateReadyForInboundFastEnd();
					if ( readyForOpen )
					{
						if ( ChevronLightup )
							topChev.TurnOn( 0.25f );
					}

					IsLocked = true;
					IsLockedInvalid = !readyForOpen;

					CurDialingSymbol = address[addrLen - 1];

					// Event.Run( StargateEvent.ChevronLocked, this, address.Length, readyForOpen );

					if ( MovieDialingType )
					{
						ChevronAnimLock( topChev, 0.2f );
					}
					else
					{
						// ChevronAnimLock( topChev, 0.2f );
						// ChevronAnimUnlock( topChev, 1f );
						topChev.SetOpen( true );
						topChev.SetOpen( false, 0.8f );
					}

					ActiveChevrons++;
				}

				await Task.DelaySeconds( chevronAfterLastDelay ); // wait after the last chevron, then open the gate or fail dial (if gate became invalid/was busy)

				if ( ShouldStopDialing )
				{
					StopDialing();
					return;
				} // check if we should stop dialing

				Busy = false;

				if ( wasTargetReadyOnStart && target.IsValid() && target != this && target.IsStargateReadyForInboundFastEnd() ) // if valid, open both gates
				{
					EstablishWormholeTo( target );
				}
				else
				{
					await Task.DelaySeconds( 0.25f ); // otherwise wait a bit, fail and stop dialing
					StopDialing();
				}
			}
			catch ( Exception )
			{
				if ( this.IsValid() ) StopDialing();
			}
		}

		// FAST INBOUND
		public override async void BeginInboundFast( int numChevs )
		{
			base.BeginInboundFast( numChevs );

			if ( !IsStargateReadyForInboundFast() ) return;

			// Event.Run( StargateEvent.InboundBegin, this );

			try
			{
				if ( Dialing )
				{
					StopDialing( true );
					ShouldStopDialing = true;
					await Task.DelaySeconds( 0.05f );
					ShouldStopDialing = false;
				}

				CurGateState = GateState.ACTIVE;
				Inbound = true;

				// duration of the dial until the gate starts opening - let's stick to 7 seconds for total (just like GMod stargates)
				// default values are for 7 chevron sequence
				var chevronsStartDelay = (numChevs == 9) ? 0.25f : ((numChevs == 8) ? 0.40f : 0.50f);
				var chevronsLoopDuration = (numChevs == 9) ? 6.75f : ((numChevs == 8) ? 6.60f : 6.75f);
				var chevronBeforeLastDelay = (numChevs == 9) ? 0.50f : ((numChevs == 8) ? 0.60f : 0.50f);
				var chevronDelay = chevronsLoopDuration / (numChevs);

				await Task.DelaySeconds( chevronsStartDelay ); // wait 0.5 sec and start locking chevrons

				for ( var i = 1; i < numChevs; i++ )
				{
					if ( ShouldStopDialing && ActiveChevrons > 0 ) return; // check if we should stop dialing or not

					var chev = GetChevronBasedOnAddressLength( i, numChevs );
					if ( chev.IsValid() )
					{
						if ( MovieDialingType )
						{
							ChevronAnimLock( chev, 0, ChevronLightup );
						}
						else
						{
							ChevronActivate( chev, 0, ChevronLightup );
						}

						ActiveChevrons++;
					}

					await Task.DelaySeconds( chevronDelay ); // each chevron delay
				}

				await Task.DelaySeconds( chevronBeforeLastDelay - 0.4f ); // wait before locking the last chevron

				var topChev = GetChevron( 7 );
				if ( topChev.IsValid() )
				{
					if ( MovieDialingType )
					{
						ChevronAnimLock( topChev, 0, ChevronLightup );
					}
					else
					{
						ChevronActivate( topChev, 0, ChevronLightup );
					}

					ActiveChevrons++;
				}
			}
			catch ( Exception )
			{
				if ( this.IsValid() ) StopDialing();
			}
		}

		// SLOW DIAL
		public override async void BeginDialSlow( string address, float initialDelay = 0 )
		{
			base.BeginDialSlow( address, initialDelay );

			if ( !CanStargateStartDial() ) return;

			// Event.Run( StargateEvent.DialBegin, this, address );

			try
			{
				CurGateState = GateState.DIALING;
				CurDialType = DialType.SLOW;

				if ( !IsValidFullAddress( address ) )
				{
					StopDialing();
					return;
				}

				if ( address.Length == 9 )
				{
					PlaySound( this, GetSound( "dial_begin_9chev" ), 0.2f );
					await Task.DelaySeconds( 1f ); // wait a bit
				}
				else
				{
					if ( initialDelay > 0 )
						await Task.DelaySeconds( initialDelay );
				}

				if ( ShouldStopDialing || !Dialing )
					return;

				Stargate target = null;

				var readyForOpen = false;
				foreach ( var sym in address )
				{
					var chevNum = address.IndexOf( sym ) + 1;
					var isLastChev = (chevNum == address.Length);

					// try to encode each symbol
					var movieOffset = ChevronAngles[Chevrons.IndexOf( GetChevronBasedOnAddressLength( chevNum, address.Length ) )];

					var offset = MovieDialingType ? movieOffset : 0;
					CurRingSymbolOffset = -offset;

					if ( ShouldStopDialing )
					{
						StopDialing();
						return;
					}

					// await Task.DelaySeconds( 0.05f );
					// var success = true;
					var success = await RotateRingToSymbol( sym, offset ); // wait for ring to rotate to the target symbol
					if ( !success || ShouldStopDialing )
					{
						StopDialing();
						return;
					}

					await Task.DelaySeconds( MovieDialingType ? 0.15f : 0.75f ); // wait a bit

					if ( isLastChev ) target = FindDestinationGateByDialingAddress( this, address ); // if its last chevron, try to find the target gate

					// go do chevron stuff
					var chev = GetChevronBasedOnAddressLength( chevNum, address.Length );
					var topChev = GetChevron( 7 );

					if ( !isLastChev )
					{
						if ( MovieDialingType )
						{
							ChevronAnimLockUnlock( chev, ChevronLightup, true );
						}
						else
						{
							// ChevronAnimLockUnlock( topChev, ChevronLightup );
							topChev.ChevronOpen();

							if ( ChevronLightup )
							{
								chev.PlayOpenSound( 0.2f );
								chev.TurnOn( 0.5f );
								topChev.TurnOn( 0.5f );
							}

							topChev.ChevronClose( 1f );

							if ( ChevronLightup )
							{
								topChev.TurnOff( 1.3f );
							}
						}

						// Event.Run( StargateEvent.ChevronEncoded, this, chevNum );
					}
					else
					{
						var valid = target.IsValid() && target != this && target.IsStargateReadyForInboundInstantSlow();
						if ( MovieDialingType )
						{
							ChevronAnimLockAll( chevNum, 0, ChevronLightup );
						}
						else
						{
							// ChevronAnimLockUnlock( topChev, valid && ChevronLightup, true );
							topChev.SetOpen( true );
							if ( valid )
							{
								topChev.TurnOn();
								topChev.SetOpen( false, 0.75f );
							}

						}

						IsLocked = true;
						IsLockedInvalid = !valid;

						// Event.Run( StargateEvent.ChevronLocked, this, chevNum, valid );
					}

					ActiveChevrons++;

					// await Task.DelaySeconds( 0.5f );

					if ( ShouldStopDialing || !Dialing )
					{
						ResetGateVariablesToIdle();
						return;
					}

					if ( isLastChev && target.IsValid() && target != this && target.IsStargateReadyForInboundInstantSlow() )
					{
						target.BeginInboundSlow( address.Length );
						readyForOpen = true;
					}

					await Task.DelaySeconds( isLastChev && MovieDialingType ? 0.5f : 1.75f ); // wait a bit

					chevNum++;
				}

				// prepare for open or fail

				Busy = false;

				if ( target.IsValid() && target != this && readyForOpen )
				{
					EstablishWormholeTo( target );
				}
				else
				{
					StopDialing();
					if ( address.Length == 9 ) PlaySound( this, GetSound( "dial_fail_9chev" ), 0.5f );
				}
			}
			catch ( Exception )
			{
				if ( this.IsValid() ) StopDialing();
			}
		}

		// SLOW INBOUND
		public override void BeginInboundSlow( int numChevs )
		{
			base.BeginInboundSlow( numChevs );

			if ( !IsStargateReadyForInboundInstantSlow() ) return;

			// Event.Run( StargateEvent.InboundBegin, this );

			try
			{
				if ( Dialing ) DoStargateReset();

				CurGateState = GateState.ACTIVE;
				Inbound = true;

				if ( MovieDialingType )
				{
					ChevronAnimLockAll( numChevs, 0, ChevronLightup );
				}
				else
				{
					for ( var i = 1; i <= numChevs; i++ )
					{
						var chev = GetChevronBasedOnAddressLength( i, numChevs );
						if ( chev.IsValid() )
						{
							ChevronActivate( chev, 0, ChevronLightup, noSound: i < numChevs );
						}
					}
				}

				ActiveChevrons = numChevs;
			}
			catch ( Exception )
			{
				if ( this.IsValid() ) StopDialing();
			}
		}

		public override async void BeginDialInstant( string address )
		{
			base.BeginDialInstant( address );

			if ( !CanStargateStartDial() ) return;

			// Event.Run( StargateEvent.DialBegin, this, address );

			try
			{
				CurGateState = GateState.DIALING;
				CurDialType = DialType.INSTANT;

				if ( !IsValidFullAddress( address ) )
				{
					StopDialing();
					return;
				}

				var otherGate = FindDestinationGateByDialingAddress( this, address );
				if ( !otherGate.IsValid() || otherGate == this || !otherGate.IsStargateReadyForInboundInstantSlow() )
				{
					StopDialing();
					return;
				}

				otherGate.BeginInboundSlow( address.Length );

				if ( MovieDialingType )
				{
					ChevronAnimLockAll( address.Length, 0, ChevronLightup );
				}
				else
				{
					for ( var i = 1; i <= address.Length; i++ )
					{
						var chev = GetChevronBasedOnAddressLength( i, address.Length );
						if ( chev.IsValid() )
						{
							ChevronActivate( chev, 0, ChevronLightup );
						}
					}
				}

				await Task.DelaySeconds( 0.5f );

				EstablishWormholeTo( otherGate );
			}
			catch ( Exception )
			{
				if ( this.IsValid() ) StopDialing();
			}
		}

		// DHD DIAL

		public override async void BeginOpenByDHD( string address )
		{
			base.BeginOpenByDHD( address );

			if ( !CanStargateStartDial() ) return;

			try
			{
				CurGateState = GateState.DIALING;
				CurDialType = DialType.DHD;

				await Task.DelaySeconds( 0.35f );

				var otherGate = FindDestinationGateByDialingAddress( this, address );
				if ( otherGate.IsValid() && otherGate != this && otherGate.IsStargateReadyForInboundDHD() )
				{
					otherGate.BeginInboundDHD( address.Length );
				}
				else
				{
					StopDialing();
					return;
				}

				await Task.DelaySeconds( 0.15f );

				EstablishWormholeTo( otherGate );
			}
			catch ( Exception )
			{
				if ( this.IsValid() ) StopDialing();
			}
		}

		public override void BeginInboundDHD( int numChevs )
		{
			base.BeginInboundDHD( numChevs );

			if ( !IsStargateReadyForInboundDHD() ) return;

			// Event.Run( StargateEvent.InboundBegin, this );

			try
			{
				if ( Dialing ) DoStargateReset();

				CurGateState = GateState.ACTIVE;
				Inbound = true;

				for ( var i = 1; i <= numChevs; i++ )
				{
					var chev = GetChevronBasedOnAddressLength( i, numChevs );
					ChevronActivate( chev, 0, ChevronLightup, noSound: i < numChevs );
				}

				if ( MovieDialingType ) ChevronAnimLockAll( numChevs, 0, ChevronLightup );
			}
			catch ( Exception )
			{
				if ( this.IsValid() ) StopDialing();
			}
		}

		// CHEVRON STUFF - DHD DIALING
		public override void DoDHDChevronEncode( char sym )
		{
			base.DoDHDChevronEncode( sym );

			var chev = GetChevronBasedOnAddressLength( DialingAddress.Length, 9 );
			EncodedChevronsOrdered.Add( chev );

			if ( MovieDialingType )
			{
				ChevronAnimLock( chev, 0.15f, ChevronLightup );
			}
			else
			{
				ChevronActivate( chev, 0.15f, ChevronLightup );
			}
		}

		public override void DoDHDChevronLock( char sym ) // only the top chevron locks, always
		{
			base.DoDHDChevronLock( sym );

			var chev = GetTopChevron();
			EncodedChevronsOrdered.Add( chev );

			var gate = FindDestinationGateByDialingAddress( this, DialingAddress );
			var valid = (gate != this && gate.IsValid() && gate.IsStargateReadyForInboundDHD() && ChevronLightup);

			if ( MovieDialingType )
			{
				ChevronAnimLock( chev, 0, valid );
			}
			else
			{
				ChevronAnimLockUnlock( chev, valid, true );
			}
		}

		public override async Task<bool> DoManualChevronEncode( char sym )
		{
			if ( !await base.DoManualChevronEncode( sym ) )
				return false;

			IsManualDialInProgress = true;

			var chevNum = DialingAddress.Length + 1;

			// try to encode each symbol
			var movieOffset = -ChevronAngles[Chevrons.IndexOf( GetChevronBasedOnAddressLength( chevNum, chevNum ) )];

			var offset = MovieDialingType ? movieOffset : 0;
			CurRingSymbolOffset = -offset;

			// var success = await RotateRingToSymbol( sym, offset ); // wait for ring to rotate to the target symbol
			await Task.DelaySeconds( 1f );
			var success = true;
			if ( !success || ShouldStopDialing )
			{
				StopDialing();
				return false;
			}

			await Task.DelaySeconds( MovieDialingType ? 0.15f : 0.65f ); // wait a bit

			// go do chevron stuff
			var chev = GetChevronBasedOnAddressLength( chevNum, chevNum );
			var topChev = GetChevron( 7 );

			if ( MovieDialingType )
			{
				ChevronAnimLockUnlock( chev, ChevronLightup, true );
			}
			else
			{
				ChevronAnimLockUnlock( topChev, ChevronLightup );
				if ( ChevronLightup ) chev.TurnOn( 0.5f );
			}

			DialingAddress += sym;
			ActiveChevrons++;

			// Event.Run( StargateEvent.ChevronEncoded, this, chevNum );

			IsManualDialInProgress = false;

			TimeSinceDialAction = 0;

			return true;
		}

		public override async Task<bool> DoManualChevronLock( char sym )
		{
			if ( !await base.DoManualChevronLock( sym ) )
				return false;

			IsManualDialInProgress = true;

			var chevNum = DialingAddress.Length + 1;

			// try to encode each symbol
			var movieOffset = -ChevronAngles[Chevrons.IndexOf( GetChevronBasedOnAddressLength( chevNum, chevNum ) )];

			var offset = MovieDialingType ? movieOffset : 0;
			CurRingSymbolOffset = -offset;

			// var success = await RotateRingToSymbol( sym, offset ); // wait for ring to rotate to the target symbol
			await Task.DelaySeconds( 1f );
			var success = true;
			if ( !success || ShouldStopDialing )
			{
				StopDialing();
				return false;
			}

			await Task.DelaySeconds( MovieDialingType ? 0.15f : 0.65f ); // wait a bit

			var target = FindDestinationGateByDialingAddress( this, DialingAddress + sym ); // last chevron, so try to find the target gate

			// go do chevron stuff
			var topChev = GetChevron( 7 );

			var valid = target.IsValid() && target != this && target.IsStargateReadyForInboundInstantSlow();
			if ( MovieDialingType )
			{
				ChevronAnimLockAll( chevNum, 0, ChevronLightup );
			}
			else
			{
				ChevronAnimLockUnlock( topChev, valid && ChevronLightup, true );
			}

			DialingAddress += sym;
			ActiveChevrons++;

			IsLocked = true;
			IsLockedInvalid = !valid;

			// Event.Run( StargateEvent.ChevronLocked, this, chevNum, valid );

			await Task.DelaySeconds( 0.75f );

			TimeSinceDialAction = 0;

			BeginManualOpen( DialingAddress );

			return true;
		}

		public override async void BeginManualOpen( string address )
		{
			try
			{
				var otherGate = FindDestinationGateByDialingAddress( this, address );
				if ( otherGate.IsValid() && otherGate != this && otherGate.IsStargateReadyForInboundInstantSlow() )
				{
					otherGate.BeginInboundSlow( address.Length );
					IsManualDialInProgress = false;

					await Task.DelaySeconds( 0.5f );

					EstablishWormholeTo( otherGate );
				}
				else
				{
					await Task.DelaySeconds( 0.75f );

					StopDialing();
					return;
				}
			}
			catch ( Exception )
			{
				if ( this.IsValid() ) StopDialing();
			}
		}

	}
}

using System.Text.Json.Serialization;
using Sandbox.Components.Stargate.Ramps;

namespace Sandbox.Components.Stargate
{
	public partial class Stargate : Component
	{
		// private StargateWorldPanel _worldPanel;
		[Property]
		public string EventHorizonMaterialGroup { get; set; } = "default";

		// [Property]
		public virtual float RingSpeedPerSecond => 40f;

		public GateBearing Bearing =>
			Components.Get<GateBearing>(FindMode.EnabledInSelfAndDescendants);

		[Sync]
		public float AutoCloseTime { get; set; } = -1;

		public Dictionary<string, string> SoundDict { get; set; } =
			new()
			{
				{ "gate_open", "baseValue" },
				{ "gate_close", "baseValue" },
				{ "chevron_open", "baseValue" },
				{ "chevron_close", "baseValue" },
				{ "dial_fail", "baseValue" },
				{ "dial_fail_noclose", "baseValue" },
			};

		[Sync]
		public TimeSince TimeSinceDialAction { get; set; } = 0f;

		public float InactiveDialShutdownTime { get; set; } = 20f;

		// public IStargateRamp Ramp { get; set; } = null;

		public Vector3 SpawnOffset { get; private set; } = new(0, 0, 95);

		public List<Chevron> Chevrons =>
			GameObject
				.Children.Where(go => go.Components.Get<Chevron>().IsValid())
				.Select(go => go.Components.Get<Chevron>())
				.ToList();

		[Property, JsonIgnore]
		public EventHorizon EventHorizon { get; private set; } = null;

		[Property]
		public StargateIris.IrisType? IrisType { get; set; }

		[Button("Add Gate Iris", "")]
		[ShowIf(nameof(HasIris), false)]
		public void ButtonAddIris()
		{
			AddIris(this, IrisType ?? StargateIris.IrisType.Standard);
		}

		[Button("Remove Gate Iris", "")]
		[ShowIf(nameof(HasIris), true)]
		public void ButtonRemoveIris()
		{
			RemoveIris(this);
		}

		[Property, JsonIgnore]
		public StargateIris Iris =>
			GameObject.Components.Get<StargateIris>(FindMode.EnabledInSelfAndDescendants);

		[Button("Toggle Iris", "")]
		[ShowIf(nameof(HasIris), true)]
		public void ButtonToggleIris()
		{
			Iris.Toggle();
		}

		public List<PowerNode> PowerNodes { get; set; } = new();

		[Property]
		public bool HasPowerNodes => PowerNodes.Count > 0;

		[Button("Add Power Nodes", "")]
		[ShowIf(nameof(HasPowerNodes), false)]
		public void ButtonAddPowerNodes()
		{
			AddPowerNodes(this);
		}

		[Button("Remove Power Nodes", "")]
		[ShowIf(nameof(HasPowerNodes), true)]
		public void ButtonRemovePowerNodes()
		{
			RemovePowerNodes(this);
		}

		public bool HasAllActivePowerNodes =>
			PowerNodes.Count == 3 && PowerNodes.All(pn => pn.EnableMovement);

		[Property, JsonIgnore]
		public GateRamp Ramp => GameObject.Components.Get<GateRamp>(FindMode.InParent);

		[Property, JsonIgnore]
		public Stargate OtherGate { get; set; } = null;

		[Property]
		public string GateAddress { get; set; } = "";

		[Property]
		public string GateGroup { get; protected set; } = "";

		public int GateGroupLength { get; set; } = 2;

		[Property]
		public string GateName { get; set; } = "";

		[Property]
		public bool AutoClose { get; set; } = false;

		[Property]
		public bool GatePrivate { get; set; } = false;

		[Property]
		public bool GateLocal { get; set; } = false;

		[Property]
		public GlyphType GateGlyphType { get; protected set; } = GlyphType.MILKYWAY;

		// Show Wormhole or not
		[Property]
		public bool ShowWormholeCinematic { get; set; } = false;

		[Sync]
		public bool Busy { get; set; } = false; // this is pretty much used anytime the gate is busy to do anything (usually during animations/transitions)

		[Sync]
		public bool Inbound { get; set; } = false;

		[Sync]
		public bool ShouldStopDialing { get; set; } = false;

		[Sync]
		public GateState CurGateState { get; set; } = GateState.IDLE;

		[Sync]
		public DialType CurDialType { get; set; } = DialType.FAST;

		[Sync]
		public bool IsManualDialInProgress { get; set; } = false;

		// gate state accessors
		public bool Idle
		{
			get => CurGateState is GateState.IDLE;
		}
		public bool IsActive
		{
			get => CurGateState is GateState.ACTIVE;
		}
		public bool Dialing
		{
			get => CurGateState is GateState.DIALING;
		}
		public bool Opening
		{
			get => CurGateState is GateState.OPENING;
		}
		public bool Open
		{
			get => CurGateState is GateState.OPEN;
		}
		public bool Closing
		{
			get => CurGateState is GateState.CLOSING;
		}

		[Sync]
		public string DialingAddress { get; set; } = "";

		[Sync]
		public int ActiveChevrons { get; set; } = 0;

		[Sync]
		public bool IsLocked { get; set; } = false;

		[Sync]
		public bool IsLockedInvalid { get; set; } = false;

		[Sync]
		public char CurDialingSymbol { get; set; } = ' ';

		[Sync]
		public char CurRingSymbol { get; set; } = ' ';

		[Sync]
		public float CurRingSymbolOffset { get; set; } = 0;

		[Property]
		public bool CanOpenMenu { get; set; } = true;

		protected override void OnStart()
		{
			if (Scene.IsEditor)
				return;

			GameObject.SetupNetworking(orphaned: NetworkOrphaned.Host);

			if (IrisType.HasValue)
			{
				AddIris(this, IrisType.Value);
			}
		}

		public Dhd GetControllingDHD()
		{
			return Scene.Components.GetAll<Dhd>().FirstOrDefault(dhd => dhd.Gate == this);
		}

		/*
		[ConCmd.Server]
		public static void RequestDial( DialType type, string address, int gate, float initialDelay = 0 )
		{
		    if ( FindByIndex( gate ) is Stargate g && g.IsValid() )
		    {
		        switch ( type )
		        {
		            case DialType.FAST:
		                g.BeginDialFast( address );
		                break;

		            case DialType.SLOW:
		                g.BeginDialSlow( address, initialDelay );
		                break;

		            case DialType.INSTANT:
		                g.BeginDialInstant( address );
		                break;
		        }
		    }
		}

		[ConCmd.Server]
		public static void RequestClose( int gateID )
		{
		    if ( FindByIndex( gateID ) is Stargate g && g.IsValid() )
		    {
		        if ( g.Busy || ((g.Open || g.IsActive || g.Dialing) && g.Inbound) )
		        {
		            return;
		        }

		        if ( g.Open )
		        {
		            g.DoStargateClose( true );
		        }
		        else if ( g.Dialing )
		        {
		            g.StopDialing();
		        }
		    }
		}

		[ConCmd.Server]
		public static void ToggleIris( int gateID, int state )
		{
		    if ( FindByIndex( gateID ) is Stargate g && g.IsValid() )
		    {
		        if ( g.Iris.IsValid() )
		        {
		            if ( state == -1 )
		                g.Iris.Toggle();

		            if ( state == 0 )
		                g.Iris.Close();

		            if ( state == 1 )
		                g.Iris.Open();
		        }
		    }
		}

		[ConCmd.Server]
		public static void RequestAddressChange( int gateID, string address )
		{
		    if ( FindByIndex( gateID ) is Stargate g && g.IsValid() )
		    {
		        if ( g.GateAddress == address || !IsValidAddressOnly( address ) )
		            return;

		        g.GateAddress = address;

		        g.RefreshGateInformation();
		    }
		}

		[ConCmd.Server]
		public static void RequestGroupChange( int gateID, string group )
		{
		    if ( FindByIndex( gateID ) is Stargate g && g.IsValid() )
		    {
		        if ( g.GateGroup == group || !IsValidGroup( group ) || group.Length != g.GateGroupLength )
		            return;

		        g.GateGroup = group;

		        g.RefreshGateInformation();
		    }
		}

		[ConCmd.Server]
		public static void RequestNameChange( int gateID, string name )
		{
		    if ( FindByIndex( gateID ) is Stargate g && g.IsValid() )
		    {
		        if ( g.GateName == name )
		            return;

		        g.GateName = name;

		        g.RefreshGateInformation();
		    }
		}

		[ConCmd.Server]
		public static void SetAutoClose( int gateID, bool state )
		{
		    if ( FindByIndex( gateID ) is Stargate g && g.IsValid() )
		    {
		        if ( g.AutoClose == state )
		            return;

		        g.AutoClose = state;

		        g.RefreshGateInformation();
		    }
		}

		[ConCmd.Server]
		public static void SetGatePrivate( int gateID, bool state )
		{
		    if ( FindByIndex( gateID ) is Stargate g && g.IsValid() )
		    {
		        if ( g.GatePrivate == state )
		            return;

		        g.GatePrivate = state;

		        g.RefreshGateInformation();
		    }
		}

		[ConCmd.Server]
		public static void SetGateLocal( int gateID, bool state )
		{
		    if ( FindByIndex( gateID ) is Stargate g && g.IsValid() )
		    {
		        if ( g.GateLocal == state )
		            return;

		        g.GateLocal = state;

		        g.RefreshGateInformation();
		    }
		}

		[ConCmd.Server]
		public static void ToggleWormhole( int gateID, bool state )
		{
		    if ( FindByIndex( gateID ) is Stargate g && g.IsValid() )
		    {
		        if ( g.ShowWormholeCinematic == state )
		            return;

		        g.ShowWormholeCinematic = state;

		        g.RefreshGateInformation();
		    }
		}
		*/

		public static Stargate FindClosestGate(
			Vector3 postition,
			float max_distance = 0,
			Stargate[] exclude = null
		)
		{
			Stargate current = null;
			float distance = float.PositiveInfinity;

			foreach (Stargate gate in Game.ActiveScene.GetAllComponents<Stargate>())
			{
				if (exclude != null && exclude.Contains(gate))
					continue;

				float currDist = gate.WorldPosition.Distance(postition);
				if (distance > currDist)
				{
					if (max_distance > 0 && currDist > max_distance)
						continue;

					distance = currDist;
					current = gate;
				}
			}

			return current;
		}

		// SOUNDS
		public virtual string GetSound(string key)
		{
			return SoundDict.GetValueOrDefault(key, "");
		}

		// VARIABLE RESET
		public virtual void ResetGateVariablesToIdle()
		{
			ShouldStopDialing = false;
			OtherGate = null;
			Inbound = false;
			Busy = false;
			CurGateState = GateState.IDLE;
			CurDialType = DialType.FAST;
			DialingAddress = "";
			ActiveChevrons = 0;
			IsLocked = false;
			IsLockedInvalid = false;
			AutoCloseTime = -1;
			CurDialingSymbol = ' ';
			IsManualDialInProgress = false;
		}

		// RING ANGLE
		public virtual float GetRingAngle()
		{
			return 0;
		}

		/*
		// USABILITY
		public bool IsUsable( Entity user )
		{
		    return true; // we should be always usable
		}

		public bool OnUse( Entity user )
		{
		    if ( CanOpenMenu )
		    {
		        OpenStargateMenu( To.Single( user ) );
		    }

		    return false; // aka SIMPLE_USE, not continuously
		}
		*/

		// EVENT HORIZON

		public void CreateEventHorizon()
		{
			var eh = new GameObject();
			eh.Name = "Event Horizon";
			eh.WorldPosition = WorldPosition;
			eh.WorldRotation = WorldRotation;
			eh.WorldScale = WorldScale;
			eh.SetParent(GameObject);
			eh.Tags.Add(StargateTags.EventHorizon, "trigger");

			EventHorizon = eh.Components.Create<EventHorizon>();
			EventHorizon.EventHorizonMaterialGroup = EventHorizonMaterialGroup;

			EventHorizon.EventHorizonModel = eh.Components.Create<ModelRenderer>(false);
			EventHorizon.EventHorizonModel.Model = Model.Load(
				"models/sbox_stargate/event_horizon/event_horizon.vmdl"
			);

			EventHorizon.EventHorizonTrigger = eh.Components.Create<EventHorizonTrigger>();
			EventHorizon.EventHorizonTrigger.Model = EventHorizon.EventHorizonModel.Model;
			EventHorizon.EventHorizonTrigger.EventHorizon = EventHorizon;
			EventHorizon.EventHorizonTrigger.IsTrigger = true;
			EventHorizon.EventHorizonTrigger.IsMainTrigger = true;

			EventHorizon.CreateTriggers();

			eh.SetupNetworking();
		}

		public void DeleteEventHorizon()
		{
			EventHorizon?.GameObject?.Destroy();
		}

		public async Task EstablishEventHorizon(float delay = 0)
		{
			await GameTask.DelaySeconds(delay);
			if (!this.IsValid())
				return;

			CreateEventHorizon();
			EventHorizon.Establish();

			await GameTask.DelaySeconds(3f);
			if (!this.IsValid() || !EventHorizon.IsValid())
				return;

			EventHorizon.IsFullyFormed = true;
		}

		public async Task CollapseEventHorizon(float sec = 0)
		{
			await GameTask.DelaySeconds(sec);
			if (!this.IsValid() || !EventHorizon.IsValid())
				return;

			EventHorizon.IsFullyFormed = false;
			EventHorizon.Collapse();

			await GameTask.DelaySeconds(sec + 2f);
			if (!this.IsValid() || !EventHorizon.IsValid())
				return;

			DeleteEventHorizon();
		}

		// IRIS
		public bool HasIris => Iris.IsValid();

		public bool IsIrisClosed => HasIris && Iris.Closed;

		public void ToggleIris()
		{
			Iris?.Toggle();
		}

		// BEARING
		public bool HasBearing()
		{
			return Bearing.IsValid();
		}

		// DIALING -- please don't touch any of these, dialing is heavy WIP

		public void MakeBusy(float duration)
		{
			Busy = true;
			AddTask(Time.Now + duration, () => Busy = false, TimedTaskCategory.SET_BUSY);
		}

		public bool CanStargateOpen()
		{
			return (!Busy && !Opening && !Open && !Closing);
		}

		public bool CanStargateClose()
		{
			return (!Busy && Open);
		}

		public bool CanStargateStartDial()
		{
			return (
				Idle
				&& !Busy
				&& !Dialing
				&& !Inbound
				&& !Open
				&& !Opening
				&& !Closing
				&& (IsManualDialInProgress ? !IsLocked : true)
			);
		}

		public bool CanStargateStopDial()
		{
			if (!Inbound)
				return (!Busy && Dialing);

			return (!Busy && IsActive);
		}

		public bool CanStargateStartManualDial()
		{
			if (!Dialing)
				return CanStargateStartDial();

			return (!IsManualDialInProgress && !IsLocked);
		}

		public bool ShouldGateStopDialing()
		{
			return ShouldStopDialing;
		}

		public async void DoStargateOpen()
		{
			if (!CanStargateOpen())
				return;

			OnStargateBeginOpen();

			await EstablishEventHorizon(0.5f);
			if (!this.IsValid())
				return;

			OnStargateOpened();
		}

		public async void DoStargateClose(bool alsoCloseOther = false)
		{
			if (!CanStargateClose())
				return;

			if (alsoCloseOther && OtherGate.IsValid() && OtherGate.Open)
				OtherGate.DoStargateClose();

			OnStargateBeginClose();

			await CollapseEventHorizon(0.25f);
			if (!this.IsValid())
				return;

			OnStargateClosed();
		}

		public bool IsStargateReadyForInboundFast() // checks if the gate is ready to do a inbound anim for fast dial
		{
			if (!Dialing)
			{
				return (!Busy && !Open && !Inbound);
			}
			else
			{
				return (
					!Busy
					&& !Open
					&& !Inbound
					&& (
						CurDialType is DialType.SLOW
						|| CurDialType is DialType.DHD
						|| CurDialType is DialType.MANUAL
					)
				);
			}
		}

		public bool IsStargateReadyForInboundFastEnd() // checks if the gate is ready to open when finishing fast dial?
		{
			return (!Busy && !Open && !Dialing && Inbound);
		}

		public bool IsStargateReadyForInboundInstantSlow() // checks if the gate is ready to do inbound for instant or slow dial
		{
			return (!Busy && !Open && !Inbound);
		}

		public bool IsStargateReadyForInboundDHD() // checks if the gate is ready to be locked onto by dhd dial
		{
			if (!Dialing)
			{
				return (!Busy && !Open && !Inbound);
			}
			else
			{
				return (
					!Busy
					&& !Open
					&& !Inbound
					&& (CurDialType == DialType.SLOW || CurDialType is DialType.MANUAL)
				);
			}
		}

		public bool IsStargateReadyForInboundDHDEnd() // checks if the gate is ready to be opened while locked onto by a gate using dhd dial
		{
			if (!Dialing)
			{
				return (!Busy && !Open && Inbound);
			}
			else
			{
				return (
					!Busy
					&& !Open
					&& Inbound
					&& (CurDialType == DialType.SLOW || CurDialType is DialType.MANUAL)
				);
			}
		}

		// begin dial
		public virtual void BeginDialFast(string address) { }

		public virtual void BeginDialSlow(string address, float initialDelay = 0) { }

		public virtual void BeginDialInstant(string address) { } // instant gate open, with kawoosh

		public virtual void BeginDialNox(string address) { } // instant gate open without kawoosh - asgard/ancient/nox style

		// begin inbound
		public virtual void BeginInboundFast(int numChevs)
		{
			if (Inbound && !Dialing)
				StopDialing(true);
		}

		public virtual void BeginInboundSlow(int numChevs) // this can be used with Instant dial, too
		{
			if (Inbound && !Dialing)
				StopDialing(true);
		}

		// DHD DIAL
		public virtual void BeginOpenByDHD(string address) { } // when dhd dial button is pressed

		public virtual void BeginInboundDHD(int numChevs) { } // when a dhd dialing gate locks onto another gate

		// stop dial
		public async void StopDialing(bool immediate = false)
		{
			if (!CanStargateStopDial())
				return;

			OnStopDialingBegin();

			if (!immediate)
			{
				await GameTask.DelaySeconds(1.25f);
				if (!this.IsValid())
					return;
			}

			OnStopDialingFinish();
		}

		public virtual void OnStopDialingBegin()
		{
			Busy = true;
			ShouldStopDialing = true; // can be used in ring/gate logic to to stop ring/gate rotation

			if (Inbound)
			{
				// Event.Run( StargateEvent.InboundAbort, this );
			}
			else
			{
				// Event.Run( StargateEvent.DialAbort, this );
			}

			ClearTasksByCategory(TimedTaskCategory.DIALING);

			if (OtherGate.IsValid())
			{
				OtherGate.ClearTasksByCategory(TimedTaskCategory.DIALING);

				if (OtherGate.Inbound && !OtherGate.ShouldStopDialing)
					OtherGate.StopDialing();
			}
		}

		public virtual void OnStopDialingFinish()
		{
			ResetGateVariablesToIdle();
			// Event.Run( StargateEvent.DialAbortFinished, this );
		}

		// opening
		public virtual void OnStargateBeginOpen()
		{
			CurGateState = GateState.OPENING;
			Busy = true;
			// Event.Run( StargateEvent.GateOpening, this );
		}

		public virtual void OnStargateOpened()
		{
			CurGateState = GateState.OPEN;
			Busy = false;
			// Event.Run( StargateEvent.GateOpen, this );
		}

		// closing
		public virtual void OnStargateBeginClose()
		{
			CurGateState = GateState.CLOSING;
			Busy = true;

			KillAllPlayersInTransit();
			// Event.Run( StargateEvent.GateClosing, this );
		}

		public virtual void OnStargateClosed()
		{
			ResetGateVariablesToIdle();

			if (GetControllingDHD() is Dhd DHD)
			{
				DHD.SetDialGuideState(false);
			}
			// Event.Run( StargateEvent.GateClosed, this );
		}

		// reset
		public virtual void DoStargateReset()
		{
			ResetGateVariablesToIdle();
			ClearTasks();

			// Event.Run( StargateEvent.Reset, this );
		}

		public virtual void EstablishWormholeTo(Stargate target)
		{
			target.OtherGate = this;
			OtherGate = target;

			target.Inbound = true;

			target.DoStargateOpen();
			DoStargateOpen();
		}

		// CHEVRON

		public virtual Chevron GetChevron(int num)
		{
			return Chevrons.Where(c => c.Number == num).FirstOrDefault();
		}

		public virtual Chevron GetTopChevron()
		{
			return GetChevron(7);
		}

		public bool IsChevronActive(int num)
		{
			var chev = GetChevron(num);

			if (!chev.IsValid())
				return false;

			return chev.On;
		}

		public virtual void SetChevronsGlowState(bool state, float delay = 0)
		{
			foreach (Chevron chev in Chevrons)
			{
				if (state)
					chev.TurnOn(delay);
				else
					chev.TurnOff(delay);
			}
		}

		public Chevron GetChevronBasedOnAddressLength(int num, int len = 7)
		{
			if (len == 8)
			{
				if (num == 7)
					return GetChevron(8);
				else if (num == 8)
					return GetChevron(7);
			}
			else if (len == 9)
			{
				if (num == 7)
					return GetChevron(8);
				else if (num == 8)
					return GetChevron(9);
				else if (num == 9)
					return GetChevron(7);
			}
			return GetChevron(num);
		}

		public int GetChevronOrderOnGateFromChevronIndex(int index)
		{
			if (index <= 3)
				return index;
			if (index >= 4 && index <= 7)
				return index + 2;
			return index - 4;
		}

		// DHD/Fast Chevron Encode/Lock
		public virtual void DoDHDChevronEncode(char sym)
		{
			if (DialingAddress.Contains(sym))
				return;

			// if we were already dialing but not via DHD, dont do anything
			if (Dialing && CurDialType != DialType.DHD)
				return;

			TimeSinceDialAction = 0;

			if (!Dialing) // if gate wasnt dialing, begin dialing
			{
				CurGateState = GateState.DIALING;
				CurDialType = DialType.DHD;
			}

			DialingAddress += sym;
			// Event.Run( StargateEvent.DHDChevronEncoded, this, sym );
		}

		public virtual void DoDHDChevronLock(char sym)
		{
			if (DialingAddress.Contains(sym))
				return;

			// if we were already dialing but not via DHD, dont do anything
			if (CurDialType != DialType.DHD)
				return;

			TimeSinceDialAction = 0;

			DialingAddress += sym;

			var gate = FindDestinationGateByDialingAddress(this, DialingAddress);
			var valid = (gate != this && gate.IsValid() && gate.IsStargateReadyForInboundDHD());

			IsLocked = true;
			IsLockedInvalid = !valid;

			// Event.Run( StargateEvent.DHDChevronLocked, this, sym, valid );
		}

		// Manual/Slow Chevron Encode/Lock
#pragma warning disable CS1998
		public virtual async Task<bool> DoManualChevronEncode(char sym)
#pragma warning restore CS1998
		{
			if (!Symbols.Contains(sym))
				return false;

			// if we try to encode 9th symbol, do a lock instead
			if (DialingAddress.Length == 8)
			{
				_ = DoManualChevronLock(sym);
				return false;
			}

			if (!CanStargateStartManualDial())
				return false;

			// if we were already dialing but not MANUAL, dont do anything
			if (Dialing && CurDialType != DialType.MANUAL)
				return false;

			if (DialingAddress.Contains(sym))
				return false;

			if (DialingAddress.Length > 8)
				return false;

			if (!Dialing) // if gate wasnt dialing, begin dialing
			{
				CurGateState = GateState.DIALING;
				CurDialType = DialType.MANUAL;

				// Event.Run( StargateEvent.DialBegin, this, "" );
			}

			TimeSinceDialAction = 0;

			return true;
		}

#pragma warning disable CS1998
		public virtual async Task<bool> DoManualChevronLock(char sym)
#pragma warning restore CS1998
		{
			if (!Symbols.Contains(sym))
				return false;

			// if we try to lock sooner than 7th symbol, do nothing
			if (DialingAddress.Length < 6)
			{
				return false;
			}

			if (!CanStargateStartManualDial())
				return false;

			if (!Dialing)
				return false;

			// if we were already dialing but not MANUAL, dont do anything
			if (Dialing && CurDialType != DialType.MANUAL)
				return false;

			if (DialingAddress.Contains(sym))
				return false;

			if (DialingAddress.Length < 6)
				return false;

			TimeSinceDialAction = 0;

			return true;
		}

		public virtual void BeginManualOpen(string address) { } // when dialing manually, open the gate do the target address

		// THINK
		public void AutoCloseThink()
		{
			if (AutoClose && AutoCloseTime != -1 && AutoCloseTime <= Time.Now && CanStargateClose())
			{
				AutoCloseTime = -1;
				DoStargateClose(true);
			}
		}

		public void CloseIfNoOtherGate()
		{
			if (Open && !OtherGate.IsValid())
			{
				DoStargateClose();
			}
		}

		public void DhdDialTimerThink()
		{
			if (
				Dialing
				&& CurDialType is DialType.DHD
				&& TimeSinceDialAction > InactiveDialShutdownTime
			)
			{
				StopDialing();
			}
		}

		public void ManualDialTimerThink()
		{
			if (
				Dialing
				&& CurDialType is DialType.MANUAL
				&& TimeSinceDialAction > InactiveDialShutdownTime
			)
			{
				StopDialing();
			}
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			StargateTick();
			TaskThink();
		}

		public void StargateTick()
		{
			AutoCloseThink();
			CloseIfNoOtherGate();
			DhdDialTimerThink();
			ManualDialTimerThink();
		}

		/*
		[GameEvent.Client.Frame]
		private void WorldPanelThink()
		{
		    var isNearGate = Position.DistanceSquared( Camera.Position ) < (512 * 512);
		    if ( isNearGate && !WorldPanel.IsValid() )
		        WorldPanel = new StargateWorldPanel( this );
		    else if ( !isNearGate && WorldPanel.IsValid() )
		        WorldPanel.Delete();
		}
		*/

		// UI Related stuff

		/*
		[ClientRpc]
		public void OpenStargateMenu( Dhd dhd = null )
		{
		    var hud = Game.RootPanel;
		    var count = 0;
		    foreach ( StargateMenuV2 menu in hud.ChildrenOfType<StargateMenuV2>() ) count++;

		    // this makes sure if we already have the menu open, we cant open it again
		    if ( count == 0 ) hud.AddChild( new StargateMenuV2( this, dhd ) );
		}

		[ClientRpc]
		public void RefreshGateInformation()
		{
		    Event.Run( "stargate.refreshgateinformation" );
		}
		*/

		public Stargate FindClosestGate()
		{
			return FindClosestGate(this.WorldPosition, 0, new Stargate[] { this });
		}

		protected override void OnDestroy()
		{
			// if ( Ramp != null ) Ramp.Gate.Remove( this );

			if (OtherGate.IsValid())
			{
				if (OtherGate.Inbound && !OtherGate.Dialing)
					OtherGate.StopDialing();
				if (OtherGate.Open)
					OtherGate.DoStargateClose();
			}

			KillAllPlayersInTransit();

			base.OnDestroy();
		}

		private void KillAllPlayersInTransit()
		{
			if (!EventHorizon.IsValid())
				return;

			foreach (var ply in EventHorizon.InTransitPlayers)
			{
				EventHorizon.DissolveEntity(ply);
			}
		}
	}
}

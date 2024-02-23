using System.Net.Sockets;

namespace Sandbox.Components.Stargate
{
    public class Dhd : Component, Component.ExecuteInEditor
    {
        public struct DhdData
        {
            public DhdData( string buttonMaterialGroup, string pressSnd, string dialSnd )
            {
                ButtonMaterialGroup = buttonMaterialGroup;
                ButtonPressSound = pressSnd;
                DialPressSound = dialSnd;
            }

            public string ButtonMaterialGroup { get; }
            public string ButtonPressSound { get; }
            public string DialPressSound { get; }
        }

        public SkinnedModelRenderer DhdModel => Components.Get<SkinnedModelRenderer>();

        public List<string> PressedActions = new();

        protected bool DialIsLock = false;
        protected bool IsDialLocking = false;

        public DhdData Data { get; set; } = new( "default", "dhd.milkyway.press", "dhd.press_dial" );

        [Property]
        public Stargate Gate { get; set; }

        public IEnumerable<DhdButton> Buttons => Components.GetAll<DhdButton>( FindMode.InChildren );

        protected virtual string ButtonSymbols => "ABCDEFGHI0123456789STUVWXYZ@JKLMNO#PQR";

        // Button positions for DhdWorldPanel
        protected virtual Dictionary<string, Vector3> ButtonPositions => new()
        {
            // Inner Ring
            ["0"] = new Vector3( -5.9916f, -1.4400f, 52.5765f ),
            ["1"] = new Vector3( -6.4918f, -4.1860f, 52.6422f ),
            ["2"] = new Vector3( -7.6213f, -6.9628f, 53.0274f ),
            ["3"] = new Vector3( -9.6784f, -8.9852f, 53.4759f ),
            ["4"] = new Vector3( -12.1340f, -10.2172f, 53.9685f ),
            ["5"] = new Vector3( -15.1579f, -10.5651f, 54.4846f ),
            ["6"] = new Vector3( -17.9682f, -9.5000f, 55.0545f ),
            ["7"] = new Vector3( -19.8674f, -7.9478f, 55.6510f ),
            ["8"] = new Vector3( -21.7267f, -5.5587f, 55.9205f ),
            ["9"] = new Vector3( -22.7824f, -2.8374f, 55.7549f ),
            ["A"] = new Vector3( -23.0255f, -0.2087f, 55.6288f ),
            ["B"] = new Vector3( -22.2049f, 2.4517f, 55.3056f ),
            ["C"] = new Vector3( -20.6056f, 4.6577f, 54.9203f ),
            ["D"] = new Vector3( -18.2240f, 6.5121f, 54.5925f ),
            ["E"] = new Vector3( -15.2521f, 7.2384f, 54.4391f ),
            ["F"] = new Vector3( -12.3034f, 6.8182f, 54.0971f ),
            ["G"] = new Vector3( -9.6883f, 5.9373f, 53.4027f ),
            ["H"] = new Vector3( -7.4578f, 3.9120f, 52.9060f ),
            ["I"] = new Vector3( -6.1105f, 1.3894f, 52.6246f ),
            // Outer Ring
            ["J"] = new Vector3( -0.3310f, -1.5342f, 49.6508f ),
            ["K"] = new Vector3( -0.9333f, -6.2703f, 49.6297f ),
            ["L"] = new Vector3( -3.1289f, -10.9383f, 50.3840f ),
            ["M"] = new Vector3( -6.8106f, -13.9513f, 51.2794f ),
            ["N"] = new Vector3( -10.9387f, -15.9653f, 52.1221f ),
            ["O"] = new Vector3( -15.4151f, -15.9868f, 53.1347f ),
            ["#"] = new Vector3( -20.3015f, -15.0339f, 53.8717f ),
            ["P"] = new Vector3( -24.1817f, -11.9662f, 54.8636f ),
            ["Q"] = new Vector3( -26.1737f, -7.9137f, 55.6503f ),
            ["R"] = new Vector3( -28.2183f, -3.7876f, 55.5180f ),
            ["S"] = new Vector3( -28.2080f, 0.8877f, 55.3551f ),
            ["T"] = new Vector3( -27.1768f, 5.2839f, 54.8367f ),
            ["U"] = new Vector3( -24.7437f, 9.5472f, 54.0941f ),
            ["V"] = new Vector3( -20.3043f, 11.6017f, 53.7054f ),
            ["W"] = new Vector3( -15.2821f, 12.6006f, 53.2243f ),
            ["X"] = new Vector3( -10.7024f, 12.3530f, 52.2391f ),
            ["Y"] = new Vector3( -6.5012f, 10.6867f, 51.2069f ),
            ["Z"] = new Vector3( -3.0627f, 7.6676f, 50.3809f ),
            ["@"] = new Vector3( -0.8726f, 3.1943f, 49.9209f ),
            // Engage
            // ["DIAL"] = new Vector3( -15.0280f, -1.5217f, 55.1249f ),
        };

        protected virtual Vector3 ButtonPositionsOffset => new( -14.8088f, -1.75652f, 8f );
        internal float LastPressTime { get; set; } = 0;
        internal float PressDelay { get; set; } = 0.5f;

        protected override void OnUpdate()
        {
            base.OnUpdate();

            // DrawSymbols();
        }

        public void DrawSymbols()
        {
            using ( Gizmo.Scope( "DhdSymbols", global::Transform.Zero ) )
            {
                foreach ( var entry in ButtonPositions )
                {
                    var name = entry.Key;
                    var _symbolPosition = entry.Value - ButtonPositionsOffset;

                    var dhdPos = Transform.World.Position;
                    var dhdRot = Transform.World.Rotation;

                    var finalPos = dhdPos + dhdRot.Forward * _symbolPosition.x + dhdRot.Left * _symbolPosition.y + dhdRot.Up * _symbolPosition.z;
                    var finalRot = dhdRot.RotateAroundAxis( Vector3.Up, 90 );

                    var t1 = global::Transform.Zero.WithPosition( finalPos ).WithRotation( finalRot ).WithScale( 0.022f );
                    var t2 = global::Transform.Zero.WithPosition( finalPos + Vector3.Up * 0.01f ).WithRotation( finalRot ).WithScale( 0.02f );

                    Gizmo.Draw.Color = Color.Black;
                    Gizmo.Draw.WorldText( name, t1, size: 128 );

                    Gizmo.Draw.Color = Color.White;
                    Gizmo.Draw.WorldText( name, t2, size: 128 );
                }
            }
        }

        // private List<DhdWorldPanel> WorldPanels { get; } = new();

        /*
        public override void ClientSpawn()
        {
            base.ClientSpawn();

            CreateWorldPanels();
        }

        public virtual void CreateWorldPanels()
        {
            foreach ( var item in ButtonPositions )
            {
                var sym = item.Key;
                var pos = item.Value - ButtonPositionsOffset;

                var panel = new DhdWorldPanel( this, sym, pos );
                WorldPanels.Add( panel );
            }
        }

        public void DeleteWorldPanels()
        {
            foreach ( var panel in WorldPanels )
                panel.Delete();
            WorldPanels.Clear();
        }
        */

        protected override void OnStart()
        {
            base.OnStart();

            PostSpawn();
        }

        public virtual async void PostSpawn()
        {
            await Task.FixedUpdate();
            TryAssignGate( Stargate.FindNearestGate( GameObject, 1024 ) );
        }

        /// <summary>
        /// Assigns a Gate to this DHD, if no other DHD has this Gate assigned.
        /// </summary>
        /// <returns>Whether assignment was successful or not.</returns>
        public bool TryAssignGate( Stargate gate )
        {
            if ( !gate.IsValid() )
                return false;

            foreach ( var dhd in Scene.GetAllComponents<Dhd>().Where( x => x != this ) )
            {
                if ( dhd.Gate == gate )
                    return false;
            }

            Gate = gate;

            if ( !Gate.Idle )
            {
                PressedActions.Clear();
                foreach ( var sym in Gate.DialingAddress )
                {
                    PressedActions.Add( sym.ToString() );
                }
            }

            return true;
        }

        public virtual void CreateSingleButton( string model, string action, bool disabled = false ) // visible model of buttons that turn on/off and animate
        {
            var button_object = new GameObject();
            button_object.Name = $"Button ({action})";
            button_object.Transform.World = GameObject.Transform.World;
            button_object.SetParent( GameObject );

            var button_component = button_object.Components.Create<DhdButton>();
            var renderer = button_object.Components.Create<SkinnedModelRenderer>();
            renderer.Model = Model.Load( model );
            renderer.MaterialGroup = Data.ButtonMaterialGroup;
            var collider = button_object.Components.Create<ModelCollider>();
            collider.Model = renderer.Model;

            button_component.Action = action;
            button_component.Disabled = disabled;
        }

        public virtual void CreateButtons() // visible models of buttons that turn on/off and animate
        {
            // SYMBOL BUTTONS
            for ( var i = 0; i < ButtonSymbols.Length; i++ )
            {
                var modelName = $"models/sbox_stargate/dhd/buttons/dhd_button_{i + 1}.vmdl";
                var actionName = ButtonSymbols[i].ToString();
                CreateSingleButton( modelName, actionName );
            }

            // CENTER DIAL BUTTON
            CreateSingleButton( "models/sbox_stargate/dhd/buttons/dhd_button_39.vmdl", "DIAL" );
        }

        public DhdButton GetButtonByAction( string action )
        {
            return Buttons.FirstOrDefault( b => b.Action == action );
        }

        public void PlayButtonPressAnim( DhdButton button )
        {
            // if ( button.IsValid() ) button.CurrentSequence.Name = "button_press";
        }

        public void SetButtonState( string action, bool glowing )
        {
            var b = GetButtonByAction( action );
            if ( b.IsValid() ) b.On = glowing;
        }

        public void SetButtonState( DhdButton b, bool glowing )
        {
            if ( b.IsValid() ) b.On = glowing;
        }

        public void ToggleButton( string action )
        {
            var b = GetButtonByAction( action );
            if ( b.IsValid() ) SetButtonState( b, !b.On );
        }

        public void ToggleButton( DhdButton b )
        {
            if ( b.IsValid() ) SetButtonState( b, !b.On );
        }

        public void EnableAllButtons()
        {
            foreach ( DhdButton b in Buttons ) SetButtonState( b, true );
        }

        public void DisableAllButtons()
        {
            foreach ( DhdButton b in Buttons ) SetButtonState( b, false );
        }

        // BUTTON PRESS LOGIC
        public string GetPressedActions()
        {
            return string.Join( "", PressedActions );
        }

        public void EnableButtonsForDialingAddress()
        {
            if ( !Gate.IsValid() )
            {
                DisableAllButtons();
                return;
            }

            DisableAllButtons();
            foreach ( char sym in Gate.DialingAddress ) SetButtonState( sym.ToString(), true );

            if ( Gate.Open || Gate.Opening || Gate.Closing )
            {
                var dial = GetButtonByAction( "DIAL" );
                if ( dial.IsValid() ) SetButtonState( dial, true );
            }
        }

        public async void TriggerAction( string action, GameObject user, float delay = 0 ) // this gets called from the Button Trigger after pressing it
        {
            if ( delay > 0 ) await GameTask.DelaySeconds( delay );

            if ( !Gate.IsValid() ) return; // if we have no gate to control, cant do much

            if ( action == "IRIS" ) // button for toggling the iris
            {
                if ( Gate.HasIris() )
                    Gate.Iris.Toggle();
                return;
            }

            if ( action == "FAST" || action == "SLOW" || action == "INSTANT" ) // button for toggling the iris
            {
                if ( Gate.Dialing )
                {
                    Gate.StopDialing();
                    return;
                }

                if ( Gate.CanStargateStartDial() )
                {
                    var closestGate = Gate.FindClosestGate();
                    if ( closestGate.IsValid() )
                    {
                        var address = Stargate.GetOtherGateAddressForMenu( Gate, closestGate );

                        if ( action == "FAST" )
                            Gate.BeginDialFast( address );
                        else if ( action == "INSTANT" )
                            Gate.BeginDialInstant( address );
                        else
                            Gate.BeginDialSlow( address );
                    }
                }
                return;
            }

            if ( Gate.Busy || Gate.Inbound ) return; // if gate is busy, we cant do anything

            if ( Gate.Dialing && Gate.CurDialType is not Stargate.DialType.DHD ) return; // if we are dialing, but not by DHD, cant do anything

            if ( action is not "DIAL" ) // if we pressed a regular symbol
            {
                if ( PressedActions.Contains( "DIAL" ) ) return; // do nothing if we already have dial pressed
                if ( !PressedActions.Contains( action ) && PressedActions.Count is 9 ) return; // do nothing if we already have max symbols pressed
                if ( !PressedActions.Contains( action ) && action is "#" )
                {
                    if ( PressedActions.Count < 6 ) return;
                }
                if ( Gate.Opening || Gate.Open || Gate.Closing ) return;
            }

            var button = GetButtonByAction( action );

            if ( action is "DIAL" ) // we presed dial button
            {
                if ( Gate.Idle ) // if gate is idle, open dial menu
                {
                    // Gate.OpenStargateMenu( To.Single( user ), this );
                    return;
                }

                if ( Gate.Open ) // if gate is open, close the gate
                {
                    if ( Gate.CanStargateClose() )
                    {
                        Gate.DoStargateClose( true );
                        PressedActions.Clear();
                        IsDialLocking = false;
                    }
                    return;
                }

                if ( DialIsLock && PressedActions.Count >= 6 && !IsDialLocking ) // if the DIAL button should also lock the last symbol, do that (Atlantis City DHD)
                {
                    IsDialLocking = true;
                    TriggerAction( "#", user );
                    TriggerAction( "DIAL", user, 1 );
                    return;
                }

                if ( PressedActions.Count < 7 ) // if we pressed less than 7 symbols, we should cancel dial
                {
                    if ( Gate.Dialing && Gate.CurDialType is Stargate.DialType.DHD )
                    {
                        PlayButtonPressAnim( button );

                        Gate.StopDialing();
                        PressedActions.Clear();
                        IsDialLocking = false;
                    }

                    return;
                }
                else // try dial
                {
                    var sequence = GetPressedActions();
                    PlayButtonPressAnim( button );

                    var target = Stargate.FindDestinationGateByDialingAddress( Gate, sequence );
                    if ( target.IsValid() && target != Gate && target.IsStargateReadyForInboundDHD() && Gate.CanStargateOpen() && !Gate.IsLockedInvalid )
                    {
                        Stargate.PlaySound( Transform.Position + Transform.Rotation.Up * 16, Data.DialPressSound );

                        Gate.CurGateState = Stargate.GateState.IDLE; // temporarily make it idle so it can 'begin' dialing
                        Gate.BeginOpenByDHD( sequence );
                        IsDialLocking = false;
                    }
                    else
                    {
                        Gate.StopDialing();
                        PressedActions.Clear();
                        IsDialLocking = false;
                        return;
                    }
                }
            }
            else // we pressed a symbol
            {
                var symbol = action[0];

                if ( symbol != '#' && PressedActions.Contains( "#" ) ) return; // if # is pressed, and we try to depress other symbol, do nothing

                // if symbol was already pressed, do nothing, can't deactivate, can only abort the whole sequence with center button
                if ( !PressedActions.Contains( action ) ) // symbol wasnt pressed, go press it
                {
                    //if ( !Gate.Dialing ) // if gate wasnt dialing, begin dialing
                    //{
                    //	Gate.CurGateState = Stargate.GateState.DIALING;
                    //	Gate.CurDialType = Stargate.DialType.DHD;
                    //}

                    if ( PressedActions.Count == 8 || symbol is '#' ) // lock if we are putting Point of Origin or 9th symbol, otherwise encode
                    {
                        if ( DialIsLock && !IsDialLocking ) // if the DIAL button should also lock the last symbol, do that (Atlantis City DHD)
                        {
                            IsDialLocking = true;
                            TriggerAction( action, user );
                            TriggerAction( "DIAL", user, 1 );
                            return;
                        }

                        Gate.DoDHDChevronLock( symbol );
                    }
                    else
                    {
                        Gate.DoDHDChevronEncode( symbol );
                    }

                    PressedActions.Add( action );
                    PlayButtonPressAnim( button );

                    Stargate.PlaySoundInWorld( MultiWorldSystem.GetWorldIndexOfObject( GameObject ), Transform.Position + Transform.Rotation.Up * 16, Data.ButtonPressSound );
                }
            }
        }

        protected override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            ButtonThink();
        }

        public void ButtonThink()
        {
            EnableButtonsForDialingAddress();

            if ( ((!Gate.IsValid()) || (Gate.IsValid() && Gate.Idle)) && PressedActions.Count != 0 )
            {
                PressedActions.Clear();
            }

            if ( !Gate.IsValid() )
            {
                TryAssignGate( Stargate.FindNearestGate( GameObject, 1024 ) );
            }
        }

        /*
        [GameEvent.Client.Frame]
        private void WorldPanelThink()
        {
            var isNearDhd = Position.DistanceSquared( Camera.Position ) < (128 * 128);
            if ( isNearDhd && WorldPanels.Count == 0 )
                CreateWorldPanels();
            else if ( !isNearDhd && WorldPanels.Count != 0 )
                DeleteWorldPanels();
        }
        */
    }
}

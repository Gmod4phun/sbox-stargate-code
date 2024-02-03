namespace Sandbox.Components.Stargate.Rings
{
    using System.Collections.Generic;
    using Sandbox;

    [Category( "Transportation Rings" )]
    public class RingPanel : Component, Component.ExecuteInEditor
    {
        public IEnumerable<RingPanelButton> Buttons => Components.GetAll<RingPanelButton>( FindMode.InChildren );
        public SkinnedModelRenderer Renderer => Components.Get<SkinnedModelRenderer>( FindMode.InSelf );
        public ModelCollider Collider => Components.Get<ModelCollider>( FindMode.InSelf );

        [Property]
        public Ringtransporter Rings { get; set; } = null;

        protected TimeSince TimeSinceButtonPressed { get; set; } = 0;
        protected float ButtonPressDelay { get; set; } = 0.35f;
        protected float ButtonGlowDelay { get; set; } = 0.2f;

        [Property]
        protected string ComposedAddress { get; private set; } = "";
        protected virtual string[] ButtonsSounds { get; } = { "goauld_button1", "goauld_button2" };
        protected virtual string ValidButtonActions => "12345678ABCDEFGHIJKL";

        public RingPanelButton GetButtonByAction( string action )
        {
            return Buttons.FirstOrDefault( b => b.Action == action );
        }

        public void SetButtonState( RingPanelButton b, bool glowing )
        {
            if ( b.IsValid() ) b.On = glowing;
        }

        public void ResetAddress()
        {
            ComposedAddress = "";
        }

        public Ringtransporter FindClosestRingTransporter()
        {
            return Scene.GetAllComponents<Ringtransporter>().OrderBy( x => x.Transform.Position.DistanceSquared( Transform.Position ) ).FirstOrDefault();
        }

        public Ringtransporter FindRingtransporterByAddress( string address )
        {
            return Scene.GetAllComponents<Ringtransporter>().FirstOrDefault( x => x.Address == address );
        }

        public void TriggerAction( string action ) // this gets called from the Panel Button after pressing it
        {
            if ( TimeSinceButtonPressed < ButtonPressDelay ) return;

            if ( (ValidButtonActions.Contains( action ) && action.Length == 1) || action is "DIAL" )
            {
                if ( action is "DIAL" ) // we pressed dial button
                {
                    if ( Rings.IsValid() )
                    {
                        Rings.DialRings( ComposedAddress.Length == 0 ? Rings.FindClosest() : FindRingtransporterByAddress( ComposedAddress ), 0.25f );
                    }

                    ResetAddress();
                }
                else // we pressed number action button
                {
                    ComposedAddress += action;
                }

                ToggleButton( action );
                TimeSinceButtonPressed = 0;
            }
            else
            {
                return;
            }
        }

        private void ButtonResetThink()
        {
            if ( TimeSinceButtonPressed > 5 && ComposedAddress != "" ) ResetAddress();
        }

        private void ClosestRingsThink()
        {
            if ( !Rings.IsValid() )
            {
                Rings = FindClosestRingTransporter();
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            ButtonResetThink();
            ClosestRingsThink();
        }

        protected async void ToggleButton( string action )
        {
            var btn = GetButtonByAction( action );
            var snd = btn?.PressSound;
            if ( snd != null )
            {
                Sound.Play( snd, Transform.Position );
            }

            SetButtonState( btn, true );

            await GameTask.DelaySeconds( ButtonGlowDelay );

            SetButtonState( btn, false );
        }
    }
}

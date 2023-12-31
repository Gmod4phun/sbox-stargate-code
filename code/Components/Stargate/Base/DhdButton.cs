namespace Sandbox.Components.Stargate
{
    public class DhdButton : Component, IUse
    {
        public ModelRenderer ButtonModel;
        public ModelCollider ButtonCollider;

        private float _glowScale = 0;

        [Net]
        public Dhd DHD { get; set; } = null;

        [Net]
        public string Action { get; set; } = "";

        [Net]
        public bool On { get; set; } = false;

        [Net]
        public bool Disabled { get; set; } = false;

        public virtual bool OnUse( GameObject user )
        {
            if ( Time.Now < DHD.LastPressTime + DHD.PressDelay )
                return false;

            DHD.LastPressTime = Time.Now;
            DHD.TriggerAction( Action, user );

            return false;
        }

        public bool IsUsable( GameObject user )
        {
            return !Disabled;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if ( ButtonModel.IsValid() && ButtonModel.SceneObject is SceneObject so )
            {
                _glowScale = _glowScale.LerpTo( On ? 1 : 0, Time.Delta * (On ? 2f : 20f) );
                so.Batchable = false;
                so.Attributes.Set( "selfillumscale", _glowScale );

                if ( ButtonModel is Superglyph glyph )
                {
                    glyph.GlyphEnabled = On;
                }

                // DrawSymbol();
            }
        }

        public void DrawSymbol()
        {
            if ( ButtonCollider.IsValid() )
            {
                var pos = ButtonCollider.KeyframeBody.MassCenter;
                if ( pos.DistanceSquared( Scene.Camera.Transform.Position ) < 4096 )
                {
                    using ( Gizmo.Scope( "DhdSymbol", global::Transform.Zero ) )
                    {
                        if ( Action != "DIAL" && !Disabled )
                        {
                            Gizmo.Draw.Color = Color.White;
                            Gizmo.Draw.Text( Action, global::Transform.Zero.WithPosition( pos ), size: 32 );
                        }
                    }
                }
            }
        }
    }
}

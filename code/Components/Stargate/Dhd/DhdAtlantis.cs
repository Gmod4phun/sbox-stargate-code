namespace Sandbox.Components.Stargate
{
    public partial class DhdAtlantis : Dhd
    {
        protected override string ButtonSymbols => "ABCDEFGHIJKLMNOPQRST123456789UVW0XYZ";
        // protected override string ButtonSymbols => "T#";

        public DhdAtlantis()
        {
            Data = new( "peg", "dhd.atlantis.press", "dhd.press_dial" );
            DialIsLock = true;
        }

        public void CreateSingleButton( string model, string action, bool disabled = false, Transform localTransformOverride = new Transform(), int glyphBodyGroup = 0 ) // visible model of buttons that turn on/off and animate
        {
            var button_object = new GameObject();
            button_object.Name = $"Button ({action})";
            button_object.Transform.World = GameObject.Transform.World;
            button_object.SetParent( GameObject );

            if ( localTransformOverride != new Transform() )
            {
                button_object.Transform.Local = localTransformOverride;
            }

            var button_component = button_object.Components.Create<DhdButton>();
            var renderer = button_object.Components.Create<Superglyph>();
            renderer.Model = Model.Load( model );
            renderer.SetBodyGroup( "glyph", glyphBodyGroup );

            renderer.GlyphEnabled = false;
            renderer.BrightnessTimeDelta = 12;

            if ( action.Length == 1 )
            {
                renderer.GlyphNumber = StargateRingPegasus.RingSymbols.IndexOf( action );
            }

            var collider = button_object.Components.Create<ModelCollider>();
            collider.Model = renderer.Model;
            renderer.Enabled = false;
            renderer.Enabled = true;

            button_component.Action = action;
            button_component.Disabled = disabled;
        }

        public override void CreateButtons() // visible models of buttons that turn on/off and animate
        {
            // SYMBOL BUTTONS
            var center = global::Transform.Zero.WithPosition( new Vector3( 3.235f, 2.18f, 37.45f ) );
            var mdl = "models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_dynamic.vmdl";

            var stepHorizontal = 3.6f;
            var stepVertical = 5.6f;

            var symbolIndex = 0;
            int[] horizontalOffsets = new[] { 0, 1, 2, 2, 1 };
            for ( var iVertical = -2; iVertical <= 2; iVertical++ )
            {
                var horizontalOffset = horizontalOffsets[iVertical + 2];
                for ( var iHorizontal = -2 - horizontalOffset; iHorizontal <= 2 + horizontalOffset; iHorizontal++ )
                {
                    if ( iHorizontal == 0 && iVertical == 0 )
                        continue;

                    var shouldRotate = symbolIndex % 2 != 0;
                    if ( iVertical == -1 || iVertical == 1 || (iVertical == 0 && iHorizontal > 0) )
                        shouldRotate = !shouldRotate;

                    var action = ButtonSymbols[symbolIndex++].ToString();
                    var buttonTransform = center.WithPosition( center.Position + center.Rotation.Left * (stepHorizontal * iHorizontal) + center.Rotation.Forward * (stepVertical * iVertical) ).WithRotation( center.Rotation.RotateAroundAxis( Vector3.Up, shouldRotate ? 180 : 0 ) );
                    CreateSingleButton( mdl, action, localTransformOverride: buttonTransform, glyphBodyGroup: shouldRotate ? 2 : 1 );
                }
            }

            // CENTER DIAL BUTTON
            // CreateSingleButton( "models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_37.vmdl", "DIAL" );
            CreateSingleButton( mdl, "DIAL", localTransformOverride: center );

            CreateSingleButton( "models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_extra_1.vmdl", "@" );
            CreateSingleButton( "models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_extra_2.vmdl", "*" );
            CreateSingleButton( "models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_extra_3.vmdl", "IRIS" );
            CreateSingleButton( "models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_extra_4.vmdl", "INSTANT" );
            CreateSingleButton( "models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_extra_5.vmdl", "FAST" );
            CreateSingleButton( "models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_extra_6.vmdl", "SLOW" );
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if ( DhdModel.IsValid() && DhdModel.SceneObject is SceneObject so )
            {
                so.Attributes.Set( "selfillumscale", 1 );
            }
        }
    }
}

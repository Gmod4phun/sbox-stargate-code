namespace Sandbox.Components.Stargate.Rings
{
    [Title( "Ring Panel (Ori)" ), Category( "Stargate" ), Icon( "chair" ), Spawnable]
    public partial class RingPanelOri : RingPanel
    {
        protected override string[] ButtonsSounds { get; } = { "ringpanel.ancient.button1", "ringpanel.ancient.button2" };

        public static void DrawGizmos( EditorContext context )
        {
            for ( var i = 1; i <= 6; i++ )
            {
                Gizmo.Draw.Model( $"models/sbox_stargate/rings_panel/ori/ring_panel_ori_button_{i}.vmdl" );
            }
        }

        public void CreateButtons() // visible models of buttons that turn on/off and animate
        {
            var actions = "12345ABCDEFGHIJKL";
            for ( var i = 1; i <= 18; i++ )
            {
                var button_object = new GameObject();
                button_object.Transform.World = Transform.World;
                button_object.SetParent( GameObject );

                var button = button_object.Components.Create<RingPanelButton>();
                var renderer = button_object.Components.Create<SkinnedModelRenderer>();

                if ( i == 18 )
                {
                    renderer.Model = Model.Load( "models/sbox_stargate/rings_panel/ori/ring_panel_ori_button_center.vmdl" );
                }
                else
                {
                    renderer.Model = Model.Load( $"models/sbox_stargate/rings_panel/ori/ring_panel_ori_button_{i}.vmdl" );
                }

                var collider = button_object.Components.Create<ModelCollider>();
                collider.Model = renderer.Model;

                var action = (i == 18) ? "DIAL" : actions[i - 1].ToString();
                button.Action = action;
            }
        }
    }
}

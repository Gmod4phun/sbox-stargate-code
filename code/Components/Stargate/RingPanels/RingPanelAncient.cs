namespace Sandbox.Components.Stargate.Rings
{
    [Title( "Ring Panel (Ancient)" ), Category( "Stargate" ), Icon( "chair" ), Spawnable]
    public partial class RingPanelAncient : RingPanel
    {
        protected override string[] ButtonsSounds { get; } = { "ringpanel.ancient.button1", "ringpanel.ancient.button2" };

        public static void DrawGizmos( EditorContext context )
        {
            for ( var i = 1; i <= 6; i++ )
            {
                Gizmo.Draw.Model( $"models/sbox_stargate/rings_panel/ancient/ring_panel_ancient_button_{i}.vmdl" );
            }
        }

        public void CreateButtons() // visible models of buttons that turn on/off and animate
        {
            for ( var i = 1; i <= 9; i++ )
            {
                var button_object = new GameObject();
                button_object.Transform.World = Transform.World;
                button_object.SetParent( GameObject );

                var button = button_object.Components.Create<RingPanelButton>();
                var renderer = button_object.Components.Create<SkinnedModelRenderer>();
                renderer.Model = Model.Load( $"models/sbox_stargate/rings_panel/ancient/ring_panel_ancient_button_{i}.vmdl" );

                var collider = button_object.Components.Create<ModelCollider>();
                collider.Model = renderer.Model;

                var action = (i == 9) ? "DIAL" : i.ToString();
                button.Action = action;
            }
        }
    }
}

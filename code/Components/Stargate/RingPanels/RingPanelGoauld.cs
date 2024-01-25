namespace Sandbox.Components.Stargate.Rings
{
    [Title( "Ring Panel (Goauld)" ), Category( "Stargate" ), Icon( "chair" ), Spawnable]
    public partial class RingPanelGoauld : RingPanel
    {
        protected override string[] ButtonsSounds { get; } = { "ringpanel.goauld.button1", "ringpanel.goauld.button2" };

        public static void DrawGizmos( EditorContext context )
        {
            for ( var i = 1; i <= 6; i++ )
            {
                Gizmo.Draw.Model( $"models/sbox_stargate/rings_panel/goauld/ring_panel_goauld_button_{i}.vmdl" );
            }
        }

        public void CreateButtons() // visible models of buttons that turn on/off and animate
        {
            for ( var i = 1; i <= 6; i++ )
            {
                var button_object = new GameObject();
                button_object.Transform.World = Transform.World;
                button_object.SetParent( GameObject );

                var button = button_object.Components.Create<RingPanelButton>();
                var renderer = button_object.Components.Create<SkinnedModelRenderer>();
                renderer.Model = Model.Load( $"models/sbox_stargate/rings_panel/goauld/ring_panel_goauld_button_{i}.vmdl" );

                var collider = button_object.Components.Create<ModelCollider>();
                collider.Model = renderer.Model;

                var action = (i == 6) ? "DIAL" : i.ToString();
                button.Action = action;
            }
        }
    }
}
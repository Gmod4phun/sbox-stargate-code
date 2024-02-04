public class AtlantisTransporter : Component, Component.ExecuteInEditor
{
    [Property]
    public bool PanelOpened { get; set; }

    [Property]
    public bool DoorsOpened { get; set; }

    private SkinnedModelRenderer Renderer => Components.Get<SkinnedModelRenderer>();
    private AtlantisTransporterTrigger Trigger => Components.Get<AtlantisTransporterTrigger>( FindMode.InChildren );

    [Property]
    private AtlantisTransporterDoor DoorRight { get; set; } = null;

    [Property]
    private AtlantisTransporterDoor DoorLeft { get; set; }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        PanelOpened = Trigger?.Touching.Any() ?? false;

        Renderer?.Set( "Open", PanelOpened );


        if ( DoorRight.IsValid() )
        {
            DoorRight.Open = DoorsOpened;
        }

        if ( DoorLeft.IsValid() )
        {
            DoorLeft.Open = DoorsOpened;
        }
    }
}

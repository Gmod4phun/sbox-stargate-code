public class AtlantisTransporter : Component, Component.ExecuteInEditor
{
    [Property]
    public bool PanelOpened { get; set; }

    [Property]
    public bool DoorsOpened { get; set; }

    private SkinnedModelRenderer Renderer => Components.Get<SkinnedModelRenderer>();
    private AtlantisTransporterTrigger Trigger => Components.Get<AtlantisTransporterTrigger>( FindMode.InChildren );

    [Property]
    private Door DoorRight { get; set; } = null;

    [Property]
    private Door DoorLeft { get; set; }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        PanelOpened = Trigger?.Touching.Any() ?? false;

        Renderer?.Set( "Open", PanelOpened );


        if ( DoorRight.IsValid() )
        {
            DoorRight.ToggleDoor();
        }

        if ( DoorLeft.IsValid() )
        {
            DoorLeft.ToggleDoor();
        }
    }
}

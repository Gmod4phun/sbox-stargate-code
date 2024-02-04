public class AtlantisTransporterDoor : Component, Component.ExecuteInEditor
{
    private ModelRenderer Renderer => Components.Get<ModelRenderer>();
    private AtlantisTransporter Transporter => GameObject.Parent.Components.Get<AtlantisTransporter>();

    [Property]
    public bool Open { get; set; } = false;

    [Property]
    public float DoorMoveDistance { get; set; } = 32;
    private float currentMoveDistance = 0;

    protected override void OnUpdate()
    {
        base.OnUpdate();

        HandleMovement();
    }

    private void HandleMovement()
    {
        currentMoveDistance = currentMoveDistance.Approach( Open ? DoorMoveDistance : 0, Time.Delta * 20 );

        if ( Transporter.IsValid() )
        {
            Transform.World = Transform.World.WithPosition( Transporter.Transform.Position - Transform.Rotation.Right * currentMoveDistance );
        }
    }
}

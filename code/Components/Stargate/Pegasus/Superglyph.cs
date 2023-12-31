public class Superglyph : ModelRenderer, Component.ExecuteInEditor
{
    [Property]
    public int GlyphNumber { get; set; }

    [Property]
    public int PositionOnRing { get; set; }

    protected override void OnPreRender()
    {
        RenderType = ShadowRenderType.Off;
        if ( SceneObject is not null && SceneObject.IsValid() )
        {
            SceneObject.Batchable = false;
            SceneObject.Attributes.Set( "frame", GlyphNumber.UnsignedMod( 36 ) );
            SceneObject.Transform = GameObject.Transform.World.WithRotation( GameObject.Transform.World.Rotation.RotateAroundAxis( Vector3.Forward, -10 * PositionOnRing ) );
        }
    }
}

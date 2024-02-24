public static class UtilityFunctions
{

    public static async Task<GameObject> SpawnProp( Vector3 pos, Rotation rot, string ident, int worldIndex )
    {
        var prop_object = new GameObject();
        prop_object.Name = "Prop";
        prop_object.Transform.Position = pos;
        prop_object.Transform.Rotation = rot;

        var package = await Package.FetchAsync( ident, false );
        await package.MountAsync();
        var model = Model.Load( package.GetMeta( "PrimaryAsset", "" ) );

        var prop = prop_object.Components.Create<Prop>();
        prop.Model = model;

        MultiWorldSystem.AssignWorldToObject( prop_object, worldIndex );

        return prop_object;
    }

    public static async Task<GameObject> SpawnCitizenRagdoll( Vector3 pos, Rotation rot, int worldIndex )
    {
        var prop_object = new GameObject();
        prop_object.Name = "Prop";
        prop_object.Transform.Position = pos;
        prop_object.Transform.Rotation = rot;

        var model = Model.Load( "models/citizen/citizen.vmdl" );

        var prop = prop_object.Components.Create<Prop>();
        prop.Model = model;

        prop.Enabled = false;
        prop.Enabled = true;

        MultiWorldSystem.AssignWorldToObject( prop_object, worldIndex );

        return prop_object;
    }

    public static async void ShootProp( Vector3 pos, Vector3 dir, float power, int worldIndex )
    {
        var prop_object = new GameObject();
        prop_object.Name = "Prop";
        prop_object.Transform.Position = pos;
        prop_object.Transform.Rotation = Rotation.LookAt( dir );

        var worldobject = GameManager.ActiveScene.GetAllObjects( true ).FirstOrDefault( x => x.Name == $"World {worldIndex}" );
        prop_object.SetParent( worldobject, false );

        var prop = prop_object.Components.Create<Prop>();
        prop.Model = Cloud.Model( "facepunch.wooden_crate" );

        var body = prop_object.Components.Get<Rigidbody>();
        if ( body.IsValid() )
        {
            body.Velocity = dir.Normal * power;
        }

        MultiWorldSystem.AssignWorldToObject( prop_object, worldIndex );
    }

    public static async void DestroyGameObjectDelayed( GameObject gameObject, float time )
    {
        await GameTask.DelaySeconds( time );
        gameObject?.Destroy();
    }
}

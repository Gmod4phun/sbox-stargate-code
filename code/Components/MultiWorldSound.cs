public class MultiWorldSound : Component
{
    public SoundHandle Handle { get; set; }
    public GameObject FollowObject { get; set; }

    public SoundHandle Play( string name, Vector3 position )
    {
        Handle = Sound.Play( name, position );
        return Handle;
    }

    public void Stop( float fadeTime = 0 )
    {
        if ( Handle.IsValid() )
        {
            Handle.Stop( fadeTime );
        }
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if ( Handle.IsValid() )
        {
            if ( FollowObject.IsValid() )
            {
                Handle.Position = FollowObject.Transform.Position;
            }

            var player = GameManager.ActiveScene.GetAllComponents<PlayerController>().FirstOrDefault( p => p.Network.OwnerConnection != null && p.Network.OwnerConnection == Connection.Local );
            Handle.Volume = MultiWorldSystem.GetWorldIndexOfObject( player ) == MultiWorldSystem.GetWorldIndexOfObject( this ) ? 1 : 0;
        }

    }
}

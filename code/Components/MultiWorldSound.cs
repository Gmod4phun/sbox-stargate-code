public class MultiWorldSound
{
    public int WorldIndex { get; set; }
    public SoundHandle Handle { get; set; }
    public GameObject FollowObject { get; set; }

    public float Volume { get; set; } = 1;

    public static MultiWorldSound Play( string name, Vector3 position, int worldIndex )
    {
        var sound = new MultiWorldSound
        {
            WorldIndex = worldIndex,
            Handle = Sound.Play( name, position )
        };

        sound.Handle.Volume = 0;

        MultiWorldSystem.AddSound( sound );

        return sound;
    }

    public static MultiWorldSound Play( string name, GameObject gameObject, bool followObject = false )
    {
        if ( followObject )
        {
            return PlayAndFollowObject( name, gameObject );
        }

        var sound = new MultiWorldSound
        {
            WorldIndex = MultiWorldSystem.GetWorldIndexOfObject( gameObject ),
            Handle = Sound.Play( name, gameObject.Transform.Position )
        };

        sound.Handle.Volume = 0;

        MultiWorldSystem.AddSound( sound );

        return sound;
    }

    private static MultiWorldSound PlayAndFollowObject( string name, GameObject followObject )
    {
        var sound = new MultiWorldSound
        {
            FollowObject = followObject,
            Handle = Sound.Play( name, followObject.Transform.Position )
        };

        sound.Handle.Volume = 0;
        sound.UpdateWorldIndexFromFollowedObject();

        MultiWorldSystem.AddSound( sound );

        return sound;
    }

    public void UpdateWorldIndexFromFollowedObject()
    {
        WorldIndex = MultiWorldSystem.GetWorldIndexOfObject( FollowObject );
    }

    public void Stop( float fadeTime = 0 )
    {
        if ( Handle.IsValid() )
        {
            Handle.Stop( fadeTime );
        }
    }
}

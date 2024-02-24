public class MultiWorldSystem : GameObjectSystem
{
    // public static MultiWorldSystem Current => GameManager.ActiveScene.GetSystem<MultiWorldSystem>();

    // public Dictionary<int, MultiWorld> Worlds = new();
    public static IEnumerable<MultiWorld> Worlds => GameManager.ActiveScene.GetAllComponents<MultiWorld>();
    public static IEnumerable<int> AllWorldIndices => Worlds.Select( w => w.WorldIndex );
    public static List<MultiWorldSound> Sounds = new();

    public MultiWorldSystem( Scene scene ) : base( scene )
    {
        Listen( Stage.PhysicsStep, 10, ProcessWorlds, "MultiWorld_ProcessWorlds" );
        Listen( Stage.FinishUpdate, 1, ProcessSounds, "MultiWorld_ProcessSounds" );

        Init();
    }

    /// <summary>
    /// Checks if the GameObject exists in a world and is not a child of another object in the world)
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    public static bool IsObjectRootInWorld( GameObject gameObject )
    {
        if ( gameObject.IsValid() && gameObject.Parent.IsValid() )
        {
            return gameObject.Parent.Components.TryGet<MultiWorld>( out var _, FindMode.InSelf );
        }

        return false;
    }

    public static string GetWorldTag( int worldIndex )
    {
        return $"world_{worldIndex}";
    }

    public static bool WorldExists( int worldIndex )
    {
        return AllWorldIndices.Contains( worldIndex );
    }

    public static MultiWorld GetWorldByIndex( int worldIndex )
    {
        return Worlds.FirstOrDefault( w => w.WorldIndex == worldIndex );
    }

    public static int GetWorldIndexOfObject( Component component )
    {
        if ( !component.IsValid() )
            return -1;

        return GetWorldIndexOfObject( component.GameObject );
    }

    public static int GetWorldIndexOfObject( GameObject gameObject )
    {
        if ( !gameObject.IsValid() )
            return -1;

        if ( gameObject.Components.TryGet<MultiWorld>( out var world, FindMode.InAncestors ) )
        {
            return world.WorldIndex;
        }

        return -1;
    }

    public static bool AreObjectsInSameWorld( GameObject a, GameObject b )
    {
        if ( a.Components.TryGet<MultiWorld>( out var worldA, FindMode.InAncestors ) )
        {
            if ( b.Components.TryGet<MultiWorld>( out var worldB, FindMode.InAncestors ) )
            {
                return worldA.WorldIndex == worldB.WorldIndex;
            }
        }

        return false;
    }

    public static void AssignWorldToObject( GameObject gameObject, int worldIndex )
    {
        if ( !WorldExists( worldIndex ) )
        {
            Log.Error( $"World {worldIndex} does not exist" );
            return;
        }

        // add to new world
        gameObject.Parent = GetWorldByIndex( worldIndex ).GameObject;

        // temp fix until tags update after parenting is fixed
        gameObject.Tags.Toggle( "_" );
        gameObject.Tags.Toggle( "_" );

        // if it's a player, handle that
        if ( gameObject.Components.TryGet<PlayerController>( out var ply ) )
        {
            AssignWorldToPlayer( ply, worldIndex );
        }

        // Log.Info( $"Object {gameObject} has moved to world {worldIndex}" );
    }

    private static void AssignWorldToPlayer( PlayerController player, int worldIndex )
    {
        if ( !player.IsValid() )
            return;

        var camera = player.Camera;
        var controller = player.Controller;

        var newWorldTag = GetWorldTag( worldIndex );
        var excludeTags = AllWorldIndices.Where( i => i != worldIndex ).Select( GetWorldTag ).ToArray();

        if ( !excludeTags.Any() )
        {
            Log.Warning( "No other worlds to exclude" );
            return;
        }
        else
        {
            // add all other worlds to exclude tags
            foreach ( var t in excludeTags )
            {
                camera.RenderExcludeTags.Add( t );
                controller.IgnoreLayers.Add( t );
                // player.Tags.Remove( t );
            }
        }

        // remove exluce tag of the world we will be in
        camera.RenderExcludeTags.Remove( newWorldTag );
        controller.IgnoreLayers.Remove( newWorldTag );
        // player.Tags.Add( newWorldTag );
        player.CurrentWorldIndex = worldIndex;
    }

    /*
    public async void AssignAllObjectsToDesiredWorlds()
    {
        await Task.Delay( 10 );

        foreach ( var gameObject in GameManager.ActiveScene.GetAllObjects( false ) )
        {
            var allWorldTags = AllWorldIndices.Select( GetWorldTag ).ToHashSet();

            foreach ( var tag in allWorldTags )
            {
                if ( gameObject.Tags.Has( tag ) )
                {
                    var worldIndex = tag.Split( '_' ).Last().ToInt();
                    AssignWorldToObject( gameObject, worldIndex );
                }
            }

            // Log.Info( $"Assigning {gameObject} to default world" );
        }
    }
    */

    public static void AddSound( MultiWorldSound sound )
    {
        Sounds.Add( sound );
    }

    public void Init()
    {
        // AddWorld( 0 ); // add the default world as world_0

        // Add some other worlds
        // AddWorld( 1 );
        // AddWorld( 2 );

        // AssignAllObjectsToDesiredWorlds();

        InitializePlayers();
    }

    private async void InitializePlayers()
    {
        await Task.Delay( 10 );

        foreach ( var player in Scene.GetAllComponents<PlayerController>() )
        {
            if ( player.IsValid() )
            {
                Log.Info( "MultiWorld: Initializing player" );
                AssignWorldToObject( player.GameObject, player.CurrentWorldIndex );
            }
        }
    }

    void ProcessWorlds()
    {
        foreach ( var player in Scene.GetAllComponents<PlayerController>() )
        {
            if ( player.IsValid() && GetWorldIndexOfObject( player.GameObject ) != player.CurrentWorldIndex )
            {
                AssignWorldToObject( player.GameObject, player.CurrentWorldIndex );
            }
        }
    }

    void ProcessSounds()
    {
        foreach ( var sound in Sounds )
        {
            if ( sound.Handle.IsValid() )
            {
                var followObjectValid = sound.FollowObject.IsValid();
                if ( followObjectValid )
                {
                    sound.Handle.Position = sound.FollowObject.Transform.Position;
                }

                var player = GameManager.ActiveScene.GetAllComponents<PlayerController>().FirstOrDefault( p => p.Network.OwnerConnection != null && p.Network.OwnerConnection == Connection.Local );
                if ( GetWorldIndexOfObject( player ) == (followObjectValid ? GetWorldIndexOfObject( sound.FollowObject ) : sound.WorldIndex) )
                {
                    sound.Handle.Volume = 1;
                }
                else
                {
                    sound.Handle.Volume = 0;
                }
            }
        }
    }
}

public class MultiWorldSystem : GameObjectSystem
{
    public static MultiWorldSystem Current => GameManager.ActiveScene.GetSystem<MultiWorldSystem>();

    public Dictionary<int, HashSet<GameObject>> WorldObjectsMap = new();
    public Dictionary<GameObject, int> GameObjectWorldMap = new();

    public IEnumerable<int> AllWorldIndices => WorldObjectsMap.Keys;
    public int HighestWorldIndex => WorldObjectsMap.Keys.Any() ? WorldObjectsMap.Keys.Max() : -1;

    public MultiWorldSystem( Scene scene ) : base( scene )
    {
        Listen( Stage.PhysicsStep, 10, ProcessWorlds, "DoingSomething" );
        Init();
    }

    public static string GetWorldTag( int worldIndex )
    {
        return $"world_{worldIndex}";
    }

    public int GetWorldIndexOfObject( GameObject gameObject )
    {
        if ( GameObjectWorldMap.TryGetValue( gameObject, out var worldIndex ) )
        {
            return worldIndex;
        }

        return -1;
    }

    public void AddNewWorld()
    {
        var newWorldIndex = HighestWorldIndex + 1;
        AddWorld( newWorldIndex );
    }

    public void AddWorld( int worldIndex )
    {
        if ( WorldObjectsMap.ContainsKey( worldIndex ) )
        {
            Log.Error( $"World {worldIndex} already exists" );
            return;
        }

        WorldObjectsMap[worldIndex] = new();
    }

    public void RemoveWorld( int worldIndex )
    {
        if ( WorldObjectsMap.TryGetValue( worldIndex, out var objects ) )
        {
            if ( objects.Any() )
            {
                Log.Error( $"World {worldIndex} has objects in it, cannot remove" );
                return;
            }
        }

        WorldObjectsMap.Remove( worldIndex );
    }

    public void AssignWorldToObject( GameObject gameObject, int worldIndex )
    {
        if ( !WorldObjectsMap.TryGetValue( worldIndex, out var objects ) )
        {
            Log.Error( $"World {worldIndex} does not exist" );
            return;
        }

        if ( GameObjectWorldMap.TryGetValue( gameObject, out var currentWorldIndex ) )
        {
            if ( currentWorldIndex == worldIndex )
            {
                Log.Warning( $"Object {gameObject} is already in the desired world {worldIndex}" );
                return;
            }

            // remove from current world (if any)
            if ( WorldObjectsMap.TryGetValue( currentWorldIndex, out var currentWorldObjects ) )
            {
                currentWorldObjects.Remove( gameObject );
                gameObject.Tags.Remove( GetWorldTag( currentWorldIndex ) );
            }
        }

        // add to new world
        WorldObjectsMap[worldIndex].Add( gameObject );
        GameObjectWorldMap[gameObject] = worldIndex;
        gameObject.Tags.Add( GetWorldTag( worldIndex ) );

        // if it's a player, handle that
        if ( gameObject.Components.TryGet<PlayerController>( out var ply ) )
        {
            AssignWorldToPlayer( ply, worldIndex );
        }
    }

    public void RemoveObjectFromWorld( GameObject gameObject )
    {
        if ( GameObjectWorldMap.TryGetValue( gameObject, out var worldIndex ) )
        {
            if ( WorldObjectsMap.TryGetValue( worldIndex, out var objects ) )
            {
                objects.Remove( gameObject );
            }

            GameObjectWorldMap.Remove( gameObject );
        }
    }

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

    public void AssignWorldToPlayer( PlayerController player, int worldIndex )
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
                player.Tags.Remove( t );
            }
        }

        // remove exluce tag of the world we will be in
        camera.RenderExcludeTags.Remove( newWorldTag );
        controller.IgnoreLayers.Remove( newWorldTag );
        player.Tags.Add( newWorldTag );
        player.CurrentWorldIndex = worldIndex;

        Log.Info( $"Player {player} is moving to {newWorldTag}" );
    }

    public void Init()
    {
        AddWorld( 0 ); // add the default world as world_0

        // Add some other worlds
        AddWorld( 1 );
        AddWorld( 2 );

        AssignAllObjectsToDesiredWorlds();
    }

    void ProcessWorlds()
    {
        // Log.Info( $"Highest world index is {HighestWorldIndex}" );

        // var allThings = Scene.GetAllComponents<MyThing>();
        // do something to all of the things

        // Log.Info( Scene.GetAllObjects( true ).Count() );

        foreach ( var player in Scene.GetAllComponents<PlayerController>() )
        {
            if ( player.IsValid() )
            {
                if ( GameObjectWorldMap.TryGetValue( player.GameObject, out var currentWorldIndex ) )
                {
                    if ( player.CurrentWorldIndex != currentWorldIndex )
                    {
                        AssignWorldToObject( player.GameObject, player.CurrentWorldIndex );
                    }
                }
            }
        }
    }
}

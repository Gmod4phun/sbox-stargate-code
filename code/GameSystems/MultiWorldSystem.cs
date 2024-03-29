public class MultiWorldSystem : GameObjectSystem
{
    // public static MultiWorldSystem Current => Game.ActiveScene.GetSystem<MultiWorldSystem>();

    // public Dictionary<int, MultiWorld> Worlds = new();
    public static IEnumerable<MultiWorld> Worlds => Game.ActiveScene.IsValid() ? Game.ActiveScene.GetAllComponents<MultiWorld>() : null;
    public static IEnumerable<int> AllWorldIndices => Worlds.Select( w => w.WorldIndex );
    public static List<MultiWorldSound> Sounds = new();

    public MultiWorldSystem( Scene scene ) : base( scene )
    {
        Listen( Stage.FinishUpdate, 2, ProcessWorlds, "MultiWorld_ProcessWorlds" );
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
        AssignBroadcast( gameObject.Id, worldIndex );
    }

    [Broadcast]
    public static void AssignBroadcast( Guid objectId, int worldIndex )
    {
        var obj = Game.ActiveScene.GetAllObjects( false ).FirstOrDefault( o => o.Id == objectId );
        if ( obj.IsValid() )
        {
            AssignWorldToObjectMain( obj, worldIndex );
        }
    }

    public static void AssignWorldToObjectMain( GameObject gameObject, int worldIndex )
    {
        if ( !WorldExists( worldIndex ) )
        {
            Log.Error( $"World {worldIndex} does not exist" );
            return;
        }

        // add to new world
        gameObject.Parent = GetWorldByIndex( worldIndex ).GameObject;

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

        // Log.Info( $"Assigning player {player} to world {worldIndex}" );

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

        // idk why this shit still has a problem, gotta figure out what broke recently
        player.Tags.Toggle( "_" );
        player.Tags.Toggle( "_" );

        // remove exluce tag of the world we will be in
        camera.RenderExcludeTags.Remove( newWorldTag );
        controller.IgnoreLayers.Remove( newWorldTag );
        player.CurrentWorldIndex = worldIndex;
    }

    public static void AddSound( MultiWorldSound sound )
    {
        Sounds.Add( sound );
    }

    public static void Init()
    {
        InitializeCollisionRules();
    }

    private static async void InitializeCollisionRules()
    {
        await Task.Delay( 10 );

        // default rules, will need to get them from the project when thats implemented
        var rules = new Sandbox.Physics.CollisionRules();

        // setup rules for Stargate shit
        rules.Pairs.Add( new( StargateTags.BehindGate, StargateTags.InBufferFront, Sandbox.Physics.CollisionRules.Result.Ignore ) );
        rules.Pairs.Add( new( StargateTags.BehindGate, StargateTags.BeforeGate, Sandbox.Physics.CollisionRules.Result.Ignore ) );

        rules.Pairs.Add( new( StargateTags.BeforeGate, StargateTags.InBufferBack, Sandbox.Physics.CollisionRules.Result.Ignore ) );

        rules.Pairs.Add( new( StargateTags.InBufferFront, StargateTags.FakeWorld, Sandbox.Physics.CollisionRules.Result.Collide ) );
        rules.Pairs.Add( new( StargateTags.InBufferFront, "world", Sandbox.Physics.CollisionRules.Result.Ignore ) );

        rules.Pairs.Add( new( StargateTags.InBufferFront, StargateTags.InBufferBack, Sandbox.Physics.CollisionRules.Result.Ignore ) );

        rules.Pairs.Add( new( StargateTags.InBufferBack, StargateTags.FakeWorld, Sandbox.Physics.CollisionRules.Result.Collide ) );
        rules.Pairs.Add( new( StargateTags.InBufferBack, "world", Sandbox.Physics.CollisionRules.Result.Ignore ) );

        rules.Pairs.Add( new( StargateTags.Ringsring, StargateTags.Ringplatform, Sandbox.Physics.CollisionRules.Result.Ignore ) );
        rules.Pairs.Add( new( StargateTags.Ringsring, StargateTags.Ringsring, Sandbox.Physics.CollisionRules.Result.Ignore ) );

        rules.Pairs.Add( new( "world", "ignoreworld", Sandbox.Physics.CollisionRules.Result.Ignore ) );
        rules.Pairs.Add( new( "terrain", StargateTags.EHTrigger, Sandbox.Physics.CollisionRules.Result.Ignore ) );

        // world exclude rules
        foreach ( var world in Worlds )
        {
            foreach ( var otherWorld in Worlds )
            {
                if ( world.WorldIndex != otherWorld.WorldIndex )
                {
                    rules.Pairs.Add( new( GetWorldTag( world.WorldIndex ), GetWorldTag( otherWorld.WorldIndex ), Sandbox.Physics.CollisionRules.Result.Ignore ) );
                }
            }
        }

        Game.ActiveScene.PhysicsWorld.SetCollisionRules( rules );
        Log.Info( "MultiWorld: Collision rules initialized" );
    }

    void ProcessWorlds()
    {
        if ( Connection.Local.IsHost )
        {
            foreach ( var player in Scene.GetAllComponents<PlayerController>() )
            {
                if ( player.IsValid() && GetWorldIndexOfObject( player.GameObject ) != player.CurrentWorldIndex )
                {
                    AssignWorldToObject( player.GameObject, player.CurrentWorldIndex );
                }
            }
        }

        var localPlayer = Game.ActiveScene.GetAllComponents<PlayerController>().FirstOrDefault( p => p.Network.OwnerConnection != null && p.Network.OwnerConnection == Connection.Local );
        var playerWorldIndex = GetWorldIndexOfObject( localPlayer );

        foreach ( var rigidbody in Scene.GetAllComponents<Rigidbody>() )
        {
            if ( rigidbody.IsValid() && GetWorldIndexOfObject( rigidbody.GameObject ) != playerWorldIndex )
            {
                rigidbody.RigidbodyFlags |= RigidbodyFlags.DisableCollisionSounds;
            }
            else
            {
                rigidbody.RigidbodyFlags &= ~RigidbodyFlags.DisableCollisionSounds;
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
                if ( sound.FollowObject != null )
                {
                    if ( !followObjectValid )
                    {
                        sound.Handle.Stop();
                    }
                    else
                    {
                        sound.Handle.Position = sound.FollowObject.Transform.Position;
                    }
                }

                var player = Game.ActiveScene.GetAllComponents<PlayerController>().FirstOrDefault( p => p.Network.OwnerConnection != null && p.Network.OwnerConnection == Connection.Local );
                if ( GetWorldIndexOfObject( player ) == (followObjectValid ? GetWorldIndexOfObject( sound.FollowObject ) : sound.WorldIndex) )
                {
                    sound.Handle.Volume = sound.Volume;
                }
                else
                {
                    sound.Handle.Volume = 0;
                }
            }
        }
    }
}

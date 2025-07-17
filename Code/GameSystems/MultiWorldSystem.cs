public class MultiWorldSystem : GameObjectSystem
{
	// public static MultiWorldSystem Current => Game.ActiveScene.GetSystem<MultiWorldSystem>();

	// public Dictionary<int, MultiWorld> Worlds = new();
	public static IEnumerable<MultiWorld> Worlds =>
		Game.ActiveScene.IsValid()
			? Game.ActiveScene.GetAllComponents<MultiWorld>()
			: new List<MultiWorld>();
	public static IEnumerable<int> AllWorldIndices => Worlds.Select(w => w.WorldIndex);
	public static List<MultiWorldSound> FollowingSounds = new();

	public MultiWorldSystem(Scene scene)
		: base(scene)
	{
		Listen(Stage.FinishUpdate, 1, ProcessWorlds, "MultiWorld_ProcessWorlds");
		Listen(Stage.FinishUpdate, 2, ProcessSounds, "MultiWorld_ProcessSounds");

		Init();
	}

	/// <summary>
	/// Checks if the GameObject exists in a world and is not a child of another object in the world)
	/// </summary>
	/// <param name="gameObject"></param>
	/// <returns></returns>
	public static bool IsObjectRootInWorld(GameObject gameObject)
	{
		if (gameObject.IsValid() && gameObject.Parent.IsValid())
		{
			return gameObject.Parent.Components.TryGet<MultiWorld>(out var _, FindMode.InSelf);
		}

		return false;
	}

	public static string GetWorldTag(int worldIndex)
	{
		return $"world_{worldIndex}";
	}

	public static string GetWorldTag(MultiWorld world)
	{
		return GetWorldTag(world.WorldIndex);
	}

	public static bool WorldExists(int worldIndex)
	{
		return AllWorldIndices.Contains(worldIndex);
	}

	public static int GetNextWorldIndex(int previousWorldIndex)
	{
		if (!AllWorldIndices.Any())
			return -1;

		if (AllWorldIndices.Count() == 1)
			return previousWorldIndex;

		var curWorldPos = AllWorldIndices.ToList().IndexOf(previousWorldIndex);
		var nextWorldIndex = AllWorldIndices.ElementAtOrDefault(curWorldPos + 1);

		if (curWorldPos == AllWorldIndices.Count() - 1)
		{
			nextWorldIndex = AllWorldIndices.First();
		}

		return nextWorldIndex;
	}

	public static MultiWorld GetWorldByIndex(int worldIndex)
	{
		return Worlds.FirstOrDefault(w => w.WorldIndex == worldIndex);
	}

	public static int GetWorldIndexOfObject(Component component)
	{
		if (!component.IsValid())
			return -1;

		return GetWorldIndexOfObject(component.GameObject);
	}

	public static int GetWorldIndexOfObject(GameObject gameObject)
	{
		if (!gameObject.IsValid())
			return -1;

		if (gameObject.Components.TryGet<MultiWorld>(out var world, FindMode.InAncestors))
		{
			return world.WorldIndex;
		}

		return -1;
	}

	public static bool AreObjectsInSameWorld(GameObject a, GameObject b)
	{
		if (a.Components.TryGet<MultiWorld>(out var worldA, FindMode.InAncestors))
		{
			if (b.Components.TryGet<MultiWorld>(out var worldB, FindMode.InAncestors))
			{
				return worldA.WorldIndex == worldB.WorldIndex;
			}
		}

		return false;
	}

	public static bool AreObjectsInSameWorld(Component a, Component b)
	{
		return AreObjectsInSameWorld(a.GameObject, b.GameObject);
	}

	public static bool AreObjectsInSameWorld(GameObject a, Component b)
	{
		return AreObjectsInSameWorld(a, b.GameObject);
	}

	public static bool AreObjectsInSameWorld(Component a, GameObject b)
	{
		return AreObjectsInSameWorld(a.GameObject, b);
	}

	public static void AssignWorldToObject(GameObject gameObject, int worldIndex)
	{
		AssignBroadcast(gameObject.Id, worldIndex);
	}

	[Rpc.Broadcast]
	public static void AssignBroadcast(Guid objectId, int worldIndex)
	{
		var obj = Game.ActiveScene.GetAllObjects(false).FirstOrDefault(o => o.Id == objectId);
		if (obj.IsValid())
		{
			AssignWorldToObjectMain(obj, worldIndex);
		}
	}

	public static void AssignWorldToObjectMain(GameObject gameObject, int worldIndex)
	{
		if (!WorldExists(worldIndex))
		{
			Log.Error($"World {worldIndex} does not exist");
			return;
		}

		// add to new world
		gameObject.SetParent(GetWorldByIndex(worldIndex).GameObject, true);

		// if it's a player, handle that
		if (gameObject.Components.TryGet<PlayerController>(out var ply))
		{
			Log.Info($"Assigning player {ply} to world {worldIndex}");
			AssignWorldToPlayer(ply, worldIndex);
		}

		// Log.Info( $"Object {gameObject} has moved to world {worldIndex}" );
	}

	private static void AssignWorldToPlayer(PlayerController player, int worldIndex)
	{
		if (!player.IsValid())
			return;

		// Log.Info( $"Assigning player {player} to world {worldIndex}" );

		var camera = player.GetCamera();
		var newWorldTag = GetWorldTag(worldIndex);
		var excludeTags = AllWorldIndices.Where(i => i != worldIndex).Select(GetWorldTag).ToArray();

		if (!excludeTags.Any())
		{
			Log.Warning("No other worlds to exclude");
			return;
		}
		else
		{
			// add all other worlds to exclude tags
			foreach (var t in excludeTags)
			{
				camera.RenderExcludeTags.Add(t);
				// player.Tags.Remove( t );
			}
		}

		// idk why this shit still has a problem, gotta figure out what broke recently
		player.Tags.Toggle("_");
		player.Tags.Toggle("_");

		// remove exluce tag of the world we will be in
		camera.RenderExcludeTags.Remove(newWorldTag);

		ProcessFog();
	}

	public static void AddSound(MultiWorldSound sound)
	{
		var world = GetWorldByIndex(sound.WorldIndex);
		if (world.IsValid())
		{
			var mixer = world.GetMixer();
			if (mixer != null && sound.Handle.IsValid())
			{
				sound.Handle.TargetMixer = mixer;
			}
		}

		if (sound.FollowObject.IsValid())
		{
			FollowingSounds.Add(sound);
		}
	}

	public static void Init()
	{
		InitializeCollisionRules();
		// ProcessFog();
	}

	private static async void InitializeCollisionRules()
	{
		await Task.Delay(10);

		// default rules, will need to get them from the project when thats implemented
		// var rules = new Sandbox.Physics.CollisionRules();
		var rules = ProjectSettings.Collision;

		// setup rules for Stargate shit
		rules.Pairs.TryAdd(
			new Sandbox.Physics.CollisionRules.Pair(
				StargateTags.BehindGate,
				StargateTags.InBufferFront
			),
			Sandbox.Physics.CollisionRules.Result.Ignore
		);
		rules.Pairs.TryAdd(
			new Sandbox.Physics.CollisionRules.Pair(
				StargateTags.BehindGate,
				StargateTags.BeforeGate
			),
			Sandbox.Physics.CollisionRules.Result.Ignore
		);

		rules.Pairs.TryAdd(
			new Sandbox.Physics.CollisionRules.Pair(
				StargateTags.BeforeGate,
				StargateTags.InBufferBack
			),
			Sandbox.Physics.CollisionRules.Result.Ignore
		);

		rules.Pairs.TryAdd(
			new Sandbox.Physics.CollisionRules.Pair(
				StargateTags.InBufferFront,
				StargateTags.FakeWorld
			),
			Sandbox.Physics.CollisionRules.Result.Collide
		);
		rules.Pairs.TryAdd(
			new Sandbox.Physics.CollisionRules.Pair(StargateTags.InBufferFront, "world"),
			Sandbox.Physics.CollisionRules.Result.Ignore
		);

		rules.Pairs.TryAdd(
			new Sandbox.Physics.CollisionRules.Pair(
				StargateTags.InBufferFront,
				StargateTags.InBufferBack
			),
			Sandbox.Physics.CollisionRules.Result.Ignore
		);

		rules.Pairs.TryAdd(
			new Sandbox.Physics.CollisionRules.Pair(
				StargateTags.InBufferBack,
				StargateTags.FakeWorld
			),
			Sandbox.Physics.CollisionRules.Result.Collide
		);
		rules.Pairs.TryAdd(
			new Sandbox.Physics.CollisionRules.Pair(StargateTags.InBufferBack, "world"),
			Sandbox.Physics.CollisionRules.Result.Ignore
		);

		rules.Pairs.TryAdd(
			new Sandbox.Physics.CollisionRules.Pair(
				StargateTags.Ringsring,
				StargateTags.Ringplatform
			),
			Sandbox.Physics.CollisionRules.Result.Ignore
		);
		rules.Pairs.TryAdd(
			new Sandbox.Physics.CollisionRules.Pair(StargateTags.Ringsring, StargateTags.Ringsring),
			Sandbox.Physics.CollisionRules.Result.Ignore
		);

		rules.Pairs.TryAdd(
			new Sandbox.Physics.CollisionRules.Pair("world", "ignoreworld"),
			Sandbox.Physics.CollisionRules.Result.Ignore
		);
		rules.Pairs.TryAdd(
			new Sandbox.Physics.CollisionRules.Pair("terrain", StargateTags.EHTrigger),
			Sandbox.Physics.CollisionRules.Result.Ignore
		);

		rules.Pairs.TryAdd(
			new Sandbox.Physics.CollisionRules.Pair("shield", "passesshield"),
			Sandbox.Physics.CollisionRules.Result.Ignore
		);

		Log.Info($"MultiWorld: Initializing collision rules for {Worlds.Count()} worlds");

		// world exclude rules
		if (Worlds != null)
		{
			foreach (var world in Worlds)
			{
				// rules.Defaults.TryAdd(
				// 	GetWorldTag(world.WorldIndex),
				// 	Sandbox.Physics.CollisionRules.Result.Collide
				// );

				foreach (var otherWorld in Worlds)
				{
					if (world.WorldIndex != otherWorld.WorldIndex)
					{
						var result = rules.Pairs.TryAdd(
							new Sandbox.Physics.CollisionRules.Pair(
								GetWorldTag(world.WorldIndex),
								GetWorldTag(otherWorld.WorldIndex)
							),
							Sandbox.Physics.CollisionRules.Result.Ignore
						);

						if (result)
						{
							Log.Info(
								$"MultiWorld: Added ignore collision rule for {world.WorldIndex} and {otherWorld.WorldIndex}"
							);
						}
						else
						{
							Log.Warning(
								$"MultiWorld: Ignore collision rule already exists for {world.WorldIndex} and {otherWorld.WorldIndex}"
							);
						}
					}
				}
			}
		}

		if (Game.ActiveScene.IsValid() && Game.ActiveScene.PhysicsWorld.IsValid())
		{
			Game.ActiveScene.PhysicsWorld.CollisionRules = rules;
			Log.Info("MultiWorld: Collision rules initialized");
		}

		ProcessFog();
	}

	void ProcessWorlds()
	{
		if (!Worlds.Any())
			return;

		// if (Connection.Local.IsHost)
		// {
		// 	foreach (var player in Scene.GetAllComponents<PlayerController>())
		// 	{
		// 		if (
		// 			player.IsValid()
		// 			&& GetWorldIndexOfObject(player.GameObject) != player.GetMultiWorld()
		// 		)
		// 		{
		// 			AssignWorldToObject(player.GameObject, player.CurrentWorldIndex);
		// 		}
		// 	}
		// }

		var localPlayer = Game
			.ActiveScene.GetAllComponents<PlayerController>()
			.FirstOrDefault(p => p.Network.Owner != null && p.Network.Owner == Connection.Local);
		var playerWorldIndex = GetWorldIndexOfObject(localPlayer);

		foreach (var rigidbody in Scene.GetAllComponents<Rigidbody>())
		{
			if (rigidbody.Tags.Has("player"))
				continue;

			if (
				rigidbody.IsValid()
				&& GetWorldIndexOfObject(rigidbody.GameObject) != playerWorldIndex
			)
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
		if (!Worlds.Any())
			return;

		var player = Game
			.ActiveScene.GetAllComponents<PlayerController>()
			.FirstOrDefault(p => p.Network.Owner != null && p.Network.Owner == Connection.Local);

		// set mixer hearable/unhearable for each player
		foreach (var world in Worlds)
		{
			var mixer = world.GetMixer();
			if (mixer != null)
			{
				mixer.Volume = GetWorldIndexOfObject(player) != world.WorldIndex ? 0 : 1; // change to Mute when implemented
			}
		}

		// update target sound mixer for following sounds if needed
		foreach (var sound in FollowingSounds)
		{
			if (sound.Handle.IsValid())
			{
				if (!sound.FollowObject.IsValid())
				{
					sound.Handle.Stop();
				}
				else
				{
					sound.Handle.Position = sound.FollowObject.WorldPosition;

					var desiredWorldIndex = GetWorldIndexOfObject(sound.FollowObject);
					if (desiredWorldIndex != -1 && desiredWorldIndex != sound.WorldIndex)
					{
						sound.Handle.TargetMixer = GetWorldByIndex(desiredWorldIndex).GetMixer();
					}
				}
			}
		}
	}

	static void AdjustComponentEnabledState<T>(PlayerController player)
		where T : Component
	{
		foreach (var c in Game.ActiveScene.GetAllComponents<T>())
		{
			c.Enabled = false;
		}

		var worldComponent = GetWorldByIndex(GetWorldIndexOfObject(player));
		if (!worldComponent.IsValid())
			return;

		foreach (
			var c in worldComponent.Components.GetAll<T>(FindMode.DisabledInSelfAndDescendants)
		)
		{
			if (c.IsValid())
			{
				c.Enabled = true;
			}
		}
	}

	public static void ProcessFog()
	{
		if (!Worlds.Any())
			return;

		var player = Game
			.ActiveScene.GetAllComponents<PlayerController>()
			.FirstOrDefault(p => p.Network.Owner != null && p.Network.Owner == Connection.Local);

		if (!player.IsValid())
			return;

		AdjustComponentEnabledState<CubemapFog>(player);
		AdjustComponentEnabledState<GradientFog>(player);
		AdjustComponentEnabledState<VolumetricFogVolume>(player);
		AdjustComponentEnabledState<DirectionalLight>(player);
		AdjustComponentEnabledState<SkyBox2D>(player);
	}
}

public static class MultiWorldSystemExtensions
{
	public static SceneTrace WithWorld(this SceneTrace trace, GameObject go)
	{
		return trace.WithTag(
			MultiWorldSystem.GetWorldTag(MultiWorldSystem.GetWorldIndexOfObject(go))
		);
	}

	public static SceneTrace WithWorld(this SceneTrace trace, MultiWorld world)
	{
		return trace.WithTag(MultiWorldSystem.GetWorldTag(world));
	}

	public static void ClearParent(this GameObject go)
	{
		var worldIndex = MultiWorldSystem.GetWorldIndexOfObject(go);
		if (MultiWorldSystem.WorldExists(worldIndex))
		{
			go.SetParent(MultiWorldSystem.GetWorldByIndex(worldIndex).GameObject, true);
		}
		else
		{
			go.SetParent(null, true);
		}
	}
}

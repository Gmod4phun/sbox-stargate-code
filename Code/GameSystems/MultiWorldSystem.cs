public class MultiWorldSystem : GameObjectSystem
{
	// public static MultiWorldSystem Current => Game.ActiveScene.GetSystem<MultiWorldSystem>();

	// public Dictionary<int, MultiWorld> Worlds = new();
	public static IEnumerable<MultiWorld> Worlds =>
		Game.ActiveScene.IsValid()
			? Game.ActiveScene.GetAllComponents<MultiWorld>()
			: new List<MultiWorld>();

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

	public static MultiWorld GetWorldByIndex(int worldIndex)
	{
		return Worlds.FirstOrDefault(w => w.WorldIndex == worldIndex);
	}

	public static bool AreObjectsInSameWorld(GameObject a, GameObject b)
	{
		return a.GetMultiWorld() == b.GetMultiWorld();
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
		var world = GetWorldByIndex(worldIndex);
		if (!world.IsValid())
		{
			Log.Error($"World {worldIndex} does not exist");
			return;
		}

		// add to new world
		gameObject.SetParent(world.GameObject, true);

		if (
			gameObject.Components.TryGet<CameraComponent>(
				out var camera,
				FindMode.EverythingInSelfAndDescendants
			)
		)
		{
			AssignWorldToCamera(camera, world);
		}
	}

	public static void AssignWorldToCamera(CameraComponent camera, MultiWorld world)
	{
		var newWorldTag = world.WorldTag;
		var excludeTags = Worlds.Where(w => w != world).Select(w => w.WorldTag).ToArray();

		if (excludeTags.Length == 0)
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
			}
		}

		camera.RenderExcludeTags.Remove(newWorldTag);

		// var world = GetWorldByIndex(worldIndex);
		ProcessEnvironmentalComponents(world);
	}

	public static void AddSound(MultiWorldSound sound)
	{
		var world = GetWorldByIndex(sound.WorldIndex);
		if (world.IsValid())
		{
			var mixer = world.AudioMixer;
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

		rules.Pairs.TryAdd(
			new Sandbox.Physics.CollisionRules.Pair(StargateTags.PowerNode, StargateTags.EHTrigger),
			Sandbox.Physics.CollisionRules.Result.Ignore
		);

		Log.Info($"MultiWorld: Initializing collision rules for {Worlds.Count()} worlds");

		// world exclude rules
		if (Worlds != null)
		{
			foreach (var world in Worlds)
			{
				foreach (var otherWorld in Worlds)
				{
					if (world.WorldIndex != otherWorld.WorldIndex)
					{
						var result = rules.Pairs.TryAdd(
							new Sandbox.Physics.CollisionRules.Pair(
								world.WorldTag,
								otherWorld.WorldTag
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
	}

	void ProcessWorlds()
	{
		if (!Worlds.Any())
			return;

		var camera = Scene.Camera;
		if (!camera.IsValid())
			return;

		foreach (var rigidbody in Scene.GetAllComponents<Rigidbody>())
		{
			if (rigidbody.Tags.Has("player")) // player controller handles its collision sounds
				continue;

			if (
				rigidbody.IsValid()
				&& rigidbody.GameObject.GetMultiWorld() != camera.GetMultiWorld()
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

		var camera = Scene.Camera;
		if (!camera.IsValid())
			return;

		// set mixer hearable/unhearable for active camera
		foreach (var world in Worlds)
		{
			var mixer = world.AudioMixer;
			if (mixer != null)
			{
				mixer.Mute = camera.GetMultiWorld() != world;
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

					var desiredWorld = sound.FollowObject.GetMultiWorld();
					if (desiredWorld.IsValid() && desiredWorld.WorldIndex != sound.WorldIndex)
					{
						sound.Handle.TargetMixer = desiredWorld.AudioMixer;
						sound.WorldIndex = desiredWorld.WorldIndex;
					}
				}
			}
		}
	}

	static void AdjustComponentEnabledState<T>(MultiWorld worldComponent)
		where T : Component
	{
		foreach (var c in Game.ActiveScene.GetAllComponents<T>())
		{
			c.Enabled = false;
		}

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

	static void ProcessEnvironmentalComponents(MultiWorld worldComponent)
	{
		AdjustComponentEnabledState<CubemapFog>(worldComponent);
		AdjustComponentEnabledState<GradientFog>(worldComponent);
		AdjustComponentEnabledState<VolumetricFogVolume>(worldComponent);
		AdjustComponentEnabledState<DirectionalLight>(worldComponent);
		AdjustComponentEnabledState<SkyBox2D>(worldComponent);
	}
}

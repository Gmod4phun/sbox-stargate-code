public class MultiWorldSceneLoader : Component
{
	[Property]
	public List<SceneFile> SceneFiles { get; set; } = [];

	public void LoadScene(SceneFile sceneFile, int worldIndex)
	{
		if (sceneFile is null)
		{
			Log.Warning("LoadScene: Scene file is null");
			return;
		}

		try
		{
			var go = sceneFile.GameObjects.First();
			go["Name"] = "MultiWorld " + worldIndex;

			var comps = go["Components"];
			var world = comps[0];
			world["WorldIndex"] = worldIndex;
		}
		catch (Exception e)
		{
			Log.Warning($"Error setting world index for scene {sceneFile}: {e.Message}");
		}

		var load = new SceneLoadOptions();
		load.SetScene(sceneFile);
		load.IsAdditive = true;
		Scene.Load(load);

		Log.Info($"Loaded scene '{sceneFile}' into world index {worldIndex}.");
	}

	protected override Task OnLoad()
	{
		if (!Scene.IsEditor && Enabled)
		{
			var existingWorldCount = MultiWorldSystem.Worlds.Count();
			for (int i = 0; i < SceneFiles.Count; i++)
			{
				LoadScene(SceneFiles[i], existingWorldCount + i);
			}
		}

		return base.OnLoad();
	}
}

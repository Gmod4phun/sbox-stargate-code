public static class MultiWorldExtensions
{
	public static MultiWorld GetMultiWorld(this GameObject go)
	{
		return go?.Components.Get<MultiWorld>(FindMode.EverythingInSelfAndAncestors);
	}

	public static MultiWorld GetMultiWorld(this Component component)
	{
		return component?.GameObject?.GetMultiWorld();
	}

	public static void SetMultiWorld(this GameObject go, MultiWorld world)
	{
		if (!world.IsValid())
			return;

		MultiWorldSystem.AssignWorldToObject(go, world.WorldIndex);
	}

	public static MultiWorld GetNextMultiWorld(this MultiWorld currentWorld)
	{
		if (!currentWorld.IsValid())
			return null;

		var worlds = MultiWorldSystem.Worlds.ToList();
		var currentIndex = worlds.IndexOf(currentWorld);
		if (currentIndex < 0)
			return null;

		var nextIndex = (currentIndex + 1) % worlds.Count;
		return worlds[nextIndex];
	}

	public static SceneTrace WithWorld(this SceneTrace trace, GameObject go)
	{
		return trace.WithWorld(go.GetMultiWorld());
	}

	public static SceneTrace WithWorld(this SceneTrace trace, MultiWorld world)
	{
		return trace.WithTag(world.WorldTag);
	}

	public static void ClearParent(this GameObject go)
	{
		var world = go.GetMultiWorld();
		go.SetParent(world?.GameObject ?? null, true);
	}
}

public static class MultiWorldExtensions
{
	public static MultiWorld GetMultiWorld(this GameObject go)
	{
		return go.Components.Get<MultiWorld>(FindMode.Enabled | FindMode.InAncestors);
	}

	public static MultiWorld GetMultiWorld(this Component component)
	{
		return component.GameObject.GetMultiWorld();
	}
}

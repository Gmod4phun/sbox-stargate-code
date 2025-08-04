public class LensFlareSystem : GameObjectSystem
{
	public static IEnumerable<DirectionalLight> Suns =>
		Game.ActiveScene.IsValid() ? Game.ActiveScene.GetAllComponents<DirectionalLight>() : [];

	public static List<MultiWorldSound> FollowingSounds = new();

	public LensFlareSystem(Scene scene)
		: base(scene)
	{
		Listen(Stage.SceneLoaded, 99, Loaded, "LensFlare_SceneLoaded");
	}

	private void Loaded()
	{
		if (Scene.IsEditor)
			return;

		foreach (var sun in Suns)
		{
			if (
				sun
					.GameObject.Components.Get<LensFlareOccluder>(FindMode.EverythingInSelf)
					.IsValid()
			)
				continue;

			sun.GameObject.Components.Create<LensFlareOccluder>();
		}
	}
}

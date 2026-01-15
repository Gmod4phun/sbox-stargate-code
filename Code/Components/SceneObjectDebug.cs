public class SceneObjectDebug : Component
{
	[Property]
	ModelRenderer Renderer { get; set; }

	[Button]
	void PrintSceneObject()
	{
		if (Renderer.IsValid())
		{
			Log.Info(Renderer.SceneObject);
		}
	}
};

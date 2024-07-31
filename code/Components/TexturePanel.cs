using Sandbox.UI;

public class TexturePanel : Component
{
	[Property]
	public PanelComponent SourcePanel { get; set; }

	[Property]
	public ModelRenderer TargetModelRenderer { get; set; }

	[Property]
	public string MaterialAttributeName { get; set; } = "color";

	private Texture texture;
	private RootPanel rootPanel;
	private SceneCustomObject customObject;

	private void EnsureRootPanel()
	{
		if (rootPanel.IsValid())
			return;

		rootPanel = new RootPanel { RenderedManually = true, Scene = Scene };
	}

	private void CreateTexture(Vector2 size)
	{
		texture?.Dispose();
		texture = Texture
			.CreateRenderTarget()
			.WithSize(size)
			.WithDynamicUsage()
			.WithUAVBinding()
			.Create();
	}

	private void Render(SceneObject sceneObject)
	{
		EnsureRootPanel();

		if (!rootPanel.IsValid())
			return;

		if (!rootPanel.RenderedManually)
		{
			rootPanel.RenderedManually = true;
		}

		if (
			SourcePanel.IsValid()
			&& SourcePanel.Panel.IsValid()
			&& SourcePanel.Panel.Parent != rootPanel
		)
		{
			SourcePanel.Panel.Parent = rootPanel;
		}

		var neededTextureDimension = Math.Min(Screen.Width, Screen.Height);
		var neededSourcePanelSize = (neededTextureDimension * 1080f / Screen.Height).Floor();

		if (SourcePanel.IsValid() && SourcePanel.Panel.IsValid())
		{
			if (SourcePanel.Panel.Parent != rootPanel)
			{
				SourcePanel.Panel.Parent = rootPanel;
			}

			SourcePanel.Panel.Style.Width = Length.Pixels(neededSourcePanelSize);
			SourcePanel.Panel.Style.Height = Length.Pixels(neededSourcePanelSize);
		}

		rootPanel.PanelBounds = new Rect(0, 0, neededTextureDimension, neededTextureDimension);

		if (texture is null || texture.Size != Vector2.One * neededTextureDimension)
		{
			CreateTexture(Vector2.One * neededTextureDimension);
			return;
		}

		Graphics.RenderTarget = RenderTarget.From(texture);
		Graphics.Attributes.SetCombo("D_WORLDPANEL", 0);
		Graphics.Viewport = new Rect(0, neededTextureDimension);
		Graphics.Clear(Color.Transparent);

		rootPanel.RenderManual(1f);

		Graphics.RenderTarget = null;
	}

	protected override void OnStart()
	{
		base.OnStart();
		customObject?.Delete();
		customObject = new SceneCustomObject(Scene.SceneWorld) { RenderOverride = Render };
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		customObject?.Delete();
		rootPanel?.Delete();
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();
		customObject?.Delete();
		customObject = new SceneCustomObject(Scene.SceneWorld) { RenderOverride = Render };
	}

	protected override void OnDisabled()
	{
		customObject?.Delete();
		customObject = null;
		if (SourcePanel.IsValid() && SourcePanel.Panel.IsValid())
		{
			SourcePanel.Panel.Parent = null;
		}
		rootPanel?.Delete();
		rootPanel = null;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (
			texture != null
			&& texture.IsLoaded
			&& TargetModelRenderer.IsValid()
			&& TargetModelRenderer.SceneObject.IsValid()
		)
		{
			TargetModelRenderer.SceneObject.Attributes.Set(MaterialAttributeName, texture);
		}
	}
}

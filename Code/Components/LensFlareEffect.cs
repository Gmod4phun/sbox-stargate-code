public class LensFlareEffect : PostProcess
{
	/*
	 * Lens Flare Post-Processing Effect
	 * By Mahalis (revised by Rush_Freak)
	 * original from GMod, adapted for S&Box by Gmod4phun
	*/

	// protected override Sandbox.Rendering.Stage RenderStage => Sandbox.Rendering.Stage.AfterUI;

	[Property, Range(0f, 1f)]
	public float Intensity { get; set; } = 1f;

	[Property]
	public bool DrawRing { get; set; } = true;

	[Property]
	public bool DrawFlare { get; set; } = true;

	[Property]
	public bool DrawBar { get; set; } = true;

	[Property]
	public bool DrawIris { get; set; } = true;

	// RenderAttributes attributes = new RenderAttributes();
	Material mat = Material.FromShader("ui_additive");
	Texture texIris = Texture.Load("materials/lensflare/iris.png");
	Texture texFlare = Texture.Load("materials/lensflare/flare.png");
	Texture texRing = Texture.Load("materials/lensflare/color_ring.png");
	Texture texBar = Texture.Load("materials/lensflare/bar.png");

	private float mulW(float x, float f)
	{
		return (x - Screen.Width / 2) * f + Screen.Width / 2;
	}

	private float mulH(float y, float f)
	{
		return (y - Screen.Height / 2) * f + Screen.Height / 2;
	}

	private void CenteredSprite(float x, float y, float sz, Color col)
	{
		var r = new Rect(x - sz / 2, y - sz / 2, sz, sz);
		CommandList.DrawQuad(r, mat, col);
	}

	public void DrawLensFlare()
	{
		var camera = Components.Get<CameraComponent>();
		if (camera == null || !camera.IsValid() || !camera.Enabled || !this.Enabled)
			return;

		var sun = Scene
			.GetAllComponents<DirectionalLight>()
			.Where(sun => MultiWorldSystem.AreObjectsInSameWorld(sun.GameObject, camera.GameObject))
			.FirstOrDefault();
		if (!sun.IsValid())
			return;

		var eyepos = camera.WorldPosition;
		var eyevector = camera.WorldRotation.Forward;
		var sundirection = sun.WorldRotation.Backward.Normal;
		var sunobstruction = 1f;

		var trace = Scene
			.Trace.Ray(eyepos, eyepos + sundirection * 128000)
			.WithWorld(camera.GetMultiWorld());

		var plyCameraController = camera.GameObject.Parent.Components.Get<PlayerCameraController>();
		if (plyCameraController.IsValid() && !plyCameraController.ThirdPerson)
		{
			trace = trace.IgnoreGameObjectHierarchy(
				plyCameraController.PlayerController.GameObject
			);
		}

		var tr = trace.Run();

		if (tr.Hit)
			sunobstruction = 0;

		var sunPos3D = eyepos + sundirection * camera.ZFar;
		var sunPos2D = camera.PointToScreenNormal(sunPos3D, out var _);

		var sunpos = sunPos2D * Screen.Size;

		var rSz = Screen.Width * 0.1f;
		var aMul = (float)
			((sundirection.Dot(eyevector) - 0.4f) * (1 - Math.Pow(1 - sunobstruction, 2))).Clamp(
				0f,
				1f
			);

		aMul *= Intensity;

		if (aMul == 0f)
			return;

		var aMulPow3 = (float)Math.Pow(aMul, 3);

		if (DrawRing)
		{
			CommandList.Attributes.Set("Texture", texRing);
			CenteredSprite(
				mulW(sunpos.x, 0.5f),
				mulH(sunpos.y, 0.5f),
				rSz * 8,
				Color.White.WithAlpha(80 * aMulPow3)
			);
		}

		if (DrawFlare)
		{
			CommandList.Attributes.Set("Texture", texFlare);
			var colorFlare = sun.LightColor;
			CenteredSprite(sunpos.x, sunpos.y, rSz * 10, colorFlare.WithAlpha(80 * aMulPow3));
		}

		if (DrawBar)
		{
			CommandList.Attributes.Set("Texture", texBar);
			CenteredSprite(
				mulW(sunpos.x, 1),
				mulH(sunpos.y, 1),
				rSz * 5,
				Color.White.WithAlpha(20f * aMul)
			);
		}

		if (DrawIris)
		{
			CommandList.Attributes.Set("Texture", texIris);
			var colorIris = Color.FromBytes(255, 230, 180, (int)(255 * aMulPow3));
			CenteredSprite(mulW(sunpos.x, 1.8f), mulH(sunpos.y, 1.8f), rSz * 0.15f, colorIris);
			CenteredSprite(mulW(sunpos.x, 1.82f), mulH(sunpos.y, 1.82f), rSz * 0.1f, colorIris);
			CenteredSprite(mulW(sunpos.x, 1.5f), mulH(sunpos.y, 1.5f), rSz * 0.05f, colorIris);
			CenteredSprite(mulW(sunpos.x, 0.6f), mulH(sunpos.y, 0.6f), rSz * 0.05f, colorIris);
			CenteredSprite(mulW(sunpos.x, 0.59f), mulH(sunpos.y, 0.59f), rSz * 0.15f, colorIris);
			CenteredSprite(mulW(sunpos.x, 0.3f), mulH(sunpos.y, 0.3f), rSz * 0.1f, colorIris);
			CenteredSprite(mulW(sunpos.x, -0.7f), mulH(sunpos.y, -0.7f), rSz * 0.1f, colorIris);
			CenteredSprite(mulW(sunpos.x, -0.72f), mulH(sunpos.y, -0.72f), rSz * 0.15f, colorIris);
			CenteredSprite(mulW(sunpos.x, -0.73f), mulH(sunpos.y, -0.73f), rSz * 0.05f, colorIris);
			CenteredSprite(mulW(sunpos.x, -0.9f), mulH(sunpos.y, -0.9f), rSz * 0.1f, colorIris);
			CenteredSprite(mulW(sunpos.x, -0.92f), mulH(sunpos.y, -0.92f), rSz * 0.05f, colorIris);
			CenteredSprite(mulW(sunpos.x, -1.3f), mulH(sunpos.y, -1.3f), rSz * 0.15f, colorIris);
			CenteredSprite(mulW(sunpos.x, -1.5f), mulH(sunpos.y, -1.5f), rSz * 1f, colorIris);
			CenteredSprite(mulW(sunpos.x, -1.7f), mulH(sunpos.y, -1.7f), rSz * 0.1f, colorIris);
		}
	}

	protected override void UpdateCommandList()
	{
		DrawLensFlare();
	}
}

using Sandbox.Rendering;

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
	public float SunObstruction { get; set; } = 0f;

	[Property]
	public bool DrawRing { get; set; } = true;

	[Property]
	public bool DrawFlare { get; set; } = true;

	[Property]
	public bool DrawBar { get; set; } = true;

	[Property]
	public bool DrawIris { get; set; } = true;

	Material mat = Material.FromShader("ui_additive");
	Texture texIris = Texture.Load("materials/lensflare/iris.png");
	Texture texFlare = Texture.Load("materials/lensflare/flare.png");
	Texture texRing = Texture.Load("materials/lensflare/color_ring.png");
	Texture texBar = Texture.Load("materials/lensflare/bar.png");

	float sunobstruction = 0f;

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
		sunobstruction = sunobstruction = sunobstruction.LerpTo(SunObstruction, Time.Delta * 60f);

		var sunPos3D = eyepos + sundirection * camera.ZFar;
		var sunPos2D = camera.PointToScreenNormal(sunPos3D, out var _);
		var sunpos = sunPos2D * Screen.Size;

		var rSz = Screen.Width * 0.1f;
		var aMul = (float)
			((sundirection.Dot(eyevector) - 0.4f) * (1 - Math.Pow(sunobstruction, 0.5f))).Clamp(
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

public class LensFlareOccluder : Component
{
	/*
	 * Occlusion for the lens flare effect
	 * Thanks to Peter Schraut for original Unity implementation
	 * https://github.com/pschraut/UnityOcclusionLensFlare
	*/

	ModelRenderer sunOccluderRenderer;

	CameraComponent occluderCamera;

	DirectionalLight Sun => Components.Get<DirectionalLight>(FindMode.EverythingInSelf);

	Texture occluderTexture;
	CommandList cmdList;

	SceneCustomObject mipMapGeneratorObject;

	CommandList GetCommandList()
	{
		CommandList cmdList = new CommandList("LensFlareOccluder");
		cmdList.DrawQuad(
			new Rect(0, 0, Screen.Width, Screen.Height),
			Material.UI.Basic,
			Color.Black
		);
		cmdList.DrawRenderer(sunOccluderRenderer);

		return cmdList;
	}

	private void GenerateOccluderTextureMipMaps(SceneObject sceneObject)
	{
		if (occluderTexture.IsValid())
		{
			Graphics.GenerateMipMaps(occluderTexture);
		}
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();

		mipMapGeneratorObject?.Delete();
		mipMapGeneratorObject = new SceneCustomObject(Scene.SceneWorld)
		{
			RenderOverride = GenerateOccluderTextureMipMaps
		};

		var sunOccluderRendererObject = new GameObject("LensFlareOccluderRenderer");
		sunOccluderRendererObject.SetParent(GameObject, false);

		sunOccluderRenderer = sunOccluderRendererObject.Components.GetOrCreate<ModelRenderer>(
			FindMode.EverythingInSelf
		);

		var occluderCameraObject = new GameObject("LensFlareOccluderCamera");
		occluderCameraObject.SetParent(GameObject, false);

		occluderCamera = occluderCameraObject.Components.Create<CameraComponent>();
		occluderCamera.IsMainCamera = false;

		if (!sunOccluderRenderer.IsValid() || !occluderCamera.IsValid())
			return;

		sunOccluderRenderer.Model = Model.Load("models/dev/sphere.vmdl");
		sunOccluderRenderer.MaterialOverride = Material.FromShader("lens_flare/sun_occluder");
		sunOccluderRenderer.RenderType = ModelRenderer.ShadowRenderType.Off;
		sunOccluderRenderer.RenderOptions.Game = false;

		occluderTexture?.Dispose();
		occluderTexture = Texture
			.CreateRenderTarget()
			.WithSize(32)
			.WithFormat(ImageFormat.A8)
			.WithDynamicUsage()
			.WithUAVBinding()
			.WithMips()
			.Create();

		cmdList = GetCommandList();

		occluderCamera.AddCommandList(cmdList, Stage.AfterPostProcess);

		occluderCamera.RenderTarget = occluderTexture;
		occluderCamera.BackgroundColor = Color.Black;
		occluderCamera.Enabled = false;
	}

	protected override void OnDisabled()
	{
		occluderCamera.ClearCommandLists();
		occluderCamera.GameObject.Destroy();

		occluderTexture?.Dispose();
		occluderTexture = null;

		sunOccluderRenderer.GameObject.Destroy();

		mipMapGeneratorObject?.Delete();
	}

	protected override void OnUpdate()
	{
		var cam = Scene.Camera;
		if (
			!occluderCamera.IsValid()
			|| !occluderTexture.IsValid()
			|| !Sun.IsValid()
			|| !cam.IsValid()
		)
			return;

		var inSameWorld = cam.GetMultiWorld() == occluderCamera.GetMultiWorld();

		occluderCamera.Enabled = inSameWorld;

		if (!inSameWorld)
			return;

		var sunDir = Sun.WorldRotation.Backward.Normal;
		var sunDistance = 10000f; // Arbitrary large distance for the sun occluder
		var sunScale = 8; // Scale factor for the sun occluder, might make this configurable later

		sunOccluderRenderer.WorldScale = sunScale;
		sunOccluderRenderer.WorldPosition = cam.WorldPosition + sunDir * sunDistance;

		// make us face the camera
		sunOccluderRenderer.WorldRotation = Rotation
			.LookAt(sunDir, Vector3.Up)
			.RotateAroundAxis(Vector3.Right, 90f);

		occluderCamera.RenderTags.RemoveAll();
		occluderCamera.RenderExcludeTags.RemoveAll();
		occluderCamera.RenderTags.Add(Scene.Camera.RenderTags);
		occluderCamera.RenderExcludeTags.Add(Scene.Camera.RenderExcludeTags);
		occluderCamera.RenderExcludeTags.Add("debugoverlay");
		occluderCamera.RenderExcludeTags.Add("skybox");

		// If sun is behind the camera, skip rendering
		if (sunDir.Dot(cam.WorldRotation.Backward.Normal) > 0.5f)
			return;

		// var sunBounds = sunOccluderRenderer.Bounds;
		// var sunDistance = (sunBounds.Center - cam.WorldTransform.Position).Length;
		// var sunDistance = cam.WorldPosition.Distance(sunOccluderRenderer.WorldPosition);
		// var sunHalfSize = MathF.Max(
		// 	sunBounds.Extents.x,
		// 	MathF.Max(sunBounds.Extents.y, sunBounds.Extents.z)
		// );
		var sunHalfSize = 32 * sunScale; // Assuming a fixed size for the sun occluder

		// Fit sun object into camera view
		// https://docs.unity3d.com/Manual/FrustumSizeAtDistance.html
		occluderCamera.ZNear = cam.ZNear;
		occluderCamera.ZFar = sunDistance + sunHalfSize;
		occluderCamera.FieldOfView =
			2.0f * MathX.RadianToDegree(MathF.Atan(sunHalfSize / sunDistance));
		occluderCamera.WorldPosition = cam.WorldPosition;
		occluderCamera.WorldRotation = Rotation.LookAt(
			sunOccluderRenderer.WorldPosition - cam.WorldPosition,
			Vector3.Up
		);

		var lensFlareEffect = cam.Components.Get<LensFlareEffect>();
		if (lensFlareEffect.IsValid())
		{
			var sampledLastMip = occluderTexture.GetPixel(0, 0, occluderTexture.Mips - 1).r / 255f;
			var obstruction = sampledLastMip.Remap(0f, 0.8f, 0f, 1f);
			lensFlareEffect.SunObstruction = obstruction;
		}

		// DebugOverlay.Texture(occluderTexture, new Rect(0, 0, 256, 256));
	}
}

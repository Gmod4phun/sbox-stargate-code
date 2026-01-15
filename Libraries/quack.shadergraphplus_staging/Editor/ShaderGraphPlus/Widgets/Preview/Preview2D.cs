using Editor;
using Sandbox.Rendering;
using System.Runtime.InteropServices;

namespace ShaderGraphPlus;

public class Throbber2D : SceneCustomObject
{
	private readonly Texture _texture;

	private bool _enabled;
	private RealTimeSince _timeSinceDisabled;
	public bool Enabled
	{
		set
		{
			_enabled = value;

			if ( !value )
			{
				_timeSinceDisabled = 0;
			}
		}
	}

	private Preview2D _preview;

	public Throbber2D( SceneWorld sceneWorld, Preview2D preview ) : base( sceneWorld )
	{
		_preview = preview;
		_texture = Texture.Load( "tools/images/common/busy.png", true );
		Bounds = BBox.FromPositionAndSize( Vector3.Zero, float.MaxValue );
	}

	public override void RenderSceneObject()
	{
		base.RenderSceneObject();

		if ( !_enabled && _timeSinceDisabled > 0.5f )
			return;

		var speed = 300;
		var delta = _enabled ? 0.0f : _timeSinceDisabled * 2.0f;
		var angle = RealTime.Now % (MathF.PI * (2.0f * speed));
		var dpiScale = _preview.DpiScale;

		var pos = new Vector2( _preview.Width - 39, 41 ) * dpiScale;
		Matrix mat = Matrix.CreateRotation( Rotation.From( 0, angle * speed, 0 ) );
		mat *= Matrix.CreateTranslation( pos );
		Graphics.Attributes.Set( "LayerMat", mat );

		Graphics.Attributes.Set( "Texture", _texture );
		Graphics.Attributes.SetComboEnum( "D_BLENDMODE", Sandbox.BlendMode.Normal );
		Graphics.DrawQuad( new Rect( -50, 100 ) * dpiScale, Material.UI.Basic, Color.Black.WithAlpha( 0.5f.LerpTo( 0.0f, delta ) ) );
		Graphics.Attributes.SetComboEnum( "D_BLENDMODE", Sandbox.BlendMode.Lighten );

		pos = new Vector2( _preview.Width - 40, 40 ) * dpiScale;
		mat = Matrix.CreateRotation( Rotation.From( 0, angle * speed, 0 ) );
		mat *= Matrix.CreateTranslation( pos );
		Graphics.Attributes.Set( "LayerMat", mat );
		Graphics.DrawQuad( new Rect( -50, 100 ) * dpiScale, Material.UI.Basic, _enabled ? Color.White : Color.White.WithAlpha( 1.0f.LerpTo( 0.0f, delta ) ) );
	}
}

public sealed class Preview2DPanel : Widget
{
	public MainWindow MainWindow { get; }
	private readonly Preview2D _preview2D;
	public Preview2D Preview2D => _preview2D;

	public Model Model
	{
		get => _preview2D.Model;
		set
		{
			if ( Model == value )
				return;

			_preview2D.Model = value;//?? _preview2d.QuadModelNoTiling;

			OnModelChanged?.Invoke( value );
		}
	}

	public Material Material
	{
		set => _preview2D.Material = value;
	}

	public Color Tint
	{
		set => _preview2D.Tint = value;
	}

	public bool IsCompiling
	{
		set
		{
			_preview2D.IsCompiling = value;
		}
	}

	public Action<Model> OnModelChanged { get; set; }

	public void SetAttribute( string id, in Float2x2 value ) // Stub - Quack
	{
		_preview2D.SetAttribute( id, value );
	}

	public void SetAttribute( string id, in Float3x3 value ) // Stub - Quack
	{
		_preview2D.SetAttribute( id, value );
	}
	public void SetAttribute( string id, in Float4x4 value ) // Stub - Quack
	{
		_preview2D.SetAttribute( id, value );
	}

	public void SetAttribute( string id, in SamplerState value )
	{
		_preview2D.SetAttribute( id, value );
	}

	public void SetAttribute( string id, in Texture value )
	{
		_preview2D.SetAttribute( id, value );
	}

	public void SetAttribute( string id, in Color value )
	{
		_preview2D.SetAttribute( id, value );
	}

	public void SetAttribute( string id, in Vector3 value )
	{
		_preview2D.SetAttribute( id, value );
	}

	public void SetAttribute( string id, in Vector2 value )
	{
		_preview2D.SetAttribute( id, value );
	}

	public void SetAttribute( string id, in float value )
	{
		_preview2D.SetAttribute( id, value );
	}

	public void SetAttribute( string id, in int value )
	{
		_preview2D.SetAttribute( id, value );
	}

	public void SetAttribute( string id, in bool value )
	{
		_preview2D.SetAttribute( id, value );
	}

	public void SetCombo( string id, bool value )
	{
		_preview2D.SetCombo( id, value );
	}

	public void SetCombo( string id, int value )
	{
		_preview2D.SetCombo( id, value );
	}

	public void SetStage( int value )
	{
		_preview2D.SetStage( value );
	}

	public void ClearAttributes()
	{
		_preview2D.ClearAttributes();
	}

	public Preview2DPanel( MainWindow mainWindow ) : base( null )
	{
		MainWindow = mainWindow;
		//Material = previewmaterial;

		Name = "Preview2DPanel";
		WindowTitle = "Preview 2D";
		SetWindowIcon( "photo" );

		MinimumSize = new Vector2( 256, 256 );

		Layout = Layout.Column();

		var toolBar = new ToolBar( this, "Preview2DToolBar" );
		toolBar.SetIconSize( 16 );
		// TODO : Fix up infinitetiling & crosstiling preview models so that the
		// material tiles properly and has no ugly seams.
		//toolBar.AddOption( null, "check_box_outline_blank", () => Model = Model.Load( "models/preview_material_quad.vmdl" ) ).ToolTip = "Toggle No Tiling";
		//toolBar.AddOption( null, "grid_on", () => Model = Model.Load( "models/preview_material_quad_infinitetiling.vmdl" ) ).ToolTip = "Toggle Infinite Tiling";
		//toolBar.AddOption( null, "add", () => Model = Model.Load( "models/preview_material_quad_crosstiling.vmdl" ) ).ToolTip = "Toggle Cross Tiling";

		toolBar.AddSeparator();

		_preview2D = new Preview2D( MainWindow, this, null );

		var combo = new Widget( toolBar );
		combo.Layout = Layout.Row();
		toolBar.AddWidget( combo );

		var stretcher = new Widget( toolBar );
		stretcher.Layout = Layout.Row();
		stretcher.Layout.AddStretchCell( 1 );
		toolBar.AddWidget( stretcher );

		toolBar.AddSeparator();

		var option = toolBar.AddOption( null, "preview" );
		option.Checkable = true;
		option.Toggled = ( e ) => _preview2D.EnableNodePreview = e;
		option.ToolTip = "Show a preview of a node's resulting texture when clicking on a node.";
		option.StatusTip = "Toggle Node Preview";

		toolBar.AddSeparator();

		toolBar.AddOption( null, "settings", OpenSettings );

		Layout.Add( toolBar );
		Layout.Add( _preview2D );

		SetSizeMode( SizeMode.Default, SizeMode.CanShrink );
	}


	private void OpenSettings()
	{
		var popup = new PopupWidget( this );
		popup.IsPopup = true;
		popup.Position = Editor.Application.CursorPosition;

		popup.Layout = Layout.Column();
		popup.Layout.Margin = 16;


		var cs = new ControlSheet();
		cs.AddProperty( _preview2D, x => x.ShowGrid );
		cs.AddProperty( _preview2D, x => x.GridSpacing );

		popup.Layout.Add( cs );

		popup.MinimumWidth = 300;
		popup.FixedWidth = 300;
		popup.OpenAtCursor();
	}

	public override void OnDestroyed()
	{
		base.OnDestroyed();
	}

	//protected override void OnPaint()
	//{
	//	base.OnPaint();
	//
	//	if ( Paint.HasMouseOver )
	//	{
	//		Paint.ClearPen();
	//		var bg = Theme.ControlBackground.Lighten( 0.3f );
	//		Paint.SetBrush( bg );
	//		Paint.DrawRect( LocalRect, Theme.ControlRadius );
	//	}
	//	//Paint.Draw( LocalRect.Shrink( 5 ), pixmap );
	//}
}

public class Preview2D : SceneRenderingWidget
{
	private SceneWorld _world => Scene.SceneWorld;

	public SceneObject TextureRect;
	public SceneModel _sceneObject;
	public Vector2 TextureSize;

	private Throbber2D _thobber;

	public Model QuadModelNoTiling { get; set; }
	public Model QuadModelInfiniteTiling { get; set; }
	public Model QuadModelCrossTiling { get; set; }
	//public Model DevPlane { get; set; }

	public bool ShowGrid { get; set; } = false;
	public float GridSpacing { get; set; } = 0.125f;

	float targetZoom = 115f;
	const float quadSize = 100f;

	private Dictionary<string, SamplerState> _samplerStateAttributes = new();
	private Dictionary<string, Texture> _textureAttributes = new();
	private Dictionary<string, Float2x2> _float2x2Attributes = new();
	private Dictionary<string, Float3x3> _float3x3Attributes = new();
	private Dictionary<string, Float4x4> _float4x4Attributes = new();
	private Dictionary<string, Color> _float4Attributes = new();
	private Dictionary<string, Vector3> _float3Attributes = new();
	private Dictionary<string, Vector2> _float2Attributes = new();
	private Dictionary<string, float> _intAttributes = new();
	private Dictionary<string, float> _floatAttributes = new();
	private Dictionary<string, bool> _boolAttributes = new();
	private Dictionary<string, bool> _comboBoolAttributes = new();
	private Dictionary<string, int> _comboIntAttributes = new();
	private int _stageId;

	private readonly Model _previewPlane = Model.Builder
	.AddMesh( CreateTessellatedPlane( 4, 0, 128 ) )
	.Create();

	private const int NoPreviewID = 0;
	private bool _enableNodePreview;
	public bool EnableNodePreview
	{
		get => _enableNodePreview;
		set
		{
			_enableNodePreview = value;

			if ( _sceneObject.IsValid() )
			{
				_sceneObject.Attributes.Set( "g_iStageId", _enableNodePreview ? _stageId : NoPreviewID );
			}
		}
	}

	private bool _renderBackfaces;
	public bool RenderBackfaces
	{
		get => _renderBackfaces;
		set
		{
			_renderBackfaces = value;

			if ( _sceneObject.IsValid() )
			{
				_sceneObject.Attributes.SetCombo( "D_RENDER_BACKFACES", _renderBackfaces );
			}
		}
	}

	public void UpdateMaterial()
	{
		var modelMaterial = _material;

		if ( _sceneObject is SceneModel sceneModel )
		{
			sceneModel.SetMaterialOverride( modelMaterial );
		}
	}

	private Texture _texture;
	public Texture Texture
	{
		get => _texture;
		set
		{
			_texture = value;
		}

	}

	private Material _material;
	public Material Material
	{
		get => _material;
		set
		{
			_material = value;
			UpdateMaterial();
		}
	}

	public Color BackgroundColor
	{
		get => Scene.Camera.IsValid() ? Scene.Camera.BackgroundColor : default;
		set
		{
			if ( !Scene.Camera.IsValid() )
				return;

			Scene.Camera.BackgroundColor = value;
		}
	}

	private Color _tint = Color.White;
	public Color Tint
	{
		get => _tint;
		set
		{
			_tint = value;
			if ( _sceneObject.IsValid() )
				_sceneObject.ColorTint = _tint;
		}
	}

	private Model _model;
	public Model Model
	{
		get => _model;
		set
		{
			_model = value;

			if ( _sceneObject.IsValid() )
			{
				_sceneObject.RenderingEnabled = false;
				_sceneObject.Delete();
			}

			_sceneObject = new SceneModel( _world, value, Transform.Zero )
			{
				ColorTint = Tint,
				Batchable = false
			};

			_sceneObject.Update( 1 );

			UpdateMaterial();

			foreach ( var samplerState in _samplerStateAttributes )
			{
				_sceneObject.Attributes.Set( samplerState.Key, samplerState.Value );
			}

			foreach ( var texture in _textureAttributes )
			{
				_sceneObject.Attributes.Set( texture.Key, texture.Value );
			}

			foreach ( var v in _float4Attributes )
			{
				_sceneObject.Attributes.Set( v.Key, v.Value );
			}

			foreach ( var v in _float3Attributes )
			{
				_sceneObject.Attributes.Set( v.Key, v.Value );
			}

			foreach ( var v in _float2Attributes )
			{
				_sceneObject.Attributes.Set( v.Key, v.Value );
			}

			foreach ( var v in _floatAttributes )
			{
				_sceneObject.Attributes.Set( v.Key, v.Value );
			}

			foreach ( var v in _intAttributes )
			{
				_sceneObject.Attributes.Set( v.Key, v.Value );
			}

			foreach ( var v in _boolAttributes )
			{
				_sceneObject.Attributes.Set( v.Key, v.Value );
			}

			foreach ( var v in _comboBoolAttributes )
			{
				_sceneObject.Attributes.SetCombo( v.Key, v.Value );
			}

			if ( _enableNodePreview )
			{
				_sceneObject.Attributes.Set( "g_iStageId", _stageId );
			}
			else
			{
				_sceneObject.Attributes.Set( "g_iStageId", NoPreviewID );
			}

			_sceneObject.Attributes.SetCombo( "D_RENDER_BACKFACES", _renderBackfaces );
		}
	}

	public void SetAttribute( string id, Float2x2 value ) // Stub - Quack
	{
		_float2x2Attributes.Add( id, value );
		_sceneObject.Attributes.SetData( id, value );
	}

	public void SetAttribute( string id, Float3x3 value ) // Stub - Quack
	{
		_float3x3Attributes.Add( id, value );
		_sceneObject.Attributes.SetData( id, value );
	}

	public void SetAttribute( string id, Float4x4 value ) // Stub - Quack
	{
		_float4x4Attributes.Add( id, value );
		_sceneObject.Attributes.SetData( id, value );
	}

	public void SetAttribute( string id, SamplerState value )
	{
		//if ( _samplerStateAttributes.ContainsKey( id ) )
		//	_samplerStateAttributes.Remove( id );
		_samplerStateAttributes.Add( id, value );
		_sceneObject.Attributes.Set( id, value );
	}

	public void SetAttribute( string id, Texture value )
	{
		_textureAttributes.Add( id, value );
		_sceneObject.Attributes.Set( id, value );
	}

	public void SetAttribute( string id, Color value )
	{
		_float4Attributes.Add( id, value );
		_sceneObject.Attributes.Set( id, value );
	}

	public void SetAttribute( string id, Vector3 value )
	{
		_float3Attributes.Add( id, value );
		_sceneObject.Attributes.Set( id, value );
	}

	public void SetAttribute( string id, Vector2 value )
	{
		_float2Attributes.Add( id, value );
		_sceneObject.Attributes.Set( id, value );
	}

	public void SetAttribute( string id, float value )
	{
		_floatAttributes.Add( id, value );
		_sceneObject.Attributes.Set( id, value );
	}

	public void SetAttribute( string id, int value )
	{
		_intAttributes.Add( id, value );
		_sceneObject.Attributes.Set( id, value );
	}

	public void SetAttribute( string id, in bool value )
	{
		_boolAttributes.Add( id, value );
		_sceneObject.Attributes.Set( id, value );
	}

	public void SetCombo( string id, bool value )
	{
		//SGPLog.Info( $"Setting DynamicCombo `{id}` to `{value}`" );

		if ( !_comboBoolAttributes.ContainsKey( id ) )
		{
			_comboBoolAttributes.Add( id, value );
			_sceneObject.Attributes.SetCombo( id, value );
		}
	}

	public void SetCombo( string id, int value )
	{
		//SGPLog.Info( $"Setting DynamicCombo `{id}` to `{value}`" );

		if ( !_comboIntAttributes.ContainsKey( id ) )
		{
			_comboIntAttributes.Add( id, value );
			_sceneObject.Attributes.SetCombo( id, value );
		}
	}

	public void SetStage( int value )
	{
		_stageId = value;

		if ( _sceneObject.IsValid() )
		{
			_sceneObject.Attributes.Set( "g_iStageId", _enableNodePreview ? _stageId : NoPreviewID );
		}
	}

	public void ClearAttributes()
	{
		_samplerStateAttributes.Clear();
		_textureAttributes.Clear();
		_float2x2Attributes.Clear();
		_float3x3Attributes.Clear();
		_float4x4Attributes.Clear();
		_float4Attributes.Clear();
		_float3Attributes.Clear();
		_float2Attributes.Clear();
		_intAttributes.Clear();
		_floatAttributes.Clear();
		_boolAttributes.Clear();
		_comboBoolAttributes.Clear();
		_comboIntAttributes.Clear();

		if ( _sceneObject.IsValid() )
		{
			_sceneObject.Attributes.Clear();
		}

	}

	public bool IsCompiling
	{
		set
		{
			_thobber.Enabled = value;
		}
	}

	private readonly SkyBox2D _sky;
	private bool _dragging;
	private Vector2 lastMouseScenePosition;
	private MainWindow _mainWindow;

	public Preview2D( MainWindow window, Widget parent, string model ) : base( parent )
	{
		MouseTracking = true;
		FocusMode = FocusMode.Click;

		_mainWindow = window;

		Scene = Scene.CreateEditorScene();

		using ( Scene.Push() )
		{
			{
				var camera = new GameObject( true, "camera" ).GetOrAddComponent<CameraComponent>();
				camera.BackgroundColor = Color.White;
			}
			{
				var sun = new GameObject( true, "sun" ).GetOrAddComponent<DirectionalLight>();
				sun.WorldRotation = Rotation.FromPitch( 50 );
				sun.LightColor = Color.White * 2.5f + Color.Cyan * 0.05f;
			}
			{
				var light = new GameObject( true, "light" ).GetOrAddComponent<PointLight>( false );
				light.WorldPosition = 100;
				light.Radius = 500;
				light.LightColor = Color.Orange * 3;
				light.Enabled = true;
			}
			{
				var light = new GameObject( true, "light" ).GetOrAddComponent<PointLight>( false );
				light.WorldPosition = -100;
				light.Radius = 500;
				light.LightColor = Color.Cyan * 3;
				light.Enabled = true;
			}
			{
				var cubemap = new GameObject( true, "cubemap" ).GetOrAddComponent<EnvmapProbe>();
				cubemap.Texture = Texture.Load( "textures/cubemaps/default2.vtex" );
			}
			{
				_sky = new GameObject( true, "sky" ).GetOrAddComponent<SkyBox2D>();

			}
		}

		Scene.Camera.WorldRotation = new Angles( 90, 180, 0 );
		Scene.Camera.WorldPosition = new Vector3( 0, 0, targetZoom );
		Scene.Camera.FieldOfView = 30;
		Scene.Camera.Orthographic = true;
		Scene.Camera.OrthographicHeight = 512f;
		Scene.Camera.BackgroundColor = Theme.ControlBackground;

		_sky.Enabled = false;

		QuadModelNoTiling = Model.Load( "models/preview_material_quad.vmdl" );
		QuadModelInfiniteTiling = Model.Load( "models/preview_material_quad_infinitetiling.vmdl" );
		QuadModelCrossTiling = Model.Load( "models/preview_material_quad_crosstiling.vmdl" );

		_thobber = new Throbber2D( _world, this );

		_material = Material.Load( "materials/core/shader_editor.vmat" );
		Model = string.IsNullOrWhiteSpace( model ) ? QuadModelNoTiling : Model.Load( model );

		_sceneObject.Position = new Vector3( 0, 0, 0 );
	}

	// Application.CursorPosition is fucked for different DPI
	private static Vector2 CursorPosition => Editor.Application.UnscaledCursorPosition;

	public override void OnDestroyed()
	{
		base.OnDestroyed();

		Scene?.Destroy();
		Scene = null;
	}

	protected override void OnWheel( WheelEvent e )
	{
		base.OnWheel( e );
		Zoom( e.Delta );
	}

	public void Zoom( float delta )
	{
		targetZoom *= 1f - (delta / 500f);
		targetZoom = targetZoom.Clamp( 1, 1000 );
	}

	//public void Fit()
	//{
	//	targetZoom = 115f;
	//	Camera.Position = new Vector3( 0, 0, targetZoom );
	//}

	protected override void OnMousePress( MouseEvent e )
	{
		base.OnMousePress( e );


		if ( e.LeftMouseButton )
		{
			_dragging = true;
		}
	}

	protected override void OnMouseReleased( MouseEvent e )
	{
		base.OnMouseReleased( e );

		_dragging = false;
	}

	protected override void OnMouseMove( MouseEvent e )
	{

		if ( !_dragging )
		{

			_dragging = false;
		}
		else
		{
			var cursorLocalPos = e.ScreenPosition - ScreenRect.Position;
			lastMouseScenePosition = cursorLocalPos;
		}


		{
			Cursor = CursorShape.None;
		}

		e.Accepted = true;

	}

	protected override void PreFrame()
	{
		Scene.EditorTick( RealTime.Now, RealTime.Delta );

		Scene.Camera.OrthographicHeight = Scene.Camera.OrthographicHeight.LerpTo( targetZoom, 0.1f );

		if ( _dragging )
		{
			//Log.Info( lastMouseScenePosition );
		}

		if ( ShowGrid )
		{
			GizmoInstance.Settings.ViewMode = "2d";

			Gizmo.Draw.Grid( _sceneObject.Position, Gizmo.GridAxis.XY, opacity: 0.5f, spacing: GridSpacing );
		}

		_sceneObject.Update( RealTime.Delta );
		_sceneObject.Attributes.Set( "g_flPreviewTime", RealTime.Now );
	}

	static Mesh CreateTessellatedPlane( int facetsPerAxis, Vector3 center, Vector2 size )
	{
		var material = Material.Load( "materials/dev/gray_grid_8.vmat" );
		var mesh = new Mesh( material );

		var vertexCount = (facetsPerAxis + 1) * (facetsPerAxis + 1);
		var indexCount = facetsPerAxis * facetsPerAxis * 6;

		var vertices = new MaterialVertex[vertexCount];
		var indices = new int[indexCount];

		var stepU = 1.0f / facetsPerAxis;
		var stepV = 1.0f / facetsPerAxis;
		var stepX = size.x / facetsPerAxis;
		var stepY = size.y / facetsPerAxis;

		var vertexIndex = 0;
		for ( var i = 0; i <= facetsPerAxis; i++ )
		{
			for ( var j = 0; j <= facetsPerAxis; j++ )
			{
				var u = j * stepU;
				var v = i * stepV;

				var texcoord = new Vector2( v, u );
				var position = new Vector3(
					center.x + (j * stepX - size.x / 2),
					center.y + (i * stepY - size.y / 2),
					center.z );

				vertices[vertexIndex] = new MaterialVertex( position, new Vector4( 0, 0, 1, 1 ), new Vector4( 1, 0, 0, -1 ), texcoord );
				vertexIndex++;
			}
		}

		var index = 0;
		for ( var i = 0; i < facetsPerAxis; i++ )
		{
			for ( var j = 0; j < facetsPerAxis; j++ )
			{
				var topLeft = i * (facetsPerAxis + 1) + j;
				var topRight = topLeft + 1;
				var bottomLeft = topLeft + (facetsPerAxis + 1);
				var bottomRight = bottomLeft + 1;

				indices[index++] = topRight;
				indices[index++] = bottomRight;
				indices[index++] = topLeft;

				indices[index++] = bottomLeft;
				indices[index++] = topLeft;
				indices[index++] = bottomRight;
			}
		}

		mesh.CreateVertexBuffer<MaterialVertex>( vertices.Length, MaterialVertex.Layout, vertices );
		mesh.CreateIndexBuffer( indices.Length, indices );

		mesh.Bounds = BBox.FromPositionAndSize( center, new Vector3( size.x, size.y, 0 ) );

		return mesh;
	}

	[StructLayout( LayoutKind.Sequential )]
	struct MaterialVertex
	{
		public MaterialVertex( Vector3 position, Vector4 normal, Vector4 tangent, Vector2 texcoord )
		{
			this.position = position;
			this.normal = normal;
			this.tangent = tangent;
			this.texcoord = texcoord;

			blend = new Vector4( 0.0f, 0.0f, 0.0f, 0.0f );
			var v = texcoord.y.Clamp( 0.0f, 1.0f );

			if ( v <= 0.2f ) blend.w = 1.0f;
			else if ( v <= 0.4f ) blend.z = 1.0f;
			else if ( v <= 0.6f ) blend.y = 1.0f;
			else if ( v <= 0.8f ) blend.x = 1.0f;
		}

		public Vector3 position;
		public Vector4 normal;
		public Vector4 tangent;
		public Vector2 texcoord;
		public Vector4 blend;

		public static readonly VertexAttribute[] Layout =
		{
			new ( VertexAttributeType.Position, VertexAttributeFormat.Float32, 3 ),
			new ( VertexAttributeType.Normal, VertexAttributeFormat.Float32, 4 ),
			new ( VertexAttributeType.Tangent, VertexAttributeFormat.Float32, 4 ),
			new ( VertexAttributeType.TexCoord, VertexAttributeFormat.Float32, 2 ),
			new ( VertexAttributeType.TexCoord, VertexAttributeFormat.Float32, 4, 4 )
		};
	}

}

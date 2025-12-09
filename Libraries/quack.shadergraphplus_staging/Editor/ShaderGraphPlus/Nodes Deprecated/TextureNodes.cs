using Editor;
using NodeEditorPlus;

namespace ShaderGraphPlus.Nodes;

public abstract class TextureSamplerBase : ShaderNodePlus, ITextureInputNode, ITextureParameterNode, IErroringNode
{
	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.FunctionNode;

	[Hide]
	protected bool IsSubgraph => (Graph is ShaderGraphPlus shaderGraph && shaderGraph.IsSubgraph);

	[Hide]
	protected bool ShowUIProperty
	{
		get
		{
			if ( IsSubgraph )
				return false;

			if ( IsTextureObjectConnected )
				return false;

			return true;
		}
	}


	#region ITextureInputNode
	[JsonIgnore, Hide, Browsable( false )]
	public string TextureInputName => UI.Name;

	[JsonIgnore, Hide, Browsable( false )]
	public bool AlreadyRegisterd { get; set; } = false;
	#endregion

	[JsonIgnore, Hide]
	protected bool IsTextureObjectConnected { get; set; } = false;

	/// <summary>
	/// Texture to sample in preview
	/// </summary>
	[ImageAssetPath]
	[ShowIf( nameof( ShowUIProperty ), true )]
	public string Image
	{
		get => _image;
		set
		{
			_image = value;
			_asset = AssetSystem.FindByPath( _image );
			if ( _asset == null )
				return;

			CompileTexture();
		}
	}

	[Hide]
	public override string Title => string.IsNullOrWhiteSpace( UI.Name ) ? null : $"{DisplayInfo.For( this ).Name} {UI.Name}";

	private Asset _asset;
	private string _texture;
	private string _image;
	private string _resourceText;

	[JsonIgnore, Hide]
	protected Asset Asset => _asset;

	[JsonIgnore, Hide]
	protected string TexturePath => _texture;

	protected void CompileTexture()
	{
		if ( _asset == null )
			return;

		if ( string.IsNullOrWhiteSpace( _image ) )
			return;

		var ui = UI;
		ui.DefaultTexture = _image;
		UI = ui;

		var resourceText = string.Format( ShaderTemplate.TextureDefinition,
			_image,
			UI.ColorSpace,
			UI.ImageFormat,
			UI.Processor );

		if ( _resourceText == resourceText )
			return;

		_resourceText = resourceText;

		var assetPath = $"shadergraphplus/{_image.Replace( ".", "_" )}_shadergraphplus.generated.vtex";
		var resourcePath = Editor.FileSystem.Root.GetFullPath( "/.source2/temp" );
		resourcePath = System.IO.Path.Combine( resourcePath, assetPath );

		if ( AssetSystem.CompileResource( resourcePath, resourceText ) )
		{
			_texture = assetPath;
		}
		else
		{
			Log.Warning( $"Failed to compile {_image}" );
		}
	}

	/// <summary>
	/// Settings for how this texture shows up in material editor
	/// </summary>
	[InlineEditor( Label = false ), Group( "UI" )]
	[ShowIf( nameof( ShowUIProperty ), true )]
	public TextureInput UI { get; set; } = new TextureInput
	{
		ImageFormat = TextureFormat.DXT5,
		SrgbRead = true,
		DefaultColor = Color.White,
	};

	protected TextureSamplerBase() : base()
	{
		Image = "materials/default/default.tga";
		ExpandSize = new Vector2( 0, 8 + Inputs.Count() * 24 );
	}

	public override void OnPaint( Rect rect )
	{
		rect = rect.Align( 130, TextFlag.LeftBottom ).Shrink( 3 );

		Paint.SetBrush( "/image/transparent-small.png" );
		Paint.DrawRect( rect.Shrink( 2 ), 2 );

		Paint.SetBrush( Theme.ControlBackground.WithAlpha( 0.7f ) );
		Paint.DrawRect( rect, 2 );

		if ( Asset != null )
		{
			Paint.Draw( rect.Shrink( 2 ), Asset.GetAssetThumb( true ) );
		}
	}

	public List<string> GetErrors()
	{
		var errors = new List<string>();

		//if ( !IsTextureObjectConnected )
		//{
		//	if ( Graph is ShaderGraphPlus sgp && sgp.IsSubgraph )
		//	{
		//		if ( string.IsNullOrWhiteSpace( UI.Name ) )
		//		{
		//			errors.Add( $"Texture parameter \"{DisplayInfo.For( this ).Name}\" is missing a name" );
		//		}
		//
		//		foreach ( var node in sgp.Nodes )
		//		{
		//			if ( node is ITextureParameterNode tpn && tpn != this && tpn.UI.Name == UI.Name )
		//			{
		//				errors.Add( $"Duplicate texture parameter name \"{UI.Name}\" on {DisplayInfo.For( this ).Name}" );
		//			}
		//		}
		//	}
		//}

		return errors;
	}

	protected bool CheckIfRegisterd( GraphCompiler compiler, TextureInput input, out KeyValuePair<string, TextureInput> existingEntry )
	{
		existingEntry = new();

		if ( AlreadyRegisterd && compiler.IsPreview )
		{
			existingEntry = compiler.GetExistingTextureInputEntry( input.Name );
			return true;
		}

		return false;
	}

	protected NodeResult Component( string component, GraphCompiler compiler )
	{
		var result = compiler.Result( new NodeInput { Identifier = Identifier, Output = nameof( Result ) } );
		return result.IsValid ? new( ResultType.Float, $"{result}.{component}", true ) : new( ResultType.Float, "0.0f", true );
	}
}

/// <summary>
/// Sample a 2D Texture
/// </summary>
[Title( "Texture 2D" ), Category( "Textures" ), Icon( "image" )]
[InternalNode]
public sealed class TextureSampler : TextureSamplerBase
{
	[Hide]
	public override int Version => 1;

	/// <summary>
	/// Coordinates to sample this texture (Defaults to vertex coordinates)
	/// </summary>
	[Title( "Coordinates" )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Coords { get; set; }

	/// <summary>
	/// How the texture is filtered and wrapped when sampled
	/// </summary>
	[Title( "Sampler" )]
	[Input( typeof( Sampler ) )]
	[Hide]
	public NodeInput Sampler { get; set; }

	/// <summary>
	/// Optional Texture2D Object input when outside of subgraphs.
	/// </summary>
	[Title( "Tex 2D" )]
	[Input( typeof( Texture2DObject ) )]
	[Hide]
	public NodeInput TextureObject { get; set; }

	[InlineEditor( Label = false ), Group( "Sampler" )]
	[HideIf( nameof( IsSubgraph ), true )]
	public Sampler SamplerState { get; set; } = new Sampler();

	/// <summary>
	/// RGBA color result
	/// </summary>
	[Hide]
	[Output( typeof( Color ) ), Title( "RGBA" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var input = UI;
		input.Type = TextureType.Tex2D;
		input.BoundNode = $"{Title}, ID:{Identifier}";
		input.BoundNodeId = $"{Identifier}";

		var textureObject = compiler.Result( TextureObject );

		var coords = compiler.Result( Coords );

		//SGPLog.Info( $"IsSubgraph? : {IsSubgraph}" , compiler.IsPreview);

		if ( textureObject.IsValid )
		{
			IsTextureObjectConnected = true;
		}
		else
		{
			//Image = "";
			IsTextureObjectConnected = false;
		}

		if ( !IsSubgraph )
			CompileTexture();

		// If TextureObject input is not valid and we are not in a SubGraph then register a texture here and use that instead.
		if ( !textureObject.IsValid && !IsSubgraph )
		{
			SGPLog.Info( $"Registering Texture 2D Object on the `{nameof( TextureSampler )}` node `{Identifier}`.", compiler.IsPreview && ConCommands.TextureNodeDebug );

			var texture = string.IsNullOrWhiteSpace( TexturePath ) ? null : Texture.Load( TexturePath );
			texture ??= Texture.White;

			// TODO :
			//if ( AlreadyRegisterd && compiler.IsPreview )
			//{
			//	var existingEntry = compiler.GetExistingTextureInputEntry( input.Name );
			//	return NodeResult.Error( $"`{input.Name}` was already registerd by node `{existingEntry.Value.BoundNode}`" );
			//}
			var samplerGlobal = compiler.ResultSamplerOrDefault( Sampler, SamplerState );
			var resultTextureGlobal = compiler.ResultTexture( input, texture );

			if ( compiler.Stage == GraphCompiler.ShaderStage.Vertex )
			{
				return new NodeResult( ResultType.Color, $"{resultTextureGlobal}.SampleLevel(" +
					$" {samplerGlobal}," +
					$" {(coords.IsValid ? $"{coords.Cast( 2 )}" : "i.vTextureCoords.xy")}, 0 )" );
			}
			else
			{
				return new NodeResult( ResultType.Color, $"{resultTextureGlobal}.Sample( {samplerGlobal}," +
					$"{(coords.IsValid ? $"{coords.Cast( 2 )}" : "i.vTextureCoords.xy")} )" );
			}
		}

		// Make sure to let the user know that the requied input is missing if we are in in a SubGraph.
		if ( !textureObject.IsValid && IsSubgraph )
		{
			return NodeResult.MissingInput( $"Tex 2D" );
		}

		// If TextureObject input is valid then use the registerd Texture2D from the connected Texture Object node.
		// Either if the textureObject input is valid or we are in a Subgraph.
		if ( textureObject.IsValid || (IsSubgraph && textureObject.IsValid) )
		{
			SGPLog.Info( $"Using Texture 2D Object `{textureObject.Code}` from TextureObject input on the `{nameof( TextureSampler )}` node `{Identifier}`.", compiler.IsPreview && ConCommands.TextureNodeDebug );
			var sampler = compiler.ResultSamplerOrDefault( Sampler, SamplerState );
			if ( compiler.Stage == GraphCompiler.ShaderStage.Vertex )
			{
				return new NodeResult( ResultType.Color, $"{textureObject.Code}.SampleLevel(" +
					$" {sampler}," +
					$" {(coords.IsValid ? $"{coords.Cast( 2 )}" : "i.vTextureCoords.xy")}, 0 )" );
			}
			else
			{
				return new NodeResult( ResultType.Color, $"{textureObject.Code}.Sample( {sampler}," +
					$"{(coords.IsValid ? $"{coords.Cast( 2 )}" : "i.vTextureCoords.xy")} )" );
			}
		}

		return NodeResult.Error( "Failed to evaluate!" );
	};

	/// <summary>
	/// Red component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "R" )]
	public NodeResult.Func R => ( GraphCompiler compiler ) => Component( "r", compiler );

	/// <summary>
	/// Green component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "G" )]
	public NodeResult.Func G => ( GraphCompiler compiler ) => Component( "g", compiler );

	/// <summary>
	/// Blue component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "B" )]
	public NodeResult.Func B => ( GraphCompiler compiler ) => Component( "b", compiler );

	/// <summary>
	/// Alpha (Opacity) component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "A" )]
	public NodeResult.Func A => ( GraphCompiler compiler ) => Component( "a", compiler );

}

/// <summary>
/// Sample a 2D texture from 3 directions, then blend based on a normal vector.
/// </summary>
[Title( "Texture Triplanar" ), Category( "Textures" ), Icon( "photo_library" )]
[InternalNode]
public sealed class TextureTriplanar : TextureSamplerBase
{
	[Hide]
	public override int Version => 1;

	/// <summary>
	/// Coordinates to sample this texture (Defaults to vertex position)
	/// </summary>
	[Title( "Coordinates" )]
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput Coords { get; set; }

	/// <summary>
	/// How the texture is filtered and wrapped when sampled
	/// </summary>
	[Title( "Sampler" )]
	[Input( typeof( Sampler ) )]
	[Hide]
	public NodeInput Sampler { get; set; }

	/// <summary>
	/// Optional Texture2D Object input when outside of subgraphs.
	/// </summary>
	[Title( "Tex 2D" )]
	[Input( typeof( Texture2DObject ) )]
	[Hide]
	public NodeInput TextureObject { get; set; }

	/// <summary>
	/// Normal to use when blending between each sampled direction (Defaults to vertex normal)
	/// </summary>
	[Title( "Normal" )]
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput Normal { get; set; }

	/// <summary>
	/// How many times to file the coordinates.
	/// </summary>
	[Title( "Tile" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Tile { get; set; }

	/// <summary>
	/// Blend factor between different samples.
	/// </summary>
	[Title( "Blend Factor" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput BlendFactor { get; set; }

	[InlineEditor( Label = false ), Group( "Sampler" )]
	[HideIf( nameof( IsSubgraph ), true )]
	public Sampler SamplerState { get; set; } = new Sampler();

	public float DefaultTile { get; set; } = 1.0f;

	public float DefaultBlendFactor { get; set; } = 4.0f;
	public TextureTriplanar()
	{
		Image = "materials/default/default.tga";
		ExpandSize = new Vector2( 0f, 128f );
	}

	/// <summary>
	/// RGBA color result
	/// </summary>
	[Hide]
	[Output( typeof( Color ) ), Title( "RGBA" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var input = UI;
		input.Type = TextureType.Tex2D;
		input.BoundNode = $"{Title}, ID:{Identifier}";
		input.BoundNodeId = $"{Identifier}";

		var textureObject = compiler.Result( TextureObject );
		var coords = compiler.Result( Coords );
		var samplerGlobal = compiler.ResultSamplerOrDefault( Sampler, SamplerState );
		var tile = compiler.ResultOrDefault( Tile, DefaultTile );
		var normal = compiler.Result( Normal );
		var blendfactor = compiler.ResultOrDefault( BlendFactor, DefaultBlendFactor );

		if ( textureObject.IsValid )
		{
			IsTextureObjectConnected = true;
		}
		else
		{
			//Image = "";
			IsTextureObjectConnected = false;
		}

		CompileTexture();

		if ( CheckIfRegisterd( compiler, input, out var existingEntry ) )
		{
			return NodeResult.Error( $"`{input.Name}` was already registerd by node `{existingEntry.Value.BoundNode}`" );
		}

		// If TextureObject input is not valid and we are not in a SubGraph then register the texture here instead.
		if ( !textureObject.IsValid && !IsSubgraph )
		{
			SGPLog.Info( $"Registering Texture 2D Object on the `{nameof( TextureTriplanar )}` node `{Identifier}`.", compiler.IsPreview && ConCommands.TextureNodeDebug );

			var texture = string.IsNullOrWhiteSpace( TexturePath ) ? null : Texture.Load( TexturePath );
			texture ??= Texture.White;

			var resultTextureGlobal = compiler.ResultTexture( input, texture );

			var result = compiler.ResultHLSLFunction( "TexTriplanar_Color",
			resultTextureGlobal,
			samplerGlobal,
			coords.IsValid ? coords.Cast( 3 ) : "(i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz) / 39.3701",
			normal.IsValid ? normal.Cast( 3 ) : "normalize( i.vNormalWs.xyz )",
			$"{blendfactor}"
			);

			return new NodeResult( ResultType.Color, result );
		}

		// Make sure to let the user know that the requied input is missing if we are in in a SubGraph.
		if ( !textureObject.IsValid && IsSubgraph )
		{
			return NodeResult.MissingInput( $"Tex2D Object" );
		}

		// If TextureObject input is valid then use the registerd Texture2D from the connected Texture Object node.
		// Either if the textureObject input is valid or we are in a Subgraph.
		if ( textureObject.IsValid || (IsSubgraph && textureObject.IsValid) )
		{
			SGPLog.Info( $"Using Texture 2D Object `{textureObject.Code}` from TextureObject input on the `{nameof( TextureTriplanar )}` node `{Identifier}`.", compiler.IsPreview && ConCommands.TextureNodeDebug );

			var result = compiler.ResultHLSLFunction( "TexTriplanar_Color",
					$"{textureObject.Code}",
					$"{samplerGlobal}",
					coords.IsValid ? coords.Cast( 3 ) : "(i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz) / 39.3701",
					normal.IsValid ? normal.Cast( 3 ) : "normalize( i.vNormalWs.xyz )",
					$"{blendfactor}"
				);

			return new NodeResult( ResultType.Color, result );
		}

		return NodeResult.Error( "Failed to evaluate!" );
	};

	/// <summary>
	/// Red component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "R" )]
	public NodeResult.Func R => ( GraphCompiler compiler ) => Component( "r", compiler );

	/// <summary>
	/// Green component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "G" )]
	public NodeResult.Func G => ( GraphCompiler compiler ) => Component( "g", compiler );

	/// <summary>
	/// Blue component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "B" )]
	public NodeResult.Func B => ( GraphCompiler compiler ) => Component( "b", compiler );

	/// <summary>
	/// Alpha (Opacity) component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "A" )]
	public NodeResult.Func A => ( GraphCompiler compiler ) => Component( "a", compiler );

}

/// <summary>
/// Sample a 2D texture from 3 directions, then blend based on a normal vector.
/// </summary>
[Title( "Normal Map Triplanar" ), Category( "Textures" ), Icon( "texture" )]
[InternalNode]
public sealed class NormalMapTriplanar : TextureSamplerBase
{
	[Hide]
	public override int Version => 1;

	/// <summary>
	/// Coordinates to sample this texture (Defaults to vertex position)
	/// </summary>
	[Title( "Coordinates" )]
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput Coords { get; set; }

	/// <summary>
	/// How the texture is filtered and wrapped when sampled
	/// </summary>
	[Title( "Sampler" )]
	[Input( typeof( Sampler ) )]
	[Hide]
	public NodeInput Sampler { get; set; }

	/// <summary>
	/// Optional Texture2D Object input when outside of subgraphs.
	/// </summary>
	[Title( "Tex 2D" )]
	[Input( typeof( Texture2DObject ) )]
	[Hide]
	public NodeInput TextureObject { get; set; }

	/// <summary>
	/// Normal to use when blending between each sampled direction (Defaults to vertex normal)
	/// </summary>
	[Title( "Normal" )]
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput Normal { get; set; }

	public NormalMapTriplanar()
	{
		ExpandSize = new Vector2( 0f, 128f );
		Image = "materials/default/default.tga";
		UI = new TextureInput
		{
			ImageFormat = TextureFormat.DXT5,
			SrgbRead = false,
			ColorSpace = TextureColorSpace.Linear,
			Extension = TextureExtension.Normal,
			Processor = TextureProcessor.NormalizeNormals,
			DefaultColor = new Color( 0.5f, 0.5f, 1f, 1f )
		};
	}

	/// <summary>
	/// How many times to file the coordinates.
	/// </summary>
	[Title( "Tile" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Tile { get; set; }

	/// <summary>
	/// Blend factor between different samples.
	/// </summary>
	[Title( "Blend Factor" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput BlendFactor { get; set; }

	//[InlineEditor( Label = false ), Group( "Sampler" )]
	//public Sampler DefaultSampler { get; set; } = new Sampler();

	[InlineEditor( Label = false ), Group( "Sampler" )]
	[HideIf( nameof( IsSubgraph ), true )]
	public Sampler SamplerState { get; set; } = new Sampler();

	public float DefaultTile { get; set; } = 1.0f;

	public float DefaultBlendFactor { get; set; } = 4.0f;

	/// <summary>
	/// RGBA color result
	/// </summary>
	[Hide]
	[Output( typeof( Vector3 ) ), Title( "XYZ" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var input = UI;
		input.Type = TextureType.Tex2D;
		input.BoundNode = $"{Title}, ID:{Identifier}";
		input.BoundNodeId = $"{Identifier}";

		var textureObject = compiler.Result( TextureObject );
		var coords = compiler.Result( Coords );
		var samplerGlobal = compiler.ResultSamplerOrDefault( Sampler, SamplerState );
		var tile = compiler.ResultOrDefault( Tile, DefaultTile );
		var normal = compiler.Result( Normal );
		var blendfactor = compiler.ResultOrDefault( BlendFactor, DefaultBlendFactor );

		if ( textureObject.IsValid )
		{
			IsTextureObjectConnected = true;
		}
		else
		{
			//Image = "";
			IsTextureObjectConnected = false;
		}

		CompileTexture();

		if ( CheckIfRegisterd( compiler, input, out var existingEntry ) )
		{
			return NodeResult.Error( $"`{input.Name}` was already registerd by node `{existingEntry.Value.BoundNode}`" );
		}

		// If TextureObject input is not valid and we are not in a SubGraph then register the texture here instead.
		if ( !textureObject.IsValid && !IsSubgraph )
		{
			SGPLog.Info( $"Registering Texture 2D Object on the `{nameof( NormalMapTriplanar )}` node `{Identifier}`.", compiler.IsPreview && ConCommands.TextureNodeDebug );

			var texture = string.IsNullOrWhiteSpace( TexturePath ) ? null : Texture.Load( TexturePath );
			texture ??= Texture.White;

			var resultTextureGlobal = compiler.ResultTexture( input, texture );

			var result = compiler.ResultHLSLFunction( "TexTriplanar_Normal",
			resultTextureGlobal,
			samplerGlobal,
			coords.IsValid ? coords.Cast( 3 ) : "(i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz) / 39.3701",
			normal.IsValid ? normal.Cast( 3 ) : "normalize( i.vNormalWs.xyz )",
			$"{blendfactor}"
			);

			return new NodeResult( ResultType.Vector3, result );
		}

		// Make sure to let the user know that the requied input is missing if we are in in a SubGraph.
		if ( !textureObject.IsValid && IsSubgraph )
		{
			return NodeResult.MissingInput( $"Tex Object" );
		}

		// If TextureObject input is valid then use the registerd Texture2D from the connected Texture Object node.
		// Either if the textureObject input is valid or we are in a Subgraph.
		if ( textureObject.IsValid || (IsSubgraph && textureObject.IsValid) )
		{
			SGPLog.Info( $"Using Texture 2D Object `{textureObject.Code}` from TextureObject input on the `{nameof( NormalMapTriplanar )}` node `{Identifier}`.", compiler.IsPreview && ConCommands.TextureNodeDebug );

			var result = compiler.ResultHLSLFunction( "TexTriplanar_Normal",
					$"{textureObject.Code}",
					$"{samplerGlobal}",
					coords.IsValid ? coords.Cast( 3 ) : "(i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz) / 39.3701",
					normal.IsValid ? normal.Cast( 3 ) : "normalize( i.vNormalWs.xyz )",
					$"{blendfactor}"
				);

			return new NodeResult( ResultType.Vector3, result );
		}

		return NodeResult.Error( "Failed to evaluate!" );
	};
}

/// <summary>
/// Sample a Cube Texture
/// </summary>
[Title( "Texture Cube" ), Category( "Textures" ), Icon( "view_in_ar" )]
[InternalNode]
public sealed class TextureCube : ShaderNodePlus, ITextureInputNode
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.FunctionNode;

	[JsonIgnore, Hide]
	public bool IsSubgraph => (Graph is ShaderGraphPlus shaderGraph && shaderGraph.IsSubgraph);

	[JsonIgnore, Hide]
	public bool ShowUIProperty
	{
		get
		{
			if ( IsSubgraph )
				return false;

			if ( IsTextureObjectConnected )
				return false;

			return true;
		}
	}

	[JsonIgnore, Hide]
	public bool IsTextureObjectConnected { get; set; } = false;

	#region ITextureInputNode
	[JsonIgnore, Hide, Browsable( false )]
	public string TextureInputName => UI.Name;

	[JsonIgnore, Hide, Browsable( false )]
	public bool AlreadyRegisterd { get; set; } = false;
	#endregion

	/// <summary>
	/// Coordinates to sample this cubemap
	/// </summary>
	[Title( "Coordinates" )]
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput Coords { get; set; }

	/// <summary>
	/// How the texture is filtered and wrapped when sampled
	/// </summary>
	[Title( "Sampler" )]
	[Input( typeof( Sampler ) )]
	[Hide]
	public NodeInput Sampler { get; set; }

	/// <summary>
	/// Optional TextureCube Object input when outside of subgraphs.
	/// </summary>
	[Title( "Tex Cube" )]
	[Input( typeof( TextureCubeObject ) )]
	[Hide]
	public NodeInput TextureCubeObject { get; set; }

	/// <summary>
	/// Texture to sample in previewW
	/// </summary>
	[ResourceType( "vtex" )]
	[ShowIf( nameof( ShowUIProperty ), true )]
	public string Texture { get; set; }

	[InlineEditor( Label = false ), Group( "Sampler" )]
	[HideIf( nameof( IsSubgraph ), true )]
	public Sampler SamplerState { get; set; } = new Sampler();

	/// <summary>
	/// Settings for how this texture shows up in material editor
	/// </summary>
	[InlineEditor( Label = false ), Group( "UI" )]
	[ShowIf( nameof( ShowUIProperty ), true )]
	public TextureInput UI { get; set; } = new TextureInput
	{
		ImageFormat = TextureFormat.DXT5,
		SrgbRead = true,
		DefaultColor = Color.White,
	};

	public TextureCube() : base()
	{
		Texture = "materials/skybox/skybox_workshop.vtex";
		ExpandSize = new Vector2( 0, 8 + Inputs.Count() * 24 );
	}

	public override void OnPaint( Rect rect )
	{
		rect = rect.Align( 130, TextFlag.LeftBottom ).Shrink( 3 );

		Paint.SetBrush( "/image/transparent-small.png" );
		Paint.DrawRect( rect.Shrink( 2 ), 2 );

		Paint.SetBrush( Theme.ControlBackground.WithAlpha( 0.7f ) );
		Paint.DrawRect( rect, 2 );

		if ( !string.IsNullOrEmpty( Texture ) )
		{
			var tex = Sandbox.Texture.Find( Texture );
			if ( tex is null ) return;
			var pixmap = Pixmap.FromTexture( tex );
			Paint.Draw( rect.Shrink( 2 ), pixmap );
		}
	}

	/// <summary>
	/// RGBA color result
	/// </summary>
	[Hide]
	[Output( typeof( Color ) ), Title( "RGBA" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var input = UI;
		input.Type = TextureType.TexCube;
		input.BoundNode = $"{Title}, ID:{Identifier}";
		input.BoundNodeId = $"{Identifier}";

		var textureCubeObject = compiler.Result( TextureCubeObject );
		var sampler = compiler.ResultSamplerOrDefault( Sampler, SamplerState );
		var coords = compiler.Result( Coords );

		if ( textureCubeObject.IsValid )
		{
			IsTextureObjectConnected = true;
		}
		else
		{
			//Texture = "";
			IsTextureObjectConnected = false;
		}

		if ( AlreadyRegisterd && compiler.IsPreview )
		{
			var existingEntry = compiler.GetExistingTextureInputEntry( input.Name );
			return NodeResult.Error( $"`{input.Name}` was already registerd by node `{existingEntry.Value.BoundNode}`" );
		}

		// If TextureCubeObject input is not valid and we are not in a SubGraph then register the texture here instead.
		if ( !textureCubeObject.IsValid && !IsSubgraph )
		{
			var resultTextureGlobal = compiler.ResultTexture( input, Sandbox.Texture.Load( Texture ) );

			return new NodeResult( ResultType.Color, $"TexCubeS( {resultTextureGlobal}," +
				$" {sampler}," +
				$" {(coords.IsValid ? $"{coords.Cast( 3 )}" : ViewDirection.Result.Invoke( compiler ))} )" );
		}

		// Make sure to let the user know that the requied input is missing if we are in in a SubGraph.
		if ( !textureCubeObject.IsValid && IsSubgraph )
		{
			return NodeResult.MissingInput( $"Tex Object" );
		}

		// If TextureCubeObject input is valid then use the registerd Texture Object from the connected Texture Cube Object node.
		// Either if the textureObject input is valid or we are in a Subgraph.
		if ( textureCubeObject.IsValid || (IsSubgraph && textureCubeObject.IsValid) )
		{
			return new NodeResult( ResultType.Color, $"TexCubeS( {textureCubeObject.Code}," +
				$" {sampler}," +
				$" {(coords.IsValid ? $"{coords.Cast( 3 )}" : ViewDirection.Result.Invoke( compiler ))} )" );
		}

		return NodeResult.Error( "Failed to evaluate!" );
	};

	private NodeResult Component( string component, GraphCompiler compiler )
	{
		var result = compiler.Result( new NodeInput { Identifier = Identifier, Output = nameof( Result ) } );
		return result.IsValid ? new( ResultType.Float, $"{result}.{component}", true ) : new( ResultType.Float, "0.0f", true );
	}

	/// <summary>
	/// Red component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "R" )]
	public NodeResult.Func R => ( GraphCompiler compiler ) => Component( "r", compiler );

	/// <summary>
	/// Green component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "G" )]
	public NodeResult.Func G => ( GraphCompiler compiler ) => Component( "g", compiler );

	/// <summary>
	/// Blue component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "B" )]
	public NodeResult.Func B => ( GraphCompiler compiler ) => Component( "b", compiler );

	/// <summary>
	/// Alpha (Opacity) component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "A" )]
	public NodeResult.Func A => ( GraphCompiler compiler ) => Component( "a", compiler );
}

/// <summary>
/// TextureCube Object.
/// </summary>
[Title( "Texture Cube Object" ), Category( "Textures" ), Icon( "image" )]
[InternalNode]
public sealed class TextureCubeObjectNode : ShaderNodePlus, IParameterNode, ITextureInputNode
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override bool CanPreview => false;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.FunctionNode;

	[Hide]
	private bool IsSubgraph => (Graph is ShaderGraphPlus shaderGraph && shaderGraph.IsSubgraph);

	[Hide]
	public override string Title
	{
		get
		{
			string name = $"{DisplayInfo.For( this ).Name}";

			if ( !IsSubgraph && !string.IsNullOrWhiteSpace( UI.Name ) )
			{
				return $"{name} ( {UI.Name} )";
			}

			return name;
		}
	}

	#region ITextureInputNode
	[JsonIgnore, Hide, Browsable( false )]
	public string TextureInputName => UI.Name;

	[JsonIgnore, Hide, Browsable( false )]
	public bool AlreadyRegisterd { get; set; } = false;
	#endregion

	/// <summary>
	/// Texture to sample in previewW
	/// </summary>
	[ResourceType( "vtex" )]
	[HideIf( nameof( IsSubgraph ), true )]
	public string Texture { get; set; }

	/// <summary>
	/// Settings for how this texture shows up in material editor
	/// </summary>
	[InlineEditor( Label = false ), Group( "UI" )]
	[HideIf( nameof( IsSubgraph ), true )]
	public TextureInput UI { get; set; } = new TextureInput
	{
		ImageFormat = TextureFormat.DXT5,
		SrgbRead = true,
		DefaultColor = Color.White,
	};

	public TextureCubeObjectNode() : base()
	{
		Texture = "materials/default/default.vtex";//"materials/skybox/skybox_workshop.vtex";
		ExpandSize = new Vector2( 32, 128 );
	}

	public override void OnPaint( Rect rect )
	{
		rect = rect.Align( 130, TextFlag.LeftBottom ).Shrink( 3 );

		Paint.SetBrush( "/image/transparent-small.png" );
		Paint.DrawRect( rect.Shrink( 2 ), 2 );

		Paint.SetBrush( Theme.ControlBackground.WithAlpha( 0.7f ) );
		Paint.DrawRect( rect, 2 );

		if ( !string.IsNullOrEmpty( Texture ) )
		{
			var tex = Sandbox.Texture.Find( Texture );
			if ( tex is null ) return;
			var pixmap = Pixmap.FromTexture( tex );
			Paint.Draw( rect.Shrink( 2 ), pixmap );
		}
	}

	[Hide, JsonIgnore]
	public string Name { get; set; }

	[Hide, JsonIgnore]
	public bool IsAttribute { get; set; }

	[Hide, JsonIgnore]
	ParameterUI IParameterNode.UI { get; set; }

	/// <summary>
	/// TextureCube object result.
	/// </summary>
	[Hide]
	[Output( typeof( TextureCubeObject ) ), Title( "Tex Cube" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var input = UI;
		input.Type = TextureType.TexCube;
		input.BoundNode = $"{Title}, ID:{Identifier}";
		input.BoundNodeId = $"{Identifier}";

		if ( IsSubgraph )
		{
			if ( string.IsNullOrWhiteSpace( Name ) )
			{
				return NodeResult.Error( $"Missing required Input Name" );
			}
		}

		if ( AlreadyRegisterd && compiler.IsPreview )
		{
			var existingEntry = compiler.GetExistingTextureInputEntry( input.Name );
			return NodeResult.Error( $"`{input.Name}` was already registerd by node `{existingEntry.Value.BoundNode}`" );
		}

		var resultTextureGlobal = compiler.ResultTexture( input, Sandbox.Texture.Load( Texture ) );

		return new NodeResult( ResultType.TextureCubeObject, resultTextureGlobal, constant: true ) { };
	};
}

/// <summary>
/// Texture2D Object.
/// </summary>
[Title( "Texture 2D Object" ), Category( "Textures" ), Icon( "image" )]
[InternalNode]
public sealed class Texture2DObjectNode : ShaderNodePlus, ITextureInputNode, ITextureParameterNode, IParameterNode, IErroringNode
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.FunctionNode;

	[JsonIgnore, Hide, Browsable( false )]
	public override bool CanPreview => false;

	[Hide]
	private bool IsSubgraph => (Graph is ShaderGraphPlus shaderGraph && shaderGraph.IsSubgraph);

	#region ITextureInputNode
	[JsonIgnore, Hide, Browsable( false )]
	public string TextureInputName => UI.Name;

	[JsonIgnore, Hide, Browsable( false )]
	public bool AlreadyRegisterd { get; set; } = false;
	#endregion

	/// <summary>
	/// Texture to sample in preview
	/// </summary>
	[ImageAssetPath]
	[HideIf( nameof( IsSubgraph ), true )]
	public string Image
	{
		get => _image;
		set
		{
			_image = value;
			_asset = AssetSystem.FindByPath( _image );
			if ( _asset == null )
				return;

			CompileTexture();
		}
	}

	[Hide]
	public override string Title
	{
		get
		{
			string name = $"{DisplayInfo.For( this ).Name}";

			UI.SetIsSubgraph( IsSubgraph );

			if ( !IsSubgraph && !string.IsNullOrWhiteSpace( UI.Name ) )
			{
				return $"{name} ( {UI.Name} )";
			}
			else if ( !IsSubgraph )
			{
				return name;
			}
			else if ( IsSubgraph && !string.IsNullOrWhiteSpace( Name ) )
			{
				return $"{name} ( {Name} )";
			}
			else
			{
				return name;
			}
		}
	}

	[Hide]
	private Asset _asset;
	[Hide]
	private string _texture;
	[Hide]
	private string _image;
	[Hide]
	private string _resourceText;

	[JsonIgnore, Hide]
	private Asset Asset => _asset;

	[JsonIgnore, Hide]
	private string TexturePath => _texture;

	private void CompileTexture()
	{
		if ( _asset == null )
			return;

		if ( string.IsNullOrWhiteSpace( _image ) )
			return;

		var resourceText = string.Format( ShaderTemplate.TextureDefinition,
			_image,
			UI.ColorSpace,
			UI.ImageFormat,
			UI.Processor );

		if ( _resourceText == resourceText )
			return;

		_resourceText = resourceText;

		var assetPath = $"shadergraphplus/{_image.Replace( ".", "_" )}_shadergraphplus.generated.vtex";
		var resourcePath = Editor.FileSystem.Root.GetFullPath( "/.source2/temp" );
		resourcePath = System.IO.Path.Combine( resourcePath, assetPath );

		if ( AssetSystem.CompileResource( resourcePath, resourceText ) )
		{
			_texture = assetPath;
		}
		else
		{
			Log.Warning( $"Failed to compile {_image}" );
		}
	}

	/// <summary>
	/// Settings for how this texture shows up in material editor
	/// </summary>
	[InlineEditor( Label = false ), Group( "UI" )]
	[HideIf( nameof( IsSubgraph ), true )]
	public TextureInput UI { get; set; } = new TextureInput
	{
		ImageFormat = TextureFormat.DXT5,
		SrgbRead = true,
		DefaultColor = Color.White,
	};

	public Texture2DObjectNode() : base()
	{
		Image = "materials/dev/white_color.tga";
		ExpandSize = new Vector2( 32, 128 );
	}

	public override void OnPaint( Rect rect )
	{
		rect = rect.Align( 130, TextFlag.LeftBottom ).Shrink( 3 );

		Paint.SetBrush( "/image/transparent-small.png" );
		Paint.DrawRect( rect.Shrink( 2 ), 2 );

		Paint.SetBrush( Theme.ControlBackground.WithAlpha( 0.7f ) );
		Paint.DrawRect( rect, 2 );

		if ( Asset != null )
		{
			Paint.Draw( rect.Shrink( 2 ), Asset.GetAssetThumb( true ) );
		}
	}

	[Hide]
	[Title( "Input Name" )]
	public string Name { get; set; }

	[Hide, JsonIgnore]
	public bool IsAttribute { get; set; }

	[Hide, JsonIgnore]
	ParameterUI IParameterNode.UI { get; set; }

	/// <summary>
	/// Texture2D object result.
	/// </summary>
	[Hide]
	[Output( typeof( Texture2DObject ) ), Title( "Tex 2D" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var input = UI;
		input.Type = TextureType.Tex2D;
		input.BoundNode = $"{Title}, ID:{Identifier}";
		input.BoundNodeId = $"{Identifier}"; ;

		CompileTexture();

		var texture = string.IsNullOrWhiteSpace( TexturePath ) ? null : Texture.Load( TexturePath );
		texture ??= Texture.White;

		if ( AlreadyRegisterd && compiler.IsPreview )
		{
			var existingEntry = compiler.GetExistingTextureInputEntry( input.Name );

			if ( !string.IsNullOrWhiteSpace( existingEntry.Value.BoundNodeId ) )
			{
				return new NodeResult( ResultType.Texture2DObject, $"g_t{existingEntry.Key}", constant: true );
			}

			//return NodeResult.Error( $"`{input.Name}` was already registerd by node `{existingEntry.Value.BoundNode}`:" );
		}

		var resultTextureGlobal = compiler.ResultTexture( input, texture );

		return new NodeResult( ResultType.Texture2DObject, resultTextureGlobal, constant: true );
	};

	public List<string> GetErrors()
	{
		var errors = new List<string>();

		if ( Graph is ShaderGraphPlus sg && sg.IsSubgraph )
		{
			//if ( string.IsNullOrWhiteSpace( UI.Name ) )
			//{
			//	errors.Add( $"Texture Object parameter \"{DisplayInfo.For( this ).Name}\" is missing a name" );
			//}

			//foreach ( var node in sg.Nodes )
			//{
			//	if ( node is ITextureParameterNode tpn && tpn != this && tpn.UI.Name == UI.Name )
			//	{
			//		errors.Add( $"Duplicate texture object parameter name \"{UI.Name}\" on {DisplayInfo.For( this ).Name}" );
			//	}
			//}
		}

		return errors;
	}
}

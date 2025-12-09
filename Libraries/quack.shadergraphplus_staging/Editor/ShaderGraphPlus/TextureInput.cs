using static ShaderGraphPlus.ShaderGraphPlusGlobals;

namespace ShaderGraphPlus;

public enum TextureExtension
{
	Color,
	Normal,
	Rough,
	AO,
	Metal,
	Trans,
	SelfIllum,
	Mask,
}

public enum TextureProcessor
{
	None,
	Mod2XCenter,
	NormalizeNormals,
	FillToPowerOfTwo,
	FillToMultipleOfFour,
	ScaleToPowerOfTwo,
	HeightToNormal,
	Inverse,
	ConvertToYCoCg,
	DilateColorInTransparentPixels,
	EncodeRGBM,
}

public enum TextureColorSpace
{
	Srgb,
	Linear,
}

public enum TextureFormat
{
	/// <summary>
	/// RGB color (5:6:5) and alpha (1).
	/// Usage : Diffuse Map, Roughness Map, Normal Map
	/// </summary>
	DXT1,
	/// <summary>
	/// RGB color (5:6:5) and alpha (4).
	/// Usage : Diffuse Map with Transparency
	/// </summary>
	DXT3,
	/// <summary>
	/// RGB color (5:6:5) and alpha (8).
	/// Usage : Diffuse Map with High Quality Transparency
	/// </summary>
	DXT5,
	/// <summary>
	/// Three-channel HDR color (16:16:16).
	/// Usage : Skyboxes
	/// </summary>
	BC6H,
	/// <summary>
	/// RGB (4-7 bits per channel) and alpha (0-8 bits).
	/// Usage : Diffuse Map, Roughness Map, Normal Map
	/// </summary>
	BC7,
	/// <summary>
	/// Single-channel (8).
	/// Usage : Height Map, Displacement Map, Ambient Occlusion Map
	/// </summary>
	ATI1N,
	/// <summary>
	/// Two-channel color (8:8).
	/// Usage :
	/// </summary>
	ATI2N,
	/// <summary>
	/// RGB color and alpha (8 bits each).
	///  You should only really use this in situations where block compression causes artifacting - because they have higher storage requirements.
	/// </summary>
	RGBA8888,
	/// <summary>
	/// Three-channel HDR color and alpha (16 bits each).
	///  You should only really use this in situations where block compression causes artifacting - because they have higher storage requirements.
	/// </summary>
	RGBA16161616F
}

public enum TextureType
{
	Tex2D,
	TexCube,
}

public struct UIGroup
{
	/// <summary>
	/// Name of this group
	/// </summary>
	[Editor( ControlWidgetCustomEditors.UIGroupEditor )]
	public string Name { get; set; }

	/// <summary>
	/// Priority of this group
	/// </summary>
	public int Priority { get; set; }
}

public struct TextureInput
{
	[Hide, JsonIgnore]
	public bool ShowNameProperty { get; set; }

	/// <summary>
	/// Name that shows up in material editor.
	/// </summary>
	[ShowIf( nameof( ShowNameProperty ), true )]
	public string Name { get; set; }

	/// <summary>
	/// If true, this parameter can be modified with <see cref="RenderAttributes"/>.
	/// </summary>
	[HideIf( nameof( IsSubgraph ), true )]
	public bool IsAttribute { get; set; }

	/// <summary>
	/// Default color that shows up in material editor when using color control
	/// </summary>
	[HideIf( nameof( IsSubgraph ), true )]
	public Color DefaultColor { get; set; }

	/// <summary>
	/// Default texture that shows up in material editor
	/// </summary>
	[ImageAssetPath]
	public string DefaultTexture { get; set; }

	/// <summary>
	/// Default texture that shows up in material editor (_color, _normal, _rough, etc..)
	/// </summary>
	[ShowIf( nameof( ShowExtension ), true )]
	[HideIf( nameof( IsSubgraph ), true )]
	public TextureExtension Extension { get; set; }

	/// <summary>
	/// Default texture that shows up in material editor (_color, _normal, _rough, etc..)
	/// </summary>
	[HideIf( nameof( IsSubgraph ), true )]
	public string CustomExtension { get; set; }

	public readonly bool ShowExtension => string.IsNullOrWhiteSpace( CustomExtension );

	[JsonIgnore, Hide]
	public string ExtensionString
	{
		get
		{
			if ( !string.IsNullOrWhiteSpace( CustomExtension ) )
			{
				var ext = CustomExtension.Trim();
				if ( ext.StartsWith( "_" ) )
					ext = ext[1..];

				if ( !string.IsNullOrWhiteSpace( ext ) )
					return ext;
			}

			return Extension.ToString();
		}
	}

	/// <summary>
	/// Processor used when compiling this texture
	/// </summary>
	[HideIf( nameof( IsSubgraph ), true )]
	public TextureProcessor Processor { get; set; }

	/// <summary>
	/// Color space used when compiling this texture
	/// </summary>
	[HideIf( nameof( IsSubgraph ), true )]
	public TextureColorSpace ColorSpace { get; set; }

	/// <summary>
	/// Format used when compiling this texture
	/// </summary>
	[HideIf( nameof( IsSubgraph ), true )]
	public TextureFormat ImageFormat { get; set; }

	/// <summary>
	/// Sample this texture as srgb
	/// </summary>
	[HideIf( nameof( IsSubgraph ), true )]
	public bool SrgbRead { get; set; }

	/// <summary>
	/// Priority of this value in the group
	/// </summary>
	public int Priority { get; set; }

	/// <summary>
	/// Primary group
	/// </summary>
	[InlineEditor( Label = false ), Group( "Group" )]
	[HideIf( nameof( IsSubgraph ), true )]
	public UIGroup PrimaryGroup { get; set; }

	/// <summary>
	/// Group within the primary group
	/// </summary>
	[InlineEditor( Label = false ), Group( "Sub Group" )]
	[HideIf( nameof( IsSubgraph ), true )]
	public UIGroup SecondaryGroup { get; set; }

	[JsonIgnore, Hide]
	public readonly string UIGroup => $"{PrimaryGroup.Name},{PrimaryGroup.Priority}/{SecondaryGroup.Name},{SecondaryGroup.Priority}/{Priority}";

	[JsonIgnore, Hide]
	public TextureType Type { get; set; }

	public readonly string CreateTexture( string name )
	{
		if ( Type == TextureType.Tex2D ) return $"Texture2D g_t{name}";
		if ( Type == TextureType.TexCube ) return $"TextureCube g_t{name}";
		return default;
	}

	[JsonIgnore, Hide]
	public readonly string CreateInput
	{
		get
		{
			if ( Type == TextureType.Tex2D ) return "CreateInputTexture2D";
			if ( Type == TextureType.TexCube ) return "CreateInputTextureCube";
			return default;
		}
	}

	#region Graph Editor Only
	[JsonIgnore, Hide]
	public string BoundNode { get; set; }

	[JsonIgnore, Hide]
	public string BoundNodeId { get; set; }
	#endregion Graph Editor Only

	public void SetOrder( int order )
	{
		Priority = order;
	}

	public void SetIsSubgraph( bool isSubgraph )
	{
		IsSubgraph = isSubgraph;
	}

	[JsonIgnore, Hide]
	public bool IsSubgraph { get; set; }


	public TextureInput()
	{
		ShowNameProperty = false;
		IsSubgraph = false;
	}
}

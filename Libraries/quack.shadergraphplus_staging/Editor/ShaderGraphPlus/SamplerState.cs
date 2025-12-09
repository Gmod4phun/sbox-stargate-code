using Editor;
using Sandbox.Rendering;
using System.Text.Json.Nodes;
using static ShaderGraphPlus.ShaderGraphPlus;

namespace ShaderGraphPlus;

public struct Sampler : ISGPJsonUpgradeable
{
	[Hide, JsonPropertyName( VersioningInfo.JsonPropertyName )]
	public readonly int Version => 2;

	/// <summary>
	/// The name of this Sampler.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// If true, this parameter can be modified with <see cref="RenderAttributes"/>.
	/// </summary>
	public bool IsAttribute { get; set; }

	/// <summary>
	/// The texture filtering mode used for sampling (e.g., point, bilinear, trilinear).
	/// </summary>
	public FilterMode Filter { get; init; }

	/// <summary>
	/// The addressing mode used for the U (X) texture coordinate.
	/// </summary>
	public TextureAddressMode AddressModeU { get; init; }

	/// <summary>
	/// The addressing mode used for the V texture coordinate.
	/// </summary>
	public TextureAddressMode AddressModeV { get; init; }

	/// <summary>
	/// The addressing mode used for the W texture coordinate.
	/// </summary>
	public TextureAddressMode AddressModeW { get; init; }

	/// <summary>
	/// The bias applied to the calculated mip level during texture sampling. Positive
	/// values make textures appear blurrier; negative values sharpen.
	/// </summary>
	public float MipLodBias { get; init; }

	/// <summary>
	/// The maximum anisotropy level used for anisotropic filtering. Higher values improve
	/// texture quality at oblique viewing angles.
	/// </summary>
	public int MaxAnisotropy { get; init; }

	/// <summary>
	/// Border color to use if TextureAddressMode.Border is specified
	/// for AddressU, AddressV, or AddressW.
	/// </summary>
	public Color BorderColor { get; init; }

	public Sampler()
	{
		Name = "";
		Filter = FilterMode.Bilinear;
		AddressModeU = TextureAddressMode.Wrap;
		AddressModeV = TextureAddressMode.Wrap;
		AddressModeW = TextureAddressMode.Wrap;
		MipLodBias = 0f;
		MaxAnisotropy = 8;
		BorderColor = Color.Transparent;
	}

	public static explicit operator SamplerState( Sampler sampler )
	{
		return new SamplerState()
		{
			Filter = sampler.Filter,
			AddressModeU = sampler.AddressModeU,
			AddressModeV = sampler.AddressModeV,
			AddressModeW = sampler.AddressModeW,
			MipLodBias = sampler.MipLodBias,
			MaxAnisotropy = sampler.MaxAnisotropy,
			BorderColor = sampler.BorderColor,
		};
	}

	[SGPJsonUpgrader( typeof( Sampler ), 2 )]
	public static void Upgrader_v2( JsonObject json )
	{
		if ( !json.ContainsKey( "Filter" ) )
		{
			return;
		}
		if ( !json.ContainsKey( "AddressU" ) )
		{
			return;
		}
		if ( !json.ContainsKey( "AddressV" ) )
		{
			return;
		}

		try
		{
			if ( json["Filter"].ToString() == "Aniso" )
			{
				json["Filter"] = JsonSerializer.SerializeToNode( FilterMode.Anisotropic, ShaderGraphPlus.SerializerOptions() );
			}

			if ( json["AddressU"].ToString() == "Mirror_Once" )
			{
				json["AddressModeU"] = JsonSerializer.SerializeToNode( TextureAddressMode.MirrorOnce, ShaderGraphPlus.SerializerOptions() );
			}

			if ( json["AddressV"].ToString() == "Mirror_Once" )
			{
				json["AddressModeV"] = JsonSerializer.SerializeToNode( TextureAddressMode.MirrorOnce, ShaderGraphPlus.SerializerOptions() );
			}

			json.Remove( "AddressU" );
			json.Remove( "AddressV" );
		}
		catch
		{
		}
	}
}

/*
[CustomEditor( typeof( Sampler ) )]
public class SamplerControlWidget : ControlObjectWidget
{
	public override bool SupportsMultiEdit => false;

	public SamplerControlWidget( SerializedProperty property ) : base( property, true )
	{
		Layout = Layout.Column();
		Layout.Spacing = 4;

		if ( SerializedObject == null )
			return;

		SerializedObject.TryGetProperty( nameof( Sampler.Name ), out var name );
		SerializedObject.TryGetProperty( nameof( Sampler.Filter ), out var filter );
		SerializedObject.TryGetProperty( nameof( Sampler.AddressModeU ), out var addressModeU );
		SerializedObject.TryGetProperty( nameof( Sampler.AddressModeV ), out var addressModeV );
		SerializedObject.TryGetProperty( nameof( Sampler.AddressModeW ), out var addressModeW );
		SerializedObject.TryGetProperty( nameof( Sampler.MipLodBias ), out var mipLodBias );
		SerializedObject.TryGetProperty( nameof( Sampler.MaxAnisotropy ), out var maxAnisotropy );
		SerializedObject.TryGetProperty( nameof( Sampler.BorderColor ), out var borderColor );

		Layout.Add( Create( name ) );
		Layout.Add( Create( filter ) );
		Layout.Add( Create( addressModeU ) );
		Layout.Add( Create( addressModeV ) );
		Layout.Add( Create( addressModeW ) );
		Layout.Add( Create( mipLodBias ) );
		Layout.Add( Create( maxAnisotropy ) );
		Layout.Add( Create( borderColor ) );
	}

	protected override void OnPaint()
	{
		// Overriding and doing nothing here will prevent the default background from being painted
	}
}
*/

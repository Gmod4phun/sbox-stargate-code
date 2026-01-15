using Sandbox.Rendering;
using ShaderGraphPlus.Nodes;
using VanillaGraph = Editor.ShaderGraph;
using VanillaNodes = Editor.ShaderGraph.Nodes;
using ShaderGraphBaseNode = Editor.ShaderGraph.BaseNode;

namespace ShaderGraphPlus.Internal;

file static class TextureSamplerExentions
{
	internal static Sampler ConvertVanillaSampler( this VanillaGraph.Sampler sampler, string name )
	{
		var filter = sampler.Filter switch
		{
			VanillaGraph.SamplerFilter.Aniso => FilterMode.Anisotropic,
			VanillaGraph.SamplerFilter.Bilinear => FilterMode.Bilinear,
			VanillaGraph.SamplerFilter.Trilinear => FilterMode.Trilinear,
			VanillaGraph.SamplerFilter.Point => FilterMode.Point,
			_ => throw new NotImplementedException(),
		};

		var addressModeU = sampler.AddressU switch
		{
			VanillaGraph.SamplerAddress.Wrap => TextureAddressMode.Wrap,
			VanillaGraph.SamplerAddress.Mirror => TextureAddressMode.Mirror,
			VanillaGraph.SamplerAddress.Clamp => TextureAddressMode.Clamp,
			VanillaGraph.SamplerAddress.Border => TextureAddressMode.Border,
			VanillaGraph.SamplerAddress.Mirror_Once => TextureAddressMode.MirrorOnce,
			_ => throw new NotImplementedException(),
		};

		var addressModeV = sampler.AddressV switch
		{
			VanillaGraph.SamplerAddress.Wrap => TextureAddressMode.Wrap,
			VanillaGraph.SamplerAddress.Mirror => TextureAddressMode.Mirror,
			VanillaGraph.SamplerAddress.Clamp => TextureAddressMode.Clamp,
			VanillaGraph.SamplerAddress.Border => TextureAddressMode.Border,
			VanillaGraph.SamplerAddress.Mirror_Once => TextureAddressMode.MirrorOnce,
			_ => throw new NotImplementedException(),
		};

		var newSampler = new Sampler()
		{
			Name = name,
			Filter = filter,
			AddressModeU = addressModeU,
			AddressModeV = addressModeV,
			AddressModeW = TextureAddressMode.Wrap,
			MipLodBias = 0f,
			MaxAnisotropy = 8,
			BorderColor = Color.Transparent,
		};

		return newSampler;
	}

	internal static TextureInput ConvertVanillaTextureInput( this VanillaGraph.TextureInput textureInput )
	{
		var newTextureInput = new TextureInput();

		newTextureInput.Name = textureInput.Name;
		newTextureInput.IsAttribute = textureInput.IsAttribute;
		newTextureInput.DefaultColor = textureInput.Default;
		newTextureInput.Extension = textureInput.Extension switch
		{
			VanillaGraph.TextureExtension.Color => TextureExtension.Color,
			VanillaGraph.TextureExtension.Normal => TextureExtension.Normal,
			VanillaGraph.TextureExtension.Rough => TextureExtension.Rough,
			VanillaGraph.TextureExtension.AO => TextureExtension.AO,
			VanillaGraph.TextureExtension.Metal => TextureExtension.Metal,
			VanillaGraph.TextureExtension.Trans => TextureExtension.Trans,
			VanillaGraph.TextureExtension.SelfIllum => TextureExtension.SelfIllum,
			VanillaGraph.TextureExtension.Mask => TextureExtension.Mask,
			_ => throw new NotImplementedException(),
		};
		newTextureInput.Processor = textureInput.Processor switch
		{
			VanillaGraph.TextureProcessor.None => TextureProcessor.None,
			VanillaGraph.TextureProcessor.Mod2XCenter => TextureProcessor.Mod2XCenter,
			VanillaGraph.TextureProcessor.NormalizeNormals => TextureProcessor.NormalizeNormals,
			VanillaGraph.TextureProcessor.FillToPowerOfTwo => TextureProcessor.FillToPowerOfTwo,
			VanillaGraph.TextureProcessor.FillToMultipleOfFour => TextureProcessor.FillToMultipleOfFour,
			VanillaGraph.TextureProcessor.ScaleToPowerOfTwo => TextureProcessor.ScaleToPowerOfTwo,
			VanillaGraph.TextureProcessor.HeightToNormal => TextureProcessor.HeightToNormal,
			VanillaGraph.TextureProcessor.Inverse => TextureProcessor.Inverse,
			VanillaGraph.TextureProcessor.ConvertToYCoCg => TextureProcessor.ConvertToYCoCg,
			VanillaGraph.TextureProcessor.DilateColorInTransparentPixels => TextureProcessor.DilateColorInTransparentPixels,
			VanillaGraph.TextureProcessor.EncodeRGBM => TextureProcessor.EncodeRGBM,
			_ => throw new NotImplementedException(),
		};
		newTextureInput.ColorSpace = textureInput.ColorSpace switch
		{
			VanillaGraph.TextureColorSpace.Srgb => TextureColorSpace.Srgb,
			VanillaGraph.TextureColorSpace.Linear => TextureColorSpace.Linear,
			_ => throw new NotImplementedException(),
		};
		newTextureInput.ImageFormat = textureInput.ImageFormat switch
		{
			VanillaGraph.TextureFormat.DXT5 => TextureFormat.DXT5,
			VanillaGraph.TextureFormat.DXT1 => TextureFormat.DXT1,
			VanillaGraph.TextureFormat.RGBA8888 => TextureFormat.RGBA8888,
			VanillaGraph.TextureFormat.BC7 => TextureFormat.BC7,
			_ => throw new NotImplementedException(),
		};
		newTextureInput.SrgbRead = textureInput.SrgbRead;
		newTextureInput.Priority = textureInput.Priority;

		newTextureInput.PrimaryGroup = new() { Name = textureInput.PrimaryGroup.Name, Priority = textureInput.PrimaryGroup.Priority };
		newTextureInput.SecondaryGroup = new() { Name = textureInput.SecondaryGroup.Name, Priority = textureInput.SecondaryGroup.Priority };
		newTextureInput.Type = textureInput.Type == VanillaGraph.TextureType.Tex2D ? TextureType.Tex2D : TextureType.TexCube;

		return newTextureInput;
	}
}

file static class TextureSamplerConversionFunctions
{
	internal static void CreateTextureParameter( ProjectConverter converter, Texture2DSamplerBase texture2DSamplerNode )
	{
		var textureInput = texture2DSamplerNode.UI;

		if ( !string.IsNullOrWhiteSpace( textureInput.Name ) )
		{
			var tex2DParameter = new Texture2DParameter();
			tex2DParameter.Name = textureInput.Name;
			tex2DParameter.Value = textureInput;

			var tex2DParameterNode = new Texture2DParameterNode()
			{
				Position = new Vector2( texture2DSamplerNode.Position.x - 192, texture2DSamplerNode.Position.y )
			};
			tex2DParameterNode.BlackboardParameterIdentifier = tex2DParameter.Identifier;
			tex2DParameterNode.UI = textureInput;

			converter.AddNewNode( tex2DParameterNode );

			//var connections = new Dictionary<string, string>()
			//{
			//	{ 
			//		$"{nameof(Texture2DParameterNode.Result)}", // From
			//		$"{nameof(Texture2DSamplerBase.Texture2DInput)}" // To
			//	},
			//};
			//
			//var connectionData = new NodeConnectionDataNew(
			//	nameof( Texture2DParameterNode.Result ),
			//	nameof( Texture2DSamplerBase.Texture2DInput ),
			//	"",
			//	texture2DSamplerNode.Identifier
			//);

			converter.AddBlackboardParameter( tex2DParameter );
		}
		else
		{

		}
	}
}

internal class TextureSamplerNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.TextureSampler );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldTextureSamplerNode = oldNode as VanillaNodes.TextureSampler;

		var newNode = new SampleTexture2DNode();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		//newNode.InternalImage = oldTextureSamplerNode.Image;
		newNode.SamplerState = oldTextureSamplerNode.Sampler.ConvertVanillaSampler( "" );
		newNode.UI = oldTextureSamplerNode.UI.ConvertVanillaTextureInput();

		TextureSamplerConversionFunctions.CreateTextureParameter( converter, newNode );

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class TextureCubeNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.TextureCube );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldTextureCubeNode = oldNode as VanillaNodes.TextureCube;

		var newNode = new SampleTextureCubeNode();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.Texture = oldTextureCubeNode.Texture;
		newNode.SamplerState = oldTextureCubeNode.Sampler.ConvertVanillaSampler( "" );
		newNode.UI = oldTextureCubeNode.UI.ConvertVanillaTextureInput();

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class TextureTriplanarNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.TextureTriplanar );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldTextureTriplanarNode = oldNode as VanillaNodes.TextureTriplanar;

		var newNode = new SampleTexture2DTriplanarNode();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.InternalImage = oldTextureTriplanarNode.Image;
		newNode.SamplerState = oldTextureTriplanarNode.Sampler.ConvertVanillaSampler( "" );
		newNode.UI = oldTextureTriplanarNode.UI.ConvertVanillaTextureInput();

		TextureSamplerConversionFunctions.CreateTextureParameter( converter, newNode );

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class NormapMapTriplanarNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.NormapMapTriplanar );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldNormapMapTriplanarNode = oldNode as VanillaNodes.NormapMapTriplanar;

		var newNode = new SampleTexture2DNormalMapTriplanarNode();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.InternalImage = oldNormapMapTriplanarNode.Image;
		newNode.SamplerState = oldNormapMapTriplanarNode.Sampler.ConvertVanillaSampler( "" );
		newNode.UI = oldNormapMapTriplanarNode.UI.ConvertVanillaTextureInput();

		TextureSamplerConversionFunctions.CreateTextureParameter( converter, newNode );

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class TextureCoordNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.TextureCoord );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldTextureCoordNode = oldNode as VanillaNodes.TextureCoord;

		var newNode = new TextureCoord();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.UseSecondaryCoord = oldTextureCoordNode.UseSecondaryCoord;
		newNode.Tiling = oldTextureCoordNode.Tiling;

		newNodes.Add( newNode );

		return newNodes;
	}
}

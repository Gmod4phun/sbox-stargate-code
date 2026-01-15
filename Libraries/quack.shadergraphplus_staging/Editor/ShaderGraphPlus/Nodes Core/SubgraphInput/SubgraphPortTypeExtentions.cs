
namespace ShaderGraphPlus.Nodes;

public static class SubgraphPortTypeExtentions
{
	public static string GetHlslType( this SubgraphPortType subgraphPortType )
	{
		return subgraphPortType switch
		{

			SubgraphPortType.Bool => "bool",
			SubgraphPortType.Int => "int",
			SubgraphPortType.Float => "float",
			SubgraphPortType.Vector2 => "float2",
			SubgraphPortType.Vector3 => "float3",
			SubgraphPortType.Vector4 => "float4",
			SubgraphPortType.Color => "float4",
			SubgraphPortType.SamplerState => "SamplerState",
			SubgraphPortType.Texture2DObject => "Texture2D",
			SubgraphPortType.TextureCubeObject => "TextureCube",
			_ => throw new NotImplementedException( $"Unknown PortType \"{subgraphPortType}\"" )
		};
	}
}

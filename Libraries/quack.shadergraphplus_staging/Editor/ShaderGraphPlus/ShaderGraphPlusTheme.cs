using Editor;
using ShaderGraphPlus.Nodes;

namespace ShaderGraphPlus;

/// <summary>
/// Storing the Node Header Primary Colors here so that they are all in one place.
/// </summary>
public static class PrimaryNodeHeaderColors
{
	public static Color SubgraphNode => Color.Parse( "#e05b0a" )!.Value;
	public static Color GraphResultNode => Color.Parse( "#84705e" )!.Value;
	public static Color MathNode => Color.Parse( "#394d62" )!.Value;
	public static Color UnaryNode => MathNode;
	public static Color BinaryNode => MathNode;
	public static Color ConstantValueNode => Color.Parse( "#736024" )!.Value;
	public static Color ParameterNode => Color.Parse( "#5d9b31" )!.Value;
	public static Color MatrixNode => Color.Parse( "#5d9b31" )!.Value;
	public static Color StageInputNode => Color.Parse( "#803334" )!.Value;
	public static Color GlobalVariableNode => Color.Parse( "#803334" )!.Value;
	public static Color FunctionNode => Color.Parse( "#1d53ac" )!.Value;
	public static Color TransformNode => Color.Parse( "#6c3baa" )!.Value;
	public static Color LogicNode => Color.Parse( "#006b54" )!.Value;
	public static Color ChannelNode => Color.Parse( "#2e2a60" )!.Value;
}

internal static class ShaderGraphPlusTheme
{
	public record struct BlackboardConfig( string Name, Color Color );

	public static Dictionary<Type, NodeHandleConfig> NodeHandleConfigs { get; private set; }
	public static Dictionary<Type, BlackboardConfig> BlackboardConfigs { get; private set; }

	static ShaderGraphPlusTheme()
	{
		Update();
	}

	[Event( "hotloaded" )]
	static void Update()
	{
		NodeHandleConfigs = new()
		{
			{ typeof( bool ), new NodeHandleConfig( "bool", Theme.Blue.AdjustHue( -80 ) ) },
			{ typeof( int ), new NodeHandleConfig( "int", Color.Parse( "#ce67e0" )!.Value.AdjustHue( -80 ) ) },
			{ typeof( float ), new NodeHandleConfig( "Float", Color.Parse( "#8ec07c" )!.Value ) },
			{ typeof( Vector2 ), new NodeHandleConfig( "Vector2", Color.Parse( "#ce67e0" )!.Value ) },
			{ typeof( Vector3 ), new NodeHandleConfig( "Vector3", Color.Parse( "#7177e1" )!.Value ) },
			{ typeof( Vector4 ), new NodeHandleConfig( "Vector4", Color.Parse( "#c7ae32" )!.Value ) },
			{ typeof( Color ), new NodeHandleConfig( "Color", Color.Parse( "#c7ae32" )!.Value ) },
			{ typeof( Float2x2 ), new NodeHandleConfig( "Float2x2", Color.Parse( "#b83385" )!.Value ) },
			{ typeof( Float3x3 ), new NodeHandleConfig( "Float3x3", Color.Parse( "#b83385" )!.Value ) },
			{ typeof( Float4x4 ), new NodeHandleConfig( "Float4x4", Color.Parse( "#b83385" )!.Value ) },
			{ typeof( Texture2DObject ), new NodeHandleConfig( "Texture2D", Color.Parse( "#ffb3a7" )!.Value ) },
			{ typeof( TextureCubeObject ), new NodeHandleConfig( "TextureCube", Color.Parse( "#ffb3a7" )!.Value ) },
			{ typeof( Sampler ), new NodeHandleConfig( "Sampler", Color.Parse( "#dddddd" )!.Value ) },
			{ typeof( Gradient ), new NodeHandleConfig( "Gradient", Color.Parse( "#dddddd" )!.Value ) },
			{ typeof( Bundle ), new NodeHandleConfig( "Bundle", Color.Parse( "#dddddd" )!.Value ) },
		};

		BlackboardConfigs = new()
		{
			{ typeof( BoolSubgraphInputParameter ), new BlackboardConfig( "bool", NodeHandleConfigs[typeof( bool )].Color ) },
			{ typeof( IntSubgraphInputParameter ), new BlackboardConfig( "int", NodeHandleConfigs[typeof( int )].Color ) },
			{ typeof( FloatSubgraphInputParameter ), new BlackboardConfig( "float", NodeHandleConfigs[typeof( float )].Color ) },
			{ typeof( Float2SubgraphInputParameter ), new BlackboardConfig( "float2", NodeHandleConfigs[typeof( Vector2 )].Color ) },
			{ typeof( Float3SubgraphInputParameter ), new BlackboardConfig( "float3", NodeHandleConfigs[typeof( Vector3 )].Color ) },
			{ typeof( Float4SubgraphInputParameter ), new BlackboardConfig( "float4", NodeHandleConfigs[typeof( Vector4 )].Color ) },
			{ typeof( ColorSubgraphInputParameter ), new BlackboardConfig( "float4", NodeHandleConfigs[typeof( Color )].Color ) },
			{ typeof( Float2x2SubgraphInputParameter ), new BlackboardConfig( "float2x2", NodeHandleConfigs[typeof( Float2x2 )].Color ) },
			{ typeof( Float3x3SubgraphInputParameter ), new BlackboardConfig( "float3x3", NodeHandleConfigs[typeof( Float3x3 )].Color ) },
			{ typeof( Float4x4SubgraphInputParameter ), new BlackboardConfig( "float4x4", NodeHandleConfigs[typeof( Float4x4 )].Color ) },
			{ typeof( Texture2DSubgraphInputParameter ), new BlackboardConfig( "Texture2D", NodeHandleConfigs[typeof( Texture2DObject )].Color ) },
			{ typeof( TextureCubeSubgraphInputParameter ), new BlackboardConfig( "TextureCube", NodeHandleConfigs[typeof( TextureCubeObject )].Color ) },
			{ typeof( BoolParameter ), new BlackboardConfig( "bool", NodeHandleConfigs[typeof( bool )].Color ) },
			{ typeof( IntParameter ), new BlackboardConfig( "int", NodeHandleConfigs[typeof( int )].Color ) },
			{ typeof( FloatParameter ), new BlackboardConfig( "float", NodeHandleConfigs[typeof( float )].Color ) },
			{ typeof( Float2Parameter ), new BlackboardConfig( "float2", NodeHandleConfigs[typeof( Vector2 )].Color ) },
			{ typeof( Float3Parameter ), new BlackboardConfig( "float3", NodeHandleConfigs[typeof( Vector3 )].Color ) },
			{ typeof( Float4Parameter ), new BlackboardConfig( "float4", NodeHandleConfigs[typeof( Vector4 )].Color ) },
			{ typeof( ColorParameter ), new BlackboardConfig( "float4", NodeHandleConfigs[typeof( Color )].Color ) },
			{ typeof( Texture2DParameter ), new BlackboardConfig( "Texture2D", NodeHandleConfigs[typeof( Texture2DObject )].Color ) },
			{ typeof( TextureCubeParameter ), new BlackboardConfig( "TextureCube", NodeHandleConfigs[typeof( TextureCubeObject )].Color ) },
		};
	}
}


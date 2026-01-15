
namespace ShaderGraphPlus.Nodes;

[Title( "Float 3x3" ), Category( "Constants/Matrix" ), Icon( "apps" ), Order( 10 )]
public sealed class Float3x3ConstantNode : MatrixConstantNode<Float3x3>
{
	[Hide] public override int Version => 1;

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	[Output( typeof( Float3x3 ) ), Title( "Matrix" )]
	[Hide]
	[NodeValueEditor( nameof( Value ) )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( Name, Value, default, default, false, IsAttribute, default );
	};

	public override BaseBlackboardParameter InitializeSubgraphInputParameter( string name )
	{
		return new Float3x3SubgraphInputParameter( name, Value )
		{
			IsRequired = false,
		};
	}
}


namespace ShaderGraphPlus.Nodes;

[Title( "Float 2x2" ), Category( "Constants/Matrix" ), Icon( "apps" ), Order( 9 )]
public sealed class Float2x2ConstantNode : MatrixConstantNode<Float2x2>
{
	[Hide] public override int Version => 1;

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	[Output( typeof( Float2x2 ) ), Title( "Matrix" )]
	[Hide]
	[NodeValueEditor( nameof( Value ) )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( Name, Value, default, default, false, IsAttribute, default );
	};

	public override BaseBlackboardParameter InitializeSubgraphInputParameter( string name )
	{
		return new Float2x2SubgraphInputParameter( name, Value )
		{
			IsRequired = false,
		};
	}
}

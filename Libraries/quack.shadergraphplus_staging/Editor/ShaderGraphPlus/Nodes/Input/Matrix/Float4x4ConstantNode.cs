
namespace ShaderGraphPlus.Nodes;

public interface IConstantMatrixNode
{

}

[Title( "Float 4x4" ), Category( "Constants/Matrix" ), Icon( "apps" ), Order( 11 )]
public sealed class Float4x4ConstantNode : MatrixConstantNode<Float4x4>
{
	[Hide] public override int Version => 1;

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	[Output( typeof( Float4x4 ) ), Title( "Matrix" )]
	[Hide]
	[NodeValueEditor( nameof( Value ) )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( Name, Value, default, default, false, IsAttribute, default );
	};

	public override BaseBlackboardParameter InitializeSubgraphInputParameter( string name )
	{
		return new Float4x4SubgraphInputParameter( name, Value )
		{
			IsRequired = false,
		};
	}
}

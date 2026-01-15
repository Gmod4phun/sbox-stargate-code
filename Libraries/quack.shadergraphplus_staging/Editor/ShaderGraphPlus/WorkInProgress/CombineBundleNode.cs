namespace ShaderGraphPlus.Nodes;

// TODO : Will pretty much behave like bundles in blender 5.0+

/*
[Title( "Combine Bundle" ), Category( "Utility" ), Icon( "route" )]
public sealed class CombineBundleNode : ShaderNodePlus, IMetaDataNode, IInitializeNode
{
	[Hide]
	public override int Version => 1;

	[Output( typeof( Bundle ) ), Hide, Title( "" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var nodeInputs = new List<NodeInput>();
		var bundle = new Bundle();

		// TODO
		//foreach ( var input in Inputs )
		//{
		//	
		//}

		return new NodeResult( ResultType.Bundle, "Bundle", bundle );
	};

	public void InitializeNode()
	{
		throw new NotImplementedException();
	}

	public NodeResult GetResult( GraphCompiler compiler )
	{
		return Result.Invoke( compiler );
	}

}
*/

public struct Bundle
{
	public List<NodeInput> Inputs { get; }

	public Bundle( List<NodeInput> inputs )
	{
		Inputs = inputs;
	}
}

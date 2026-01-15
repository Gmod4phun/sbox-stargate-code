using ShaderGraphPlus.Nodes;
using VanillaGraph = Editor.ShaderGraph;
using VanillaNodes = Editor.ShaderGraph.Nodes;
using ShaderGraphBaseNode = Editor.ShaderGraph.BaseNode;

namespace ShaderGraphPlus.Internal;


internal class FuzzyNoiseNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.FuzzyNoise );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldFuzzyNoiseNode = oldNode as VanillaNodes.FuzzyNoise;

		var newNode = new Nodes.FuzzyNoise();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class ValueNoiseNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.ValueNoise );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldValueNoiseNode = oldNode as VanillaNodes.ValueNoise;

		var newNode = new Nodes.ValueNoise();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class SimplexNoiseNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.SimplexNoise );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldSimplexNoiseNode = oldNode as VanillaNodes.SimplexNoise; ;

		var newNode = new Nodes.SimplexNoise();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class VoronoiNoiseNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.VoronoiNoise );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldVoronoiNoiseNode = oldNode as VanillaNodes.VoronoiNoise; ;

		var newNode = new Nodes.VoronoiNoise();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.AngleOffset = oldVoronoiNoiseNode.AngleOffset;
		newNode.CellDensity = oldVoronoiNoiseNode.CellDensity;
		newNode.Worley = oldVoronoiNoiseNode.Worley;

		newNodes.Add( newNode );

		return newNodes;
	}
}

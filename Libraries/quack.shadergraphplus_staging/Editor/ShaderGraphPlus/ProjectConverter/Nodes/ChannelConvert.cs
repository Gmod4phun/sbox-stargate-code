using ShaderGraphPlus.Nodes;
using VanillaGraph = Editor.ShaderGraph;
using VanillaNodes = Editor.ShaderGraph.Nodes;
using ShaderGraphBaseNode = Editor.ShaderGraph.BaseNode;

namespace ShaderGraphPlus.Internal;

internal class SplitVectorNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.SplitVector );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldSplitVectorNode = oldNode as VanillaNodes.SplitVector;

		//SGPLog.Info( "Convert splitVector node" );

		var newNode = new SplitVector();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class CombineVectorNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.CombineVector );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldCombineVectorNode = oldNode as VanillaNodes.CombineVector;

		//SGPLog.Info( "Convert combineVector node" );

		var newNode = new CombineVector();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.DefaultX = oldCombineVectorNode.DefaultX;
		newNode.DefaultY = oldCombineVectorNode.DefaultY;
		newNode.DefaultZ = oldCombineVectorNode.DefaultZ;
		newNode.DefaultW = oldCombineVectorNode.DefaultW;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class SwizzleVectorNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.SwizzleVector );

	private SwizzleChannel GetSwizzle( VanillaNodes.SwizzleChannel vanillaSwizzleChannel )
	{
		return vanillaSwizzleChannel switch
		{
			VanillaNodes.SwizzleChannel.Red => SwizzleChannel.Red,
			VanillaNodes.SwizzleChannel.Green => SwizzleChannel.Green,
			VanillaNodes.SwizzleChannel.Blue => SwizzleChannel.Blue,
			VanillaNodes.SwizzleChannel.Alpha => SwizzleChannel.Alpha,
			_ => throw new NotImplementedException(),
		};
	}

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldSwizzleVectorNode = oldNode as VanillaNodes.SwizzleVector;

		//SGPLog.Info( "Convert swizzleVector node" );

		var newNode = new SwizzleVector();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.RedOut = GetSwizzle( oldSwizzleVectorNode.RedOut );
		newNode.GreenOut = GetSwizzle( oldSwizzleVectorNode.GreenOut );
		newNode.BlueOut = GetSwizzle( oldSwizzleVectorNode.BlueOut );
		newNode.AlphaOut = GetSwizzle( oldSwizzleVectorNode.AlphaOut );

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class AppendVectorNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.AppendVector );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldAppendVectorNode = oldNode as VanillaNodes.AppendVector;

		//SGPLog.Info( "Convert appendVector node" );

		var newNode = new AppendVector();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

using Editor;
using ShaderGraphPlus.Nodes;
using VanillaGraph = Editor.ShaderGraph;
using VanillaNodes = Editor.ShaderGraph.Nodes;
using ShaderGraphBaseNode = Editor.ShaderGraph.BaseNode;


namespace ShaderGraphPlus.Internal;

internal class AddNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Add );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldAddNode = oldNode as VanillaNodes.Add;

		//SGPLog.Info( "Convert add node" );

		var newNode = new Add();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.DefaultA = oldAddNode.DefaultA;
		newNode.DefaultB = oldAddNode.DefaultB;

		//newNode.SetInputs( oldAddNode.Inputs );

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class SubtractNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Subtract );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldSubtractNode = oldNode as VanillaNodes.Subtract;

		//SGPLog.Info( "Convert subtract node" );

		var newNode = new Subtract();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.DefaultA = oldSubtractNode.DefaultA;
		newNode.DefaultB = oldSubtractNode.DefaultB;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class MultiplyNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Multiply );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldMultiplyNode = oldNode as VanillaNodes.Multiply;

		//SGPLog.Info( "Convert multiply node" );

		var newNode = new Multiply();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.DefaultA = oldMultiplyNode.DefaultA;
		newNode.DefaultB = oldMultiplyNode.DefaultB;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class DivideNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Divide );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldDivideNode = oldNode as VanillaNodes.Divide;

		//SGPLog.Info( "Convert divide node" );

		var newNode = new Divide();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.DefaultA = oldDivideNode.DefaultA;
		newNode.DefaultB = oldDivideNode.DefaultB;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class ModNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Mod );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldModNode = oldNode as VanillaNodes.Mod;

		//SGPLog.Info( "Convert mod node" );

		var newNode = new Mod();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.DefaultA = oldModNode.DefaultA;
		newNode.DefaultB = oldModNode.DefaultB;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class LerpNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Lerp );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldLerpNode = oldNode as VanillaNodes.Lerp;

		//SGPLog.Info( "Convert lerp node" );

		var newNode = new Lerp();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.DefaultA = oldLerpNode.DefaultA;
		newNode.DefaultB = oldLerpNode.DefaultB;
		newNode.Fraction = oldLerpNode.Fraction;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class CrossProductNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.CrossProduct );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldCrossProductNode = oldNode as VanillaNodes.CrossProduct;

		//SGPLog.Info( "Convert crossProduct node" );

		var newNode = new CrossProduct();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.DefaultA = oldCrossProductNode.DefaultA;
		newNode.DefaultB = oldCrossProductNode.DefaultB;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class RemapValueNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.RemapValue );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldRemapValueNode = oldNode as VanillaNodes.RemapValue;

		//SGPLog.Info( "Convert remapValue node" );

		var newNode = new RemapValue();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.In = oldRemapValueNode.In;
		newNode.InMin = oldRemapValueNode.InMin;
		newNode.InMax = oldRemapValueNode.InMax;
		newNode.OutMin = oldRemapValueNode.OutMin;
		newNode.OutMax = oldRemapValueNode.OutMax;
		newNode.Clamp = oldRemapValueNode.Clamp;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class Arctan2NodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Arctan2 );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldArctan2Node = oldNode as VanillaNodes.Arctan2;

		//SGPLog.Info( "Convert arctan2 node" );

		var newNode = new Arctan2();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.DefaultY = oldArctan2Node.DefaultX;
		newNode.DefaultX = oldArctan2Node.DefaultY;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class PowerNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Power );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldPowerNode = oldNode as VanillaNodes.Power;

		//SGPLog.Info( "Convert power node" );

		var newNode = new Power();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.DefaultA = oldPowerNode.DefaultA;
		newNode.DefaultB = oldPowerNode.DefaultB;

		newNodes.Add( newNode );

		return newNodes;
	}
}

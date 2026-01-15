using ShaderGraphPlus.Nodes;
using VanillaGraph = Editor.ShaderGraph;
using VanillaNodes = Editor.ShaderGraph.Nodes;
using ShaderGraphBaseNode = Editor.ShaderGraph.BaseNode;

namespace ShaderGraphPlus.Internal;

internal class WorldNormalNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.WorldNormal );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldWorldNormalNode = oldNode as VanillaNodes.WorldNormal;

		//SGPLog.Info( "Convert worldNormal node" );

		var newNode = new WorldNormal();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class WorldTangentNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.WorldTangent );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldWorldTangentNode = oldNode as VanillaNodes.WorldTangent;

		//SGPLog.Info( "Convert worldTangent node" );

		var newNode = new WorldTangent();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class IsFrontFaceNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.IsFrontFace );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldIsFrontFaceNode = oldNode as VanillaNodes.IsFrontFace;

		//SGPLog.Info( "Convert isFrontFace node" );

		var newNode = new IsFrontFace();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class ObjectSpaceNormalNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.ObjectSpaceNormal );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldObjectSpaceNormalNode = oldNode as VanillaNodes.ObjectSpaceNormal;

		//SGPLog.Info( "Convert ObjectSpaceNormal node" );

		var newNode = new ObjectSpaceNormal();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class ScreenPositionNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.ScreenPosition );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldScreenPositionNode = oldNode as VanillaNodes.ScreenPosition;

		//SGPLog.Info( "Convert screenPosition node" );

		var newNode = new ScreenPosition();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class ScreenCoordinateNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.ScreenCoordinate );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldScreenCoordinateNode = oldNode as VanillaNodes.ScreenCoordinate;

		//SGPLog.Info( "Convert screenCoordinate node" );

		var newNode = new ScreenCoordinate();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class WorldPositionNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.WorldPosition );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldWorldPositionNode = oldNode as VanillaNodes.WorldPosition;

		//SGPLog.Info( "Convert worldPosition node" );

		var newNode = new WorldPosition();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class ObjectPositionNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.ObjectPosition );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldObjectPositionNode = oldNode as VanillaNodes.ObjectPosition;

		//SGPLog.Info( "Convert objectPosition node" );

		var newNode = new ObjectPosition();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class ViewDirectionNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.ViewDirection );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldViewDirectionNode = oldNode as VanillaNodes.ViewDirection;

		//SGPLog.Info( "Convert viewDirection node" );

		var newNode = new ViewDirection();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class VertexColorNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.VertexColor );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldVertexColorNode = oldNode as VanillaNodes.VertexColor;

		//SGPLog.Info( "Convert vertexColor node" );

		var newNode = new VertexColor();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class TintNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Tint );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldTintNode = oldNode as VanillaNodes.Tint;

		//SGPLog.Info( "Convert tint node" );

		var newNode = new Tint();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

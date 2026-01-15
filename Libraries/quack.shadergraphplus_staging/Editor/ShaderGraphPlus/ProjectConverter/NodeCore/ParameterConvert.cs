using Editor;
using ShaderGraphPlus.Nodes;
using VanillaGraph = Editor.ShaderGraph;
using VanillaNodes = Editor.ShaderGraph.Nodes;
using ShaderGraphBaseNode = Editor.ShaderGraph.BaseNode;

namespace ShaderGraphPlus.Internal;

file static class VanillaParameterUIExentions
{
	internal static ParameterUI ConvertVanillaUI( this VanillaGraph.ParameterUI parameterUI )
	{
		var newUi = new ParameterUI();

		newUi.Type = parameterUI.Type switch
		{
			VanillaGraph.UIType.Default => UIType.Default,
			VanillaGraph.UIType.Slider => UIType.Slider,
			VanillaGraph.UIType.Color => UIType.Color,
			_ => throw new NotImplementedException(),
		};

		newUi.Step = parameterUI.Step;
		newUi.Priority = parameterUI.Priority;
		newUi.PrimaryGroup = new() { Name = parameterUI.PrimaryGroup.Name, Priority = parameterUI.PrimaryGroup.Priority };
		newUi.SecondaryGroup = new() { Name = parameterUI.SecondaryGroup.Name, Priority = parameterUI.SecondaryGroup.Priority };

		return newUi;
	}
}

internal class FloatNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Float );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldFloatNode = oldNode as VanillaNodes.Float;

		//SGPLog.Info( "Convert float node" );

		if ( string.IsNullOrWhiteSpace( oldFloatNode.Name ) )
		{
			var newConstantNode = new FloatConstantNode();
			newConstantNode.Value = oldFloatNode.Value;
			newConstantNode.Min = oldFloatNode.Min;
			newConstantNode.Max = oldFloatNode.Max;
			newConstantNode.Step = oldFloatNode.Step;

			newNodes.Add( newConstantNode );
		}
		else
		{
			var newNode = new FloatParameterNode
			{
				BlackboardParameterIdentifier = Guid.NewGuid(),
				Identifier = oldNode.Identifier,
				Position = oldNode.Position,
				Name = oldFloatNode.Name,
				Value = oldFloatNode.Value,
				Min = oldFloatNode.Min,
				Max = oldFloatNode.Max,
				IsAttribute = oldFloatNode.IsAttribute,
				UI = oldFloatNode.UI.ConvertVanillaUI()
			};

			BaseBlackboardParameter blackboardParameter = new FloatParameter()
			{
				Identifier = newNode.BlackboardParameterIdentifier,
				Name = newNode.Name,
				Value = newNode.Value,
				Min = newNode.Min,
				Max = newNode.Max,
				UI = newNode.UI
			};

			converter.AddBlackboardParameter( blackboardParameter );

			newNodes.Add( newNode );
		}

		return newNodes;
	}
}

internal class Float2NodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Float2 );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldFloat2Node = oldNode as VanillaNodes.Float2;

		//SGPLog.Info( "Convert float2 node" );

		if ( string.IsNullOrWhiteSpace( oldFloat2Node.Name ) )
		{
			var newConstantNode = new Float2ConstantNode();
			newConstantNode.Value = oldFloat2Node.Value;
			newConstantNode.Min = oldFloat2Node.Min;
			newConstantNode.Max = oldFloat2Node.Max;
			newConstantNode.Step = oldFloat2Node.Step;

			newNodes.Add( newConstantNode );
		}
		else
		{
			var newNode = new Float2ParameterNode
			{
				BlackboardParameterIdentifier = Guid.NewGuid(),
				Identifier = oldNode.Identifier,
				Position = oldNode.Position,
				Name = oldFloat2Node.Name,
				Value = oldFloat2Node.Value,
				Min = oldFloat2Node.Min,
				Max = oldFloat2Node.Max,
				IsAttribute = oldFloat2Node.IsAttribute,
				UI = oldFloat2Node.UI.ConvertVanillaUI()
			};

			BaseBlackboardParameter blackboardParameter = new Float2Parameter()
			{
				Identifier = newNode.BlackboardParameterIdentifier,
				Name = newNode.Name,
				Value = newNode.Value,
				Min = newNode.Min,
				Max = newNode.Max,
				UI = newNode.UI
			};

			converter.AddBlackboardParameter( blackboardParameter );

			newNodes.Add( newNode );
		}

		return newNodes;
	}
}

internal class Float3NodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Float3 );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldFloat3Node = oldNode as VanillaNodes.Float3;

		//SGPLog.Info( "Convert float3 node" );

		if ( string.IsNullOrWhiteSpace( oldFloat3Node.Name ) )
		{
			var newConstantNode = new Float3ConstantNode();
			newConstantNode.Value = oldFloat3Node.Value;
			newConstantNode.Min = oldFloat3Node.Min;
			newConstantNode.Max = oldFloat3Node.Max;
			newConstantNode.Step = oldFloat3Node.Step;

			newNodes.Add( newConstantNode );
		}
		else
		{
			var newNode = new Float3ParameterNode
			{
				BlackboardParameterIdentifier = Guid.NewGuid(),
				Identifier = oldNode.Identifier,
				Position = oldNode.Position,
				Name = oldFloat3Node.Name,
				Value = oldFloat3Node.Value,
				Min = oldFloat3Node.Min,
				Max = oldFloat3Node.Max,
				IsAttribute = oldFloat3Node.IsAttribute,
				UI = oldFloat3Node.UI.ConvertVanillaUI()
			};

			BaseBlackboardParameter blackboardParameter = new Float3Parameter()
			{
				Identifier = newNode.BlackboardParameterIdentifier,
				Name = newNode.Name,
				Value = newNode.Value,
				Min = newNode.Min,
				Max = newNode.Max,
				UI = newNode.UI
			};

			converter.AddBlackboardParameter( blackboardParameter );

			newNodes.Add( newNode );
		}

		return newNodes;
	}
}

internal class Float4NodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Float4 );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldFloat4Node = oldNode as VanillaNodes.Float4;

		//SGPLog.Info( "Convert float4 node" );

		if ( string.IsNullOrWhiteSpace( oldFloat4Node.Name ) )
		{
			var newConstantNode = new ColorConstantNode();
			newConstantNode.Value = oldFloat4Node.Value;

			newNodes.Add( newConstantNode );
		}
		else
		{
			var newNode = new ColorParameterNode
			{
				BlackboardParameterIdentifier = Guid.NewGuid(),
				Identifier = oldNode.Identifier,
				Position = oldNode.Position,
				Value = oldFloat4Node.Value,
				Name = oldFloat4Node.Name,
				IsAttribute = oldFloat4Node.IsAttribute,
				UI = oldFloat4Node.UI.ConvertVanillaUI()
			};

			BaseBlackboardParameter blackboardParameter = new ColorParameter()
			{
				Identifier = newNode.BlackboardParameterIdentifier,
				Name = newNode.Name,
				Value = newNode.Value,
				UI = newNode.UI,
			};

			converter.AddBlackboardParameter( blackboardParameter );

			newNodes.Add( newNode );
		}

		return newNodes;
	}
}

using VanillaGraph = Editor.ShaderGraph;
using VanillaNodes = Editor.ShaderGraph.Nodes;
using ShaderGraphBaseNode = Editor.ShaderGraph.BaseNode;

namespace ShaderGraphPlus.Internal;

internal class FunctionResultConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaGraph.FunctionResult );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldFunctionResult = oldNode as VanillaGraph.FunctionResult;

		//SGPLog.Info( "Convert functionResult node" );

		Vector2 lastOffset = Vector2.Zero;
		foreach ( var oldInput in oldFunctionResult.FunctionOutputs )
		{
			var newSubgraphOutput = new SubgraphOutput();
			lastOffset.y += 64;
			newSubgraphOutput.Position = oldFunctionResult.Position + new Vector2( 0, lastOffset.y );
			newSubgraphOutput.OutputName = oldInput.Name;
			newSubgraphOutput.OutputDescription = "";
			newSubgraphOutput.SetSubgraphPortTypeFromType( oldInput.Type );
			newSubgraphOutput.Preview = oldInput.Preview switch
			{
				VanillaGraph.FunctionOutput.PreviewType.None => SubgraphOutputPreviewType.None,
				VanillaGraph.FunctionOutput.PreviewType.Albedo => SubgraphOutputPreviewType.Albedo,
				VanillaGraph.FunctionOutput.PreviewType.Emission => SubgraphOutputPreviewType.Emission,
				VanillaGraph.FunctionOutput.PreviewType.Opacity => SubgraphOutputPreviewType.Opacity,
				VanillaGraph.FunctionOutput.PreviewType.Normal => SubgraphOutputPreviewType.Normal,
				VanillaGraph.FunctionOutput.PreviewType.Roughness => SubgraphOutputPreviewType.Roughness,
				VanillaGraph.FunctionOutput.PreviewType.Metalness => SubgraphOutputPreviewType.Metalness,
				VanillaGraph.FunctionOutput.PreviewType.AmbientOcclusion => SubgraphOutputPreviewType.AmbientOcclusion,
				VanillaGraph.FunctionOutput.PreviewType.PositionOffset => SubgraphOutputPreviewType.PositionOffset,
				_ => throw new NotImplementedException(),
			};
			newSubgraphOutput.PortOrder = oldInput.Priority;

			newSubgraphOutput.InitializeNode();

			converter.AddNewSubgraphOutputID( newSubgraphOutput.OutputName );

			newNodes.Add( newSubgraphOutput );
		}

		return newNodes;
	}
}

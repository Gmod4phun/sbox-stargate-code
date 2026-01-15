
namespace ShaderGraphPlus.Nodes;

/// <summary>
/// Returns the tangent view vector, which is the direction from the camera to the position in tangent space.
/// </summary>
[Title( "Get Tangent View Vector" ), Category( "Utility" ), Icon( "visibility" )]
public sealed class GetTangentViewVectorNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.FunctionNode;

	[Title( "Position" ), Hide]
	[Input( typeof( Vector3 ) )]
	public NodeInput WorldSpacePosition { get; set; }

	[Title( "Normal" ), Hide]
	[Input( typeof( Vector3 ) )]
	public NodeInput WorldNormal { get; set; }

	[Title( "World Tangent U" ), Hide]
	[Input( typeof( Vector3 ) )]
	public NodeInput TangentUWs { get; set; }

	[Title( "World Tangent V" ), Hide]
	[Input( typeof( Vector3 ) )]
	public NodeInput TangentVWs { get; set; }

	public GetTangentViewVectorNode()
	{
		ExpandSize = new Vector2( 32, 0 );
	}

	[Output( typeof( Vector3 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		if ( compiler.Graph.Domain is ShaderDomain.PostProcess )
		{
			return NodeResult.Error( $"{DisplayInfo.Name} Is not ment for postprocessing shaders!" );
		}

		var worldPosition = compiler.Result( WorldSpacePosition );
		var worldNormal = compiler.Result( WorldNormal );
		var tangentUws = compiler.Result( TangentUWs );
		var tangentVws = compiler.Result( TangentVWs );

		var result = compiler.ResultHLSLFunction( "GetTangentViewVector",
				$"{(worldPosition.IsValid ? worldPosition : "i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz")}",
				$"{(worldNormal.IsValid ? worldNormal : "i.vNormalWs")}",
				$"{(tangentUws.IsValid ? tangentUws : "i.vTangentUWs")}",
				$"{(tangentVws.IsValid ? tangentVws : "i.vTangentVWs")}"
		);

		return new NodeResult( ResultType.Vector3, $"{result}" );
	};
}

namespace ShaderGraphPlus.Nodes;

/// <summary>
/// Sample depth texture
/// </summary>
[Title( "Scene Depth" ), Category( "Camera" )]
public sealed class Depth : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.GlobalVariableNode;

	public enum DepthSamplingMode
	{
		/// <summary>
		/// Depth value as-is from the depth buffer.
		/// </summary>
		Raw,
		/// <summary>
		/// Normalized depth value.
		/// </summary>
		Normalized,
		/// <summary>
		/// Linearized depth value, which is absolute coordinates away from the camera
		/// </summary>
		Linear
	}

	[Hide]
	public override string Title => $"{DisplayInfo.For( this ).Name} ({SamplingMode})";

	[Input( typeof( Vector2 ) ), Title( "Screen Pos" ), Hide]
	public NodeInput ScreenPosition { get; set; }

	/// <summary>
	/// How to sample the depth buffer.
	/// </summary>
	public DepthSamplingMode SamplingMode { get; set; } = DepthSamplingMode.Linear;

	[Output( typeof( float ) ), Hide]
	public NodeResult.Func Out => ( GraphCompiler compiler ) =>
	{
		var result = ScreenPosition.IsValid() ? compiler.Result( ScreenPosition ).Cast( 2 ) :
			compiler.IsVs ? "i.vPositionPs.xy" : "i.vPositionSs.xy";

		string funcCall = "";
		switch ( SamplingMode )
		{
			case DepthSamplingMode.Raw: funcCall = $"Depth::Get( {result} )"; break;
			case DepthSamplingMode.Normalized: funcCall = $"Depth::GetNormalized( {result} )"; break;
			case DepthSamplingMode.Linear: funcCall = $"Depth::GetLinear( {result} )"; break;
			default: SGPLog.Error( $"Unknown Mode : \"{SamplingMode}\"" ); break;
		}

		return new NodeResult( ResultType.Float, funcCall );
	};
}

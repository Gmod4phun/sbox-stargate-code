
namespace ShaderGraphPlus.Nodes;

/// <summary>
/// If True, do this, if False, do that.
/// </summary>
[Title( "Switch" ), Category( "Utility/Logic" ), Icon( "alt_route" )]
public sealed class SwitchNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.LogicNode;

	[Hide]
	private bool IsSubgraph => (Graph is ShaderGraphPlus shaderGraph && shaderGraph.IsSubgraph);

	[Title( "Predicate" )]
	[Input( typeof( bool ) ), Hide]
	public NodeInput InputPredicate { get; set; }

	[Input, Hide]
	public NodeInput True { get; set; }

	[Input, Hide]
	public NodeInput False { get; set; }

	public string Name { get; set; } = "";

	public bool IsAttribute { get; set; } = true;

	public bool Enabled { get; set; } = true;

	[InlineEditor( Label = false ), Group( "UI" )]
	public ParameterUI UI { get; set; }

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var results = compiler.Result( True, False, 0.0f, 0.0f );
		var resultPredicate = compiler.Result( InputPredicate );

		if ( resultPredicate.IsValid )
		{
			return new NodeResult( results.Item1.ResultType, $"{resultPredicate} ? {results.Item1} : {results.Item2}" );
		}
		else
		{
			return new NodeResult( results.Item1.ResultType, $"{(compiler.ResultParameter( Name, Enabled, default, default, false, IsAttribute, UI ))} ?" +
				$" {results.Item1} :" +
				$" {results.Item2}" );
		}
	};
}

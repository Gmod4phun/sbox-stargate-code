
namespace ShaderGraphPlus.Nodes;

/*
[Title( "If" ), Category( "Utility/Logic" ), Icon( "alt_route" )]
public sealed class IfNode : ShaderNodePlus, IWarningNode
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color PrimaryHeaderColor => PrimaryNodeHeaderColors.LogicNode;

	[Hide]
	public override string Title => $"{DisplayInfo.For( this ).Name} (A {Op} B)";

	[Hide]
	private bool IsSubgraph => (Graph is ShaderGraphPlus shaderGraph && shaderGraph.IsSubgraph);

	[Input, Hide]
	public NodeInput A { get; set; }

	[Input, Hide]
	public NodeInput B { get; set; }

	[Input, Hide]
	[Title( "A > B" )]
	public NodeInput InputC { get; set; }

	[Input, Hide]
	[Title( "A == B" )]
	public NodeInput InputD { get; set; }

	[Input, Hide]
	[Title( "A < B" )]
	public NodeInput InputE { get; set; }

	public string Name { get; set; } = "";

	public enum OperatorType
	{
		Equal,
		NotEqual,
		GreaterThan,
		LessThan,
		GreaterThanOrEqual,
		LessThanOrEqual
	}

	public OperatorType Operator { get; set; }

	[InlineEditor( Label = false ), Group( "UI" )]
	public ParameterUI UI { get; set; }

	[Hide]
	private string Op
	{
		get
		{
			return Operator switch
			{
				OperatorType.Equal => "==",
				OperatorType.NotEqual => "!=",
				OperatorType.GreaterThan => ">",
				OperatorType.LessThan => "<",
				OperatorType.GreaterThanOrEqual => ">=",
				OperatorType.LessThanOrEqual => "<=",
				_ => throw new System.NotImplementedException(),
			};
		}
	}

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		//var results = compiler.Result( True, False, 0.0f, 0.0f );
		//var resultA = compiler.ResultOrDefault( A, 0.0f );
		//var resultB = compiler.ResultOrDefault( B, 0.0f );
		//
		//var result = $"{resultA.Cast( 1 )} {Op} {resultB.Cast( 1 )} ? {results.Item1} : {results.Item2}";
		//
		//return new NodeResult( results.Item1.ResultType, $"{result}" );
		return new NodeResult( ResultType.Float, $"0.0f" );
	};

	public List<string> GetWarnings()
	{
		var warnings = new List<string>();

		return warnings;
	}
}
*/

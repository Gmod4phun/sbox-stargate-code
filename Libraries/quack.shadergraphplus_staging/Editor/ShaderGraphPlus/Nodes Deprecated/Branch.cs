
namespace ShaderGraphPlus.Nodes;

// TODO : Remove in future release.
/// <summary>
/// If True, do this, if False, do that.
/// Give it a name to use a bool attribute.
/// Use no name to use condition from A and B inputs.
/// </summary>
[Title( "Branch" ), Category( "Utility/Logic" ), Icon( "alt_route" )]
[InternalNode]
public sealed class Branch : ShaderNodePlus, IWarningNode
{
	[Hide]
	public override int Version => 1;

	[Hide]
	public override string Title => UseCondition ?
		$"{DisplayInfo.For( this ).Name} (A {Op} B)" :
		$"{DisplayInfo.For( this ).Name}  {(!InputPredicate.IsValid ? $"( {Name} )" : $"")}";

	[Hide]
	private bool IsSubgraph => (Graph is ShaderGraphPlus shaderGraph && shaderGraph.IsSubgraph);

	[Hide]
	private bool UseCondition => string.IsNullOrWhiteSpace( Name );

	[Title( "Predicate" )]
	[Input( typeof( bool ) ), Hide]
	public NodeInput InputPredicate { get; set; }

	[Input, Hide]
	public NodeInput True { get; set; }

	[Input, Hide]
	public NodeInput False { get; set; }

	[Input, Hide]
	public NodeInput A { get; set; }

	[Input, Hide]
	public NodeInput B { get; set; }


	public string Name { get; set; } = "";

	public bool IsAttribute { get; set; } = true;

	public enum OperatorType
	{
		Equal,
		NotEqual,
		GreaterThan,
		LessThan,
		GreaterThanOrEqual,
		LessThanOrEqual
	}

	[HideIf( nameof( UseCondition ), false )]
	public OperatorType Operator { get; set; }

	[HideIf( nameof( UseCondition ), true )]
	public bool Enabled { get; set; }

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
		var useCondition = UseCondition;
		var results = compiler.Result( True, False, 0.0f, 0.0f );
		var resultA = useCondition ? compiler.ResultOrDefault( A, 0.0f ) : default;
		var resultB = useCondition ? compiler.ResultOrDefault( B, 0.0f ) : default;
		var resultPredicate = compiler.Result( InputPredicate );

		if ( resultPredicate.IsValid )
		{
			return new NodeResult( results.Item1.ResultType, $"{resultPredicate} ? {results.Item1} : {results.Item2}" );
		}
		else
		{
			return new NodeResult( results.Item1.ResultType, $"{(useCondition ?
				$"{resultA.Cast( 1 )} {Op} {resultB.Cast( 1 )}" : compiler.ResultParameter( Name, Enabled, default, default, false, IsAttribute, UI ))} ?" +
				$" {results.Item1} :" +
				$" {results.Item2}" );
		}
	};

	public List<string> GetWarnings()
	{
		var warnings = new List<string>();

		warnings.Add( $"Branch Node is depreciated and will be removed in a future update. Use \"Switch\" node for a true or false switch. And \"Comparison\" node for value comparisons." );

		HasWarning = true;

		return warnings;
	}
}

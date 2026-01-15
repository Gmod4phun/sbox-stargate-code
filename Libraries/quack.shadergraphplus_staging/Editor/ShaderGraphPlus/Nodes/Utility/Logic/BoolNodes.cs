namespace ShaderGraphPlus.Nodes;

/// <summary>
/// Checks if all components of the specified input are non-zero.
/// </summary>
[Title( "All" ), Category( "Utility/Logic" ), Icon( "question_mark" )]
public sealed class AllNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.LogicNode;

	[Input]
	[Hide]
	public NodeInput Input { get; set; }

	[Output( typeof( bool ) ), Title( "Result" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var inputResult = compiler.ResultOrDefault( Input, 0.0f );

		if ( !inputResult.ResultType.IsFloatOrVectorFloatType() )
		{
			return NodeResult.Error( "Input must be a float or vector float type!" );
		}

		return new NodeResult( ResultType.Bool, $"all( {inputResult} )" );
	};
}

/// <summary>
/// Returns true if both boolean inputs A and B are true.
/// </summary>
[Title( "And" ), Category( "Utility/Logic" ), Icon( "question_mark" )]
public sealed class AndNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.LogicNode;

	[Input( typeof( bool ) ), Title( "A" )]
	[Hide]
	public NodeInput InputA { get; set; }

	[Input( typeof( bool ) ), Title( "B" )]
	[Hide]
	public NodeInput InputB { get; set; }

	[Output( typeof( bool ) ), Title( "Result" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var inputResultA = compiler.ResultOrDefault( InputA, false );
		var inputResultB = compiler.ResultOrDefault( InputB, false );

		if ( inputResultA.ResultType != ResultType.Bool )
		{
			return NodeResult.Error( "Input A must be of type bool!" );
		}

		if ( inputResultB.ResultType != ResultType.Bool )
		{
			return NodeResult.Error( "Input B must be of type bool!" );
		}

		return new NodeResult( ResultType.Bool, $"( {inputResultA} && {inputResultB} )" );
	};
}

/// <summary>
/// Checks if any components of the specified input are non-zero.
/// </summary>
[Title( "Any" ), Category( "Utility/Logic" ), Icon( "question_mark" )]
public sealed class AnyNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.LogicNode;

	[Input]
	[Hide]
	public NodeInput Input { get; set; }

	[Output( typeof( bool ) ), Title( "Result" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var inputResult = compiler.ResultOrDefault( Input, 0.0f );

		if ( !inputResult.ResultType.IsFloatOrVectorFloatType() )
		{
			return NodeResult.Error( "Input must be a float or vector float type!" );
		}

		return new NodeResult( ResultType.Bool, $"any( {inputResult} )" );
	};
}

/// <summary>
/// Checks if the specified input is infinite.
/// </summary>
[Title( "Is Infinite" ), Category( "Utility/Logic" ), Icon( "all_inclusive" )]
public sealed class IsInfiniteNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.LogicNode;

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Input { get; set; }

	[Output( typeof( bool ) ), Title( "Result" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var inputResult = compiler.ResultOrDefault( Input, 0.0f );

		if ( inputResult.ResultType != ResultType.Float )
		{
			return NodeResult.Error( "Input must be of type float!" );
		}

		return new NodeResult( ResultType.Bool, $"isinf( {inputResult} )" );
	};
}

/// <summary>
/// Checks if the specified input is NAN.
/// </summary>
[Title( "Is Nan" ), Category( "Utility/Logic" ), Icon( "question_mark" )]
public sealed class IsNanNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.LogicNode;

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Input { get; set; }

	[Output( typeof( bool ) ), Title( "Result" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var inputResult = compiler.ResultOrDefault( Input, 0.0f );

		if ( inputResult.ResultType != ResultType.Float )
		{
			return NodeResult.Error( "Input must be of type float!" );
		}

		return new NodeResult( ResultType.Bool, $"( {inputResult} < 0.0f || {inputResult} > 0.0f || {inputResult} == 0.0f ) ? false : true;" );
	};
}

/// <summary>
/// Returns true if both boolean inputs A and B are false.
/// </summary>
[Title( "Nand" ), Category( "Utility/Logic" ), Icon( "question_mark" )]
public sealed class NandNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.LogicNode;

	[Input( typeof( bool ) ), Title( "A" )]
	[Hide]
	public NodeInput InputA { get; set; }

	[Input( typeof( bool ) ), Title( "B" )]
	[Hide]
	public NodeInput InputB { get; set; }

	[Output( typeof( bool ) ), Title( "Result" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var inputResultA = compiler.ResultOrDefault( InputA, false );
		var inputResultB = compiler.ResultOrDefault( InputB, false );

		if ( inputResultA.ResultType != ResultType.Bool )
		{
			return NodeResult.Error( "Input A must be of type bool!" );
		}

		if ( inputResultB.ResultType != ResultType.Bool )
		{
			return NodeResult.Error( "Input B must be of type bool!" );
		}

		return new NodeResult( ResultType.Bool, $"( !{inputResultA} && !{inputResultB} )" );
	};
}

/// <summary>
/// Returns the opposite of the boolean input.
/// </summary>
[Title( "Not" ), Category( "Utility/Logic" ), Icon( "question_mark" )]
public sealed class NotNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.LogicNode;

	[Input( typeof( bool ) )]
	[Hide]
	public NodeInput Input { get; set; }

	[Output( typeof( bool ) ), Title( "Result" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var inputResult = compiler.ResultOrDefault( Input, false );

		if ( inputResult.ResultType != ResultType.Bool )
		{
			return NodeResult.Error( "Input must be of type bool!" );
		}

		return new NodeResult( ResultType.Bool, $"!( {inputResult} )" );
	};
}

/// <summary>
/// Return true if either of the boolean inputs A and B are true.
/// </summary>
[Title( "Or" ), Category( "Utility/Logic" ), Icon( "question_mark" )]
public sealed class OrNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.LogicNode;

	[Input( typeof( bool ) ), Title( "A" )]
	[Hide]
	public NodeInput InputA { get; set; }

	[Input( typeof( bool ) ), Title( "B" )]
	[Hide]
	public NodeInput InputB { get; set; }

	[Output( typeof( bool ) ), Title( "Result" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var inputResultA = compiler.ResultOrDefault( InputA, false );
		var inputResultB = compiler.ResultOrDefault( InputB, false );

		if ( inputResultA.ResultType != ResultType.Bool )
		{
			return NodeResult.Error( "Input A must be of type bool!" );
		}

		if ( inputResultB.ResultType != ResultType.Bool )
		{
			return NodeResult.Error( "Input B must be of type bool!" );
		}

		return new NodeResult( ResultType.Bool, $"( {inputResultA} || {inputResultB} )" );
	};
}

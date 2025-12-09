using ShaderGraphPlus.Diagnostics;
using ShaderGraphPlus.Utilities;
using Assert = ShaderGraphPlus.Diagnostics.Assert;

namespace ShaderGraphPlus.Nodes;

public enum VoidFunctionArgumentType
{
	Input,
	Output
}

public struct VoidFunctionArgument
{
	public string TargetProperty;
	public string DefaultTargetProperty;
	public ResultType ResultType;
	public string VarName;
	public VoidFunctionArgumentType ArgumentType;

	public VoidFunctionArgument( string targetProperty, string varName, VoidFunctionArgumentType argumentType, ResultType resultType )
	{
		TargetProperty = targetProperty;
		DefaultTargetProperty = "";
		ArgumentType = argumentType;
		ResultType = resultType;
		VarName = varName;
	}

	public VoidFunctionArgument( string targetProperty, string defaultTargetProperty, string varName, VoidFunctionArgumentType argumentType, ResultType resultType )
	: this( targetProperty, varName, argumentType, resultType )
	{
		if ( ArgumentType != VoidFunctionArgumentType.Input && !string.IsNullOrWhiteSpace( defaultTargetProperty ) )
		{
			EdtiorSound.OhFiddleSticks();
			throw new Exception( $"`defaultTargetProperty` should not be set if the argument type is not an `{VoidFunctionArgumentType.Input}`" );
		}

		DefaultTargetProperty = !string.IsNullOrWhiteSpace( defaultTargetProperty ) ? defaultTargetProperty : "";
	}

}

internal interface IVoidFunctionNode
{
	public void Register( GraphCompiler compiler );
	public void RegisterVoidFunction( GraphCompiler compiler );
}

public abstract class VoidFunctionBase : ShaderNodePlus, IVoidFunctionNode
{
	/// <summary>
	/// Register anything that this node uses.
	/// </summary>
	/// <param name="compiler"></param>
	public virtual void Register( GraphCompiler compiler )
	{
	}

	public virtual void BuildFunctionCall( GraphCompiler compiler, ref List<VoidFunctionArgument> args, ref string functionName, ref string functionCall )
	{
	}

	public void RegisterVoidFunction( GraphCompiler compiler )
	{
		Dictionary<string, string> outputs = new();
		var args = new List<VoidFunctionArgument>();
		var functionName = "";
		var functionCall = "";

		BuildFunctionCall( compiler, ref args, ref functionName, ref functionCall );

		Assert.CheckAreNotEqual( args.Count, 0, $"args.Count == \"{args.Count}\"" );

		if ( !string.IsNullOrWhiteSpace( functionName ) )
		{
			compiler.RegisterVoidFunction( functionCall, Identifier, args, out var functionOutputs );

			foreach ( var funcOutput in functionOutputs )
			{
				var propertyInfo = this.GetType().GetProperty( funcOutput.userAssigned );

				if ( propertyInfo != null )
				{
					propertyInfo.SetValue( this, funcOutput.compilerAssigned, null );
				}
			}
		}

		Assert.CheckAreNotEqual( functionName, "", $"functionName is empty!" );
	}
}

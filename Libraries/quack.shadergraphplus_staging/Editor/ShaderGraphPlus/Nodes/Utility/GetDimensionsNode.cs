
namespace ShaderGraphPlus.Nodes;

/// <summary>
/// Get the dimensions of a Texture2D Object in the width and height.
/// </summary>
[Title( "Get Texture Dimensions" ), Category( "Textures" ), Icon( "straighten" )]
public sealed class GetDimensionsNode : VoidFunctionBase
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.FunctionNode;

	[Title( "Texture" )]
	[Input( typeof( Texture2DObject ) )]
	[Hide]
	public NodeInput TextureInput { get; set; }

	[JsonIgnore, Hide]
	public override bool CanPreview => false;

	[JsonIgnore, Hide]
	public string TextureObjectSize { get; set; } = "";

	public override void BuildFunctionCall( GraphCompiler compiler, ref List<VoidFunctionArgument> args, ref string functionName, ref string functionCall )
	{
		args.Add( new VoidFunctionArgument( nameof( TextureInput ), "$in0", VoidFunctionArgumentType.Input, ResultType.TextureCubeObject ) );
		args.Add( new VoidFunctionArgument( nameof( TextureObjectSize ), "$out0", VoidFunctionArgumentType.Output, ResultType.Vector2 ) );

		var textureInput = compiler.Result( TextureInput );
		var textureGlobal = "";

		if ( textureInput.IsValid )
		{
			var textureInputData = textureInput.GetMetadata<TextureInput>( "TextureInput" );
			textureGlobal = $"g_t{GraphCompiler.CleanName( textureInputData.Name )}";
		}

		functionName = $"{textureGlobal}.GetDimensions";
		functionCall = $"{functionName}( {args[1].VarName}.x, {args[1].VarName}.y )";
	}

	[Output( typeof( Vector2 ) )]
	[Title( "Size" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) => new NodeResult( ResultType.Vector2, TextureObjectSize, constant: false );
}

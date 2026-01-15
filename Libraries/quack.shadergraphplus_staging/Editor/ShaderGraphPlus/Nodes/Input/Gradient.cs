
namespace ShaderGraphPlus.Nodes;

/// <summary>
/// Constant gradient value.
/// </summary>
[Title( "Gradient" ), Category( "Constants/Gradient" ), Icon( "gradient" ), Order( 6 )]
public sealed class GradientConstantNode : ShaderNodePlus, IConstantNode
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.ConstantValueNode;

	[Hide]
	public override string Title => string.IsNullOrEmpty( Name ) ?
	$"{DisplayInfo.For( this ).Name}" :
	$"{DisplayInfo.For( this ).Name} ({Name})";

	/// <summary>
	/// Name of the gradient.
	/// </summary>
	public string Name { get; set; }

	[InlineEditor]
	public Gradient Gradient { get; set; } = new Gradient();

	//public Gradient.BlendMode blendMode { get; set; } 

	[Output( typeof( Gradient ) )]
	[Hide, NodeValueEditor( nameof( Gradient ) )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		if ( Gradient.Colors.Count > 8 )
		{
			return NodeResult.Error( $"{DisplayInfo.Name} has {Gradient.Colors.Count} color keys which is greater than the maximum amount of 8 allowed color keys." );
		}
		else
		{
			// Register gradient with the compiler.
			var result = compiler.RegisterGradient( Gradient, Name );

			// Return the gradent name that will only be used to search for it in a dictonary.
			return new NodeResult( ResultType.Gradient, result, constant: true );
		}
	};

	public object GetValue()
	{
		return Gradient;
	}

	public BaseBlackboardParameter InitializeMaterialParameter( string name )
	{
		throw new NotImplementedException();
	}

	public BaseNodePlus InitializeMaterialParameterNode()
	{
		throw new NotImplementedException();
	}

	public BaseBlackboardParameter InitializeSubgraphInputParameter( string name )
	{
		return new GradientSubgraphInputParameter( name, Gradient )
		{
		};
	}
}

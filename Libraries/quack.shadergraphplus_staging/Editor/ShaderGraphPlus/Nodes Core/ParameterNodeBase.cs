
namespace ShaderGraphPlus;

public interface IParameterNode
{
	string Name { get; set; }

	bool IsAttribute { get; set; }

	ParameterUI UI { get; set; }
}

public interface ITextureParameterNode
{
	string Image { get; set; }
	TextureInput UI { get; set; }

	/// <summary>
	/// Only used by Preview.
	/// </summary>
	bool AlreadyRegisterd { get; set; }
}

public abstract class ParameterNodeBase<T> : ShaderNodePlus, IParameterNode, IBlackboardSyncableNode, IErroringNode//, IReplaceNode
{
	[Hide]
	protected bool IsSubgraph => (Graph is ShaderGraphPlus shaderGraph && shaderGraph.IsSubgraph);

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.ParameterNode;

	[Hide, Browsable( false )]
	public Guid BlackboardParameterIdentifier { get; set; }

	[JsonIgnore, Hide, Browsable( false )]
	public override string Title => string.IsNullOrWhiteSpace( Name ) ?
		$"{DisplayInfo.For( this ).Name}" :
		$"{Name}";

	//[JsonIgnore, Hide, Browsable( false )]
	//public override string Subtitle => !string.IsNullOrWhiteSpace( Name ) ? Name : "";

	public T Value { get; set; }

	[HideIf( nameof( IsSubgraph ), true )]
	public string Name { get; set; } = "";

	/// <summary>
	/// If true, this parameter can be modified with <see cref="RenderAttributes"/>.
	/// </summary>
	[HideIf( nameof( IsSubgraph ), true )]
	public bool IsAttribute { get; set; }

	[InlineEditor( Label = false ), Group( "UI" )]
	public ParameterUI UI { get; set; }

	protected NodeResult Component( string component, float value, GraphCompiler compiler )
	{
		if ( compiler.IsPreview )
			return compiler.ResultValue( value );

		var result = compiler.Result( new NodeInput { Identifier = Identifier, Output = nameof( Result ) } );
		return new( ResultType.Float, $"{result}.{component}", true );
	}

	public virtual void UpdateFromBlackboard( BaseBlackboardParameter parameter )
	{
	}

	public List<string> GetErrors()
	{
		var errors = new List<string>();

		//if ( Name.Contains( ' ' ) )
		//{
		//	errors.Add( $"Parameter name \"{Name}\" cannot contain spaces" );
		//}

		return errors;
	}
}

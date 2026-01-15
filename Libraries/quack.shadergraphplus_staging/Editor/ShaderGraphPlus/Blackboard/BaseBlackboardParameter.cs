namespace ShaderGraphPlus;

internal interface ISubgraphInputBlackboardParameter
{
	public string Name { get; set; }
	public string Description { get; set; }
	public bool IsRequired { get; set; }
	public int PortOrder { get; set; }

	public object GetValue();
}

internal interface IShaderFeatureParameter
{
}

internal interface IBlackboardSyncableNode
{
	Guid BlackboardParameterIdentifier { get; set; }
	void UpdateFromBlackboard( BaseBlackboardParameter parameter );
}

public abstract class BaseBlackboardParameter : IValid, IBlackboardParameter
{
	[Hide, Editor( "sgp_guidreadonly" ), Sandbox.ReadOnly, Browsable( false )]
	public Guid Identifier { get; set; }

	[JsonIgnore, Hide, Browsable( false )]
	public DisplayInfo DisplayInfo { get; }

	[Hide, JsonIgnore, Browsable( false )]
	private ShaderGraphPlus _graph;
	[Hide, JsonIgnore, Browsable( false )]
	public ShaderGraphPlus Graph
	{
		get => _graph;
		set
		{
			_graph = value;
		}
	}

	[Hide, JsonIgnore, Browsable( false )]
	public bool IsSubgraph => Graph.IsSubgraph;

	[Hide, JsonIgnore, Browsable( false )]
	public virtual bool IsValid => true;

	public virtual string Name { get; set; }

	public BaseBlackboardParameter()
	{
		DisplayInfo = DisplayInfo.For( this );
		NewIdentifier();
		Name = "";
	}

	public BaseBlackboardParameter( string name ) : this()
	{
		Name = name;
	}

	public virtual object GetValue()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Called when a blackboard parameter needs to 
	/// get converted into an accompanying node.
	/// </summary>
	public abstract BaseNodePlus InitializeNode();

	public Guid NewIdentifier()
	{
		Identifier = Guid.NewGuid();
		return Identifier;
	}

	public string GetCleanName()
	{
		if ( string.IsNullOrWhiteSpace( Name ) )
			return "";

		Name = Name.Trim();
		Name = new string( Name.Where( x => char.IsLetter( x ) || char.IsNumber( x ) || x == '_' ).ToArray() );

		return Name;
	}

	public override string ToString()
	{
		return $"{DisplayInfo.Fullname}.{Identifier}";
	}

	/// <summary>
	/// Check parameter for any issues.
	/// </summary>
	/// <param name="issues">Any issues that are found.</param>
	/// <returns>False when check has failed otherwise returns true when check has passed.</returns>
	public bool CheckParameter( out List<string> issues )
	{
		issues = new List<string>();

		if ( string.IsNullOrWhiteSpace( Name ) )
		{
			issues.Add( $"Parameter with identifier \"{Identifier}\" must have name!" );

			return false;
		}

		if ( this is ShaderFeatureEnumParameter featureEnumParameter )
		{
			foreach ( var option in featureEnumParameter.Options.Index() )
			{
				if ( !string.IsNullOrWhiteSpace( option.Item ) )
				{
					continue;
				}

				if ( !string.IsNullOrWhiteSpace( featureEnumParameter.Name ) )
				{
					issues.Add( $"Option at element index \"{option.Index}\" of enum shader feature \"{featureEnumParameter.Name}\" is blank!" );

					return false;
				}
				else
				{
					issues.Add( $"Option at element index \"{option.Index}\" of enum shader feature with identifier \"{Identifier}\" is blank!" );

					return false;
				}
			}
		}

		return true;
	}
}

public abstract class BlackboardGenericParameter<T> : BaseBlackboardParameter
{
	[InlineEditor( Label = false ), Group( "Value" )]
	public T Value { get; set; }

	public BlackboardGenericParameter() : base()
	{
	}

	public BlackboardGenericParameter( string name, T value ) : base( name )
	{
		Value = value;
	}

	public override object GetValue()
	{
		return Value;
	}
}

public abstract class BlackboardMaterialParameter<T> : BlackboardGenericParameter<T>
{
	[InlineEditor( Label = false ), Group( "UI" )]
	public ParameterUI UI { get; set; }

	public bool IsAttribute { get; set; }

	public BlackboardMaterialParameter() : base()
	{
		UI = new ParameterUI();
		IsAttribute = false;
	}

	public BlackboardMaterialParameter( string name, T value, bool isAttribute ) : this()
	{
		Name = name;
		Value = value;
		IsAttribute = isAttribute;
	}
}

public abstract class BlackboardSubgraphInputParameter<T> : BlackboardGenericParameter<T>, ISubgraphInputBlackboardParameter
{
	[Title( "Input Name" )]
	public override string Name { get; set; }

	/// <summary>
	/// Tooltip description of the input when hovering over with the mouse.
	/// </summary>
	[Title( "Input Description" )]
	[TextArea]
	public string Description { get; set; }

	/// <summary>
	/// Is this input required to have a valid connection?
	/// </summary>
	public bool IsRequired { get; set; }

	/// <summary>
	/// In what order will this input port on an instanced subgraph node.
	/// </summary>
	public int PortOrder { get; set; }

	public BlackboardSubgraphInputParameter() : base()
	{
		Name = "";
		Description = "";
		IsRequired = false;
		PortOrder = 0;
	}

	public BlackboardSubgraphInputParameter( string name, T value ) : this()
	{
		Name = name;
		Value = value;
	}

	public override object GetValue()
	{
		return Value;
	}
}

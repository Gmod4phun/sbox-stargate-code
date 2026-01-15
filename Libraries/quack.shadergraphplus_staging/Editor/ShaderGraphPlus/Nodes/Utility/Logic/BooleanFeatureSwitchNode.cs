using NodeEditorPlus;

namespace ShaderGraphPlus.Nodes;

[Title( "Boolean Combo Switch" ), Category( "Utility/Logic" ), Icon( "alt_route" )]
[InternalNode]
public sealed class BooleanFeatureSwitchNode : ShaderNodePlus, IBlackboardSyncableNode
{
	[Hide]
	public override int Version => 1;

	[Hide]
	public override string Title
	{
		get
		{
			return $"F_{Feature.Name.ToUpper().Replace( " ", "_" )}";
		}
	}

	[Hide, JsonIgnore, Browsable( false )]
	public override Color NodeTitleColor { get; set; } = PrimaryNodeHeaderColors.LogicNode;

	[Hide, Browsable( false )]
	public Guid BlackboardParameterIdentifier { get; set; }

	[Input]
	[Title( "True" )]
	[Hide]
	public NodeInput InputTrue { get; set; }

	[Input]
	[Title( "False" )]
	[Hide]
	public NodeInput InputFalse { get; set; }

	[Hide]
	public ShaderFeatureBoolean Feature { get; set; } = new();

	[Title( "Preview" )]
	public bool Preview { get; set; } = false;

	[Output, Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var inputs = new List<NodeInput>
		{
			InputTrue,
			InputFalse
		};

		var result = compiler.ResultFeatureSwitch( inputs, Feature, Preview ? 1 : 0 );

		return result.IsValid ? result : new NodeResult( ResultType.Float, $"1.0f" );
	};

	public void UpdateFromBlackboard( BaseBlackboardParameter parameter )
	{
		if ( parameter is ShaderFeatureBooleanParameter boolFeatureParam )
		{
			if ( boolFeatureParam.IsValid )
			{
				Feature = new ShaderFeatureBoolean
				{
					Name = boolFeatureParam.Name,
					Description = boolFeatureParam.Description,
					HeaderName = boolFeatureParam.HeaderName,
				};
			}
		}
	}
}

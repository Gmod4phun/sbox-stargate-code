using static ShaderGraphPlus.ShaderGraphPlusGlobals;

namespace ShaderGraphPlus.Nodes;

[Title( "Enum Combo Switch" ), Category( "Utility/Logic" ), Icon( "alt_route" )]
[InternalNode]
public sealed class EnumFeatureSwitchNode : ShaderNodePlus, IInitializeNode, IBlackboardSyncableNode, IErroringNode
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

	[Hide]
	public ShaderFeatureEnum Feature { get; set; } = new();

	[global::Editor( ControlWidgetCustomEditors.ShaderFeatureEnumPreviewIndexEditor )]
	[Title( "Preview" )]
	public int PreviewIndex { get; set; } = 0;

	[Hide]
	private List<IPlugIn> InternalInputs = new();

	[Hide]
	public override IEnumerable<IPlugIn> Inputs => InternalInputs;

	[Hide, JsonIgnore]
	int _lastHashCodeInputs = 0;

	//[Hide, JsonIgnore]
	//bool _hasFeatureError = false;

	public override void OnFrame()
	{
		var hashCodeInput = Feature.GetHashCode();
		if ( hashCodeInput != _lastHashCodeInputs )
		{
			//var oldHashCode = _lastHashCodeInputs;
			_lastHashCodeInputs = hashCodeInput;

			//SGPLog.Info( $"HashCode changed from : {oldHashCode} to {_lastHashCodeInputs}" );

			// Dont update or change if feature is not valid!
			if ( Feature.IsValid )
			{
				CreateInputs();
				Update();
			}
		}
	}

	[Output, Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var inputs = new List<NodeInput>();

		foreach ( var input in Inputs )
		{
			if ( input.ConnectedOutput is null )
			{
				NodeInput nodeInput = default;

				inputs.Add( nodeInput );
			}
			else
			{
				NodeInput nodeInput = new NodeInput { Identifier = input.ConnectedOutput.Node.Identifier, Output = input.ConnectedOutput.Identifier };

				inputs.Add( nodeInput );
			}
		}

		var result = compiler.ResultFeatureSwitch( inputs, Feature, PreviewIndex );

		return result.IsValid ? result : new NodeResult( ResultType.Float, $"1.0f" );
	};

	public void InitializeNode()
	{
		OnNodeCreated();
	}

	private void OnNodeCreated()
	{
		CreateInputs();
		Update();
	}

	public void CreateInputs()
	{
		var inPlugs = new List<IPlugIn>();

		if ( Feature.Options == null )
		{
			InternalInputs = new();
		}
		else
		{
			foreach ( var input in Feature.Options )
			{
				var inputName = input;
				// Default to float.
				var inputType = typeof( float );//typeof( object );

				if ( string.IsNullOrWhiteSpace( inputName ) ) continue;

				var info = new PlugInfo()
				{
					Name = inputName,
					Type = inputType,
					DisplayInfo = new DisplayInfo()
					{
						Name = inputName,
						Fullname = inputType.FullName
					}
				};

				var plug = new BasePlugIn( this, info, inputType );
				var oldPlug = InternalInputs.FirstOrDefault( x => x is BasePlugIn plugIn && plugIn.Info.Name == info.Name && plugIn.Info.Type == info.Type ) as BasePlugIn;
				if ( oldPlug is not null )
				{
					oldPlug.Info.Name = info.Name;
					oldPlug.Info.Type = info.Type;
					oldPlug.Info.DisplayInfo = info.DisplayInfo;
					plug = oldPlug;
				}

				inPlugs.Add( plug );
			}

			InternalInputs = inPlugs;
		}
	}

	public void UpdateFromBlackboard( BaseBlackboardParameter parameter )
	{
		if ( parameter is ShaderFeatureEnumParameter enumFeatureParam )
		{
			if ( enumFeatureParam.IsValid )
			{
				Feature = new ShaderFeatureEnum
				{
					Name = enumFeatureParam.Name,
					Description = enumFeatureParam.Description,
					HeaderName = enumFeatureParam.HeaderName,
					Options = enumFeatureParam.Options,
				};

				//_hasFeatureError = false;
			}
			else
			{
				//_hasFeatureError = true;
			}

		}
	}

	public List<string> GetErrors()
	{
		var errors = new List<string>();

		//foreach ( var option in Feature.Options )
		//{
		//	if ( string.IsNullOrWhiteSpace( option ) )
		//	{
		//		errors.Add( $"element \"{Feature.Options.IndexOf( option )}\" of feature \"{Feature.Name}\" cannot have a blank name!" );
		//	}
		//}

		return errors;
	}
}

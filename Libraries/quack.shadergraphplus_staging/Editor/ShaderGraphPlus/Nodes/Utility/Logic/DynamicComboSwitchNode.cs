
namespace ShaderGraphPlus.Nodes;

// TODO
/*
[Title( "Dynamic Combo Switch" ), Category( "Utility/Logic" ), Icon( "alt_route" )]
[InternalNode]
public sealed class DynamicComboSwitchNode : ShaderNodePlus, IInitializeNode, IBlackboardSyncable, IErroringNode
{
	[Hide]
	public override int Version => 1;

	[Hide]
	public override string Title
	{
		get
		{
			return $"D_{Combo.Name.ToUpper().Replace( " ", "_" )}";
		}
	}

	[Hide, Browsable( false )]
	public Guid BlackboardParameterIdentifier { get; set; }

	[Hide]
	public DynamicCombo Combo { get; set; } = new();

	//[ShaderFeatureEnumPreviewIndex]
	[DynamicComboPreviewIndex]
	[Title( "Preview" )]
	public int PreviewIndex { get; set; } = 0;

	[Hide]
	private List<IPlugIn> InternalInputs = new();

	[Hide]
	public override IEnumerable<IPlugIn> Inputs => InternalInputs;

	[Hide, JsonIgnore]
	int _lastHashCodeInputs = 0;

	[Hide, JsonIgnore]
	bool _hasComboError = false;

	public override void OnFrame()
	{
		var hashCodeInput = Feature.GetHashCode();
		if ( hashCodeInput != _lastHashCodeInputs )
		{
			//var oldHashCode = _lastHashCodeInputs;
			_lastHashCodeInputs = hashCodeInput;

			//SGPLog.Info( $"HashCode changed from : {oldHashCode} to {_lastHashCodeInputs}" );

			if ( !_hasComboError )
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

		return compiler.ResultComboSwitch( inputs, Feature, PreviewIndex );
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
		if ( parameter is DynamicComboParameter dynamicComboParam )
		{
			if ( dynamicComboParam.IsValid )
			{
				// TODO : Init some struct and assign it to the Combo property on this class.


				_hasComboError = false;
			}
			else
			{
				_hasComboError = true;
			}

		}
	}

	public List<string> GetErrors()
	{
		var errors = new List<string>();

		return errors;
	}
}
*/

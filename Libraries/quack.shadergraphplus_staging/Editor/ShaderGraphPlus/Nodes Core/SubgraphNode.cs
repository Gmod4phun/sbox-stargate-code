using Editor;

namespace ShaderGraphPlus;

public sealed class SubgraphNode : ShaderNodePlus, IErroringNode, IWarningNode
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.SubgraphNode;

	[Hide]
	public bool IsSubgraph => (Graph is ShaderGraphPlus shaderGraph && shaderGraph.IsSubgraph);

	[Hide]
	public string SubgraphPath { get; set; }

	[Hide, JsonIgnore]
	public ShaderGraphPlus Subgraph { get; set; }

	[Hide]
	private List<IPlugIn> InternalInputs = new();

	[Hide]
	public override IEnumerable<IPlugIn> Inputs => InternalInputs;

	[Hide]
	private List<IPlugOut> InternalOutputs = new();

	[Hide]
	public override IEnumerable<IPlugOut> Outputs => InternalOutputs;

	//[JsonIgnore, Hide]
	//public override Color PrimaryColor => Color.Lerp( Theme.Blue, Theme.Green, 0.5f );

	[JsonIgnore, Hide]
	public override bool CanPreview => false;

	[global::Editor( "subgraphplus.defaultvalues" ), WideMode( HasLabel = false )]
	public Dictionary<string, object> DefaultValues { get; set; } = new();

	[Hide]
	public override DisplayInfo DisplayInfo => new()
	{
		Name = Subgraph?.Title ?? (string.IsNullOrEmpty( Subgraph.Title ) ? "Untitled Subgraph" : Subgraph.Title),
		Description = Subgraph?.Description ?? "",
		Icon = Subgraph?.Icon ?? ""
	};

	public void OnNodeCreated()
	{
		if ( Subgraph is not null ) return;

		if ( SubgraphPath != null )
		{

			Subgraph = new ShaderGraphPlus();
			if ( !Editor.FileSystem.Content.FileExists( SubgraphPath ) ) return;
			var json = Editor.FileSystem.Content.ReadAllText( SubgraphPath );
			Subgraph.Deserialize( json, SubgraphPath, SubgraphPath );
			Subgraph.Path = SubgraphPath;

			CreateInputs();
			CreateOutputs();

			//foreach ( var node in Subgraph.Nodes )
			//{
			//	if ( node is Texture2DObjectNode texNode && DefaultValues.TryGetValue( $"__tex_{texNode.UI.Name}", out var defaultTexVal ) )
			//	{
			//		texNode.Image = defaultTexVal.ToString();
			//	}
			//
			//	if ( node is Texture2DObjectNode texNode2 && DefaultValues.TryGetValue( $"{texNode2.Name}", out var textureInput ) )
			//	{
			//		//texNode2.UI = (TextureInput)textureInput;
			//	}
			//}

			Update();
		}
	}

	[Hide, JsonIgnore]
	internal Dictionary<IPlugIn, (SubgraphInput inputNode, Type inputNodeValueType)> InputReferences = new();

	public void CreateInputs()
	{
		var plugs = new List<IPlugIn>();
		var defaults = new Dictionary<Type, int>();
		InputReferences.Clear();

		// Get all SubgraphInput nodes only - no more legacy IParameterNode support
		var subgraphInputs = Subgraph.Nodes.OfType<SubgraphInput>()
			.Where( x => !string.IsNullOrWhiteSpace( x.InputName ) )
			.OrderBy( x => x.PortOrder )
			.ThenBy( x => x.InputName );

		foreach ( var subgraphInput in subgraphInputs )
		{
			var inputName = subgraphInput.InputName;

			if ( string.IsNullOrWhiteSpace( inputName ) ) continue;

			var type = subgraphInput.DefaultData.GetType();
			var plugInfo = new PlugInfo()
			{
				Name = inputName,
				Type = type,
				DisplayInfo = new DisplayInfo()
				{
					Name = inputName,
					Fullname = type.FullName,
					Description = subgraphInput.InputDescription
				}
			};

			var plug = new BasePlugIn( this, plugInfo, type );
			var oldPlug = InternalInputs.FirstOrDefault( x => x is BasePlugIn plugIn && plugIn.Info.Name == plugInfo.Name && plugIn.Info.Type == plugInfo.Type ) as BasePlugIn;
			if ( oldPlug is not null )
			{
				oldPlug.Info.Name = plugInfo.Name;
				oldPlug.Info.Type = plugInfo.Type;
				oldPlug.Info.DisplayInfo = plugInfo.DisplayInfo;
				plug = oldPlug;
			}

			plugs.Add( plug );

			InputReferences[plug] = (subgraphInput, type);

			if ( !DefaultValues.ContainsKey( plug.Identifier ) )
			{
				if ( subgraphInput.DefaultData != null )
				{
					DefaultValues[plug.Identifier] = subgraphInput.DefaultData;
				}
			}
		}

		InternalInputs = plugs;
	}

	[Hide, JsonIgnore]
	internal Dictionary<IPlugOut, IPlugIn> OutputReferences = new();
	public void CreateOutputs()
	{
		var plugs = new List<IPlugOut>();

		foreach ( var subgraphOutput in Subgraph.Nodes.OfType<SubgraphOutput>().OrderBy( x => x.PortOrder ) )
		{
			var outputType = subgraphOutput.OutputType switch
			{
				SubgraphPortType.Bool => typeof( bool ),
				SubgraphPortType.Int => typeof( int ),
				SubgraphPortType.Float => typeof( float ),
				SubgraphPortType.Vector2 => typeof( Vector2 ),
				SubgraphPortType.Vector3 => typeof( Vector3 ),
				SubgraphPortType.Vector4 => typeof( Vector4 ),
				SubgraphPortType.Color => typeof( Color ),
				SubgraphPortType.Float2x2 => typeof( Float2x2 ),
				SubgraphPortType.Float3x3 => typeof( Float3x3 ),
				SubgraphPortType.Float4x4 => typeof( Float4x4 ),
				SubgraphPortType.Gradient => typeof( Gradient ),
				SubgraphPortType.SamplerState => typeof( Sampler ),
				SubgraphPortType.Texture2DObject => typeof( Texture2DObject ),
				_ => throw new Exception( $"Unknown PortType \"{subgraphOutput.OutputType}\"" )
			};

			if ( outputType is null ) continue;
			var info = new PlugInfo()
			{
				Name = subgraphOutput.OutputName,
				Type = outputType,
				DisplayInfo = new DisplayInfo()
				{
					Name = subgraphOutput.OutputName,
					Fullname = outputType.FullName,
					Description = subgraphOutput.OutputDescription
				}
			};
			var plug = new BasePlugOut( this, info, outputType );
			var oldPlug = InternalOutputs.FirstOrDefault( x => x is BasePlugOut plugOut && plugOut.Info.Name == info.Name && plugOut.Info.Type == info.Type ) as BasePlugOut;
			if ( oldPlug is not null )
			{
				oldPlug.Info.Name = info.Name;
				oldPlug.Info.Type = info.Type;
				oldPlug.Info.DisplayInfo = info.DisplayInfo;
				plugs.Add( oldPlug );
			}
			else
			{
				plugs.Add( plug );
			}
		}
		InternalOutputs = plugs;
	}

	public List<string> GetWarnings()
	{
		var warnings = new List<string>();

		foreach ( var node in Subgraph.Nodes )
		{
			if ( node is IWarningNode warningNode )
			{
				warnings.AddRange( warningNode.GetWarnings().Select( x => $"[{DisplayInfo.Name}] {x}" ) );
			}
		}

		return warnings;
	}

	public List<string> GetErrors()
	{
		OnNodeCreated();
		if ( Subgraph is null )
		{
			return new List<string> { $"Cannot find subgraph at \"{SubgraphPath}\"" };
		}

		var errors = new List<string>();

		foreach ( var node in Subgraph.Nodes )
		{
			if ( node is IErroringNode erroringNode )
			{
				errors.AddRange( erroringNode.GetErrors().Select( x => $"[{DisplayInfo.Name}] {x}" ) );
			}
		}

		foreach ( var input in InputReferences )
		{
			var plug = input.Key;
			var inputNode = input.Value.inputNode;
			var inputName = inputNode.InputName;
			if ( string.IsNullOrWhiteSpace( inputName ) ) inputName = input.Key.DisplayInfo.Name;
			if ( inputNode.IsRequired && plug.ConnectedOutput is null )
			{
				errors.Add( $"Required Input \"{inputName}\" is missing on Node \"{Subgraph.Title}\"" );
			}
		}

		return errors;
	}

	public override void OnDoubleClick( MouseEvent e )
	{
		base.OnDoubleClick( e );

		if ( string.IsNullOrEmpty( SubgraphPath ) ) return;

		var shader = AssetSystem.FindByPath( SubgraphPath );
		if ( shader is null ) return;

		shader.OpenInEditor();
	}

}

[CustomEditor( typeof( Dictionary<string, object> ), NamedEditor = "subgraphplus.defaultvalues", WithAllAttributes = [typeof( WideModeAttribute )] )]
internal class SubgraphNodeControlWidget : ControlWidget
{
	public override bool SupportsMultiEdit => false;

	SubgraphNode Node;
	ControlSheet Sheet;

	public SubgraphNodeControlWidget( SerializedProperty property ) : base( property )
	{
		Node = property.Parent.Targets.First() as SubgraphNode;

		Layout = Layout.Column();
		Layout.Spacing = 2;
		Sheet = new ControlSheet();
		Layout.Add( Sheet );

		Rebuild();
	}

	protected override void OnPaint()
	{

	}

	private void Rebuild()
	{
		Sheet.Clear( true );

		// TODO

		foreach ( var inputRef in Node.InputReferences )
		{
			//if ( inputRef.Value.paramNode.IsAttribute ) continue;
			var name = inputRef.Key.Identifier;
			var type = inputRef.Value.inputNodeValueType;
			var getter = () =>
			{
				if ( Node.DefaultValues.ContainsKey( name ) )
				{
					return Node.DefaultValues[name];
				}
				else
				{
					var val = inputRef.Value.inputNode.DefaultData;
					if ( val is JsonElement el ) return el.GetDouble();
					return val;
				}

			};

			var attributes = new List<Attribute>();
			var properties = new List<SerializedProperty>();
			var displayName = $"Default {name}";

			if ( type == typeof( bool ) )
			{
				Sheet.AddRow( TypeLibrary.CreateProperty<bool>(
					displayName, () =>
					{
						var val = getter();

						if ( val is JsonElement el )
						{
							return bool.Parse( el.GetRawText() );
						}

						return (bool)val;
					}, x => SetDefaultValue( name, x ),
					attributes.ToArray()
				) );
			}
			else if ( type == typeof( int ) )
			{
				Sheet.AddRow( TypeLibrary.CreateProperty<int>(
					displayName, () =>
					{
						var val = getter();

						if ( val is JsonElement el )
						{
							return int.Parse( el.GetRawText() );
						}

						return (int)val;
					}, x => SetDefaultValue( name, x ),
					attributes.ToArray()
				) );
			}
			else if ( type == typeof( float ) )
			{
				Sheet.AddRow( TypeLibrary.CreateProperty<float>(
					displayName, () =>
					{
						var val = getter();

						if ( val is JsonElement el )
						{
							return float.Parse( el.GetRawText() );
						}

						return (float)val;
					}, x => SetDefaultValue( name, x ),
					attributes.ToArray()
				) );
			}
			else if ( type == typeof( Vector2 ) )
			{
				Sheet.AddRow( TypeLibrary.CreateProperty<Vector2>(
					displayName, () =>
					{
						var val = getter();

						if ( val is JsonElement el )
						{
							return Vector2.Parse( el.GetString() );
						}

						return (Vector2)val;
					}, x => SetDefaultValue( name, x ),
					attributes.ToArray()
				) );
			}
			else if ( type == typeof( Vector3 ) )
			{
				Sheet.AddRow( TypeLibrary.CreateProperty<Vector3>(
					displayName, () =>
					{
						var val = getter();

						if ( val is JsonElement el )
						{
							return Vector3.Parse( el.GetString() );
						}

						return (Vector3)val;
					}, x => SetDefaultValue( name, x ),
					attributes.ToArray()
				) );
			}
			else if ( type == typeof( Vector4 ) )
			{
				Sheet.AddRow( TypeLibrary.CreateProperty<Vector4>(
					displayName, () =>
					{
						var val = getter();

						if ( val is JsonElement el )
						{
							return Vector4.Parse( el.GetString() );
						}

						return (Vector4)val;
					}, x => SetDefaultValue( name, x ),
					attributes.ToArray()
				) );
			}
			else if ( type == typeof( Color ) )
			{
				Sheet.AddRow( TypeLibrary.CreateProperty<Color>(
					displayName, () =>
					{
						var val = getter();

						if ( val is JsonElement el )
						{
							return Color.Parse( el.GetString() ) ?? Color.White;
						}

						return (Color)val;
					}, x => SetDefaultValue( name, x ),
					attributes.ToArray()
				) );
			}
			else if ( type == typeof( Float2x2 ) )
			{
				Sheet.AddRow( EditorTypeLibrary.CreateProperty<Float2x2>(
					displayName, () =>
					{
						var val = getter();

						if ( val is JsonElement el )
						{
							return JsonSerializer.Deserialize<Float2x2>( el, ShaderGraphPlus.SerializerOptions() )!;
						}

						return (Float2x2)val;
					}, x => SetDefaultValue( name, x ),
					attributes.ToArray()
				) );
			}
			else if ( type == typeof( Float3x3 ) )
			{
				Sheet.AddRow( EditorTypeLibrary.CreateProperty<Float3x3>(
					displayName, () =>
					{
						var val = getter();

						if ( val is JsonElement el )
						{
							return JsonSerializer.Deserialize<Float3x3>( el, ShaderGraphPlus.SerializerOptions() )!;
						}

						return (Float3x3)val;
					}, x => SetDefaultValue( name, x ),
					attributes.ToArray()
				) );
			}
			else if ( type == typeof( Float4x4 ) )
			{
				Sheet.AddRow( EditorTypeLibrary.CreateProperty<Float4x4>(
					displayName, () =>
					{
						var val = getter();

						if ( val is JsonElement el )
						{
							return JsonSerializer.Deserialize<Float4x4>( el, ShaderGraphPlus.SerializerOptions() )!;
						}

						return (Float4x4)val;
					}, x => SetDefaultValue( name, x ),
					attributes.ToArray()
				) );
			}
			//else if ( !Node.IsSubgraph && type == typeof( Gradient ) )
			else if ( type == typeof( Gradient ) )
			{
				Sheet.AddRow( EditorTypeLibrary.CreateProperty<Gradient>(
					displayName, () =>
					{
						var val = getter();

						if ( val is JsonElement el )
						{
							return JsonSerializer.Deserialize<Gradient>( el, ShaderGraphPlus.SerializerOptions() )!;
						}

						return (Gradient)val;
					}, x => SetDefaultValue( name, x ),
					attributes.ToArray()
				) );
			}
			else if ( !Node.IsSubgraph && type == typeof( Sampler ) )
			{
				attributes.Add( new InlineEditorAttribute() { Label = false } );
				properties.Add( EditorTypeLibrary.CreateProperty<Sampler>(
					displayName, () =>
					{
						var val = getter();

						if ( val is JsonElement el )
						{
							return JsonSerializer.Deserialize<Sampler>( el, ShaderGraphPlus.SerializerOptions() )!;
						}

						return (Sampler)val;
					}, x => SetDefaultValue( name, x ),
					attributes.ToArray()
				) );

				Sheet.AddGroup( displayName, properties.ToArray() );
			}
			else if ( !Node.IsSubgraph && type == typeof( Texture2DObject ) )
			{
				attributes.Add( new InlineEditorAttribute() { Label = false } );
				properties.Add( EditorTypeLibrary.CreateProperty<TextureInput>(
					displayName, () =>
					{
						var val = getter();

						if ( val is JsonElement el )
						{
							return JsonSerializer.Deserialize<TextureInput>( el, ShaderGraphPlus.SerializerOptions() )! with { ShowNameProperty = true, Type = TextureType.Tex2D };
						}

						return ((TextureInput)val) with { ShowNameProperty = true, Type = TextureType.Tex2D };
					}, x =>
					{
						SetDefaultValue( name, x );
					},
					attributes.ToArray()
				) );

				Sheet.AddGroup( displayName, properties.ToArray() );
			}
			else if ( !Node.IsSubgraph && type == typeof( TextureCubeObject ) )
			{
				attributes.Add( new InlineEditorAttribute() { Label = false } );
				properties.Add( EditorTypeLibrary.CreateProperty<TextureInput>(
					displayName, () =>
					{
						var val = getter();

						if ( val is JsonElement el )
						{
							return JsonSerializer.Deserialize<TextureInput>( el, ShaderGraphPlus.SerializerOptions() )! with { ShowNameProperty = true, Type = TextureType.TexCube };
						}

						return ((TextureInput)val) with { ShowNameProperty = true, Type = TextureType.TexCube };
					}, x =>
					{
						SetDefaultValue( name, x );
					},
					attributes.ToArray()
				) );

				Sheet.AddGroup( displayName, properties.ToArray() );
			}
		}

	}

	private void SetDefaultValue( string name, object value )
	{
		Node.DefaultValues[name] = value;
		Node.Update();
		Node.IsDirty = true;
	}
}

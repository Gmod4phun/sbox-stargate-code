using System.Text;

namespace ShaderGraphPlus;

public struct FeatureRule : IValid
{
	/// <summary>
	/// Features bound to this rule.
	/// </summary>
	[InlineEditor( Label = false )]
	public List<string> Features { get; set; }

	/// <summary>
	/// Text hint when hovering over features
	/// </summary>
	public string HoverHint { get; set; }

	[Hide, JsonIgnore]
	public bool IsValid
	{
		get
		{
			if ( Features.Any() )
			{
				foreach ( var feature in Features )
				{
					if ( string.IsNullOrWhiteSpace( feature ) )
					{
						return false;
					}
					else
					{
						continue;
					}
				}

				return true;
			}
			else
			{
				return false;
			}
		}
	}

	public FeatureRule()
	{
		Features = new();
		HoverHint = string.Empty;
	}
}

public sealed partial class GraphCompiler
{
	private partial class CompileResult
	{
		public List<string> ShaderFeatureResultStrings { get; set; } = new();
	}

	/// <summary>
	/// Registerd ShaderFeatures of the current project.
	/// </summary>
	public Dictionary<string, ShaderFeatureBase> ShaderFeatures = new();

	struct SwitchBlockResultHolder
	{
		public string GeneratedLocals { get; set; }
		public NodeResult Result { get; set; }

		public SwitchBlockResultHolder( string locals, NodeResult nodeResult )
		{
			GeneratedLocals = locals;
			Result = nodeResult;
		}
	}

	public void SetComboPreview( string comboName, int preview )
	{
		if ( IsNotPreview )
		{
			SGPLog.Warning( $"{nameof( SetComboPreview )} was called when IsPreview is false!" );
			return;
		}

		if ( comboName.StartsWith( $"D_" ) )
		{
			var comboWrapper = new DynamicComboWrapper( comboName, preview );
			OnAttribute?.Invoke( comboWrapper.ComboName, comboWrapper );
		}
		else
		{
			SGPLog.Error( $"Cannot set a static combo \"{comboName}\"" );
		}
	}

	public NodeResult ResultFeatureSwitch( IEnumerable<NodeInput> inputs, ShaderFeatureBase shaderFeature, int previewInt )
	{
		var sb = new StringBuilder();
		var blockResults = new List<SwitchBlockResultHolder>();

		foreach ( var input in inputs )
		{
			if ( !input.IsValid )
			{
				blockResults.Add( new SwitchBlockResultHolder( $"float l_0 = 1.0f;", new NodeResult( ResultType.Float, "l_0" ) ) );
			}
			else
			{
				blockResults.Add( SubGenerate( input, input.Identifier, shaderFeature ) );
			}
		}

		if ( !blockResults.Any() )
			return default;

		var resultType = blockResults.Select( x => x.Result.ResultType ).Where( x => !((int)x > 6) ).Max();
		var id = 0;

		while ( ShaderResult.ShaderFeatureResultStrings.Contains( $"{shaderFeature.Name}_result{id}" ) )
		{
			id++;
		}

		var resultAssignmentLocal = $"{shaderFeature.Name}_result{id}";
		var resultDataType = resultType.GetHLSLDataType();

		foreach ( var (index, result) in blockResults.Index() )
		{
			var lastResult = new NodeResult( resultType, result.Result.Cast( resultType.GetComponentCount() ) );

			if ( index == 0 )
			{
				sb.AppendLine( LocalResultInitialize( resultType, resultAssignmentLocal ) ); //$"{resultDataType} {resultLocal};" );

				if ( shaderFeature is ShaderFeatureBoolean boolFeature )
				{
					sb.AppendLine( $"#if ( {(IsPreview ? "D" : "S")}_{shaderFeature.Name.ToUpper()} == SWITCH_TRUE )" );
				}
				else
				{
					sb.AppendLine( $"#if ( {(IsPreview ? "D" : "S")}_{shaderFeature.Name.ToUpper()} == {index} )" );
				}

				BuildSwitchBlock( sb, result.GeneratedLocals, resultAssignmentLocal, lastResult );
			}

			if ( shaderFeature is ShaderFeatureBoolean )
			{
				if ( index == blockResults.Count - 1 )
				{
					sb.AppendLine( $"#else" );
					BuildSwitchBlock( sb, result.GeneratedLocals, resultAssignmentLocal, lastResult );
					sb.AppendLine( $"#endif" );
				}
			}
			else
			{
				if ( index != 0 && index != blockResults.Count - 1 )
				{
					sb.AppendLine( $"#elif ( {(IsPreview ? "D" : "S")}_{shaderFeature.Name.ToUpper()} == {index} )" );
					BuildSwitchBlock( sb, result.GeneratedLocals, resultAssignmentLocal, lastResult );
				}
				else if ( index == blockResults.Count - 1 )
				{
					sb.AppendLine( $"#elif ( {(IsPreview ? "D" : "S")}_{shaderFeature.Name.ToUpper()} == {index} )" );
					BuildSwitchBlock( sb, result.GeneratedLocals, resultAssignmentLocal, lastResult );
					sb.AppendLine( $"#endif" );
				}
			}
		}

		//SGPLog.Info( $"Generated Switch D_{shaderFeature.Name.ToUpper()}: \n {sb.ToString()}" );

		// TODO : Once SceneObject.Attributes.SetFeature is added. Replace SetComboPreview with something like SetFeaturePreview.
		if ( IsPreview )
		{
			SetComboPreview( shaderFeature.GetDynamicComboString(), previewInt );
		}

		var finalResult = new NodeResult( resultType, resultAssignmentLocal );
		finalResult.AddMetadataEntry( nameof( MetadataType.ComboSwitchBody ), sb.ToString() );

		if ( !ShaderResult.ShaderFeatureResultStrings.Contains( resultAssignmentLocal ) )
		{
			ShaderResult.ShaderFeatureResultStrings.Add( resultAssignmentLocal );
		}

		return finalResult;
	}

	private SwitchBlockResultHolder SubGenerate( NodeInput input, string blockName, ShaderFeatureBase shaderFeature )
	{
		var outerResult = ShaderResult;
		var outerInputStack = InputStack;

		if ( IsVs )
		{
			VertexResult = new();
			VertexResult.SetAttributes( outerResult.Attributes );
		}
		else
		{
			PixelResult = new();
			PixelResult.SetAttributes( outerResult.Attributes );
		}
		InputStack = new();

		var result = Result( input );
		var blockCode = GenerateLocals( true );

		foreach ( var attribute in ShaderResult.Attributes )
		{
			outerResult.Attributes[attribute.Key] = attribute.Value;
		}

		if ( IsVs )
		{
			VertexResult = outerResult;
		}
		else
		{
			PixelResult = outerResult;
		}
		InputStack = outerInputStack;

		//SGPLog.Info( $"GeneratedBlock {block} : \n {{ {IndentString( blockCode, 1)} \n }}" );

		return new( blockCode, result );
	}

	private static StringBuilder BuildSwitchBlock( StringBuilder sb, string generatedLocals, string resultAssignmentLocal, NodeResult lastResult )
	{
		sb.AppendLine( $"{{" );
		sb.AppendLine( $"{IndentString( generatedLocals, 1 )}" );
		sb.AppendLine( $"{IndentString( $"{resultAssignmentLocal} = {lastResult}", 1 )};" );
		sb.AppendLine( $"}}" );

		return sb;
	}

	private static string BuildFeatureOptionsBody( List<string> options )
	{
		var options_body = "";
		int count = 0;

		foreach ( var option in options )
		{
			if ( count == 0 ) // first option starts at 0 :)
			{
				options_body += $"0=\"{option}\", ";
				count++;
			}
			else if ( count != (options.Count - 1) )  // These options dont get the privilege of being the first >:)
			{
				options_body += $"{count}=\"{option}\", ";
				count++;
			}
			else // Last option in the list oh well...:(
			{
				options_body += $"{count}=\"{option}\"";
			}
		}

		return options_body;
	}

	private static string LocalResultInitialize( ResultType resultType, string name )
	{
		switch ( resultType )
		{
			case ResultType.Bool:
				return $"bool {name} = false;";
			case ResultType.Int:
				return $"int {name} = 0;";
			case ResultType.Float:
				return $"float {name} = 0.0f;";
			case ResultType.Vector2:
				return $"float2 {name} = float2( 0.0f, 0.0f );";
			case ResultType.Vector3:
				return $"float3 {name} = float3( 0.0f, 0.0f, 0.0f );";
			case ResultType.Vector4:
				return $"float4 {name} = float4( 0.0f, 0.0f, 0.0f, 0.0f );";
			case ResultType.Color:
				return $"float4 {name} = float4( 0.0f, 0.0f, 0.0f, 0.0f );";
			case ResultType.Float2x2:
				return $"float2x2 {name} = float2x2( 0.0f, 0.0f, 0.0f, 0.0f );";
			case ResultType.Float3x3:
				return $"float3x3 {name} = float3x3( 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f );";
			case ResultType.Float4x4:
				return $"float4x4 {name} = float4x4( 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f );";
			default:
				throw new NotImplementedException( $"Unknown resulttype \"{resultType}\"" );
		}
	}
}

using Sandbox.Resources;
using System.Text;
using ShaderGraphPlus.Nodes;

namespace ShaderGraphPlus;

public enum SubgraphOutputPreviewType
{
	[Icon( "clear" )]
	None,
	[Icon( "palette" )]
	Albedo,
	[Icon( "brightness_5" )]
	Emission,
	[Icon( "opacity" )]
	Opacity,
	[Icon( "texture" )]
	Normal,
	[Icon( "terrain" )]
	Roughness,
	[Icon( "auto_awesome" )]
	Metalness,
	[Icon( "tonality" )]
	AmbientOcclusion,
	[Icon( "arrow_forward" )]
	PositionOffset
}

/// <summary>
/// Output of a subgraph.
/// </summary>
[Title( "Subgraph Output" ), Icon( "output" ), SubgraphOnly]
[InternalNode]
public sealed class SubgraphOutput : BaseResult, IInitializeNode, IErroringNode
{
	[Hide, Browsable( false )]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.SubgraphNode;

	[JsonIgnore, Hide, Browsable( false )]
	public override bool CanRemove => true;

	[JsonIgnore, Hide, Browsable( false )]
	public bool CannotPreviewOutputType
	{
		get
		{
			return (OutputType == SubgraphPortType.Bool ||
				OutputType == SubgraphPortType.Float2x2 ||
				OutputType == SubgraphPortType.Float3x3 ||
				OutputType == SubgraphPortType.Float4x4 ||
				OutputType == SubgraphPortType.Gradient ||
				OutputType == SubgraphPortType.Texture2DObject ||
				OutputType == SubgraphPortType.TextureCubeObject ||
				OutputType == SubgraphPortType.SamplerState);
		}

	}

	[Hide, Browsable( false )]
	public Guid OutputIdentifier { get; set; }

	public string OutputName { get; set; } = "Ouat0";

	[TextArea]
	public string OutputDescription { get; set; } = "";

	public SubgraphPortType OutputType { get; set; } = SubgraphPortType.Vector3;

	[HideIf( nameof( CannotPreviewOutputType ), true )]
	public SubgraphOutputPreviewType Preview { get; set; } = SubgraphOutputPreviewType.None;

	public int PortOrder { get; set; } = 0;

	[Hide]
	private List<IPlugIn> InternalInputs = new();

	[Hide]
	public override IEnumerable<IPlugIn> Inputs => InternalInputs;

	[JsonIgnore, Hide, Browsable( false )]
	int _lastHashCode = 0;

	public SubgraphOutput() : base()
	{
		OutputIdentifier = Guid.NewGuid();
	}

	public override void OnFrame()
	{
		var hashCodeInput = 0;
		hashCodeInput = System.HashCode.Combine( OutputIdentifier, OutputName, OutputDescription, OutputType, PortOrder );

		if ( hashCodeInput != _lastHashCode )
		{
			//var oldhashCode = _lastHashCode;
			_lastHashCode = hashCodeInput;

			//SGPLog.Info( $"SubgraphFunctionOutput hashcode changed from \"{oldhashCode}\" to \"{_lastHashCode}\"" );

			InitializeNode();
		}
	}

	public void InitializeNode()
	{
		CreateInput();
		Update();
	}

	private void CreateInput()
	{
		var Plugs = new List<IPlugIn>();

		var type = OutputType switch
		{
			SubgraphPortType.Bool => typeof( bool ),
			SubgraphPortType.Int => typeof( int ),
			SubgraphPortType.Float => typeof( float ),
			SubgraphPortType.Vector2 => typeof( Vector2 ),
			SubgraphPortType.Vector3 => typeof( Vector3 ),
			SubgraphPortType.Vector4 => typeof( Color ),
			SubgraphPortType.Color => typeof( Color ),
			SubgraphPortType.Float2x2 => typeof( Float2x2 ),
			SubgraphPortType.Float3x3 => typeof( Float3x3 ),
			SubgraphPortType.Float4x4 => typeof( Float4x4 ),
			SubgraphPortType.Gradient => typeof( Gradient ),
			SubgraphPortType.SamplerState => typeof( Sampler ),
			SubgraphPortType.Texture2DObject => typeof( Texture2DObject ),
			SubgraphPortType.TextureCubeObject => typeof( TextureCubeObject ),
			_ => throw new Exception( $"Unknown PortType \"{OutputType}\"" )
		};

		var info = new PlugInfo()
		{
			Id = OutputIdentifier,
			Name = OutputName,
			Type = type,
			DisplayInfo = new()
			{
				Name = OutputName,
				Fullname = type.FullName,
				Description = OutputDescription,
			}
		};

		var plug = new BasePlugIn( this, info, info.Type );
		var oldPlug = InternalInputs.FirstOrDefault( x => x is BasePlugIn plugIn && plugIn.Info.Id == info.Id ) as BasePlugIn;
		if ( oldPlug is not null )
		{
			oldPlug.Info.Name = info.Name;
			oldPlug.Info.Type = type;
			oldPlug.Info.DisplayInfo = info.DisplayInfo;

			// Change the old plug type to the new type.
			var oldplugType = oldPlug as IPlugIn;
			oldplugType.Type = type;

			Plugs.Add( oldplugType );
		}
		else
		{
			Plugs.Add( plug );
		}

		InternalInputs = Plugs;
	}

	public void SetSubgraphPortTypeFromType( Type type )
	{
		switch ( type )
		{
			case Type t when t == typeof( bool ):
				OutputType = SubgraphPortType.Bool;
				break;
			case Type t when t == typeof( int ):
				OutputType = SubgraphPortType.Int;
				break;
			case Type t when t == typeof( float ):
				OutputType = SubgraphPortType.Float;
				break;
			case Type t when t == typeof( Vector2 ):
				OutputType = SubgraphPortType.Vector2;
				break;
			case Type t when t == typeof( Vector3 ):
				OutputType = SubgraphPortType.Vector3;
				break;
			case Type t when t == typeof( Vector4 ):
				OutputType = SubgraphPortType.Color;
				break;
			case Type t when t == typeof( Color ):
				OutputType = SubgraphPortType.Color;
				break;
			case Type t when t == typeof( ColorTextureGenerator ):
				OutputType = SubgraphPortType.Color;
				break;
			case Type t when t == typeof( Float2x2 ):
				OutputType = SubgraphPortType.Float2x2;
				break;
			case Type t when t == typeof( Float3x3 ):
				OutputType = SubgraphPortType.Float3x3;
				break;
			case Type t when t == typeof( Float4x4 ):
				OutputType = SubgraphPortType.Float4x4;
				break;
			case Type t when t == typeof( Gradient ):
				OutputType = SubgraphPortType.Gradient;
				break;
			case Type t when t == typeof( Sampler ):
				OutputType = SubgraphPortType.SamplerState;
				break;
			case Type t when t == typeof( Texture2DObject ):
				OutputType = SubgraphPortType.Texture2DObject;
				break;
			case Type t when t == typeof( TextureCubeObject ):
				OutputType = SubgraphPortType.TextureCubeObject;
				break;
			default:
				throw new Exception( $"Unknown type \"{type}\"" );
		}
	}

	public void AddMaterialOutput( GraphCompiler compiler, StringBuilder sb, SubgraphOutputPreviewType previewType, out List<string> errors )
	{
		errors = new List<string>();

		// Make sure we dont try to preview outputs that we cant.
		if ( CannotPreviewOutputType )
		{
			Preview = SubgraphOutputPreviewType.None;
			return;
		}

		if ( previewType == SubgraphOutputPreviewType.Albedo )
		{
			var albedoResult = GetAlbedoResult( compiler );

			if ( !albedoResult.IsValid )
				return;

			sb.AppendLine( $"m.Albedo = {albedoResult.Cast( 3 )};" );
		}
		if ( previewType == SubgraphOutputPreviewType.Emission )
		{
			var emissionResult = GetEmissionResult( compiler );

			if ( !emissionResult.IsValid )
				return;

			sb.AppendLine( $"m.Emission = {emissionResult.Cast( 3 )};" );
		}
		if ( previewType == SubgraphOutputPreviewType.Opacity )
		{
			var opacityResult = GetOpacityResult( compiler );

			if ( !opacityResult.IsValid )
				return;

			sb.AppendLine( $"m.Opacity = {opacityResult.Cast( 1 )};" );
		}
		if ( previewType == SubgraphOutputPreviewType.Normal )
		{
			var normalResult = GetNormalResult( compiler );

			if ( !normalResult.IsValid )
				return;

			sb.AppendLine( $"m.Normal = {normalResult.Cast( 3 )};" );
		}
		if ( previewType == SubgraphOutputPreviewType.Roughness )
		{
			var roughnessResult = GetRoughnessResult( compiler );

			if ( !roughnessResult.IsValid )
				return;

			sb.AppendLine( $"m.Roughness = {roughnessResult.Cast( 1 )};" );
		}
		if ( previewType == SubgraphOutputPreviewType.Metalness )
		{
			var metalnessResult = GetMetalnessResult( compiler );

			if ( !metalnessResult.IsValid )
				return;

			sb.AppendLine( $"m.Metalness = {metalnessResult.Cast( 1 )};" );
		}
		if ( previewType == SubgraphOutputPreviewType.AmbientOcclusion )
		{
			var ambientOcclusionResult = GetAmbientOcclusionResult( compiler );

			if ( !ambientOcclusionResult.IsValid )
				return;

			sb.AppendLine( $"m.AmbientOcclusion = {ambientOcclusionResult.Cast( 1 )};" );
		}
	}

	public NodeInput? GetInputFromPreview( SubgraphOutputPreviewType previewType )
	{
		// Make sure we dont try to preview outputs that we cant.
		if ( CannotPreviewOutputType )
		{
			Preview = SubgraphOutputPreviewType.None;
		}

		if ( Preview == previewType )
		{
			var input = Inputs.FirstOrDefault( x => x is BasePlugIn plugIn && plugIn.Info.Id == OutputIdentifier );
			if ( input is BasePlugIn plugIn )
			{

				if ( plugIn.Info.ConnectedPlug is not null )
				{
					return new NodeInput
					{
						Identifier = plugIn.Info.ConnectedPlug.Node.Identifier,
						Output = plugIn.Info.ConnectedPlug.Identifier
					};
				}

				return plugIn.Info.GetInput( plugIn.Node );
			}
		}

		return null;
	}

	public override NodeInput GetAlbedo()
	{
		var input = GetInputFromPreview( SubgraphOutputPreviewType.Albedo );
		if ( input is not null )
		{
			return input.Value;
		}
		return default;
	}

	public override NodeInput GetEmission()
	{
		var input = GetInputFromPreview( SubgraphOutputPreviewType.Emission );
		if ( input is not null )
		{
			return input.Value;
		}
		return default;
	}

	public override NodeInput GetOpacity()
	{
		var input = GetInputFromPreview( SubgraphOutputPreviewType.Opacity );
		if ( input is not null )
		{
			return input.Value;
		}
		return default;
	}

	public override NodeInput GetNormal()
	{
		var input = GetInputFromPreview( SubgraphOutputPreviewType.Normal );
		if ( input is not null )
		{
			return input.Value;
		}
		return default;
	}

	public override NodeInput GetRoughness()
	{
		var input = GetInputFromPreview( SubgraphOutputPreviewType.Roughness );
		if ( input is not null )
		{
			return input.Value;
		}
		return default;
	}

	public override NodeInput GetMetalness()
	{
		var input = GetInputFromPreview( SubgraphOutputPreviewType.Metalness );
		if ( input is not null )
		{
			return input.Value;
		}
		return default;
	}

	public override NodeInput GetAmbientOcclusion()
	{
		var input = GetInputFromPreview( SubgraphOutputPreviewType.AmbientOcclusion );
		if ( input is not null )
		{
			return input.Value;
		}
		return default;
	}

	public override NodeInput GetPositionOffset()
	{
		var input = GetInputFromPreview( SubgraphOutputPreviewType.PositionOffset );
		if ( input is not null )
		{
			return input.Value;
		}
		return default;
	}

	public override Color GetDefaultAlbedo()
	{
		return base.GetDefaultAlbedo();
	}

	public List<string> GetErrors()
	{
		var errors = new List<string>();

		if ( Graph is ShaderGraphPlus shaderGraphPlus && shaderGraphPlus.IsSubgraph )
		{
			foreach ( var node in Graph.Nodes )
			{
				if ( node == this ) continue;

				if ( node is SubgraphOutput otherOutput && otherOutput.OutputName == OutputName )
				{
					errors.Add( $"Duplicate output name \"{OutputName}\"" );
					break;
				}
			}
		}

		return errors;
	}
}

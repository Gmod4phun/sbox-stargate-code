using ShaderGraphPlus.Nodes;

namespace ShaderGraphPlus;

public enum ResultType
{
	/// <summary>
	/// No Components, just True or False.
	/// </summary>
	Bool,
	/// <summary>
	/// 
	/// </summary>
	Int,
	/// <summary>
	/// 1 Component
	/// </summary>
	Float,
	/// <summary>
	/// 2 Component's
	/// </summary>
	Vector2,
	/// <summary>
	/// 3 Component's
	/// </summary>
	Vector3,
	/// <summary>
	/// 4 Component's
	/// </summary>
	Vector4,
	/// <summary>
	/// 4 Component's
	/// </summary>
	Color,
	/// <summary>
	/// 4 Component's
	/// </summary>
	Float2x2,
	/// <summary>
	/// 9 Component's
	/// </summary>
	Float3x3,
	/// <summary>
	/// 16 Component's
	/// </summary>
	Float4x4,
	/// <summary>
	/// 
	/// </summary>
	Sampler,
	/// <summary>
	/// 
	/// </summary>
	Texture2DObject,
	/// <summary>
	/// 
	/// </summary>
	TextureCubeObject,
	/// <summary>
	/// 
	/// </summary>
	Gradient,
	/// <summary>
	/// 
	/// </summary>
	Void,
	/// <summary>
	/// Group of noderesults all bundled up.
	/// </summary>
	Bundle,
	/// <summary>
	/// 
	/// </summary>
	Invalid,
}

internal enum MetadataType
{
	ImagePath,
	Bool,
	ComboSwitchInfo,
	ComboSwitchBody,
	VoidComponents,
	VoidResultUserDefinedName
}

public struct NodeResult : IValid
{
	public delegate NodeResult Func( GraphCompiler compiler );
	public string Code { get; private set; }
	public ResultType ResultType { get; private set; }
	public string[] Errors { get; private init; }
	public string[] Warnings { get; private init; }
	public bool IsDepreciated { get; private set; }
	public int Components { get; private set; }
	public int PreviewID { get; private set; }
	public string VoidLocalTargetID { get; private set; }

	public readonly bool IsValid
	{
		get
		{
			if ( IsMetaDataResult )
			{
				return ResultType != ResultType.Invalid && Metadata.Any();
			}
			else
			{
				return ResultType != ResultType.Invalid && !string.IsNullOrWhiteSpace( Code );
			}
		}
	}

	public readonly string TypeName => ResultType.GetHLSLDataType();

	public readonly Type ComponentType
	{
		get
		{
			if ( ResultType == ResultType.Void )
			{
				return Components switch
				{
					//int r when r == 0 => typeof( bool ),
					//int r when r == 1 => typeof( int ),
					int r when r == 1 => typeof( float ),
					int r when r == 2 => typeof( Vector2 ),
					int r when r == 3 => typeof( Vector3 ),
					int r when r == 4 => typeof( Color ),
					_ => throw new System.NotImplementedException(),
				};
			}
			else
			{
				return ResultType.GetRepresentedType();
			}
		}
	}

	public bool SkipLocalGeneration { get; set; } = false;
	public bool Constant { get; set; }
	public bool ShouldPreview { get; set; }
	public bool IsMetaDataResult { get; set; } = false;

	/// <summary>
	/// Generic-Ish metadata related to this NodeResult.
	/// </summary>
	internal Dictionary<string, object> Metadata { get; private set; }

	public readonly bool CanPreview
	{
		get
		{
			switch ( ResultType )
			{
				case ResultType.Bool:
					return false;
				case ResultType.Int:
					return true;
				case ResultType.Float:
					return true;
				case ResultType.Vector2:
					return true;
				case ResultType.Vector3:
					return true;
				case ResultType.Vector4:
					return true;
				case ResultType.Color:
					return true;
				case ResultType.Float2x2:
					return false;
				case ResultType.Float3x3:
					return false;
				case ResultType.Float4x4:
					return false;
				case ResultType.Sampler:
					return false;
				case ResultType.Texture2DObject:
					return false;
				case ResultType.TextureCubeObject:
					return false;
				case ResultType.Gradient:
					return false;
				case ResultType.Void:
					return false;
				case ResultType.Invalid:
					throw new Exception( "Result Type Is Invalid!" );
				default:
					return false;
			}
		}
	}

	public readonly bool CanCast
	{
		get
		{
			switch ( ResultType )
			{
				case ResultType.Float2x2:
					return false;
				case ResultType.Float3x3:
					return false;
				case ResultType.Float4x4:
					return false;
				case ResultType.Sampler:
					return false;
				case ResultType.Texture2DObject:
					return false;
				case ResultType.TextureCubeObject:
					return false;
				case ResultType.Gradient:
					return false;
				case ResultType.Void:
					return false;
				case ResultType.Invalid:
					return false;
				case ResultType.Bool:
					return false;
				case ResultType.Int:
					return true;
				case ResultType.Float:
					return true;
				case ResultType.Vector2:
					return true;
				case ResultType.Vector3:
					return true;
				case ResultType.Vector4:
					return true;
				case ResultType.Color:
					return true;
				default:
					return false;
			}
		}
	}

	public NodeResult( ResultType resultType, string code, bool constant = false, Dictionary<string, object> metadata = null )
	{
		ResultType = resultType;
		Code = code;
		Constant = constant;
		IsMetaDataResult = false;

		if ( metadata == null )
		{
			Metadata = new();
		}
		else
		{
			Metadata = metadata;
		}

		Components = ResultType switch
		{
			ResultType.Bool => 1,
			ResultType.Int => 1,
			ResultType.Float => 1,
			ResultType.Vector2 => 2,
			ResultType.Vector3 => 3,
			ResultType.Vector4 => 4,
			ResultType.Color => 4,
			ResultType.Void => 0,
			_ => 0
		};
	}

	public NodeResult( ResultType resultType, string metaDataName, object actualMetaData )
	{
		ResultType = resultType;
		Code = "";
		Constant = true;
		IsMetaDataResult = true;

		Metadata = new();
		AddMetadataEntry( metaDataName, actualMetaData );

		Components = ResultType switch
		{
			ResultType.Bool => 1,
			ResultType.Int => 1,
			ResultType.Float => 1,
			ResultType.Vector2 => 2,
			ResultType.Vector3 => 3,
			ResultType.Vector4 => 4,
			ResultType.Color => 4,
			ResultType.Void => 0,
			_ => 0
		};
	}

	public static NodeResult Error( params string[] errors ) => new() { Errors = errors };
	public static NodeResult Warning( params string[] warnings ) => new() { Warnings = warnings };
	public static NodeResult MissingInput( string name ) => Error( $"Missing required input '{name}'." );
	public static NodeResult Depreciated( (string, string) name ) => Error( $"'{name.Item1}' is depreciated please use '{name.Item2} instead'." );

	public void SetPreviewID( int previewid ) { PreviewID = previewid; }
	public void SetVoidLocalTargetID( string voidLocalTargetID ) { VoidLocalTargetID = voidLocalTargetID; }

	#region Metdata
	internal T GetMetadata<T>( string metaName, bool ignoreException = false )
	{
		if ( Metadata.TryGetValue( metaName, out var actualData ) )
		{
			if ( typeof( T ) == actualData.GetType() )
			{
				return (T)actualData;
			}
			else
			{
				throw new InvalidCastException( $"Generic type of `{typeof( T )}` is not of metadata actual data type `{actualData.GetType()}`" );
			}
		}

		if ( !ignoreException )
		{
			throw new Exception( $"Unable to get metadata with name `{metaName}`" );
		}

		return default( T );
	}

	internal bool TryGetMetaData<T>( string metaName, out T data )
	{
		data = default;

		if ( Metadata.TryGetValue( metaName, out var actualData ) )
		{
			if ( typeof( T ) == actualData.GetType() )
			{
				data = (T)actualData;
				return true;
			}
			else
			{
				return false;
				//throw new InvalidCastException( $"Generic type of `{typeof( T )}` is not of metadata actual data type `{actualData.GetType()}`" );
			}
		}

		return false;
	}

	internal void AddMetadataEntry( string metaName, object actualData )
	{
		if ( !Metadata.ContainsKey( metaName ) )
		{
			Metadata.Add( metaName, actualData );
		}
		else
		{
			throw new Exception( "Metadata entry already exists!" );
		}
	}

	internal void SetMetadata( Dictionary<string, object> metadata )
	{
		Metadata = metadata;
	}

	internal void SetMetadataValue( string metaName, object actualData )
	{
		if ( Metadata.ContainsKey( metaName ) )
		{
			Metadata[metaName] = actualData;
		}
		else
		{
			//SGPLog.Warning( $"Metadata entry `{metaName}` dosent exist! Creating new Metadata entry instead." );
			AddMetadataEntry( metaName, actualData );
		}
	}
	#endregion Metdata

	/// <summary>
	/// "Cast" this result to different float types
	/// </summary>
	public string Cast( int components, float defaultValue = 0.0f )
	{
		if ( components > 4 )
		{
			throw new Exception( $"There is no float type with a component count of \"{components}\"" );
		}

		if ( !CanCast )
		{
			throw new Exception( $"ResultType `{ResultType}` cannot be cast." );
		}

		if ( ResultType == ResultType.Int )
		{
			if ( Components == components )
			{
				return $"{Code}";
			}

			return $"float{components}( {string.Join( ", ", Enumerable.Repeat( Code, components ) )} )";
		}

		if ( Components == components )
		{
			return Code;
		}

		if ( Components > components )
		{
			return $"{Code}.{"xyzw"[..components]}";
		}
		else if ( Components == 1 )
		{
			return $"float{components}( {string.Join( ", ", Enumerable.Repeat( Code, components ) )} )";
		}
		else
		{
			if ( !string.IsNullOrWhiteSpace( Code ) )
				return $"float{components}( {Code}, {string.Join( ", ", Enumerable.Repeat( $"{defaultValue}", components - Components ) )} )";

			return $"float{components}( {string.Join( ", ", Enumerable.Repeat( $"{defaultValue}", components ) )} )";
		}
	}

	public override readonly string ToString()
	{
		return Code;
	}
}

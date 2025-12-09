namespace ShaderGraphPlus;

public static class ResultTypeExtentions
{
	public static int GetComponentCount( this ResultType resultType )
	{
		switch ( resultType )
		{
			//case ResultType.Bool:
			//	return 1;
			case ResultType.Int:
				return 1;
			case ResultType.Float:
				return 1;
			case ResultType.Vector2:
				return 2;
			case ResultType.Vector3:
				return 3;
			case ResultType.Vector4:
				return 4;
			case ResultType.Color:
				return 4;
			case ResultType.Float2x2:
				return 4;
			case ResultType.Float3x3:
				return 9;
			case ResultType.Float4x4:
				return 16;
			default:
				SGPLog.Warning( $"ResultType \"{resultType}\" has no components" );
				return 0;
		}
	}

	public static string GetHLSLDataType( this ResultType resultType )
	{
		switch ( resultType )
		{
			case ResultType.Bool:
				return "bool";
			case ResultType.Int:
				return "int";
			case ResultType.Float:
				return "float";
			case ResultType.Vector2:
				return "float2";
			case ResultType.Vector3:
				return "float3";
			case ResultType.Vector4:
				return "float4";
			case ResultType.Color:
				return "float4";
			case ResultType.Float2x2:
				return "float2x2";
			case ResultType.Float3x3:
				return "float3x3";
			case ResultType.Float4x4:
				return "float4x4";
			case ResultType.Gradient:
				return "Gradient";
			case ResultType.Sampler:
				return "SamplerState";
			case ResultType.Texture2DObject:
				return "Texture2D";
			case ResultType.TextureCubeObject:
				return "TextureCube";
			default:
				throw new Exception( $"Unsupported ResultType `{resultType}`" );
		}
	}

	public static Type GetRepresentedType( this ResultType resultType )
	{
		switch ( resultType )
		{
			case ResultType.Bool:
				return typeof( bool );
			case ResultType.Int:
				return typeof( int );
			case ResultType.Float:
				return typeof( float );
			case ResultType.Vector2:
				return typeof( Vector2 );
			case ResultType.Vector3:
				return typeof( Vector3 );
			case ResultType.Vector4:
				return typeof( Vector4 );
			case ResultType.Color:
				return typeof( Color );
			case ResultType.Float2x2:
				return typeof( Float2x2 );
			case ResultType.Float3x3:
				return typeof( Float3x3 );
			case ResultType.Float4x4:
				return typeof( Float4x4 );
			case ResultType.Gradient:
				return typeof( Gradient );
			case ResultType.Sampler:
				return typeof( Sampler );
			case ResultType.Texture2DObject:
				return typeof( Texture2DObject );
			case ResultType.TextureCubeObject:
				return typeof( TextureCubeObject );
			default:
				throw new Exception( $"Unsupported ResultType \"{resultType}\"" );
		}
	}

	public static bool IsFloatOrVectorFloatType( this ResultType resultType )
	{
		return resultType switch
		{
			ResultType.Float => true,
			ResultType.Vector2 => true,
			ResultType.Vector3 => true,
			ResultType.Vector4 => true,
			ResultType.Color => true,
			_ => false,
		};
	}

	public static bool IsMatrixType( this ResultType resultType )
	{
		return resultType switch
		{
			ResultType.Float2x2 => true,
			ResultType.Float3x3 => true,
			ResultType.Float4x4 => true,
			_ => false,
		};
	}
}

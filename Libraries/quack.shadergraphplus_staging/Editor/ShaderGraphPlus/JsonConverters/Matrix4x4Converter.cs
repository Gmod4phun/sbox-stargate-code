using Sandbox.Internal;

namespace ShaderGraphPlus;

internal class Matrix4x4Converter : JsonConverter<Float4x4>
{
	public override Float4x4 Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
	{
		if ( reader.TokenType == JsonTokenType.String )
		{
			return Float4x4.Parse( reader.GetString() );
		}

		if ( reader.TokenType == JsonTokenType.StartArray )
		{
			reader.Read();
			Float4x4 result = default( Float4x4 );

			// Row 1
			if ( reader.TokenType == JsonTokenType.Number )
			{
				result.M11 = reader.GetSingle();
				reader.Read();
			}
			if ( reader.TokenType == JsonTokenType.Number )
			{
				result.M12 = reader.GetSingle();
				reader.Read();
			}
			if ( reader.TokenType == JsonTokenType.Number )
			{
				result.M13 = reader.GetSingle();
				reader.Read();
			}
			if ( reader.TokenType == JsonTokenType.Number )
			{
				result.M14 = reader.GetSingle();
				reader.Read();
			}

			// Row 2
			if ( reader.TokenType == JsonTokenType.Number )
			{
				result.M21 = reader.GetSingle();
				reader.Read();
			}
			if ( reader.TokenType == JsonTokenType.Number )
			{
				result.M22 = reader.GetSingle();
				reader.Read();
			}
			if ( reader.TokenType == JsonTokenType.Number )
			{
				result.M23 = reader.GetSingle();
				reader.Read();
			}
			if ( reader.TokenType == JsonTokenType.Number )
			{
				result.M24 = reader.GetSingle();
				reader.Read();
			}

			// Row 3
			if ( reader.TokenType == JsonTokenType.Number )
			{
				result.M31 = reader.GetSingle();
				reader.Read();
			}
			if ( reader.TokenType == JsonTokenType.Number )
			{
				result.M32 = reader.GetSingle();
				reader.Read();
			}
			if ( reader.TokenType == JsonTokenType.Number )
			{
				result.M33 = reader.GetSingle();
				reader.Read();
			}
			if ( reader.TokenType == JsonTokenType.Number )
			{
				result.M34 = reader.GetSingle();
				reader.Read();
			}

			// Row 4
			if ( reader.TokenType == JsonTokenType.Number )
			{
				result.M41 = reader.GetSingle();
				reader.Read();
			}
			if ( reader.TokenType == JsonTokenType.Number )
			{
				result.M42 = reader.GetSingle();
				reader.Read();
			}
			if ( reader.TokenType == JsonTokenType.Number )
			{
				result.M43 = reader.GetSingle();
				reader.Read();
			}
			if ( reader.TokenType == JsonTokenType.Number )
			{
				result.M44 = reader.GetSingle();
				reader.Read();
			}

			while ( reader.TokenType != JsonTokenType.EndArray )
			{
				reader.Read();
			}

			return result;

		}

		GlobalSystemNamespace.Log.Warning( $"Float4x4FromJson - unable to read from {reader.TokenType}" );
		return default( Float4x4 );
	}

	public override void Write( Utf8JsonWriter writer, Float4x4 val, JsonSerializerOptions options )
	{
		writer.WriteStringValue(
			$"{val.M11:0.#################################}," +
			$"{val.M12:0.#################################}," +
			$"{val.M13:0.#################################}," +
			$"{val.M14:0.#################################}," +

			$"{val.M21:0.#################################}," +
			$"{val.M22:0.#################################}," +
			$"{val.M23:0.#################################}," +
			$"{val.M24:0.#################################}," +

			$"{val.M31:0.#################################}," +
			$"{val.M32:0.#################################}," +
			$"{val.M33:0.#################################}," +
			$"{val.M34:0.#################################}," +

			$"{val.M41:0.#################################}," +
			$"{val.M42:0.#################################}," +
			$"{val.M43:0.#################################}," +
			$"{val.M44:0.#################################}"
		);
	}
}

using Sandbox.Internal;

namespace ShaderGraphPlus;

internal class Matrix3x3Converter : JsonConverter<Float3x3>
{
	public override Float3x3 Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
	{
		if ( reader.TokenType == JsonTokenType.String )
		{
			return Float3x3.Parse( reader.GetString() );
		}

		if ( reader.TokenType == JsonTokenType.StartArray )
		{
			reader.Read();
			Float3x3 result = default( Float3x3 );

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

			while ( reader.TokenType != JsonTokenType.EndArray )
			{
				reader.Read();
			}

			return result;

		}

		GlobalSystemNamespace.Log.Warning( $"Float3x3FromJson - unable to read from {reader.TokenType}" );
		return default( Float3x3 );
	}

	public override void Write( Utf8JsonWriter writer, Float3x3 val, JsonSerializerOptions options )
	{
		writer.WriteStringValue(
			$"{val.M11:0.#################################}," +
			$"{val.M12:0.#################################}," +
			$"{val.M13:0.#################################}," +

			$"{val.M21:0.#################################}," +
			$"{val.M22:0.#################################}," +
			$"{val.M23:0.#################################}," +

			$"{val.M31:0.#################################}," +
			$"{val.M32:0.#################################}," +
			$"{val.M33:0.#################################}"
		);
	}
}

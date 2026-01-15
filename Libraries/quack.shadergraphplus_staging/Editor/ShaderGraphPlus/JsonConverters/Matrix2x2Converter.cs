using Sandbox.Internal;

namespace ShaderGraphPlus;

internal class Matrix2x2Converter : JsonConverter<Float2x2>
{
	public override Float2x2 Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
	{
		if ( reader.TokenType == JsonTokenType.String )
		{
			return Float2x2.Parse( reader.GetString() );
		}

		if ( reader.TokenType == JsonTokenType.StartArray )
		{
			reader.Read();
			Float2x2 result = default( Float2x2 );


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

			while ( reader.TokenType != JsonTokenType.EndArray )
			{
				reader.Read();
			}

			return result;

		}

		GlobalSystemNamespace.Log.Warning( $"Float2x2FromJson - unable to read from {reader.TokenType}" );
		return default( Float2x2 );
	}

	public override void Write( Utf8JsonWriter writer, Float2x2 val, JsonSerializerOptions options )
	{
		writer.WriteStringValue(
			$"{val.M11:0.#################################}," +
			$"{val.M12:0.#################################}," +

			$"{val.M21:0.#################################}," +
			$"{val.M22:0.#################################}"
		);
	}
}

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Security.Principal;

namespace ShaderGraphPlus;

[JsonConverter( typeof( Matrix2x2Converter ) )]
public struct Float2x2
{
	[Hide]
	internal System.Numerics.Matrix3x2 _mat3x2;

	public static readonly Float2x2 Identity = new Float2x2(
		1f, 0f,
		0f, 1f
	);

	public float M11
	{
		readonly get
		{
			return _mat3x2.M11;
		}
		set
		{
			_mat3x2.M11 = value;
		}
	}

	public float M12
	{
		readonly get
		{
			return _mat3x2.M12;
		}
		set
		{
			_mat3x2.M12 = value;
		}
	}

	public float M21
	{
		readonly get
		{
			return _mat3x2.M21;
		}
		set
		{
			_mat3x2.M21 = value;
		}
	}

	public float M22
	{
		readonly get
		{
			return _mat3x2.M22;
		}
		set
		{
			_mat3x2.M22 = value;
		}
	}

	public Float2x2
	(
		float m11, float m12,
		float m21, float m22
	)
	{
		M11 = m11;
		M12 = m12;

		M21 = m21;
		M22 = m22;
	}

	public static Float2x2 Parse( string str, IFormatProvider provider )
	{
		return Parse( str );
	}

	public static Float2x2 Parse( string str )
	{
		if ( TryParse( str, CultureInfo.InvariantCulture, out var result ) )
		{
			return result;
		}

		return default( Float2x2 );
	}
	public static bool TryParse( string str, out Float2x2 result )
	{
		return TryParse( str, CultureInfo.InvariantCulture, out result );
	}

	//
	// Summary:
	//     Given a string, try to convert this into a vector. Example input formats that
	//     work would be "1,1,1", "1;1;1", "[1 1 1]". This handles a bunch of different
	//     separators ( ' ', ',', ';', '\n', '\r' ). It also trims surrounding characters
	//     ('[', ']', ' ', '\n', '\r', '\t', '"').
	public static bool TryParse( [NotNullWhen( true )] string str, IFormatProvider provider, [MaybeNullWhen( false )] out Float2x2 result )
	{
		result = Identity;
		if ( string.IsNullOrWhiteSpace( str ) )
		{
			return false;
		}

		str = str.Trim( '[', ']', ' ', '\n', '\r', '\t', '"' );
		string[] array = str.Split( new char[5] { ' ', ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries );

		if (
		!float.TryParse( array[0], NumberStyles.Float, provider, out var m11 ) ||
		!float.TryParse( array[1], NumberStyles.Float, provider, out var m12 ) ||
		!float.TryParse( array[2], NumberStyles.Float, provider, out var m21 ) ||
		!float.TryParse( array[3], NumberStyles.Float, provider, out var m22 )
		)
		{
			return false;
		}

		result = new Float2x2(
			m11,
			m12,
			m21,
			m22
		);

		return true;
	}

	public override readonly string ToString()
	{
		return
			$"{M11:0.#####}," +
			$"{M12:0.#####}," +
			$"{M21:0.#####}," +
			$"{M22:0.#####}";
	}

	public override readonly int GetHashCode()
	{
		return HashCode.Combine( _mat3x2 );
	}
}

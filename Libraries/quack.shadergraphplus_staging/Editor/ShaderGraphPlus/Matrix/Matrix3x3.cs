using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace ShaderGraphPlus;

[JsonConverter( typeof( Matrix3x3Converter ) )]
public struct Float3x3
{
	[Hide]
	internal System.Numerics.Matrix4x4 _mat4x4;

	public static readonly Float3x3 Identity = new Float3x3
	(
		1f, 0f, 0f,
		0f, 1f, 0f,
		0f, 0f, 1f
	);

	public float M11
	{
		readonly get
		{
			return _mat4x4.M11;
		}
		set
		{
			_mat4x4.M11 = value;
		}
	}

	public float M12
	{
		readonly get
		{
			return _mat4x4.M12;
		}
		set
		{
			_mat4x4.M12 = value;
		}
	}

	public float M13
	{
		readonly get
		{
			return _mat4x4.M13;
		}
		set
		{
			_mat4x4.M13 = value;
		}
	}

	public float M21
	{
		readonly get
		{
			return _mat4x4.M21;
		}
		set
		{
			_mat4x4.M21 = value;
		}
	}

	public float M22
	{
		readonly get
		{
			return _mat4x4.M22;
		}
		set
		{
			_mat4x4.M22 = value;
		}
	}

	public float M23
	{
		readonly get
		{
			return _mat4x4.M23;
		}
		set
		{
			_mat4x4.M23 = value;
		}
	}

	public float M31
	{
		readonly get
		{
			return _mat4x4.M31;
		}
		set
		{
			_mat4x4.M31 = value;
		}
	}

	public float M32
	{
		readonly get
		{
			return _mat4x4.M32;
		}
		set
		{
			_mat4x4.M32 = value;
		}
	}

	public float M33
	{
		readonly get
		{
			return _mat4x4.M33;
		}
		set
		{
			_mat4x4.M33 = value;
		}
	}

	public Float3x3
	(
		float m11, float m12, float m13,
		float m21, float m22, float m23,
		float m31, float m32, float m33
	)
	{
		_mat4x4 = new System.Numerics.Matrix4x4(
			m11, m12, m13, 0f,
			m21, m22, m23, 0f,
			m31, m32, m33, 0f,
			0f, 0f, 0f, 1f
		);
	}


	public static Float3x3 Parse( string str, IFormatProvider provider )
	{
		return Parse( str );
	}

	public static Float3x3 Parse( string str )
	{
		if ( TryParse( str, CultureInfo.InvariantCulture, out var result ) )
		{
			return result;
		}

		return default( Float3x3 );
	}
	public static bool TryParse( string str, out Float3x3 result )
	{
		return TryParse( str, CultureInfo.InvariantCulture, out result );
	}

	//
	// Summary:
	//     Given a string, try to convert this into a vector. Example input formats that
	//     work would be "1,1,1", "1;1;1", "[1 1 1]". This handles a bunch of different
	//     separators ( ' ', ',', ';', '\n', '\r' ). It also trims surrounding characters
	//     ('[', ']', ' ', '\n', '\r', '\t', '"').
	public static bool TryParse( [NotNullWhen( true )] string str, IFormatProvider provider, [MaybeNullWhen( false )] out Float3x3 result )
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
		!float.TryParse( array[2], NumberStyles.Float, provider, out var m13 ) ||

		!float.TryParse( array[3], NumberStyles.Float, provider, out var m21 ) ||
		!float.TryParse( array[4], NumberStyles.Float, provider, out var m22 ) ||
		!float.TryParse( array[5], NumberStyles.Float, provider, out var m23 ) ||

		!float.TryParse( array[6], NumberStyles.Float, provider, out var m31 ) ||
		!float.TryParse( array[7], NumberStyles.Float, provider, out var m32 ) ||
		!float.TryParse( array[8], NumberStyles.Float, provider, out var m33 )

		)
		{
			return false;
		}

		result = new Float3x3(
			m11,
			m12,
			m13,

			m21,
			m22,
			m23,

			m31,
			m32,
			m33
		);

		return true;
	}


	public override readonly string ToString()
	{
		return
			$"{M11:0.#####}," +
			$"{M12:0.#####}," +
			$"{M13:0.#####}," +

			$"{M21:0.#####}," +
			$"{M22:0.#####}," +
			$"{M23:0.#####}," +

			$"{M31:0.#####}," +
			$"{M32:0.#####}," +
			$"{M33:0.#####}";
	}


	public override readonly int GetHashCode()
	{
		return HashCode.Combine( _mat4x4 );
	}
}

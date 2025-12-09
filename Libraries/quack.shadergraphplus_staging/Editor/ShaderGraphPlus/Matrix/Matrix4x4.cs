using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace ShaderGraphPlus;

[JsonConverter( typeof( Matrix4x4Converter ) )]
public struct Float4x4
{
	[Hide]
	internal System.Numerics.Matrix4x4 _mat4x4;

	//
	// Summary:
	//     A rotation that represents no rotation.
	public static readonly Float4x4 Identity = new Float4x4
	{
		_mat4x4 = Matrix4x4.Identity
	};

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

	public float M14
	{
		readonly get
		{
			return _mat4x4.M14;
		}
		set
		{
			_mat4x4.M14 = value;
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

	public float M24
	{
		readonly get
		{
			return _mat4x4.M24;
		}
		set
		{
			_mat4x4.M24 = value;
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

	public float M34
	{
		readonly get
		{
			return _mat4x4.M34;
		}
		set
		{
			_mat4x4.M34 = value;
		}
	}

	public float M41
	{
		readonly get
		{
			return _mat4x4.M41;
		}
		set
		{
			_mat4x4.M41 = value;
		}
	}

	public float M42
	{
		readonly get
		{
			return _mat4x4.M42;
		}
		set
		{
			_mat4x4.M42 = value;
		}
	}

	public float M43
	{
		readonly get
		{
			return _mat4x4.M43;
		}
		set
		{
			_mat4x4.M43 = value;
		}
	}

	public float M44
	{
		readonly get
		{
			return _mat4x4.M44;
		}
		set
		{
			_mat4x4.M44 = value;
		}
	}

	//
	// Summary:
	//     Initializes this flaot4x4 to identity.
	public Float4x4()
	{
		_mat4x4 = Matrix4x4.Identity;

	}

	//
	// Summary:
	//	   Initializes the 4x4 matrix from given components.
	//
	// Parameters:
	//	 m11:
	//		 The value for the first element in the first row.
	//	 m12:
	//		 The value for the second element in the first row.
	//	 m13:
	//		 The value for the third element in the first row.
	//	 m14:
	//		 The value for the fourth element in the first row.
	//	 m21:
	//		 The value for the first element in the second row.
	//	 m22:
	//		 The value for the second element in the second row.
	//	 m23:
	//		 The value for the third element in the second row.
	//	 m24:
	//		 The value for the third element in the second row.
	//	 m31:
	//		 The value for the first element in the third row.
	//	 m32:
	//		 The value for the second element in the third row.
	//	 m33:
	//		 The value for the third element in the third row.
	//	 m34:
	//		 The value for the fourth element in the third row.
	//	 m41:
	//		 The value for the first element in the fourth row.
	//	 m42:
	//		 The value for the second element in the fourth row.
	//	 m43:
	//		 The value for the third element in the fourth row.
	//	 m44:
	//		 The value for the fourth element in the fourth row.
	public Float4x4( float m11, float m12, float m13, float m14,
					float m21, float m22, float m23, float m24,
					float m31, float m32, float m33, float m34,
					float m41, float m42, float m43, float m44 )
	{
		_mat4x4 = new System.Numerics.Matrix4x4(
			m11, m12, m13, m14,
			m21, m22, m23, m24,
			m31, m32, m33, m34,
			m41, m42, m43, m44
		);
	}

	/// <summary>Creates a <see cref="Float4x4" /> object from a specified <see cref="System.Numerics.Matrix3x2" /> object.</summary>
	/// <param name="value">A 3x2 matrix.</param>
	/// <remarks>This constructor creates a float 4x4 matrix whose <see cref="Float4x4.M13" />, <see cref="Float4x4.M14" />, <see cref="Float4x4.M23" />, <see cref="Float4x4.M24" />, <see cref="Float4x4.M31" />, <see cref="Float4x4.M32" />, <see cref="Float4x4.M34" />, and <see cref="Float4x4.M43" /> components are zero, and whose <see cref="Float4x4.M33" /> and <see cref="Float4x4.M44" /> components are one.</remarks>
	public Float4x4( Matrix3x2 value )
	{
		_mat4x4 = new System.Numerics.Matrix4x4(
			value.M11, value.M12, 0f, 0f,
			value.M21, value.M22, 0f, 0f,
			0f, 0f, 1f, 0f,
			value.M31, value.M32, 0f, 1f
		);
	}


	public static Float4x4 Parse( string str, IFormatProvider provider )
	{
		return Parse( str );
	}

	public static Float4x4 Parse( string str )
	{
		if ( TryParse( str, CultureInfo.InvariantCulture, out var result ) )
		{
			return result;
		}

		return default( Float4x4 );
	}
	public static bool TryParse( string str, out Float4x4 result )
	{
		return TryParse( str, CultureInfo.InvariantCulture, out result );
	}

	//
	// Summary:
	//     Given a string, try to convert this into a vector. Example input formats that
	//     work would be "1,1,1", "1;1;1", "[1 1 1]". This handles a bunch of different
	//     separators ( ' ', ',', ';', '\n', '\r' ). It also trims surrounding characters
	//     ('[', ']', ' ', '\n', '\r', '\t', '"').
	public static bool TryParse( [NotNullWhen( true )] string str, IFormatProvider provider, [MaybeNullWhen( false )] out Float4x4 result )
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
		!float.TryParse( array[3], NumberStyles.Float, provider, out var m14 ) ||

		!float.TryParse( array[4], NumberStyles.Float, provider, out var m21 ) ||
		!float.TryParse( array[5], NumberStyles.Float, provider, out var m22 ) ||
		!float.TryParse( array[6], NumberStyles.Float, provider, out var m23 ) ||
		!float.TryParse( array[7], NumberStyles.Float, provider, out var m24 ) ||

		!float.TryParse( array[8], NumberStyles.Float, provider, out var m31 ) ||
		!float.TryParse( array[9], NumberStyles.Float, provider, out var m32 ) ||
		!float.TryParse( array[10], NumberStyles.Float, provider, out var m33 ) ||
		!float.TryParse( array[11], NumberStyles.Float, provider, out var m34 ) ||

		!float.TryParse( array[12], NumberStyles.Float, provider, out var m41 ) ||
		!float.TryParse( array[13], NumberStyles.Float, provider, out var m42 ) ||
		!float.TryParse( array[14], NumberStyles.Float, provider, out var m43 ) ||
		!float.TryParse( array[15], NumberStyles.Float, provider, out var m44 )
		)
		{
			return false;
		}

		result = new Float4x4(
			m11,
			m12,
			m13,
			m14,
			m21,
			m22,
			m23,
			m24,
			m31,
			m32,
			m33,
			m34,
			m41,
			m42,
			m43,
			m44
		);

		return true;
	}


	public override readonly string ToString()
	{
		return
			$"{M11:0.#####}," +
			$"{M12:0.#####}," +
			$"{M13:0.#####}," +
			$"{M14:0.#####}," +
			$"{M21:0.#####}," +
			$"{M22:0.#####}," +
			$"{M23:0.#####}," +
			$"{M24:0.#####}," +
			$"{M31:0.#####}," +
			$"{M32:0.#####}," +
			$"{M33:0.#####}," +
			$"{M34:0.#####}," +
			$"{M41:0.#####}," +
			$"{M42:0.#####}," +
			$"{M43:0.#####}," +
			$"{M44:0.#####}";
	}

	public override readonly int GetHashCode()
	{
		return HashCode.Combine( _mat4x4 );
	}

}

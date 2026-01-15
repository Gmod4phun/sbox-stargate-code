
namespace ShaderGraphPlus.Nodes;

public enum SwizzleChannel
{
	Red = 0,
	Green = 1,
	Blue = 2,
	Alpha = 3,
}

[Title( "Component Mask" ), Category( "Channel" ), Icon( "call_split" )]
public sealed class ComponentMask : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.ChannelNode;

	[Hide]
	public override string Title
	{
		get
		{
			List<string> components = new List<string>();

			if ( R && _showR ) components.Add( "R" );
			if ( G && _showG ) components.Add( "G" );
			if ( B && _showB ) components.Add( "B" );
			if ( A && _showA ) components.Add( "A" );

			var suffix = components.Count > 0 ? $"{string.Join( " ", components )}" : "";

			return !string.IsNullOrWhiteSpace( suffix ) ? $"{DisplayInfo.For( this ).Name} ( {suffix} )" : DisplayInfo.For( this ).Name;
		}
	}

	[Input, Hide]
	public NodeInput Input { get; set; }

	[ShowIf( nameof( _showR ), true )]
	public bool R { get; set; } = true;

	[ShowIf( nameof( _showG ), true )]
	public bool G { get; set; } = true;

	[ShowIf( nameof( _showB ), true )]
	public bool B { get; set; } = true;

	[ShowIf( nameof( _showA ), true )]
	public bool A { get; set; } = true;

	[Hide, JsonIgnore]
	private bool _showR = false;
	[Hide, JsonIgnore]
	private bool _showG = false;
	[Hide, JsonIgnore]
	private bool _showB = false;
	[Hide, JsonIgnore]
	private bool _showA = false;

	[Output, Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var result = compiler.Result( Input );

		if ( !result.IsValid )
		{
			(_showR, _showG, _showB, _showA) = (true, true, true, true);

			return new NodeResult( ResultType.Float, "0.0f" );
		}
		else
		{

			if ( result.ResultType != ResultType.Float && result.ResultType != ResultType.Vector2 &&
				result.ResultType != ResultType.Vector3 && result.ResultType != ResultType.Vector4 && result.ResultType != ResultType.Color )
			{
				HasError = true;
				return NodeResult.Error( $"Cannot mask ResultType \"{result.ResultType}\"" );
			}

			HasError = false;

			var resultType = ResultType.Float;
			var components = string.Empty;

			switch ( result.Components )
			{
				case 1:
					(_showR, _showG, _showB, _showA) = (false, false, false, false);

					break;
				case 2:
					(_showR, _showG, _showB, _showA) = (true, true, false, false);

					if ( R ) components += "x";
					if ( G ) components += "y";
					break;

				case 3:
					(_showR, _showG, _showB, _showA) = (true, true, true, false);

					if ( R ) components += "x";
					if ( G ) components += "y";
					if ( B ) components += "z";
					break;

				case 4:
					(_showR, _showG, _showB, _showA) = (true, true, true, true);

					if ( R ) components += "x";
					if ( G ) components += "y";
					if ( B ) components += "z";
					if ( A ) components += "w";
					break;
			}

			// result.ResultType was a float. So there is nothing to Mask.
			if ( result.ResultType == ResultType.Float )
			{
				return new NodeResult( ResultType.Float, $"{result}" );
			}

			//if ( components == string.Empty )
			//{
			//	return new NodeResult( ResultType.Float, "0.0f" );
			//}

			if ( components.Length == 1 ) resultType = ResultType.Float;
			if ( components.Length == 2 ) resultType = ResultType.Vector2;
			if ( components.Length == 3 ) resultType = ResultType.Vector3;
			if ( components.Length == 4 ) resultType = ResultType.Color;

			return new NodeResult( resultType, $"{result}.{components}" );
		}
	};
}


/// <summary>
/// Split value into individual components
/// </summary>
[Title( "Split" ), Category( "Channel" ), Icon( "call_split" )]
public sealed class SplitVector : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.ChannelNode;

	[Input, Hide]
	public NodeInput Input { get; set; }

	[Output( typeof( float ) ), Hide]
	public NodeResult.Func X => ( GraphCompiler compiler ) =>
	{
		var result = compiler.Result( Input );
		if ( result.IsValid && result.Components > 0 ) return new NodeResult( ResultType.Float, $"{result}.x" );
		return new NodeResult( ResultType.Float, "0.0f" );
	};

	[Output( typeof( float ) ), Hide]
	public NodeResult.Func Y => ( GraphCompiler compiler ) =>
	{
		var result = compiler.Result( Input );
		if ( result.IsValid && result.Components > 1 ) return new NodeResult( ResultType.Float, $"{result}.y" );
		return new NodeResult( ResultType.Float, "0.0f" );
	};

	[Output( typeof( float ) ), Hide]
	public NodeResult.Func Z => ( GraphCompiler compiler ) =>
	{
		var result = compiler.Result( Input );
		if ( result.IsValid && result.Components > 2 ) return new NodeResult( ResultType.Float, $"{result}.z" );
		return new NodeResult( ResultType.Float, "0.0f" );
	};

	[Output( typeof( float ) ), Hide]
	public NodeResult.Func W => ( GraphCompiler compiler ) =>
	{
		var result = compiler.Result( Input );
		if ( result.IsValid && result.Components > 3 ) return new NodeResult( ResultType.Float, $"{result}.w" );
		return new NodeResult( ResultType.Float, "0.0f" );
	};
}

/// <summary>
/// Combine input values into 3 separate vectors
/// </summary>
[Title( "Combine" ), Category( "Channel" ), Icon( "call_merge" )]
public sealed class CombineVector : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.ChannelNode;

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput X { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Y { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Z { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput W { get; set; }

	public float DefaultX { get; set; } = 0.0f;
	public float DefaultY { get; set; } = 0.0f;
	public float DefaultZ { get; set; } = 0.0f;
	public float DefaultW { get; set; } = 0.0f;

	[Output( typeof( Vector4 ) )]
	[Hide]
	public NodeResult.Func XYZW => ( GraphCompiler compiler ) =>
	{
		var x = compiler.ResultOrDefault( X, DefaultX ).Cast( 1 );
		var y = compiler.ResultOrDefault( Y, DefaultY ).Cast( 1 );
		var z = compiler.ResultOrDefault( Z, DefaultZ ).Cast( 1 );
		var w = compiler.ResultOrDefault( W, DefaultW ).Cast( 1 );

		return new NodeResult( ResultType.Color, $"float4( {x}, {y}, {z}, {w} )" );
	};

	[Output( typeof( Vector3 ) )]
	[Hide]
	public NodeResult.Func XYZ => ( GraphCompiler compiler ) =>
	{
		var x = compiler.ResultOrDefault( X, DefaultX ).Cast( 1 );
		var y = compiler.ResultOrDefault( Y, DefaultY ).Cast( 1 );
		var z = compiler.ResultOrDefault( Z, DefaultZ ).Cast( 1 );

		return new NodeResult( ResultType.Vector3, $"float3( {x}, {y}, {z} )" );
	};

	[Output( typeof( Vector2 ) )]
	[Hide]
	public NodeResult.Func XY => ( GraphCompiler compiler ) =>
	{
		var x = compiler.ResultOrDefault( X, DefaultX ).Cast( 1 );
		var y = compiler.ResultOrDefault( Y, DefaultY ).Cast( 1 );

		return new NodeResult( ResultType.Vector2, $"float2( {x}, {y})" );
	};
}

/// <summary>
/// Swap components of a color around
/// </summary>
[Title( "Swizzle" ), Category( "Channel" ), Icon( "swap_horiz" )]
public sealed class SwizzleVector : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.ChannelNode;

	[Input, Hide]
	public NodeInput Input { get; set; }

	public SwizzleChannel RedOut { get; set; } = SwizzleChannel.Red;
	public SwizzleChannel GreenOut { get; set; } = SwizzleChannel.Green;
	public SwizzleChannel BlueOut { get; set; } = SwizzleChannel.Blue;
	public SwizzleChannel AlphaOut { get; set; } = SwizzleChannel.Alpha;

	private static char SwizzleToChannel( SwizzleChannel channel )
	{
		return channel switch
		{
			SwizzleChannel.Green => 'y',
			SwizzleChannel.Blue => 'z',
			SwizzleChannel.Alpha => 'w',
			_ => 'x',
		};
	}

	[Output( typeof( Vector4 ) ), Hide]
	public NodeResult.Func Output => ( GraphCompiler compiler ) =>
	{
		var input = compiler.Result( Input );
		if ( !input.IsValid )
			return default;

		var swizzle = $".";
		swizzle += SwizzleToChannel( RedOut );
		swizzle += SwizzleToChannel( GreenOut );
		swizzle += SwizzleToChannel( BlueOut );
		swizzle += SwizzleToChannel( AlphaOut );

		return new NodeResult( ResultType.Color, $"{input.Cast( 4 )}{swizzle}" );
	};
}

/// <summary>
/// Append constants to change number of channels
/// </summary>
[Title( "Append" ), Category( "Channel" )]
public sealed class AppendVector : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.ChannelNode;

	[Input, Hide]
	public NodeInput A { get; set; }

	[Input, Hide]
	public NodeInput B { get; set; }

	[Output, Hide]
	public NodeResult.Func Output => ( GraphCompiler compiler ) =>
	{
		var resultA = compiler.ResultOrDefault( A, 0.0f );
		var resultB = compiler.ResultOrDefault( B, 0.0f );

		var components = resultB.Components + resultA.Components;
		if ( components < 1 || components > 4 )
			return NodeResult.Error( $"Can't append {resultB.TypeName} to {resultA.TypeName}" );

		return new NodeResult( (ResultType)components, $"float{components}( {resultA}, {resultB} )" );
	};
}

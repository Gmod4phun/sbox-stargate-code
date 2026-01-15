using Editor;

namespace ShaderGraphPlus;

public class DefaultEditor : ValueEditor
{
	NodePlug Plug;
	int _textHash;
	float _labelWidth;
	Rect _boundingRect;

	public override Rect BoundingRect => _boundingRect;
	public override bool HideLabel => false;

	public DefaultEditor( GraphicsItem parent ) : base( parent )
	{
		HoverEvents = true;
		Cursor = CursorShape.Finger;

		if ( parent is NodePlug plug )
		{
			Plug = plug;
		}

		ZIndex = -1;
	}

	protected override void OnMousePressed( GraphicsMouseEvent e )
	{
		Plug.MousePressed( e );
	}

	protected override void OnMouseReleased( GraphicsMouseEvent e )
	{
		Plug.MouseReleased( e );
	}

	protected override void OnMouseMove( GraphicsMouseEvent e )
	{
		Plug.MouseMove( e );
	}

	protected override void OnPaint()
	{
		if ( !Enabled ) return;
		if ( Plug is null ) return;
		if ( Plug?.Node?.Node is not ShaderNodePlus node ) return;
		if ( Plug.Inner is IPlugOut plugOut ) return;
		if ( Plug.Inner is IPlugIn plugIn )
		{
			if ( plugIn.ConnectedOutput is not null ) return;
		}

		var lastTextHash = _textHash;
		var lastLabelWidth = _labelWidth;

		var so = node.GetSerialized();
		Type type = typeof( Type );
		object rawVal = null;
		string val = null;
		foreach ( var property in so )
		{
			if ( property.TryGetAttribute<BaseNodePlus.InputDefaultAttribute>( out var inputDefault ) )
			{
				if ( inputDefault.Input == Plug.Inner.Identifier )
				{
					type = property.PropertyType;
					rawVal = property.GetValue<object>();
					val = rawVal.ToString();
					break;
				}
			}
		}
		if ( val is null && node is SubgraphNode subgraphNode && Plug.Inner is IPlugIn innerPlugIn )
		{
			if ( subgraphNode.InputReferences.TryGetValue( innerPlugIn, out var entry ) )
			{
				var subgraphInput = entry.inputNode;
				if ( subgraphInput.IsRequired ) return;
				type = entry.inputNodeValueType;
				if ( innerPlugIn.ConnectedOutput is not null )
				{
					rawVal = subgraphInput.DefaultData;
					val = rawVal.ToString();
				}
				else
				{
					rawVal = subgraphNode.DefaultValues.GetValueOrDefault( innerPlugIn.Identifier );
					val = rawVal?.ToString() ?? "";
				}
			}
		}

		if ( string.IsNullOrEmpty( val ) ) return;

		Paint.Antialiasing = true;
		Paint.TextAntialiasing = true;
		var rect = Parent.LocalRect;

		var shrink = 10f;
		var extraWidth = 0f;
		val = PaintHelper.FormatValue( type, rawVal, out extraWidth, out rawVal );

		if ( string.IsNullOrWhiteSpace( val ) ) return;

		var textSize = Paint.MeasureText( val ) + extraWidth;

		var valueRect = new Rect( rect.Left - textSize.x - shrink * 2 - 8f, rect.Top, textSize.x + shrink * 2,
		rect.Height )
				.Shrink( 0f, 2f, 0f, 2f );

		//if ( eventArgs.Icon is not null )
		//{
		//	valueRect = valueRect.Grow( 20f, 0f, 0f, 0f );
		//}

		var handleConfig = Plug.HandleConfig;
		Paint.SetPen( handleConfig.Color, 4f );
		Paint.DrawLine( rect.Center.WithX( rect.Left - 9f - 2f ), rect.Center.WithX( rect.Left ) );
		PaintHelper.DrawValue( handleConfig, valueRect, val, 1f, "", rawVal );

		_boundingRect = valueRect.Grow( 24, 0 );

		_textHash = val.FastHash();
		_labelWidth = valueRect.Width;
		if ( _textHash != lastTextHash || Math.Abs( _labelWidth - lastLabelWidth ) > 0.01f )
		{
			PrepareGeometryChange();
		}
	}
}

internal static class PaintHelper
{
	public static string FormatValue( Type type, object value, out float extraWidth, out object rawValue )
	{
		extraWidth = 0f;
		rawValue = value;

		if ( rawValue is JsonElement element )
		{
			rawValue = DeserializeElement( element, type );
		}

		switch ( rawValue )
		{
			case null when !type.IsValueType:
				return "null";

			case string str:
				return $"\"{str}\"";

			case Resource resource:
				return resource.ResourcePath;

			case Color32 color32:
				rawValue = (Color)color32;
				extraWidth = 20f;

				return color32.a >= 255
					? color32.Hex
					: $"{(color32 with { a = 255 }).Hex}, {color32.a * 100f / 255f:F0}%";

			case bool boolVal:
				return $"{boolVal}";

			case int int32Val:
				return $"{int32Val}";

			case float floatVal:
				return $"{floatVal:F3}";

			case double doubleVal:
				return $"{doubleVal:F3}";

			case Vector2 vec2:
				return $"x: {vec2.x:F3}, y: {vec2.y:F3}";

			case Vector2Int vec2Int:
				return $"x: {vec2Int.x}, y: {vec2Int.y}";

			case Vector3 vec3:
				return $"x: {vec3.x:F3}, y: {vec3.y:F3}, z: {vec3.z:F3}";

			case Vector3Int vec3Int:
				return $"x: {vec3Int.x}, y: {vec3Int.y}, z: {vec3Int.z}";

			case Vector4 vec4:
				return $"x: {vec4.x:F3}, y: {vec4.y:F3}, z: {vec4.z:F3}, w: {vec4.w:F3}";

			case Color color:
				extraWidth = 20f;

				return color.a >= 0.995f
					? color.WithAlpha( 1f ).Hex
					: $"{color.WithAlpha( 1f ).Hex}, {color.a * 100:F0}%";

			// Nothing for Gradient type for now...
			case Gradient gradient:
				return $"";

			case Rotation rot:
				rawValue = (Angles)rot;
				return $"p: {rot.Pitch():F2}, y: {rot.Yaw():F2}, r: {rot.Roll():F2}";

			case Angles angles:
				return $"p: {angles.pitch:F2}, y: {angles.yaw:F2}, r: {angles.roll:F2}";

			case Sampler sampler:
				return $"{sampler.Name}";

			case TextureInput input:
				return $"{input.Name}";

			default:
				return $"{rawValue}";
		}
	}

	private static object DeserializeElement( JsonElement element, Type type )
	{
		if ( type == typeof( bool ) )
		{
			return bool.Parse( element.GetRawText() );
		}
		else if ( type == typeof( int ) )
		{
			return int.Parse( element.GetRawText() );
		}
		else if ( type == typeof( float ) )
		{
			return float.Parse( element.GetRawText() );
		}
		else if ( type == typeof( Vector2 ) )
		{
			return Vector2.Parse( element.GetRawText() );
		}
		else if ( type == typeof( Vector2Int ) )
		{
			return Vector2Int.Parse( element.GetRawText() );
		}
		else if ( type == typeof( Vector3 ) )
		{
			return Vector3.Parse( element.GetRawText() );
		}
		else if ( type == typeof( Vector3Int ) )
		{
			return Vector3Int.Parse( element.GetRawText() );
		}
		else if ( type == typeof( Vector4 ) )
		{
			return Vector4.Parse( element.GetRawText() );
		}
		else if ( type == typeof( Color ) )
		{
			return Color.Parse( element.GetRawText() );
		}
		else if ( type == typeof( Sampler ) )
		{
			return JsonSerializer.Deserialize<Sampler>( element, ShaderGraphPlus.SerializerOptions() )!;
		}
		else if ( type == typeof( Texture2DObject ) )
		{
			return (JsonSerializer.Deserialize<TextureInput>( element, ShaderGraphPlus.SerializerOptions() )!) with { Type = TextureType.Tex2D };
		}
		else if ( type == typeof( TextureCubeObject ) )
		{
			return (JsonSerializer.Deserialize<TextureInput>( element, ShaderGraphPlus.SerializerOptions() )!) with { Type = TextureType.TexCube };
		}
		throw new Exception( $"Cannot Deserialize `{type}`" );
	}

	public static void DrawValue( NodeHandleConfig handleConfig, Rect valueRect, string text, float pulseScale = 1f, string icon = null, object rawValue = null )
	{
		var bg = Theme.ControlBackground;
		var fg = Theme.TextControl;

		var borderColor = handleConfig.Color.Desaturate( 0.2f ).Darken( 0.3f );

		if ( pulseScale > 1f )
		{
			bg = Color.Lerp( bg, borderColor, (pulseScale - 1f) * 0.25f );
		}

		Paint.SetPen( borderColor, 2f * (pulseScale * 0.5f + 0.5f) );
		Paint.SetBrush( bg );
		Paint.DrawRect( valueRect, 2 );

		if ( rawValue is Color color )
		{
			ColorPalette.PaintSwatch( color, new Rect( valueRect.Left + 3f, valueRect.Top + 3f, 14f, 14f ), false, radius: 2, disabled: false );
			valueRect = valueRect.Shrink( 14f, 0f, 0f, 0f );
		}

		Paint.SetPen( fg );

		if ( !string.IsNullOrEmpty( icon ) )
		{
			Paint.DrawIcon( new Rect( valueRect.Left + 8f, valueRect.Top, 16f, valueRect.Height ), icon, 16f );
			valueRect = valueRect.Shrink( 20f, 0f, 0f, 0f );
		}

		Paint.DrawText( valueRect, text, TextFlag.Center );
	}
}

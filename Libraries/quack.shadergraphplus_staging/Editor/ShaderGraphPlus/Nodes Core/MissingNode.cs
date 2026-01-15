using Editor;

namespace ShaderGraphPlus.Nodes;

[InternalNode]
public class MissingNode : BaseNodePlus
{
	[Hide]
	public override int Version => 1;

	[Hide, Browsable( false ), JsonIgnore]
	public override Color NodeTitleColor => Color.Gray;

	[Hide, Browsable( false ), JsonIgnore]
	public override Color NodeBodyTintColor => Color.White;

	[Hide]
	public string Title { get; set; }

	[Hide]
	private string _content = "";

	[Hide]
	public string Content
	{
		get => _content;
		set
		{
			_content = value;
			Paint.SetDefaultFont();
			ContentSize = Paint.MeasureText( Content );
			ExpandSize = new Vector3( 30, ContentSize.y + 16 );
		}
	}

	[Hide]
	Vector2 ContentSize = new();

	public MissingNode()
	{
	}

	public MissingNode( string title, JsonElement json ) : base()
	{
		Title = title;
		Content = json.ToString();
	}

	public override void OnPaint( Rect rect )
	{
		Paint.SetDefaultFont();
		Paint.DrawText( rect.Shrink( 8, 22, 8, 8 ), Content, TextFlag.LeftTop );
	}

	[JsonIgnore, Hide, Browsable( false )]
	public override DisplayInfo DisplayInfo
	{
		get
		{
			var info = base.DisplayInfo;
			info.Name = "Missing " + Title ?? info.Name;
			info.Icon = "error";
			return info;
		}
	}

	public override NodeUI CreateUI( GraphView view )
	{
		return new NodeUI( view, this, true );
	}
}

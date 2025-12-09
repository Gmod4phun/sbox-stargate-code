using Editor;

namespace ShaderGraphPlus;

public abstract class ShaderNodePlus : BaseNodePlus
{
	[Hide]
	public virtual string Title => null;

	[JsonIgnore, Hide, Browsable( false )]
	public override string Subtitle => "";

	[Hide]
	public bool IsDirty = false;

	[JsonIgnore, Hide, Browsable( false )]
	public override DisplayInfo DisplayInfo
	{
		get
		{
			var info = base.DisplayInfo;
			info.Name = Title ?? info.Name;
			return info;
		}
	}

	public override NodeUI CreateUI( GraphView view )
	{
		return base.CreateUI( view );
	}
}

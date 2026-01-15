
using Editor;

namespace ShaderGraphPlus;

internal class GamePerformanceBar : Widget
{
	private readonly Func<string> _getValue;
	private RealTimeSince _timeSinceUpdate;

	public GamePerformanceBar( Func<string> val ) : base( null )
	{
		_getValue = val;

		MinimumHeight = Theme.RowHeight;
		MinimumWidth = 60;
	}

	protected override void DoLayout()
	{
		base.DoLayout();
	}

	protected override void OnPaint()
	{
		base.OnPaint();

		Paint.ClearPen();
		Paint.SetBrush( Theme.ControlBackground );
		Paint.DrawRect( LocalRect, Theme.ControlRadius );

		Paint.SetPen( Theme.Green.WithAlpha( 0.4f ) );
		Paint.DrawText( LocalRect.Shrink( 8, 0 ), _getValue(), TextFlag.RightCenter );
	}

	[EditorEvent.Frame]
	public void Frame()
	{
		if ( _timeSinceUpdate < 0.6f )
			return;

		_timeSinceUpdate = Random.Shared.Float( 0, 0.1f );

		Update();
	}
}

using Editor;
using ShaderGraphPlus.Nodes;
using static ShaderGraphPlus.ShaderGraphPlusGlobals;

namespace ShaderGraphPlus;

[CustomEditor( typeof( int ), NamedEditor = ControlWidgetCustomEditors.ShaderFeatureEnumPreviewIndexEditor )]
internal sealed class SGPFeatureEnumPreviewIndexControlWidget : DropdownControlWidgetPlus<int>
{
	EnumFeatureSwitchNode Node;

	Entry SelectedEntry;
	int SelectedIndex;

	public SGPFeatureEnumPreviewIndexControlWidget( SerializedProperty property ) : base( property )
	{
		Node = property.Parent.Targets.OfType<EnumFeatureSwitchNode>().FirstOrDefault();

		if ( Node is null ) return;

		var currentSelctedIndex = SerializedProperty.GetValue<int>();
		if ( TryGetEntryFromIndex( currentSelctedIndex, out var entry ) )
		{
			SelectedEntry = entry;
			SelectedIndex = currentSelctedIndex;
		}
		else
		{
			SGPLog.Error( $"Couldnt find entry at index \"{currentSelctedIndex}\"" );
		}
	}

	private bool TryGetEntryFromIndex( int selectedIndex, out Entry foundEntry )
	{
		foundEntry = new();

		foreach ( var entry in GetDropdownValues().OfType<Entry>().Index() )
		{
			if ( entry.Index == selectedIndex )
			{
				foundEntry = entry.Item;

				return true;
			}
		}

		return false;
	}

	private bool TryGetyIndexFromEntry( Entry selectedEntry, out int foundIndex )
	{
		foundIndex = new();

		foreach ( var entry in Node.Feature.Options.Index() )
		{
			if ( entry.Item == selectedEntry.Label )
			{
				foundIndex = entry.Index;

				return true;
			}
		}

		return false;
	}

	protected override void OnSelectItem( object item )
	{
		base.OnSelectItem( item );

		if ( item is Entry e )
		{
			SelectedEntry = e;

			if ( TryGetyIndexFromEntry( SelectedEntry, out var newSelectedIndex ) )
			{
				SelectedIndex = newSelectedIndex;
			}
		}
	}

	protected override IEnumerable<object> GetDropdownValues()
	{
		List<object> list = new();

		foreach ( var option in Node.Feature.Options.Index() )
		{
			var entry = new Entry();
			entry.Value = option.Index;
			entry.Label = option.Item;
			entry.Description = "";
			list.Add( entry );
		}

		return list;
	}

	protected override void OnPaint()
	{
		Paint.Antialiasing = true;
		Paint.TextAntialiasing = true;
		PaintUnder();
		PaintControl();
		PaintOver();
	}

	protected override void PaintControl()
	{
		var entryLabel = SelectedEntry.Label; // at index {SelectedIndex}";
		var color = IsControlHovered ? Theme.Blue : Theme.TextControl;
		var rect = LocalRect;

		rect = rect.Shrink( 8, 0 );

		Paint.SetPen( color );
		Paint.SetDefaultFont();

		if ( SerializedProperty.IsMultipleDifferentValues )
		{
			Paint.SetPen( Theme.MultipleValues );
			Paint.DrawText( rect, "Multiple Values", TextFlag.LeftCenter );
		}
		else
		{
			Paint.DrawText( rect, entryLabel ?? "None", TextFlag.LeftCenter );
		}

		Paint.SetPen( color );
		Paint.DrawIcon( rect, "Arrow_Drop_Down", 17, TextFlag.RightCenter );
	}
}

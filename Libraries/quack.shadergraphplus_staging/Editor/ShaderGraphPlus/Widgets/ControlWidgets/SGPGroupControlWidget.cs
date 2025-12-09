using Editor;
using static ShaderGraphPlus.ShaderGraphPlusGlobals;

namespace ShaderGraphPlus;

[CustomEditor( typeof( string ), NamedEditor = ControlWidgetCustomEditors.UIGroupEditor )]
internal class SGPGroupControlWidget : ControlWidget
{
	public override bool SupportsMultiEdit => false;

	ComboBox _comboBox;

	public SGPGroupControlWidget( SerializedProperty property ) : base( property )
	{
		Layout = Layout.Row();

		_comboBox = Layout.Add( new ComboBox( this ) );

		var currentVal = SerializedProperty.GetValue<string>();
		List<string> namesSoFar = [currentVal];

		_comboBox.AddItem( "" );
		if ( !string.IsNullOrEmpty( currentVal ) )
		{
			_comboBox.AddItem( currentVal );
			_comboBox.CurrentIndex = 1;
		}

		var groupProperty = GetGroupProperty( property );
		var parentNode = GetShaderNode( property );

		if ( parentNode is not null )
		{
			if ( groupProperty is not null )
			{
				foreach ( var node in parentNode.Graph.Nodes )
				{
					var serialized = node.GetSerialized();
					LoopOverProperties( serialized, groupProperty, ref namesSoFar );
				}
			}
		}
		else // Blackboard
		{
			var graph = GetBlackboardParameter( property ).Graph;

			if ( graph is not null )
			{
				foreach ( var parameter in graph.Parameters )
				{
					var serialized = parameter.GetSerialized();
					LoopOverProperties( serialized, groupProperty, ref namesSoFar );
				}
			}
		}

		_comboBox.Editable = true;
		_comboBox.Insertion = ComboBox.InsertMode.Skip;

		_comboBox.TextChanged += () =>
		{
			SerializedProperty.SetValue<string>( _comboBox.CurrentText );
		};
	}

	void LoopOverProperties( SerializedObject serialized, SerializedProperty groupProperty, ref List<string> namesSoFar )
	{
		foreach ( var prop in serialized )
		{
			if ( prop.PropertyType == typeof( ParameterUI ) || prop.PropertyType == typeof( TextureInput ) )
			{
				if ( prop.TryGetAsObject( out var propObj ) )
				{
					// Get same property name so groups only show group names, sub-groups only show sub-group names, ect
					var innerProp = propObj.GetProperty( groupProperty?.Name );
					var groupVal = innerProp?.GetValue<UIGroup>();
					if ( !string.IsNullOrEmpty( groupVal?.Name ) && !namesSoFar.Contains( groupVal?.Name ) )
					{
						_comboBox.AddItem( groupVal?.Name );
						namesSoFar.Add( groupVal?.Name );
					}
				}
			}
		}
	}

	SerializedProperty GetGroupProperty( SerializedProperty originalProperty )
	{
		if ( originalProperty is null )
		{
			return null;
		}
		if ( originalProperty.PropertyType == typeof( UIGroup ) )
		{
			return originalProperty;
		}
		return GetGroupProperty( originalProperty.Parent?.ParentProperty );
	}

	ShaderNodePlus GetShaderNode( SerializedProperty originalProperty )
	{
		if ( originalProperty is null )
		{
			return null;
		}
		if ( originalProperty.Parent.Targets.First() is ShaderNodePlus shaderNode )
		{
			return shaderNode;
		}
		return GetShaderNode( originalProperty.Parent?.ParentProperty );
	}

	BaseBlackboardParameter GetBlackboardParameter( SerializedProperty originalProperty )
	{
		if ( originalProperty is null )
		{
			return null;
		}
		if ( originalProperty.Parent.Targets.First() is BaseBlackboardParameter bp )
		{
			return bp;
		}

		return GetBlackboardParameter( originalProperty.Parent?.ParentProperty.Parent?.ParentProperty );
	}
}

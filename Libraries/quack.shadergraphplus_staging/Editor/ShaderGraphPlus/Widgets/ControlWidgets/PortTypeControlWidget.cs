using Editor;
using ShaderGraphPlus.Nodes;
using static ShaderGraphPlus.ShaderGraphPlusGlobals;

namespace ShaderGraphPlus;

/// <summary>
/// 
/// </summary>
[CustomEditor( typeof( string ), NamedEditor = ControlWidgetCustomEditors.PortTypeChoiceEditor )]
sealed class PortTypeControlWidget : DropdownControlWidget<string>
{
	public PortTypeControlWidget( SerializedProperty property ) : base( property )
	{
	}

	protected override IEnumerable<object> GetDropdownValues()
	{
		List<object> list = new();
		foreach ( var type in GraphCompiler.ValueTypes )
		{
			if ( type.Key == typeof( float ) ) list.Add( "float" );
			else if ( type.Key == typeof( int ) ) list.Add( "int" );
			else if ( type.Key == typeof( bool ) ) list.Add( "bool" );
			else if ( type.Key == typeof( Texture2DObject ) ) list.Add( "Texture2D" );
			else if ( type.Key == typeof( TextureCubeObject ) ) list.Add( "TextureCube" );
			else if ( type.Key == typeof( Sampler ) ) list.Add( "SamplerState" );
			else if ( type.Key == typeof( Float2x2 ) ) list.Add( "float2x2" );
			else if ( type.Key == typeof( Float3x3 ) ) list.Add( "float3x3" );
			else if ( type.Key == typeof( Float4x4 ) ) list.Add( "float4x4" );
			else list.Add( type.Key );
		}
		return list;
	}
}


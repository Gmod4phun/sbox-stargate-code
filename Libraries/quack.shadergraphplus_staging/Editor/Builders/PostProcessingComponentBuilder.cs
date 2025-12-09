using System.Text;

namespace ShaderGraphPlus;

internal sealed class PostProcessingComponentBuilder
{
	private string m_ppComponentTemplate = PostProcessingClassTemplate.Class;

	private string m_ParamName;
	private string m_ParamStringValue;

	private static string m_RegExPattern = @"Default[234]?\(\s*([^)]*)\s*\)";

	private StringBuilder m_StringBuilderProps;
	private StringBuilder m_StringBuilderAttributes;
	private PostProcessingComponentInfo m_PostProcessingComponentInfo;

	public PostProcessingComponentBuilder( string parmName, string parmStringVal )
	{
		m_ParamName = parmName;
		m_ParamStringValue = parmStringVal;
	}

	public PostProcessingComponentBuilder( PostProcessingComponentInfo info )
	{
		m_StringBuilderProps = new StringBuilder();
		m_StringBuilderAttributes = new StringBuilder();
		m_PostProcessingComponentInfo = info;
	}

	public StringBuilder AddBoolProperty( string paramName, string paramStringValue )
	{
		Match match = Regex.Match( paramStringValue, m_RegExPattern );

		AppendProperty( paramName );
		var property = $"public bool {paramName.Remove( 0, 2 )} {{ get; set; }} = {(match.Groups[1].Value == "0" ? "false" : "true")};";

		m_StringBuilderAttributes.AppendLine( $"attributes.Set( \"{paramName.Remove( 0, 3 )}\", {paramName.Remove( 0, 2 )} );" );

		return m_StringBuilderProps.AppendLine( property );
	}

	public StringBuilder AddFloatProperty( string type, string paramName, string paramStringValue )
	{
		Match match = Regex.Match( paramStringValue, m_RegExPattern );

		AppendProperty( paramName );
		var property = $"public {type} {paramName.Remove( 0, 2 )} {{ get; set; }} = {match.Groups[1].Value}f;";

		m_StringBuilderAttributes.AppendLine( $"attributes.Set( \"{paramName.Remove( 0, 3 )}\", {paramName.Remove( 0, 2 )} );" );

		return m_StringBuilderProps.AppendLine( property );
	}

	public StringBuilder AddVector2Property( string type, string paramName, string paramStringValue )
	{
		Match match = Regex.Match( paramStringValue, m_RegExPattern );

		Vector2 vec2 = (Vector2)Utilities.Parse.ParseVector( match.Groups[1].Value );
		var result = $"Vector2({vec2.x}f,{vec2.y}f)";

		AppendProperty( paramName );
		var property = $"public {type} {paramName.Remove( 0, 2 )} {{ get; set; }} = new {result};";

		m_StringBuilderAttributes.AppendLine( $"attributes.Set( \"{paramName.Remove( 0, 3 )}\", {paramName.Remove( 0, 2 )} );" );

		return m_StringBuilderProps.AppendLine( property );
	}

	public StringBuilder AddVector3Property( string type, string paramName, string paramStringValue )
	{
		Match match = Regex.Match( paramStringValue, m_RegExPattern );

		Vector3 vec3 = (Vector3)Utilities.Parse.ParseVector( match.Groups[1].Value );
		var result = $"Vector3({vec3.x}f,{vec3.y}f,{vec3.z}f)";

		AppendProperty( paramName );
		var property = $"public {type} {paramName.Remove( 0, 2 )} {{ get; set; }} = new {result};";

		m_StringBuilderAttributes.AppendLine( $"attributes.Set( \"{paramName.Remove( 0, 3 )}\", {paramName.Remove( 0, 2 )} );" );

		return m_StringBuilderProps.AppendLine( property );
	}

	public StringBuilder AddVector4Property( string type, string paramName, string paramStringValue )
	{
		Match match = Regex.Match( paramStringValue, m_RegExPattern );

		Vector4 vec4 = (Vector4)Utilities.Parse.ParseVector( match.Groups[1].Value );
		var result = $"Vector4({vec4.x}f,{vec4.y}f,{vec4.z}f,{vec4.w}f)";

		AppendProperty( paramName );
		var property = $"public Vector4 {paramName.Remove( 0, 2 )} {{ get; set; }} = new {result};";

		m_StringBuilderAttributes.AppendLine( $"attributes.Set( \"{paramName.Remove( 0, 3 )}\", {paramName.Remove( 0, 2 )} );" );

		return m_StringBuilderProps.AppendLine( property );
	}

	/// <summary>
	/// When your done building the class you call this to get the full text of said class.
	/// </summary>
	public string Finish( string className, string shaderPath )
	{
		return string.Format( m_ppComponentTemplate,
			m_PostProcessingComponentInfo.ComponentTitle,  //  Title
			m_PostProcessingComponentInfo.ComponentCategory, // Catagory
			string.IsNullOrWhiteSpace( m_PostProcessingComponentInfo.Icon ) ? nameof( MaterialDesign.MaterialIcons.Camera ) : m_PostProcessingComponentInfo.Icon, // Icon
			className, // Class Name
			IndentString( PropsToString(), 1 ), // Class Properties
			m_PostProcessingComponentInfo.Order,
			IndentString( AttribsToString(), 2 ), // Shader Attributes
			shaderPath // Path of Shader
		);
	}

	private void AppendProperty( string paramName )
	{
		m_StringBuilderProps.AppendLine();
		m_StringBuilderProps.AppendLine( "[Property]" );
		m_StringBuilderProps.AppendLine( $"[Title(\"{paramName.Remove( 0, 3 )}\")]" );
	}

	private string PropsToString()
	{
		return m_StringBuilderProps.ToString();
	}

	private string AttribsToString()
	{
		return m_StringBuilderAttributes.ToString();
	}

	private static string IndentString( string input, int tabCount )
	{
		if ( string.IsNullOrWhiteSpace( input ) )
			return input;

		var tabs = new string( '\t', tabCount );
		var lines = input.Split( '\n' );

		for ( int i = 0; i < lines.Length; i++ )
		{
			lines[i] = tabs + lines[i];
		}

		return string.Join( "\n", lines );
	}
}

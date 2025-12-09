namespace ShaderGraphPlus;

[System.AttributeUsage( AttributeTargets.Property )]
internal sealed class ShaderFeatureReferenceAttribute : Attribute
{
}

public class ShaderFeatureBase : IValid
{
	/// <summary>
	/// Name of this feature.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// What this feature does.
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	/// Header Name of this Feature that shows up in the Material Editor.
	/// </summary>
	public string HeaderName { get; set; }

	[Hide, JsonIgnore, Browsable( false )]
	public virtual bool IsValid => throw new NotImplementedException();

	public ShaderFeatureBase()
	{
		Name = "";
		Description = "";
		HeaderName = "";
	}

	public string GetFeatureString()
	{
		return $"F_{Name.ToUpper().Replace( " ", "_" )}";
	}

	public string GetDynamicComboString()
	{
		return $"D_{Name.ToUpper().Replace( " ", "_" )}";
	}

	public string GetStaticComboString()
	{
		return $"S_{Name.ToUpper().Replace( " ", "_" )}";
	}

	public virtual string GetOptionRangeString()
	{
		return $"";
	}
}

public class ShaderFeatureBoolean : ShaderFeatureBase
{
	[Hide, JsonIgnore, Browsable( false )]
	public override bool IsValid => !string.IsNullOrWhiteSpace( Name );

	public ShaderFeatureBoolean() : base()
	{

	}

	public override int GetHashCode()
	{
		return System.HashCode.Combine( Name, Description, HeaderName );
	}

	public override string GetOptionRangeString()
	{
		return "0..1";
	}
}

public class ShaderFeatureEnum : ShaderFeatureBase
{
	[Hide, JsonIgnore, Browsable( false )]
	public override bool IsValid => !string.IsNullOrWhiteSpace( Name ) && Options.All( x => !string.IsNullOrWhiteSpace( x ) );

	/// <summary>
	/// Options of your feature. Must have no special characters. Note : all lowercase letters will be converted to uppercase.
	/// </summary>
	public List<string> Options { get; set; }

	public ShaderFeatureEnum() : base()
	{
		Options = new List<string>();
	}

	public override int GetHashCode()
	{
		var hashcode = System.HashCode.Combine( Name, Description, HeaderName );

		foreach ( var option in Options )
		{
			hashcode += option.GetHashCode();
		}

		return hashcode;
	}

	public override string GetOptionRangeString()
	{
		return $"0..{Options.Count - 1}";
	}
}

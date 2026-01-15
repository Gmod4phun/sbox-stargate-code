namespace ShaderGraphPlus;

public static class HLSLIncludeTemplate
{
	public static string Contents => @"
#ifndef {0}_H
#define {0}_H	

	{1}
	{{
{2}
	}}

#endif // {0}_H
";
}

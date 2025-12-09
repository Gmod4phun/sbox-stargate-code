
namespace ShaderGraphPlus;

internal interface IBlackboardParameter
{
	Guid Identifier { get; }
	string Name { get; set; }

	//Color GetTypeColor( BlackboardView view );
}

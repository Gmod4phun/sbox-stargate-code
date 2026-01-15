
namespace ShaderGraphPlus.Nodes;

public abstract class MatrixConstantNode<T> : ShaderNodePlus, IConstantNode, IConstantMatrixNode
{
	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.ConstantValueNode;

	public string Name { get; set; } = "";

	[Hide]
	public override string Title => string.IsNullOrWhiteSpace( Name ) ?
		$"{DisplayInfo.For( this ).Name}" :
		$"{DisplayInfo.For( this ).Name} ( {Name} )";

	[InlineEditor]
	public T Value { get; set; }

	/// <summary>
	/// Enable this if you want to be able to set this via an Attribute via code. 
	/// False means it wont be generated as a global in the generated shader and thus will be local to the code.
	/// </summary>
	[Hide]
	public bool IsAttribute { get; set; } = false;

	public object GetValue()
	{
		return Value;
	}

	public BaseNodePlus InitializeMaterialParameterNode()
	{
		throw new NotImplementedException();
	}

	public virtual BaseBlackboardParameter InitializeMaterialParameter( string name )
	{
		throw new NotImplementedException();
	}

	public virtual BaseBlackboardParameter InitializeSubgraphInputParameter( string name )
	{
		throw new NotImplementedException();
	}
}

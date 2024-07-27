using Sandbox;

namespace Editor.ShaderGraph.Nodes;

[Title("Sphere Mask"), Category("Custom")]
public sealed class SphereMask : ShaderNode
{
	[Input(typeof(Vector4))]
	[Hide]
	public NodeInput Coords { get; set; }

	[Input(typeof(Vector4))]
	[Hide]
	public NodeInput Center { get; set; }

	[Input(typeof(float))]
	[Hide]
	public NodeInput Radius { get; set; }

	[Input(typeof(float))]
	[Hide]
	public NodeInput Hardness { get; set; }

	[Output(typeof(Vector3))]
	[Hide]
	public NodeResult.Func Out =>
		(GraphCompiler compiler) =>
		{
			var coords = compiler.ResultOrDefault(Coords, Vector4.Zero);
			var center = compiler.ResultOrDefault(Center, Vector4.Zero);
			var radius = compiler.ResultOrDefault(Radius, 1.0f);
			var hardness = compiler.ResultOrDefault(Hardness, 0.0f);

			return new NodeResult(
				4,
				$"1 - saturate((distance({coords}, {center}) - {radius}) / (1 - {hardness}))"
			);
		};
}

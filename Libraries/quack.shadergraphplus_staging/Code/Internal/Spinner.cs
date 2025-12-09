
using Sandbox;

namespace ShaderGraphPlus.Internal;

internal sealed class ObjectSpinner : Component, Component.ExecuteInEditor
{
	public enum RotationAxis
	{
		[Title( "X" )]
		PositveX,
		[Title( "Y" )]
		PositveY,
		[Title( "Z" )]
		PositveZ,
		[Title( "-X" )]
		NegativeX,
		[Title( "-Y" )]
		NegativeY,
		[Title( "-Z" )]
		NegativeZ
	}

	[Property]
	[Range( 0.0f, 1024.0f )]
	public float SpinningSpeed { get; set; } = 1.0f;

	[Property]
	public RotationAxis Axis { get; set; } = RotationAxis.PositveX;

	private Vector3 RotationAxisVector
	{
		get
		{
			switch ( Axis )
			{
				case RotationAxis.PositveX:
					return Vector3.Forward;
				case RotationAxis.PositveY:
					return Vector3.Left;
				case RotationAxis.PositveZ:
					return Vector3.Up;
				case RotationAxis.NegativeX:
					return Vector3.Backward;
				case RotationAxis.NegativeY:
					return Vector3.Right;
				case RotationAxis.NegativeZ:
					return Vector3.Down;
				default:
					return Vector3.Forward;
			}
		}
	}

	protected override void OnUpdate()
	{
		if ( SpinningSpeed > 0.0f )
		{
			LocalRotation.RotateAroundAxis( RotationAxisVector, Time.Delta * SpinningSpeed );
		}
	}
}

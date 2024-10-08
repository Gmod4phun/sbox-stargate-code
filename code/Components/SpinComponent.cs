public class SpinComponent : Component
{
	[Property]
	public Angles SpinAngles { get; set; }

	protected override void OnUpdate()
	{
		if (IsProxy)
			return;

		LocalRotation *= (SpinAngles * Time.Delta).ToRotation();
	}
}

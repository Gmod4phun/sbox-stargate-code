enum RingState
{
	STOPPED,
	STARTING,
	FULLSPEED,
	STOPPING
}

public class TestRotatingRing : Component
{
	[Property]
	public ModelRenderer Renderer { get; set; }

	[Property]
	RingState _ringState = RingState.STOPPED;

	double RingAngle = 0;

	[Property]
	float SpeedPerSecond = 2f;

	[Property]
	float CurSpeedMul = 0;

	[Property]
	float SpinUpTime = 0.8f;

	[Property]
	float SpinDownTime = 1.2f;

	[Property]
	float TargetAngle = 0;

	int _ringDirection = 1;

	[Button("Toggle Rotation")]
	public void ToggleRotation()
	{
		if (_ringState == RingState.STOPPED)
		{
			_ringState = RingState.STARTING;
		}
		else
		{
			_ringState = RingState.STOPPING;
		}
	}

	[Button("Rotate to target angle")]
	public async void RotateToTargetAngle()
	{
		var success = await RotateToAngle(TargetAngle);

		Log.Info($"Rotation success: {success}");
	}

	public float GetDesiredRingAngle(float desiredAng, float angOffset = 0)
	{
		// if its a valid symbol, lets calc the required angle
		//var symAng = symPos * 9; // there are 40 symbols, each 9 degrees apart
		var symAng = desiredAng;

		// clockwise and counterclockwise symbol angles relative to 0 (the top chevron)
		var D_CW = (float)(-symAng + RingAngle + angOffset); // offset, if we want it to be relative to another chevron (for movie stargate dialing)
		var D_CCW = 360 - D_CW;

		D_CW = D_CW.UnsignedMod(360);
		D_CCW = D_CCW.UnsignedMod(360);

		// angle differences are setup, choose based on the direction of ring rotation
		// if the required angle is to too small, spin it around once
		var angToRotate = (_ringDirection == 1) ? D_CCW : D_CW;
		if (angToRotate < 170)
			angToRotate += 360f;

		// set the final angle to the current angle + the angle needed to rotate, also considering ring direction
		var finalAng = RingAngle + (angToRotate * _ringDirection);

		//Log.Info($"Sym = {sym}, RingAng = {RingAngle}, SymPos = {symPos}, D_CCW = {D_CCW}, D_CW = {D_CW}, finalAng = {finalAng}" );

		return (float)finalAng;
	}

	public async Task<bool> RotateToAngle(float angle)
	{
		if (_ringState != RingState.STOPPED)
		{
			return false;
		}

		var desiredAngle = GetDesiredRingAngle(angle, 0);
		var angleToRotate = Math.Abs(desiredAngle - RingAngle);

		var totalAngleSpentSpeeding = SpinUpTime * SpeedPerSecond / 2;
		var totalAngleSpentSlowing = SpinDownTime * SpeedPerSecond / 2;
		var totalAngleSpentFullSpeed =
			angleToRotate - totalAngleSpentSpeeding - totalAngleSpentSlowing;
		var angleWhenToStartSlowing =
			angleToRotate - totalAngleSpentSpeeding - totalAngleSpentFullSpeed;

		_ringState = RingState.STARTING;

		while (_ringState != RingState.STOPPED)
		{
			if (_ringState == RingState.FULLSPEED)
			{
				if (Math.Abs(desiredAngle - RingAngle) <= angleWhenToStartSlowing)
				{
					_ringState = RingState.STOPPING;
				}
			}
			await Task.FrameEnd();
		}

		return Math.Abs(desiredAngle.UnsignedMod(360) - RingAngle) <= Time.Delta * SpeedPerSecond;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (!Renderer.IsValid())
			return;

		if (_ringState != RingState.STOPPED)
		{
			if (_ringState == RingState.STARTING)
			{
				CurSpeedMul += Time.Delta / SpinUpTime;
				if (CurSpeedMul >= 1)
				{
					_ringState = RingState.FULLSPEED;
					CurSpeedMul = 1;
				}
			}
			else if (_ringState == RingState.STOPPING)
			{
				CurSpeedMul -= Time.Delta / SpinDownTime;
				if (CurSpeedMul <= 0)
				{
					_ringState = RingState.STOPPED;
					CurSpeedMul = 0;
					_ringDirection = -_ringDirection;
					RingAngle = ((float)RingAngle).UnsignedMod(360);
				}
			}

			RingAngle += SpeedPerSecond * Time.Delta * CurSpeedMul * _ringDirection;
		}

		Renderer.WorldRotation = Rotation.FromAxis(Vector3.Forward, (float)RingAngle);
	}
}

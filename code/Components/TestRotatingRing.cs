enum RingRotationState
{
	STOPPED,
	SPEEDING,
	FULLSPEED,
	SLOWING
}

public class TestRotatingRing : Component
{
	[Property]
	public ModelRenderer Renderer { get; set; }

	[Property]
	RingRotationState State = RingRotationState.STOPPED;

	double currentAngle = 0;

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

	int currentRingDirection = 1;

	[Button("Toggle Rotation")]
	public void ToggleRotation()
	{
		if (State == RingRotationState.STOPPED)
		{
			State = RingRotationState.SPEEDING;
		}
		else
		{
			State = RingRotationState.SLOWING;
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
		var D_CW = (float)(-symAng + currentAngle + angOffset); // offset, if we want it to be relative to another chevron (for movie stargate dialing)
		var D_CCW = 360 - D_CW;

		D_CW = D_CW.UnsignedMod(360);
		D_CCW = D_CCW.UnsignedMod(360);

		// angle differences are setup, choose based on the direction of ring rotation
		// if the required angle is to too small, spin it around once
		var angToRotate = (currentRingDirection == 1) ? D_CCW : D_CW;
		if (angToRotate < 170)
			angToRotate += 360f;

		// set the final angle to the current angle + the angle needed to rotate, also considering ring direction
		var finalAng = currentAngle + (angToRotate * currentRingDirection);

		//Log.Info($"Sym = {sym}, RingAng = {RingAngle}, SymPos = {symPos}, D_CCW = {D_CCW}, D_CW = {D_CW}, finalAng = {finalAng}" );

		return (float)finalAng;
	}

	public async Task<bool> RotateToAngle(float angle)
	{
		if (State != RingRotationState.STOPPED)
		{
			return false;
		}

		TargetAngle = angle;

		var desiredAngle = GetDesiredRingAngle(TargetAngle, 0);
		var angleToRotate = Math.Abs(desiredAngle - currentAngle);

		var totalAngleSpentSpeeding = SpinUpTime * SpeedPerSecond / 2;
		var totalAngleSpentSlowing = SpinDownTime * SpeedPerSecond / 2;
		var totalAngleSpentFullSpeed =
			angleToRotate - totalAngleSpentSpeeding - totalAngleSpentSlowing;
		var angleWhenToStartSlowing =
			angleToRotate - totalAngleSpentSpeeding - totalAngleSpentFullSpeed;

		State = RingRotationState.SPEEDING;

		while (State != RingRotationState.STOPPED)
		{
			if (State == RingRotationState.FULLSPEED)
			{
				if (Math.Abs(desiredAngle - currentAngle) <= angleWhenToStartSlowing)
				{
					State = RingRotationState.SLOWING;
				}
			}
			await Task.FrameEnd();
		}

		return Math.Abs(desiredAngle.UnsignedMod(360) - currentAngle)
			<= Time.Delta * SpeedPerSecond;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (!Renderer.IsValid())
			return;

		if (State != RingRotationState.STOPPED)
		{
			if (State == RingRotationState.SPEEDING)
			{
				CurSpeedMul += Time.Delta / SpinUpTime;
				if (CurSpeedMul >= 1)
				{
					State = RingRotationState.FULLSPEED;
					CurSpeedMul = 1;
				}
			}
			else if (State == RingRotationState.SLOWING)
			{
				CurSpeedMul -= Time.Delta / SpinDownTime;
				if (CurSpeedMul <= 0)
				{
					State = RingRotationState.STOPPED;
					CurSpeedMul = 0;
					currentRingDirection = -currentRingDirection;
					currentAngle = ((float)currentAngle).UnsignedMod(360);
				}
			}

			currentAngle += SpeedPerSecond * Time.Delta * CurSpeedMul * currentRingDirection;
		}

		Renderer.Transform.Rotation = Rotation.FromAxis(Vector3.Forward, (float)currentAngle);
	}
}

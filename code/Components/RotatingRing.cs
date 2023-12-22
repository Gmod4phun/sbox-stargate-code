using System;

public class RotatingRing : PropertyChangeComponent, Component.ExecuteInEditor
{
    [Property, OnChange(nameof(OnRingRotationEnabledChanged))]
    public bool RingRotationEnabled { get; set; }

    // other code

    public float RotationSpeed = 20f; // Initial rotation speed
    public float Acceleration = 2f;   // Acceleration rate
    public float Deceleration = 2f;   // Deceleration rate
    public int TotalSegments = 39;    // Total number of segments in the ring

	[Property]
	public int TargetSymbol {get; set;} = 0;

	[Property]
	private float _currentRingAngle {get; set;} = 0;
	private float _targetRingAngle {get; set;} = 0;

	protected override void OnUpdate()
    {
        base.OnUpdate();

		Transform.LocalRotation = Transform.LocalRotation.Angles().WithRoll(_currentRingAngle).ToRotation();
    }

	public async Task<bool> RotateToSymbol(int symbol) {
		var targetAngle = 360f/39 * symbol;
		while (true) {
			if (!RingRotationEnabled)
				return false;

			if (Math.Abs(_currentRingAngle - targetAngle) < 0.5f) {
				_currentRingAngle = targetAngle;
				return true;
			}

			_currentRingAngle += 0.005f * 50f;

			if (_currentRingAngle > 360 || _currentRingAngle < -360)
				_currentRingAngle = _currentRingAngle.UnsignedMod(360);

			await Task.DelayRealtime(10);
		}
	}

    private async void OnRingRotationEnabledChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
			var result = await RotateToSymbol(TargetSymbol);
			Log.Info(result);
        }
    }
}

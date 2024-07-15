namespace Sandbox.Components.Stargate
{
	public class StargateRing : PropertyChangeComponent, Component.ExecuteInEditor
	{
		public enum RingState
		{
			STOPPED,
			STARTING,
			FULLSPEED,
			STOPPING
		}

		public Stargate Gate =>
			GameObject.Parent.Components.Get<Stargate>(FindMode.EnabledInSelfAndDescendants);

		public bool IsMoving => _ringState != RingState.STOPPED;

		[Property, Sync]
		public float RingAngle { get; private set; } = 0;

		[Sync]
		private int _ringDirection { get; set; } = -1;

		[Property]
		public string RingSymbols { get; protected set; } =
			"#0JKNTR3MBZX*H69IGPL@QFS1E4AU85OCW72YVD";

		[Sync]
		private string _currentRotatingToSymbol { get; set; } = "";

		[Property, System.ComponentModel.ReadOnly(true), Sync]
		public string CurRingSymbol { get; private set; } = "";

		[Sync]
		private float _curRingSymbolOffset { get; set; } = 0f;

		[Property, OnChange(nameof(OnRingStateChanged)), System.ComponentModel.ReadOnly(true), Sync]
		public RingState _ringState { get; set; } = RingState.STOPPED;

		public float SpeedPerSecond => Gate.IsValid() ? Gate.RingSpeedPerSecond : 40;

		public float SpinUpTime { get; set; } = 1.25f;

		public float SpinDownTime { get; set; } = 1.25f;

		[Sync]
		private float _curSpeedMul { get; set; } = 0;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			RingAngleThink();
			RingSymbolThink();
			// DrawSymbols();

			Transform.LocalRotation = Transform
				.LocalRotation.Angles()
				.WithRoll(RingAngle)
				.ToRotation();
		}

		// symbol pos/ang
		public virtual float GetSymbolPosition(char sym) // gets the symbols position on the ring
		{
			sym = sym.ToString().ToUpper()[0];
			return RingSymbols.Contains(sym) ? RingSymbols.IndexOf(sym) : -1;
		}

		public virtual float GetSymbolAngle(char sym) // gets the symbols angle on the ring
		{
			sym = sym.ToString().ToUpper()[0];
			return GetSymbolPosition(sym) * (360f / RingSymbols.Length);
		}

		public void RingSymbolThink() // keeps track of the current symbol under the top chevron
		{
			var symRange = 360f / RingSymbols.Length;
			var symCoverage = (RingAngle + symRange / 2f).UnsignedMod(symRange);
			var symIndex = (
				(int)Math.Round((RingAngle + _curRingSymbolOffset) / (symRange))
			).UnsignedMod(RingSymbols.Length);

			var threshold = 0.05f;
			var symCoverageThresholdMin = symRange * threshold;
			var symCoverageThresholdMax = symRange * (1 - threshold);

			CurRingSymbol =
				(symCoverage < symCoverageThresholdMax && symCoverage > symCoverageThresholdMin)
					? RingSymbols[symIndex].ToString()
					: "";
		}

		public virtual float GetDesiredRingAngleForSymbol(char sym, float angOffset = 0)
		{
			// if ( sym is '#' && (Gate.IsValid() && Gate.EarthPointOfOrigin) ) sym = '?';
			// eliminate custom points of origin for now, use just Giza, we will only have one (probably will have Abydos/Beta switchable on the gate model tho)

			// get the symbol's position on the ring
			var symPos = GetSymbolPosition(sym);

			// if we input an invalid symbol, return current ring angles
			if (symPos == -1)
				return RingAngle;

			// if its a valid symbol, lets calc the required angle
			//var symAng = symPos * 9; // there are 40 symbols, each 9 degrees apart
			var symAng = GetSymbolAngle(sym);

			// clockwise and counterclockwise symbol angles relative to 0 (the top chevron)
			var D_CW = -symAng + RingAngle + angOffset; // offset, if we want it to be relative to another chevron (for movie stargate dialing)
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

			return finalAng;
		}

		private void RingAngleThink()
		{
			if (_ringState != RingState.STOPPED)
			{
				if (_ringState == RingState.STARTING)
				{
					_curSpeedMul += Time.Delta / SpinUpTime;
					if (_curSpeedMul >= 1)
					{
						_ringState = RingState.FULLSPEED;
						_curSpeedMul = 1;
					}
				}
				else if (_ringState == RingState.STOPPING)
				{
					_curSpeedMul -= Time.Delta / SpinDownTime;
					if (_curSpeedMul <= 0)
					{
						_ringState = RingState.STOPPED;
						_curSpeedMul = 0;
						_ringDirection = -_ringDirection;
						RingAngle = ((float)RingAngle).UnsignedMod(360);
					}
				}

				RingAngle += SpeedPerSecond * Time.Delta * _curSpeedMul * _ringDirection;
			}
		}

		public async Task<bool> RotateToSymbol(char sym, float angleOffset = 0)
		{
			if (_ringState != RingState.STOPPED)
			{
				return false;
			}

			_curRingSymbolOffset = angleOffset;

			var desiredAngle = GetDesiredRingAngleForSymbol(sym, _curRingSymbolOffset);
			var angleToRotate = Math.Abs(desiredAngle - RingAngle);

			// this assumes that the ring will rotate at least as much to achieve full speed (which it should), otherwise this might get fucked
			var totalAngleSpentSpeeding = SpinUpTime * SpeedPerSecond / 2;
			var totalAngleSpentSlowing = SpinDownTime * SpeedPerSecond / 2;
			var totalAngleSpentFullSpeed =
				angleToRotate - totalAngleSpentSpeeding - totalAngleSpentSlowing;
			var angleWhenToStartSlowing =
				angleToRotate - totalAngleSpentSpeeding - totalAngleSpentFullSpeed;

			_ringState = RingState.STARTING;

			var wasStoppedOnPurpose = false;
			while (_ringState != RingState.STOPPED)
			{
				if (_ringState == RingState.FULLSPEED)
				{
					if (Math.Abs(desiredAngle - RingAngle) <= angleWhenToStartSlowing)
					{
						_ringState = RingState.STOPPING;
						wasStoppedOnPurpose = true;
					}
				}
				await Task.FrameEnd();
			}

			return wasStoppedOnPurpose;
		}

		private void OnRingStateChanged(RingState oldValue, RingState newValue)
		{
			if (newValue == RingState.STARTING)
				OnStarting();
			else if (newValue == RingState.FULLSPEED)
				OnFullspeed();
			else if (newValue == RingState.STOPPING)
				OnStopping();
			else if (newValue == RingState.STOPPED)
				OnStopped();
		}

		public virtual void OnStarting() { }

		public virtual void OnFullspeed() { }

		public virtual void OnStopping() { }

		public virtual void OnStopped() { }

		public void SpinUp()
		{
			if (_ringState == RingState.STOPPED)
				_ringState = RingState.STARTING;
		}

		public void SpinDown()
		{
			if (_ringState != RingState.STOPPED)
				_ringState = RingState.STOPPING;
		}

		// DEBUG
		public void DrawSymbols()
		{
			var deg = 360f / RingSymbols.Length;
			var ang = Transform.Local.Rotation.Angles();
			foreach (char sym in RingSymbols)
			{
				var rotAng = ang.WithRoll(-ang.roll + (RingSymbols.IndexOf(sym) * deg));
				var newRot = rotAng.ToRotation();
				var pos = Transform.Position + newRot.Forward * 4 + newRot.Up * 117.5f;

				Gizmo.Draw.Color = _currentRotatingToSymbol.Contains(sym)
					? Color.Yellow
					: Color.White;
				Gizmo.Draw.ScreenText(sym.ToString(), Gizmo.Camera.ToScreen(pos));
			}

			if (CurRingSymbol.Length == 1)
			{
				Gizmo.Draw.Color = Color.Magenta;
				Gizmo.Draw.ScreenText(
					CurRingSymbol,
					Gizmo.Camera.ToScreen(Transform.Position),
					size: 24
				);
			}
		}
	}
}

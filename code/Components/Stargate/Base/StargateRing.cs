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

		public Stargate Gate => GameObject.Parent.Components.Get<Stargate>( FindMode.EnabledInSelfAndDescendants );

		public bool IsMoving => _ringState != RingState.STOPPED;

		[Property]
		public float RingAngle { get; private set; } = 0;
		private int _ringDirection { get; set; } = -1;

		[Property]
		public string RingSymbols { get; protected set; } = "#0JKNTR3MBZX*H69IGPL@QFS1E4AU85OCW72YVD";
		private string _currentRotatingToSymbol { get; set; } = "";

		[Property, System.ComponentModel.ReadOnly( true )]
		public string CurRingSymbol { get; private set; } = "";

		private float _curRingSymbolOffset = 0f;

		[Property, OnChange( nameof( OnRingStateChanged ) ), System.ComponentModel.ReadOnly( true )]
		public RingState _ringState { get; set; } = RingState.STOPPED;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			RingSymbolThink();
			// DrawSymbols();

			Transform.LocalRotation = Transform.LocalRotation.Angles().WithRoll( RingAngle ).ToRotation();
		}

		// symbol pos/ang
		public virtual float GetSymbolPosition( char sym ) // gets the symbols position on the ring
		{
			sym = sym.ToString().ToUpper()[0];
			return RingSymbols.Contains( sym ) ? RingSymbols.IndexOf( sym ) : -1;
		}

		public virtual float GetSymbolAngle( char sym ) // gets the symbols angle on the ring
		{
			sym = sym.ToString().ToUpper()[0];
			return GetSymbolPosition( sym ) * (360f / RingSymbols.Length);
		}

		public void RingSymbolThink() // keeps track of the current symbol under the top chevron
		{
			var symRange = 360f / RingSymbols.Length;
			var symCoverage = (RingAngle + symRange / 2f).UnsignedMod( symRange );
			var symIndex = ((int)Math.Round( (RingAngle + _curRingSymbolOffset) / (symRange) )).UnsignedMod( RingSymbols.Length );

			var threshold = 0.05f;
			var symCoverageThresholdMin = symRange * threshold;
			var symCoverageThresholdMax = symRange * (1 - threshold);

			CurRingSymbol = (symCoverage < symCoverageThresholdMax && symCoverage > symCoverageThresholdMin) ? RingSymbols[symIndex].ToString() : "";
		}

		public virtual float GetDesiredRingAngleForSymbol( char sym, float angOffset = 0 )
		{
			// if ( sym is '#' && (Gate.IsValid() && Gate.EarthPointOfOrigin) ) sym = '?';
			// eliminate custom points of origin for now, use just Giza, we will only have one (probably will have Abydos/Beta switchable on the gate model tho)

			// get the symbol's position on the ring
			var symPos = GetSymbolPosition( sym );

			// if we input an invalid symbol, return current ring angles
			if ( symPos == -1 ) return RingAngle;

			// if its a valid symbol, lets calc the required angle
			//var symAng = symPos * 9; // there are 40 symbols, each 9 degrees apart
			var symAng = GetSymbolAngle( sym );

			// clockwise and counterclockwise symbol angles relative to 0 (the top chevron)
			var D_CW = -symAng + RingAngle + angOffset; // offset, if we want it to be relative to another chevron (for movie stargate dialing)
			var D_CCW = 360 - D_CW;

			D_CW = D_CW.UnsignedMod( 360 );
			D_CCW = D_CCW.UnsignedMod( 360 );

			// angle differences are setup, choose based on the direction of ring rotation
			// if the required angle is to too small, spin it around once
			var angToRotate = (_ringDirection == 1) ? D_CCW : D_CW;
			if ( angToRotate < 170 ) angToRotate += 360f;

			// set the final angle to the current angle + the angle needed to rotate, also considering ring direction
			var finalAng = RingAngle + (angToRotate * _ringDirection);

			//Log.Info($"Sym = {sym}, RingAng = {RingAngle}, SymPos = {symPos}, D_CCW = {D_CCW}, D_CW = {D_CW}, finalAng = {finalAng}" );

			return finalAng;
		}

		public async Task<bool> RotateToSymbol( char sym, float angleOffset = 0 )
		{
			if ( _ringState != RingState.STOPPED )
			{
				return false;
			}

			_curRingSymbolOffset = angleOffset;

			var desiredAngle = GetDesiredRingAngleForSymbol( sym, _curRingSymbolOffset );
			var toTravelTotal = Math.Abs( desiredAngle - RingAngle ); // total angle needed to travel
			var traveledAngle = 0f; // currently traveled angle
			_currentRotatingToSymbol = sym.ToString();
			_ringState = RingState.STARTING;

			var stepSize = Gate.RingRotationStepSize;

			var multiplier = 0f;
			var traveledAngleForMaxMultiplier = 0f; // this will be the angle since travel start and fullspeed
													// that means if we dont use same accel and deccel, its the same angle since fullspeed rotation and stop
			for ( var i = 0; i < 100; i++ )
			{
				multiplier = (multiplier + 0.01f).Clamp( 0, 1 );
				traveledAngleForMaxMultiplier += stepSize * multiplier;
			}

			var toBeginStoppingRing = toTravelTotal - traveledAngleForMaxMultiplier;
			multiplier = 0f; // reset actual multiplier

			// this will probably never be the case, but in case its neede in the future, its implemented
			var canReachMaxMulAndStopWithMaxMul = toTravelTotal > traveledAngleForMaxMultiplier * 2f;
			if ( !canReachMaxMulAndStopWithMaxMul )
			{
				toBeginStoppingRing = toTravelTotal / 2f;
			}

			// rotation loop
			while ( true )
			{
				if ( traveledAngle >= toBeginStoppingRing && _ringState != RingState.STOPPING )
				{
					_ringState = RingState.STOPPING;
				}

				if ( _ringState == RingState.STARTING )
				{
					multiplier = (multiplier + 0.01f).Clamp( 0, 1 );
					if ( multiplier == 1 )
					{
						_ringState = RingState.FULLSPEED;
					}
				}

				if ( _ringState == RingState.STOPPING )
				{
					multiplier = (multiplier - 0.01f).Clamp( 0, 1 );
					if ( multiplier == 0 )
					{
						_ringState = RingState.STOPPED;
						RingAngle = RingAngle.UnsignedMod( 360f );
						_ringDirection = -_ringDirection;
						_currentRotatingToSymbol = "";

						// ring is stopped, now check if it reached what we wanted
						if ( Math.Abs( traveledAngle - toTravelTotal ) <= stepSize )
						{
							// if we want to rotate the gate upright (sgu) or to the point of origin, force it upright
							if ( desiredAngle % 360 == 0 )
							{
								RingAngle = 0;
							}

							return true;
						}
						else
						{
							return false;
						}
					}
				}

				var toTravelStep = stepSize * multiplier;
				traveledAngle += toTravelStep;
				RingAngle += toTravelStep * _ringDirection;
				RingAngle = RingAngle.UnsignedMod( 360f );

				await Task.Delay( 1 );
			}
		}

		private void OnRingStateChanged( RingState oldValue, RingState newValue )
		{
			if ( newValue == RingState.STARTING )
				OnStarting();
			else if ( newValue == RingState.FULLSPEED )
				OnFullspeed();
			else if ( newValue == RingState.STOPPING )
				OnStopping();
			else if ( newValue == RingState.STOPPED )
				OnStopped();
		}

		public virtual void OnStarting() { }
		public virtual void OnFullspeed() { }
		public virtual void OnStopping() { }
		public virtual void OnStopped() { }

		public async Task SpinUp()
		{
			if ( _ringState != RingState.STOPPED )
			{
				return;
			}

			_ringState = RingState.STARTING;

			var stepSize = Gate.RingRotationStepSize;
			var multiplier = 0f;

			// rotation loop
			while ( true )
			{
				if ( _ringState == RingState.STARTING )
				{
					multiplier = (multiplier + 0.01f).Clamp( 0, 1 );
					if ( multiplier == 1 )
					{
						_ringState = RingState.FULLSPEED;
					}
				}

				if ( _ringState == RingState.STOPPING )
				{
					multiplier = (multiplier - 0.01f).Clamp( 0, 1 );
					if ( multiplier == 0 )
					{
						_ringState = RingState.STOPPED;
						RingAngle = RingAngle.UnsignedMod( 360f );
						_ringDirection = -_ringDirection;
						return;
					}
				}

				var toTravelStep = stepSize * multiplier;
				RingAngle += toTravelStep * _ringDirection;
				RingAngle = RingAngle.UnsignedMod( 360f );

				await Task.Delay( 1 );
			}
		}

		public void SpinDown()
		{
			if ( _ringState != RingState.STOPPED )
				_ringState = RingState.STOPPING;
		}

		// DEBUG
		public void DrawSymbols()
		{
			var deg = 360f / RingSymbols.Length;
			var ang = Transform.Local.Rotation.Angles();
			foreach ( char sym in RingSymbols )
			{
				var rotAng = ang.WithRoll( -ang.roll + (RingSymbols.IndexOf( sym ) * deg) );
				var newRot = rotAng.ToRotation();
				var pos = Transform.Position + newRot.Forward * 4 + newRot.Up * 117.5f;

				Gizmo.Draw.Color = _currentRotatingToSymbol.Contains( sym ) ? Color.Yellow : Color.White;
				Gizmo.Draw.ScreenText( sym.ToString(), Gizmo.Camera.ToScreen( pos ) );
			}

			if ( CurRingSymbol.Length == 1 )
			{
				Gizmo.Draw.Color = Color.Magenta;
				Gizmo.Draw.ScreenText( CurRingSymbol, Gizmo.Camera.ToScreen( Transform.Position ), size: 24 );
			}
		}
	}
}

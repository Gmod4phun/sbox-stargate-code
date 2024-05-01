using Sandbox;
using System;
using System.Collections.Generic;

public class VRSettings
{
	public string MovementMode { get; set; }

	public string DirectionMode { get; set; }

	public string RotationMode { get; set; }

	public VRSettings( string movementMode, string directionMode, string rotationMode )
	{
		MovementMode = movementMode;
		DirectionMode = directionMode;
		RotationMode = rotationMode;
	}

	public string ToJson()
	{
		return Json.Serialize( this );
	}
}



public sealed class VRPlayerMovement : Component
{
	public static VRPlayerMovement instance;

	public enum MovementType
	{
		ArmSwinger,
		Joystick,
		Teleport
	}

	public enum RotationType
	{
		Snap,
		Smooth
	}

	public enum DirectionType
	{
		Head,
		Controller
	}

	[Property] GameObject Head { get; set; }
	[Property] GameObject LeftHand { get; set; }
	[Property] GameObject RightHand { get; set; }

	[Property] public MovementType movementMethod { get; set; }

	[Property] public DirectionType movementDirection { get; set; }

	[Property] public float JoystickMovementSpeed { get; set; } = 3f;

	[Property] public RotationType rotationMethod { get; set; }

	[Property] public float SnapRotateIncrements { get; set; } = 30f;
	[Property] public float SmoothRotateSpeed { get; set; } = 60f;

	[Property] public float JumpForce { get; set; } = 400f;

	[Property] public float JumpUpMult { get; set; } = 2f;

	[Property] public float JumpForwardMult { get; set; } = 1f;

	public float YawRotation = 0f;

	[Property] GameObject OptionsMenu { get; set; }

	protected override void OnStart()
	{
		instance = this;

		YawRotation = Transform.Rotation.Yaw();

		Scene.NavMesh.IsEnabled = true;

		Scene.NavMesh.Generate( Scene.PhysicsWorld );
		//teleportNodes = mesh.Nodes.Values.ToList();
		Components.Get<VRLegs>().OnRespawn += RegenerateNavmesh;
	}

	public void RegenerateNavmesh()
	{
		Scene.NavMesh.IsEnabled = true;

		Scene.NavMesh.Generate( Scene.PhysicsWorld );
		//teleportNodes = mesh.Nodes.Values.ToList();
	}

	public string[] OptionSelections = new string[3] { "Movement", "Direction", "Rotation" };
	public int SelectedOption = 0;
	bool JustChanged;

	bool JustOpened;

	protected override void OnFixedUpdate()
	{
		if ( !Input.VR.LeftHand.ButtonB.IsPressed && JustOpened )
		{
			JustOpened = false;
		}

		if ( Input.VR.LeftHand.ButtonB.IsPressed && !JustOpened )
		{
			OptionsMenu.Enabled = !OptionsMenu.Enabled;

			foreach ( var item in OptionsMenu.Children )
			{
				item.Enabled = false;
				item.Enabled = true;
			}

			JustOpened = true;

			OptionsMenu.Transform.Position = Head.Transform.Position + Head.Transform.Rotation.Forward * 64f;
			OptionsMenu.Transform.Rotation = Head.Transform.Rotation * Rotation.FromYaw( 180 );
			SelectedOption = 0;
		}

		if ( OptionsMenu.Enabled )
		{
			if ( Input.VR.LeftHand.Joystick.Value.y < -0.5f && !JustChanged )
			{
				SelectedOption = (SelectedOption + 1) % OptionSelections.Count();
				if ( SelectedOption >= OptionSelections.Count() )
				{
					SelectedOption = 0;
				}
				JustChanged = true;
			}

			if ( Input.VR.LeftHand.Joystick.Value.y > 0.5f && !JustChanged )
			{
				SelectedOption = (SelectedOption - 1) % OptionSelections.Count();
				if ( SelectedOption < 0 )
				{
					SelectedOption = OptionSelections.Count() - 1;
				}
				JustChanged = true;
			}

			if ( Input.VR.LeftHand.Joystick.Value.x > 0.5f && !JustChanged )
			{
				OptionsMenu.Components.Get<SettingsUI>( FindMode.EverythingInSelfAndChildren ).NextOption();
				JustChanged = true;
			}

			if ( Input.VR.LeftHand.Joystick.Value.x < -0.5f && !JustChanged )
			{
				OptionsMenu.Components.Get<SettingsUI>( FindMode.EverythingInSelfAndChildren ).PrevOption();
				JustChanged = true;
			}

			if ( Input.VR.LeftHand.Joystick.Value.Length < 0.5f && JustChanged )
			{
				JustChanged = false;
			}
			return;
		}

		DoButtonJump();

		if ( VRLegs.instance.Grounded )
		{
			switch ( movementMethod )
			{
				case MovementType.ArmSwinger:
					DoArmSwingerMovement();
					break;
				case MovementType.Joystick:
					DoJoystickMovement();
					break;
				case MovementType.Teleport:
					DoTeleportMovement();
					break;
				default:
					break;
			}
		}

		switch ( rotationMethod )
		{
			case RotationType.Snap:
				DoSnapturn();
				break;
			case RotationType.Smooth:
				DoSmoothturn();
				break;
			default:
				break;
		}
	}

	bool JustReleasedJump;

	bool TryingToJump;

	float JumpStartTime;

	float JumpPowerMult;

	public void DoButtonJump()
	{
		if ( Input.VR.RightHand.ButtonB.IsPressed && VRLegs.instance.Grounded && !VRLegs.instance.facetr.StartedSolid )
		{
			TryingToJump = true;
			JustReleasedJump = false;
			VRLegs.instance.Jumping = false;
			VRLegs.instance.HeightOffset = -15f * JumpPowerMult;

			if ( JumpPowerMult < 1f )
			{
				JumpPowerMult += Time.Delta;
			}

			JumpStartTime = Time.Now;
		}
		else if ( !JustReleasedJump && TryingToJump && VRLegs.instance.Grounded )
		{
			JustReleasedJump = true;
			VRLegs.instance.Jumping = true;
			switch ( movementDirection )
			{
				case DirectionType.Head:
					VRLegs.instance.rigbod.Velocity = (Head.Transform.Rotation.Forward.WithZ( 0 ) * JumpForwardMult + Vector3.Up * JumpUpMult) * JumpForce * JumpPowerMult;
					break;
				case DirectionType.Controller:
					VRLegs.instance.rigbod.Velocity = (RightHand.Transform.Rotation.Forward.WithZ( 0 ) * JumpForwardMult + Vector3.Up * JumpUpMult) * JumpForce * JumpPowerMult;
					break;
				default:
					break;
			}


		}
		else if ( JustReleasedJump && MathF.Abs( JumpStartTime - Time.Now ) > 0.2f && TryingToJump )
		{
			JumpPowerMult = 0f;
			VRLegs.instance.Jumping = false;
			JustReleasedJump = false;
			TryingToJump = false;
			VRLegs.instance.HeightOffset = 0f;
		}

		if ( TryingToJump && !VRLegs.instance.Grounded && MathF.Abs( JumpStartTime - Time.Now ) > 0.2f )
		{
			JumpPowerMult = 0f;
			VRLegs.instance.HeightOffset = 0f;
			VRLegs.instance.Jumping = false;
			TryingToJump = false;
		}
	}

	bool CanSnap = true;

	public void DoSnapturn()
	{
		if ( CanSnap )
		{
			if ( Input.VR.RightHand.Joystick.Value.x > 0.5f )
			{
				Transform.World = Transform.World.RotateAround( Head.Transform.Position, Rotation.FromYaw( -SnapRotateIncrements ) );
			}

			if ( Input.VR.RightHand.Joystick.Value.x < -0.5f )
			{
				Transform.World = Transform.World.RotateAround( Head.Transform.Position, Rotation.FromYaw( SnapRotateIncrements ) );
			}
			CanSnap = false;
		}

		if ( !CanSnap && Input.VR.RightHand.Joystick.Value.Length < 0.5f )
		{
			CanSnap = true;

		}

		Transform.Rotation = Rotation.FromYaw( Transform.Rotation.Yaw() );
	}

	public void DoSmoothturn()
	{

		if ( Input.VR.RightHand.Joystick.Value.Length > 0.5f )
		{
			Transform.World = Transform.World.RotateAround( Head.Transform.Position, Rotation.FromYaw( Time.Delta * SmoothRotateSpeed * MathF.Sign( -Input.VR.RightHand.Joystick.Value.x ) ) );
		}

		Transform.Rotation = Rotation.FromYaw( Transform.Rotation.Yaw() );
	}

	public void DoArmSwingerMovement()
	{
		switch ( movementDirection )
		{
			case DirectionType.Head:
				if ( Input.VR.LeftHand.ButtonA.IsPressed )
				{
					Transform.Position += Head.Transform.Rotation.Forward.WithZ( 0 ) * Input.VR.LeftHand.Velocity.Length * Time.Delta;
				}

				if ( Input.VR.RightHand.ButtonA.IsPressed )
				{
					Transform.Position += Head.Transform.Rotation.Forward.WithZ( 0 ) * Input.VR.RightHand.Velocity.Length * Time.Delta;
				}
				break;
			case DirectionType.Controller:
				if ( Input.VR.LeftHand.ButtonA.IsPressed )
				{
					Transform.Position += LeftHand.Transform.Rotation.Forward.WithZ( 0 ) * Input.VR.LeftHand.Velocity.Length * Time.Delta;
				}

				if ( Input.VR.RightHand.ButtonA.IsPressed )
				{
					Transform.Position += RightHand.Transform.Rotation.Forward.WithZ( 0 ) * Input.VR.RightHand.Velocity.Length * Time.Delta;
				}
				break;
			default:
				break;
		}

	}

	public void DoJoystickMovement()
	{
		switch ( movementDirection )
		{
			case DirectionType.Head:
				Transform.Position += (new Vector3( Input.VR.LeftHand.Joystick.Value.y, -Input.VR.LeftHand.Joystick.Value.x, 0 ) * Head.Transform.Rotation * JoystickMovementSpeed).WithZ( 0 );
				break;
			case DirectionType.Controller:
				Transform.Position += (new Vector3( Input.VR.LeftHand.Joystick.Value.y, -Input.VR.LeftHand.Joystick.Value.x, 0 ) * LeftHand.Transform.Rotation * JoystickMovementSpeed).WithZ( 0 );
				break;
			default:
				break;
		}
	}

	Vector3 foundteleport;

	Vector3 hitnormal;

	bool ShowTeleport = false;

	protected override void OnUpdate()
	{
		if ( ShowTeleport )
		{
			if ( Scene.NavMesh.IsEnabled )
			{
				Gizmo.Draw.Color = Color.Cyan.WithAlpha( 0.25f );

				Gizmo.Draw.Color = Color.Cyan;

				Vector3 hitpoint = DrawTeleportArc( LeftHand.Transform.Position, LeftHand.Transform.Rotation.Forward * JumpForce / 30f );

				bool CanFit = !Scene.Trace.Ray( hitpoint + hitnormal, hitpoint + hitnormal * Head.Transform.LocalPosition.z ).Run().Hit;

				if ( hitpoint != Vector3.Zero && IsOnNavmesh( hitpoint ) && hitnormal.z > 0.5f && CanFit )
				{
					Gizmo.Draw.LineCylinder( hitpoint + hitnormal * 1f, hitpoint + hitnormal * Head.Transform.LocalPosition.z, 10f, 10f, 12 );
					foundteleport = hitpoint;
				}
				else
				{
					Gizmo.Draw.Color = Color.Red;
					Gizmo.Draw.LineCylinder( hitpoint + hitnormal * 1f, hitpoint + hitnormal * Head.Transform.LocalPosition.z, 10f, 10f, 12 );
					foundteleport = Vector3.Zero;
				}
			}
		}
	}

	public void DoTeleportMovement()
	{
		if ( Input.VR.LeftHand.Joystick.Value.y > 0.5f )
		{
			ShowTeleport = true;
		}
		else
		{
			ShowTeleport = false;
			if ( foundteleport != Vector3.Zero )
			{
				Transform.Position = foundteleport - (Head.Transform.LocalPosition.WithZ( 0 ) * Transform.Rotation);
				foundteleport = Vector3.Zero;
			}
		}
	}

	public bool IsOnNavmesh( Vector3 position )
	{

		if ( Vector3.DistanceBetween( Scene.NavMesh.GetClosestPoint( position ) ?? Vector3.Zero, position ) < 15f )
		{
			return true;
		}

		return false;
	}

	private Vector3 DrawTeleportArc( Vector3 startPos, Vector3 startVelocity )
	{
		float timeStep = 0.1f;
		float maxTime = 100f;
		Vector3 currentPosition = startPos;

		for ( float time = 0; time < maxTime; time += timeStep )
		{
			float deltaTime = time + timeStep;

			float x = startPos.x + startVelocity.x * deltaTime;
			float y = startPos.y + startVelocity.y * deltaTime;
			float z = startPos.z + startVelocity.z * deltaTime - 0.5f * 1f * deltaTime * deltaTime;

			Vector3 nextPosition = new Vector3( x, y, z );

			Gizmo.Draw.Line( currentPosition, nextPosition );

			var tr = Scene.Trace.Ray( currentPosition, nextPosition ).Run();
			if ( tr.Hit )
			{
				hitnormal = tr.Normal;
				return nextPosition;
			}

			currentPosition = nextPosition;
		}

		return Vector3.Zero;
	}
}

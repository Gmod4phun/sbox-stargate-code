using Sandbox;
using System;

public sealed class VRLegs : Component
{
	public static VRLegs instance;

	[Property] GameObject Head { get; set; }

	[Property] float FootBuffer { get; set; } = 5f;

	[Property] float MaxStandableAngle { get; set; } = 35f;

	[Property] float LedgeHopPower { get; set; } = 90f;

	[Property] public float HeightOffset { get; set; }

	public bool Jumping { get; set; }

	public bool Grounded;

	public Rigidbody rigbod;

	public delegate void RespawnEvent();
	public event RespawnEvent OnRespawn;

	protected override void OnAwake()
	{
		instance = this;
		rigbod = Components.Get<Rigidbody>( false );
	}
	MapInstance mapInstance;
	protected override void OnStart()
	{
		mapInstance = Scene.GetAllComponents<MapInstance>().First();
		if ( mapInstance != null )
		{
			mapInstance.OnMapLoaded += RespawnPlayer;

			// already loaded
			if ( mapInstance.IsLoaded )
			{
				RespawnPlayer();
			}
		}
	}

	public void RespawnPlayer()
	{
		if ( mapInstance != null && mapInstance.IsLoaded )
		{
			var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();
			var randomSpawnPoint = new Transform( Vector3.Zero, Rotation.Identity );
			if ( spawnPoints.Length > 0 ) randomSpawnPoint = spawnPoints[Random.Shared.Int( 0, spawnPoints.Length - 1 )].Transform.World;

			Transform.World = randomSpawnPoint;
			rigbod.Velocity = Vector3.Zero;
			OnRespawn.Invoke();
		}
		else
		{
			mapInstance = Scene.GetAllComponents<MapInstance>().First();
		}
	}

	Vector3 ClampMagnitude( Vector3 vector, float maxMagnitude )
	{
		float currentMagnitude = vector.Length;

		if ( currentMagnitude > maxMagnitude )// Check if the current magnitude is greater than the maximum allowed magnitude
		{
			vector = vector.Normal * maxMagnitude;// Normalize the vector and multiply by the desired magnitude to maintain direction
		}

		return vector;
	}

	bool JustLeftLedge = true;

	Vector3 PositionDelta;

	Vector3 LastPosition;

	public SceneTraceResult facetr, chesttr;

	protected override void OnFixedUpdate()
	{
		if ( Transform.Position.z < -1000f )
		{
			RespawnPlayer();
		}

		PositionDelta = Head.Transform.Position - LastPosition;//Keep track of head movement for wall detection.

		var legtr = Scene.Trace.Ray( Head.Transform.Position, Head.Transform.Position.WithZ( Transform.Position.z - HeightOffset ) + Vector3.Down * FootBuffer ).WithoutTags( "player" ).Run();//Ground detection.

		var ledgetr = Scene.Trace.Ray( Head.Transform.Position, Head.Transform.Position.WithZ( Transform.Position.z - HeightOffset ) + Vector3.Down * FootBuffer * 3f ).WithoutTags( "player" ).Run();//Ledge detection.

		Grounded = legtr.Hit;

		Vector3 clampedPosDelta = ClampMagnitude( PositionDelta, VRPlayerMovement.instance.movementMethod != VRPlayerMovement.MovementType.Teleport ? 80f : 1f );

		clampedPosDelta.z = MathX.Clamp( clampedPosDelta.z, -20f, 20f );//Clamp the height a bit more agressively so the legs actually work when falling from high up.

		facetr = Scene.Trace.Ray( Head.Transform.Position, Head.Transform.Position + clampedPosDelta * 2f ).Radius( 8f ).WithoutTags( "player" ).Run();//Face trace for keeping people from walking through walls.

		if ( facetr.Hit )//Bounce player off the walls
		{
			rigbod.Velocity += facetr.Normal * 8f;
			Transform.Position += facetr.Normal * PositionDelta.Length;
		}

		chesttr = Scene.Trace.Ray( Head.Transform.Position - (Vector3.Up * Head.Transform.LocalPosition.z / 2f), Head.Transform.Position - (Vector3.Up * Head.Transform.LocalPosition.z / 2f) + clampedPosDelta * 2f ).Radius( 8f ).WithoutTags( "player" ).Run();//Face trace for keeping people from walking through walls.

		if ( chesttr.Hit && !Jumping )//Bounce player off tables if not jumping
		{
			rigbod.Velocity += chesttr.Normal * 8f;
			Transform.Position += chesttr.Normal * PositionDelta.Length;
		}

		if ( Jumping )//Remove the ledge jump boost if you're already normal-jumping.
		{
			JustLeftLedge = true;
		}

		if ( Grounded && !Jumping )//Force the player to be ground level using rigidbody forces.
		{
			JustLeftLedge = false;

			Vector3 PositionDelta = Transform.Position.WithZ( legtr.HitPosition.z + HeightOffset ) - (Transform.Position);

			if ( facetr.Hit )
			{
				PositionDelta += facetr.Normal * 8f;
			}

			Vector3 VelocityTarget = (PositionDelta * 300f * Time.Delta) + new Vector3( rigbod.Velocity.x * 0.9f, rigbod.Velocity.y * 0.9f, 0 );

			float standingAngle = Vector3.GetAngle( legtr.Normal, Vector3.Up );

			if ( standingAngle > MaxStandableAngle )
			{
				VelocityTarget += Rotation.LookAt( legtr.Normal, Vector3.Up ).Down * standingAngle;
			}

			rigbod.Velocity = Vec3MoveTowards.MoveTowards( rigbod.Velocity, VelocityTarget, 1000f );
		}
		else if ( !JustLeftLedge && !ledgetr.Hit && !Jumping )//We just left a ledge, add a little boost for a ledge exit hop.
		{
			JustLeftLedge = true;

			rigbod.Velocity = ClampMagnitude( (PositionDelta + Vector3.Up) * LedgeHopPower, 500f );
		}

		rigbod.AngularVelocity = Vector3.Zero;

		LastPosition = Head.Transform.Position;
	}
}

public class Vec3MoveTowards
{
	private const float Epsilon = 0.00001f;
	public static Vector3 MoveTowards( Vector3 current, Vector3 target, float maxDistanceDelta )
	{
		Vector3 toVector = target - current;
		float magnitude = toVector.Length;

		if ( magnitude <= maxDistanceDelta || magnitude < Epsilon )
		{
			return target;
		}

		return current + toVector / magnitude * maxDistanceDelta;
	}
}

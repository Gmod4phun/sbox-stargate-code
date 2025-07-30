using Sandbox.Components.Stargate;

public interface ITeleportable
{
	/// <summary>
	/// Called after gate teleportation occurs.
	/// </summary>
	void PostGateTeleport(Stargate from, Stargate to);
}

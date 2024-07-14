public static class GameObjectExtensions
{
	public static void SetupNetworking(
		this GameObject go,
		OwnerTransfer transfer = OwnerTransfer.Takeover,
		NetworkOrphaned orphaned = NetworkOrphaned.ClearOwner
	)
	{
		go.NetworkMode = NetworkMode.Object;

		if (!go.Network.Active && !go.Scene.IsEditor)
			go.NetworkSpawn();

		go.Network.SetOwnerTransfer(transfer);
		go.Network.SetOrphanedMode(orphaned);
	}
}

using Sandbox.Network;

public class GameNetworkManager : Component, Component.INetworkListener
{
	/// <summary>
	/// Create a server (if we're not joining one)
	/// </summary>
	[Property]
	public bool StartServer { get; set; } = true;

	/// <summary>
	/// The prefab to spawn for the player to control.
	/// </summary>
	[Property]
	public GameObject PlayerPrefab { get; set; }

	/// <summary>
	/// A list of points to choose from randomly to spawn the player in. If not set, we'll spawn at the
	/// location of the NetworkHelper object.
	/// </summary>
	[Property]
	public List<GameObject> SpawnPoints { get; set; }

	public static GameNetworkManager Current =>
		Game.ActiveScene.GetAllComponents<GameNetworkManager>().FirstOrDefault();

	protected override async Task OnLoad()
	{
		if (Scene.IsEditor)
			return;

		if (StartServer && !Networking.IsActive)
		{
			LoadingScreen.Title = "Creating Lobby";
			await Task.DelayRealtimeSeconds(0.1f);
			Networking.CreateLobby();
		}
	}

	/// <summary>
	/// A client is fully connected to the server. This is called on the host.
	/// </summary>
	public void OnActive(Connection channel)
	{
		Log.Info($"Player '{channel.DisplayName}' has joined the game");

		SpawnPlayer(channel);
	}

	public void SpawnPlayer(Connection channel)
	{
		if (PlayerPrefab is null)
			return;

		//
		// Find a spawn location for this player
		//
		var startLocation = FindSpawnLocation().WithScale(1);

		// Spawn this object and make the client the owner
		var player = PlayerPrefab.Clone(startLocation, name: $"Player - {channel.DisplayName}");

		// var nameTag = player.Components.Get<NameTagPanel>( FindMode.EverythingInSelfAndDescendants );
		// if ( nameTag is not null )
		// {
		// 	nameTag.Name = channel.DisplayName;
		// }

		var clothing = new ClothingContainer();
		clothing.Deserialize(channel.GetUserData("avatar"));

		// Assume that if they have a skinned model renderer, it's the citizen's body
		if (
			player.Components.TryGet<SkinnedModelRenderer>(
				out var body,
				FindMode.EverythingInSelfAndDescendants
			)
		)
		{
			clothing.Apply(body);
		}

		player.NetworkSpawn(channel);

		if (
			player.Components.TryGet<PlayerController>(
				out var controller,
				FindMode.EverythingInSelfAndDescendants
			)
		)
		{
			controller.CurrentWorldIndex = MultiWorldSystem.Worlds.First().WorldIndex;
		}
	}

	/// <summary>
	/// Find the most appropriate place to respawn
	/// </summary>
	Transform FindSpawnLocation()
	{
		//
		// If they have spawn point set then use those
		//
		if (SpawnPoints is not null && SpawnPoints.Count > 0)
		{
			return Random.Shared.FromList(SpawnPoints, default).Transform.World;
		}

		//
		// If we have any SpawnPoint components in the scene, then use those
		//
		var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();
		if (spawnPoints.Length > 0)
		{
			return Random.Shared.FromArray(spawnPoints).Transform.World;
		}

		//
		// Failing that, spawn where we are
		//
		return Transform.World;
	}
}

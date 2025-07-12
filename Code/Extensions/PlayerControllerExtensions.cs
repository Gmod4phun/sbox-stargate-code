public static class PlayerControllerExtensions
{
	public static void ActivateTeleportScreenOverlay(this PlayerController player, float duration)
	{
		var cam = player.GetCamera();
		if (cam.IsValid() && cam.Components.TryGet<TeleportScreenoverlay>(out var overlay))
		{
			overlay.ActivateFor(duration);
		}
	}

	public static CameraComponent GetCamera(this PlayerController player)
	{
		return player.Components.Get<CameraComponent>(FindMode.InDescendants);
	}
}

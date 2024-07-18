public class ZPMHub : Component, IUse
{
	[Property]
	public IEnumerable<AttachPoint> AttachPoints =>
		Components.GetAll<AttachPoint>(FindMode.EnabledInSelfAndChildren);

	[Property]
	public int AttachPointCount => AttachPoints.Count();

	float useDelay = 1.2f;
	TimeSince lastUse = 0;

	Dictionary<AttachPoint, bool> attachPointStates = new();

	public async Task MoveAttachPoint(AttachPoint attachPoint)
	{
		var moveDistance = 8f;
		var moveTime = 1f;

		if (!attachPointStates.TryGetValue(attachPoint, out var _))
		{
			attachPointStates[attachPoint] = false;
		}

		var isUp = attachPointStates[attachPoint];

		var totalMovedDistance = 0f;
		while (totalMovedDistance < moveDistance)
		{
			var step = Time.Delta / moveTime * moveDistance;
			totalMovedDistance += step;
			attachPoint.Transform.Position += (isUp ? Vector3.Down : Vector3.Up) * step;
			attachPoint.Transform.ClearInterpolation();
			await Task.Frame();
		}

		attachPointStates[attachPoint] = !isUp;
	}

	public bool OnUse(GameObject user)
	{
		foreach (var attachPoint in AttachPoints)
		{
			_ = MoveAttachPoint(attachPoint);
		}

		lastUse = 0;
		return false;
	}

	public bool IsUsable(GameObject user)
	{
		return lastUse > useDelay;
	}
}

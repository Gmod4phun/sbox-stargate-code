namespace Sandbox.Components.Stargate;

public class DhdDialGuide : Component
{
	[Property]
	public Dhd DHD => GameObject.Components.Get<Dhd>();

	[Property]
	public Stargate DestinationGate { get; set; }

	protected override void OnDisabled()
	{
		base.OnDisabled();
		DHD?.ButtonGlowOutline?.Targets.Clear();
	}

	protected override void OnUpdate()
	{
		if (!DHD.IsValid())
			return;

		var outline = DHD.ButtonGlowOutline;
		if (!outline.IsValid())
			return;

		if (outline.Targets.Count != 0)
		{
			outline.Targets.Clear();
		}

		if (!DestinationGate.IsValid())
			return;

		var gate = DHD.Gate;
		if (!gate.IsValid() || gate.Busy || gate.Open)
			return;

		var activeSymbols = DHD.PressedActions;

		var otherAddress = Stargate.GetOtherGateAddressForMenu(DHD.Gate, DestinationGate);

		var activeSymbolsString = string.Join("", activeSymbols);
		if (activeSymbols.Count > 0 && !otherAddress.StartsWith(activeSymbolsString))
			return;

		var nextAction = otherAddress
			.ToCharArray()
			.FirstOrDefault(c => !activeSymbols.Contains(c.ToString()))
			.ToString();

		if (!DHD.DialIsLock && activeSymbols.Count == otherAddress.Length)
		{
			nextAction = "DIAL";
		}

		if (DHD.DialIsLock && nextAction == "#")
		{
			nextAction = "DIAL";
		}

		if (string.IsNullOrEmpty(nextAction))
			return;

		var buttonToActivate = DHD.GetButtonByAction(nextAction);
		if (!buttonToActivate.IsValid() || !buttonToActivate.ButtonModel.IsValid())
			return;

		if (Scene?.Camera?.WorldPosition.DistanceSquared(DHD.GetSymbolPosition(nextAction)) > 4096)
			return;

		if (!outline.Targets.Contains(buttonToActivate.ButtonModel))
		{
			outline.Targets.Add(buttonToActivate.ButtonModel);
		}
	}
}

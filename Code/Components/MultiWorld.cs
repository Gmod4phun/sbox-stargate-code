using Sandbox.Audio;

public class MultiWorld : Component
{
	[Property]
	public int WorldIndex { get; private set; } = -1;

	public string GetMixerName() => "World " + WorldIndex + " Mixer";

	public Mixer GetMixer() => Mixer.FindMixerByName(GetMixerName());

	public IEnumerable<GameObject> GetAllChildrenRecursive()
	{
		foreach (var child in GameObject.Children)
		{
			yield return child;
			foreach (var grandChild in child.Components.Get<MultiWorld>().GetAllChildrenRecursive())
			{
				yield return grandChild;
			}
		}
	}

	public Mixer CreateMixer()
	{
		var mixer = Mixer.Master.AddChild();
		mixer.Name = GetMixerName();
		return mixer;
	}

	protected override void OnStart()
	{
		base.OnStart();

		Tags.Add(MultiWorldSystem.GetWorldTag(WorldIndex));

		if (Mixer.FindMixerByName(GetMixerName()) == null)
		{
			CreateMixer();
		}

		// MultiWorldSystem.Init();
	}
}

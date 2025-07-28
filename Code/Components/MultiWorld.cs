using Sandbox.Audio;

public class MultiWorld : Component
{
	[Property]
	public int WorldIndex { get; private set; } = -1;

	public string MixerName => "World " + WorldIndex + " Mixer";

	public Mixer AudioMixer => Mixer.FindMixerByName(MixerName);

	public string WorldTag => $"world_{WorldIndex}";

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

	public Mixer CreateAudioMixer()
	{
		var mixer = Mixer.Master.AddChild();
		mixer.Name = MixerName;
		return mixer;
	}

	protected override void OnStart()
	{
		if (Scene.IsEditor)
			return;

		Tags.Add(WorldTag);

		if (Mixer.FindMixerByName(MixerName) == null)
		{
			CreateAudioMixer();
		}
	}
}

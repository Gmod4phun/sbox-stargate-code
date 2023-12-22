using Sandbox;
using Sandbox.Citizen;
using Sandbox.Components.Stargate;
using System.Diagnostics;
using System.Drawing;
using System.Runtime;

public class MorphController : Component, Component.ExecuteInEditor
{
	[Property]
	public SkinnedModelRenderer Target {get; set;}

	[Property]
	public string MorphName {get; set;}

	[Property]
	public float MorphValue {get; set;}

	[Property, System.ComponentModel.ReadOnly(true)]
	public float CurrentMorphValue {get; set;}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (Target.IsValid() && Target.SceneModel.IsValid()) {
			Target.SceneObject.Flags.IsOpaque = false;
			Target.SceneObject.Flags.IsTranslucent = true;
			Target.SceneObject.Batchable = false;

			Target.SceneModel.Morphs.Set(MorphName, MorphValue);
			CurrentMorphValue = Target.SceneModel.Morphs.Get(MorphName);
		}
	}
}

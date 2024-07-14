namespace Sandbox.Components.Stargate
{
	public class GateBearing : Component
	{
		private float _selfillumscale = 0;

		public SkinnedModelRenderer BearingModel { get; set; }

		public bool On { get; private set; } = false;

		// public Stargate Gate => GameObject.Parent.Components.Get<Stargate>( FindMode.EnabledInSelfAndDescendants );

		public async void TurnOn(float delay = 0)
		{
			if (delay > 0)
				await Task.DelaySeconds(delay);

			On = true;
		}

		public async void TurnOff(float delay = 0)
		{
			if (delay > 0)
				await Task.DelaySeconds(delay);

			On = false;
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			_selfillumscale = _selfillumscale.Approach(On ? 1 : 0, (On ? 8f : 4f) * Time.Delta);

			if (BearingModel.IsValid() && BearingModel.SceneObject.IsValid())
			{
				BearingModel.SceneObject.Batchable = false;
				BearingModel.SceneObject.Attributes.Set("selfillumscale", _selfillumscale);
			}
		}
	}
}

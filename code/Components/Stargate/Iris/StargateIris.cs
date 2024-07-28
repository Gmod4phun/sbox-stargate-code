namespace Sandbox.Components.Stargate
{
	public class StargateIris : Component, Component.ExecuteInEditor
	{
		public enum IrisType
		{
			Standard,
			Atlantis,
			Goauld
		}

		protected virtual float _openCloseDelay => 3f;
		public bool Busy { get; set; } = false;

		public Stargate Gate =>
			GameObject.Parent.Components.Get<Stargate>(FindMode.EnabledInSelfAndDescendants);

		[Property]
		public SkinnedModelRenderer IrisModel { get; set; }

		[Property]
		public ModelCollider IrisCollider { get; set; }

		public bool Closed { get; protected set; } = false;

		public virtual async void Close()
		{
			if (Busy || Closed)
				return;

			IrisModel.SceneModel.RenderingEnabled = true;
			IrisCollider.Enabled = true;

			Busy = true;
			Closed = true;

			IrisModel.SceneModel.SetAnimParameter("Open", false);

			Stargate.PlaySound(this, "stargate.iris.close");

			await Task.DelaySeconds(_openCloseDelay);

			Busy = false;
		}

		public virtual async void Open()
		{
			if (Busy || !Closed)
				return;

			Busy = true;
			Closed = false;

			IrisCollider.Enabled = false;

			IrisModel.SceneModel.SetAnimParameter("Open", true);

			Stargate.PlaySound(this, "stargate.iris.open");

			await Task.DelaySeconds(_openCloseDelay);

			IrisModel.SceneModel.RenderingEnabled = false;

			Busy = false;
		}

		public void Toggle()
		{
			if (Busy)
				return;

			if (Closed)
				Open();
			else
				Close();
		}

		public virtual void PlayHitSound()
		{
			Stargate.PlaySound(this, "stargate.iris.hit");
		}
	}
}

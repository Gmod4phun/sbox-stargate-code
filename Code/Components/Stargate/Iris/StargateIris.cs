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

			if (IrisModel.IsValid() && IrisModel.SceneModel.IsValid())
			{
				IrisModel.SceneModel.RenderingEnabled = true;
				IrisModel.Set("Open", false);
			}

			if (IrisCollider.IsValid())
			{
				IrisCollider.Enabled = true;
			}

			Busy = true;
			Closed = true;

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

			if (IrisCollider.IsValid())
			{
				IrisCollider.Enabled = false;
			}

			if (IrisModel.IsValid() && IrisModel.SceneModel.IsValid())
			{
				IrisModel.SceneModel.SetAnimParameter("Open", true);
			}

			Stargate.PlaySound(this, "stargate.iris.open");

			await Task.DelaySeconds(_openCloseDelay);

			if (IrisModel.IsValid() && IrisModel.SceneModel.IsValid())
			{
				IrisModel.SceneModel.RenderingEnabled = false;
			}

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

		protected override void OnStart()
		{
			if (Scene.IsEditor)
				return;

			Busy = false;
			Closed = false;

			if (IrisCollider.IsValid())
			{
				IrisCollider.Enabled = false;
			}
			if (IrisModel.IsValid() && IrisModel.SceneModel.IsValid())
			{
				IrisModel.Set("Open", true);
				IrisModel.SceneModel.RenderingEnabled = true;
				IrisModel.Set("CanTransition", true);
			}
		}

		public virtual void PlayHitSound()
		{
			Stargate.PlaySound(this, "stargate.iris.hit");
		}
	}
}

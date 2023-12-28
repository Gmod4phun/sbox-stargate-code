namespace Sandbox.Components.Stargate
{
	public class StargateIris : Component, Component.ExecuteInEditor
	{
		protected virtual float _openCloseDelay => 3f;
		public bool Busy { get; set; } = false;

		public Stargate Gate => GameObject.Parent.Components.Get<Stargate>( FindMode.EnabledInSelfAndDescendants );

		[Property]
		public SkinnedModelRenderer IrisModel { get; set; }

		[Property]
		public ModelCollider IrisCollider { get; set; }

		public bool Closed { get; protected set; } = false;

		public virtual async void Close()
		{
			if ( Busy || Closed ) return;

			IrisModel.SceneModel.RenderingEnabled = true;
			IrisCollider.Enabled = true;

			Busy = true;
			Closed = true;

			IrisModel.SceneModel.SetAnimParameter( "Open", false );

			Sound.Play( "stargate.iris.close", Transform.Position );

			await Task.DelaySeconds( _openCloseDelay );

			Busy = false;
		}

		public virtual async void Open()
		{
			if ( Busy || !Closed ) return;

			Busy = true;
			Closed = false;

			IrisCollider.Enabled = false;

			IrisModel.SceneModel.SetAnimParameter( "Open", true );

			Sound.Play( "stargate.iris.open", Transform.Position );

			await Task.DelaySeconds( _openCloseDelay );

			IrisModel.SceneModel.RenderingEnabled = false;

			Busy = false;
		}

		public void Toggle()
		{
			if ( Busy ) return;

			if ( Closed )
				Open();
			else
				Close();
		}

		public virtual void PlayHitSound()
		{
			Sound.Play( "stargate.iris.hit", Transform.Position );
		}
	}
}

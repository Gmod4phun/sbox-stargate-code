namespace Sandbox.Components.Stargate
{
	public class StargateIrisAtlantis : StargateIris, Component.IDamageable
	{
		protected override float _openCloseDelay => 1f;
		private MultiWorldSound IrisLoop;
		private float _currentAlpha = 0;

		public override async void Close()
		{
			if (Busy || Closed)
				return;

			Busy = true;
			Closed = true;
			IrisModel.SceneModel.RenderingEnabled = true;
			IrisCollider.Enabled = true;

			Stargate.PlaySound(this, "stargate.iris.atlantis.close");

			await Task.DelaySeconds(_openCloseDelay);

			Busy = false;

			await Task.DelaySeconds(0.5f);

			if (Closed)
			{
				IrisLoop?.Stop(0.1f);
				IrisLoop = Stargate.PlayFollowingSound(GameObject, "stargate.iris.atlantis.loop");
			}
		}

		public override async void Open()
		{
			if (Busy || !Closed)
				return;
			IrisLoop?.Stop(0.1f);

			Busy = true;
			Closed = false;
			IrisCollider.Enabled = false;
			Stargate.PlaySound(this, "stargate.iris.atlantis.open");

			await Task.DelaySeconds(_openCloseDelay);

			IrisModel.SceneModel.RenderingEnabled = false;
			Busy = false;
		}

		public override void PlayHitSound()
		{
			Stargate.PlaySound(this, "stargate.iris.atlantis.hit");
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			IrisLoop?.Stop();

			if (Closed)
			{
				Stargate.PlaySound(this, "stargate.iris.atlantis.open");
			}
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			_currentAlpha = _currentAlpha.LerpTo(Closed ? 1 : 0, Time.Delta * 6);

			if (IrisModel.IsValid() && IrisModel.SceneModel.IsValid())
			{
				var sm = IrisModel.SceneModel;
				sm.Flags.IsOpaque = false;
				sm.Flags.IsTranslucent = true;
				sm.ColorTint = sm.ColorTint.WithAlpha(_currentAlpha);
			}
		}

		public void OnDamage(in DamageInfo damage)
		{
			if (damage.Damage > 0 && damage.Shape?.Body?.GetGameObject() == GameObject)
			{
				PlayHitSound();
			}
		}
	}
}

namespace Sandbox.Components.Stargate
{
	public partial class Kawoosh : Component, Component.ExecuteInEditor
	{
		// private CapsuleLightEntity _light;

		[Property]
		public SkinnedModelRenderer KawooshModel { get; set; }

		[Property]
		public SkinnedModelRenderer KawooshModelInside { get; set; }

		private bool _isExpanding = false;
		private float _currentProgress = 0;
		private static float _minProgress = 0;
		private static float _maxProgress = 1.5f;

		public EventHorizon EventHorizon =>
			GameObject.Parent.Components.Get<EventHorizon>(FindMode.EnabledInSelfAndDescendants);

		public Collider Trigger;

		private Plane KawooshClipPlane =>
			new(
				Transform.Position
					- Scene.Camera.Transform.Position
					+ Transform.Rotation.Forward * 10f,
				Transform.Rotation.Forward.Normal
			);

		protected override void OnStart()
		{
			base.OnStart();
		}

		// protected override void OnDestroy()
		// {
		//     base.OnDestroy();
		//     Trigger?.Delete();
		//     _light?.Delete();
		//     KawooshModelInside?.Delete();
		// }

		public async void DoKawooshAnimation()
		{
			// Trigger = new EventHorizonTrigger( EventHorizon, "models/sbox_stargate/event_horizon/event_horizon_trigger_kawoosh.vmdl" )
			// {	Position = EventHorizon.Position + EventHorizon.Rotation.Forward * 2,
			//     Rotation = EventHorizon.Rotation,
			//     Parent = EventHorizon.Gate
			// };

			KawooshClientAnim();

			await GameTask.DelaySeconds(1.5f);

			KawooshClientAnim(true);
		}

		// [ClientRpc]
		public void KawooshClientAnim(bool ending = false)
		{
			if (
				!KawooshModel.IsValid() || !KawooshModel.SceneObject.IsValid() /*|| !KawooshModelInside.IsValid()*/
			)
				return;

			_isExpanding = !ending;
			if (_isExpanding)
			{
				_currentProgress = 0;

				KawooshModel.SceneObject.RenderingEnabled = false;
				KawooshModelInside.SceneObject.RenderingEnabled = false;

				// _light = new CapsuleLightEntity
				// {
				//     Position = Position,
				//     Parent = this,
				//     Rotation = Rotation,
				//     Color = Color.FromBytes(25, 150, 250),
				//     LightSize = 80f,
				//     Brightness = 0,
				//     Enabled = true
				// };
			}
		}

		// [GameEvent.Client.Frame]
		protected override void OnUpdate()
		{
			base.OnUpdate();

			if (
				!KawooshModel.IsValid()
				|| !KawooshModel.SceneObject.IsValid()
				|| !KawooshModelInside.IsValid()
				|| !KawooshModelInside.SceneObject.IsValid()
			)
				return;

			var delta = Time.Delta * (_isExpanding ? 1.8f : 3.2f);
			_currentProgress = _currentProgress.LerpTo(
				_isExpanding ? _maxProgress : _minProgress,
				delta
			);

			var morphValue = CalculateAnimationValue(_maxProgress - _currentProgress);
			KawooshModel.SceneModel.Morphs.Set("Shrinkwrap", morphValue);
			KawooshModelInside.SceneModel.Morphs.Set("Shrinkwrap", morphValue);
			GameObject.Transform.LocalScale = GameObject.Transform.LocalScale.WithX(1 - morphValue); // scale the kawoosh along the X axis, too

			if (morphValue < 0.98)
			{
				KawooshModel.SceneObject.RenderingEnabled = true;
				KawooshModelInside.SceneObject.RenderingEnabled = true;
			}

			/*
			if ( _light.IsValid() )
			{
			    var remappedProgress = _currentProgress.Remap( _minProgress, _maxProgress, 0, 1 );
			    var lightLength = 128 * remappedProgress;

			    _light.Enabled = true;
			    _light.CapsuleLength = lightLength;
			    _light.Position = Position + Rotation.Forward * lightLength;
			    _light.Brightness = remappedProgress * 0.1f;
			}
			*/

			KawooshModel.SceneObject.Flags.IsOpaque = false;
			KawooshModel.SceneObject.Flags.IsTranslucent = true;
			KawooshModel.SceneObject.Batchable = false;
			KawooshModel.SceneObject.ClipPlaneEnabled = true;
			KawooshModel.SceneObject.ClipPlane = KawooshClipPlane;

			KawooshModelInside.SceneObject.Flags.IsOpaque = true;
			KawooshModelInside.SceneObject.Flags.IsTranslucent = true;
			KawooshModelInside.SceneObject.Batchable = false;
			KawooshModelInside.SceneObject.ClipPlaneEnabled = true;
			KawooshModelInside.SceneObject.ClipPlane = KawooshClipPlane;
			KawooshModelInside.SceneObject.Attributes.Set("emission", 6);
		}

		/// <summary>
		/// Calculates the animation value based on a given input.
		/// </summary>
		/// <param name="inputValue">Input float value which serves as a base for the calculation.</param>
		/// <returns>Animation value calculated by applying a sinusoidal function on the cubed input value, scaled by a constant factor.</returns>
		private float CalculateAnimationValue(float inputValue)
		{
			return (float)Math.Sin(0.5f * Math.Pow(inputValue, 3));
		}
	}
}

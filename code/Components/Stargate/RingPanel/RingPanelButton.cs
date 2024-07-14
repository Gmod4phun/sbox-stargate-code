namespace Sandbox.Components.Stargate.Rings
{
	public class RingPanelButton : Component, Component.ExecuteInEditor, IUse
	{
		public RingPanel RingPanel => Components.Get<RingPanel>(FindMode.InParent);
		public ModelRenderer Renderer => Components.Get<ModelRenderer>(FindMode.InSelf);
		public ModelCollider Collider => Components.Get<ModelCollider>(FindMode.InSelf);

		[Property]
		public string Action { get; set; } = "";

		[Property]
		public SoundEvent PressSound { get; set; }

		[Sync]
		public bool On { get; set; } = false;
		private float _glowScale = 0;

		public bool OnUse(GameObject ent)
		{
			RingPanel.TriggerAction(Action);
			return false;
		}

		public bool IsUsable(GameObject ent)
		{
			return true;
		}

		public void ButtonGlowLogic()
		{
			if (!Renderer.IsValid())
				return;

			var so = Renderer.SceneObject;
			if (!so.IsValid())
				return;

			_glowScale = _glowScale.LerpTo(On ? 1 : 0, Time.Delta * (On ? 20f : 10f));

			so.Batchable = false;
			so.Attributes.Set("selfillumscale", _glowScale);
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			ButtonGlowLogic();
			// DrawButtonActions();
		}

		// private void DrawButtonActions() // doing anything with world panels is fucking trash, cant position stuff properly, keep debugoverlay for now
		// {
		//     var pos = Transform.PointToWorld( Model.RenderBounds.Center );
		//     DebugOverlay.Text( Action, pos, Color.White, 0, 86 );
		// }
	}
}

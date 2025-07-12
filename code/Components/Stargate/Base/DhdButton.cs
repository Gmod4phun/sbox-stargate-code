namespace Sandbox.Components.Stargate
{
	public class DhdButton : Component, Component.ExecuteInEditor, Component.IPressable
	{
		public ModelRenderer ButtonModel => Components.Get<ModelRenderer>();
		public ModelCollider ButtonCollider => Components.Get<ModelCollider>();

		[Property]
		public Dhd DHD => GameObject.Components.Get<Dhd>(FindMode.InParent);

		[Property, Sync]
		public string Action { get; set; } = "";

		[Property, Sync]
		public bool Disabled { get; set; } = false;

		[Sync]
		public bool On { get; set; } = false;
		private float _glowScale = 0;

		HighlightOutline _outline;

		protected override void OnEnabled()
		{
			CreateOutline();
		}

		void CreateOutline()
		{
			_outline?.Destroy();
			_outline = GameObject.Components.GetOrCreate<HighlightOutline>();
			_outline.Color = new Color(640, 300, 50, 0.5f);
			_outline.ObscuredColor = _outline.Color;
			_outline.Enabled = false;
		}

		public bool Press(IPressable.Event e)
		{
			if (Time.Now < DHD.LastPressTime + DHD.PressDelay)
				return false;

			var user = e.Source.GameObject;

			Network.TakeOwnership();
			DHD.Network.TakeOwnership();
			DHD.Gate?.Network.TakeOwnership();

			DHD.LastPressTime = Time.Now;
			DHD.TriggerAction(Action, user);

			return true;
		}

		public bool CanPress(IPressable.Event e)
		{
			return !Disabled;
		}

		public void Hover(IPressable.Event e)
		{
			_outline.Enabled = true;
		}

		public void Blur(IPressable.Event e)
		{
			_outline.Enabled = false;
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			if (ButtonModel.IsValid() && ButtonModel.SceneObject is SceneObject so)
			{
				_glowScale = _glowScale.LerpTo(On ? 1 : 0, Time.Delta * (On ? 2f : 20f));
				so.Batchable = false;
				so.Attributes.Set("selfillumscale", _glowScale);

				if (ButtonModel is Superglyph glyph)
				{
					glyph.GlyphEnabled = On;
				}

				// DrawSymbol();
			}
		}

		/*
		public void DrawSymbol()
		{
		    if ( Scene.Camera.IsValid() && ButtonCollider.IsValid() )
		    {
		        if ( !ButtonCollider.KeyframeBody.IsValid() )
		            return;

		        var pos = ButtonCollider.KeyframeBody.MassCenter;
		        var screenPos = Scene.Camera.PointToScreenPixels( pos );
		        if (
		            screenPos.x < 1
		            || screenPos.x > Screen.Width - 1
		            || screenPos.y < 1
		            || screenPos.y > Screen.Height - 1
		        )
		            return;

		        if ( pos.DistanceSquared( Scene.Camera.WorldPosition ) < 4096 )
		        {
		            var player = Scene
		                .GetAllComponents<PlayerController>()
		                .FirstOrDefault( p => p.GetCamera() == Scene.Camera );

		            if ( !player.IsValid() )
		                return;

		            if ( !MultiWorldSystem.AreObjectsInSameWorld( player.GameObject, GameObject ) )
		                return;

		            using ( Gizmo.Scope( "DhdSymbol", global::Transform.Zero ) )
		            {
		                if ( Action != "DIAL" && !Disabled )
		                {
		                    Gizmo.Draw.Color = Color.White;
		                    Gizmo.Draw.Text(
		                        Action,
		                        global::Transform.Zero.WithPosition( pos ),
		                        size: 32
		                    );
		                }
		            }
		        }
		    }
		}
		*/
	}
}

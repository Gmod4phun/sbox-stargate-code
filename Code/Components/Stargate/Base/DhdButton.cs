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

		TimeSince lastPressed;
		readonly float longPressDelay = 0.5f;

		public bool Press(IPressable.Event e)
		{
			lastPressed = 0;
			return true;
		}

		public bool Pressing(IPressable.Event e)
		{
			return lastPressed < longPressDelay;
		}

		public void Release(IPressable.Event e)
		{
			if (lastPressed < longPressDelay)
			{
				OnShortPress(e.Source.GameObject);
				lastPressed = 0;
				return;
			}

			OnLongPress();
			lastPressed = 0;
		}

		void OnShortPress(GameObject user)
		{
			if (Time.Now < DHD.LastPressTime + DHD.PressDelay)
				return;

			Network.TakeOwnership();
			DHD.Network.TakeOwnership();
			DHD.Gate?.Network.TakeOwnership();

			DHD.LastPressTime = Time.Now;
			DHD.TriggerAction(Action, user);
		}

		void OnLongPress()
		{
			if (Action == "DIAL")
			{
				ToggleGateListPanel();
			}
		}

		void ToggleGateListPanel()
		{
			var dhdGateList = DHD.Components.Get<DhdGateList>(FindMode.EverythingInDescendants);
			if (dhdGateList.IsValid() && dhdGateList.GameObject.IsValid())
			{
				dhdGateList.GameObject.Enabled = !dhdGateList.GameObject.Enabled;

				if (!dhdGateList.GameObject.Enabled)
				{
					DHD.SetDialGuideState(false);
				}
			}
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

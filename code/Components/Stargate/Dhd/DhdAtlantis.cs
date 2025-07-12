namespace Sandbox.Components.Stargate
{
	public partial class DhdAtlantis : Dhd
	{
		protected override string ButtonSymbols => "ABCDEFGHIJKLMNOPQRST123456789UVW0XYZ";

		protected override Dictionary<string, Vector3> ButtonPositions =>
			new()
			{
				["A"] = new Vector3(-7.7f, -4.893f, 37.8f),
				["B"] = new Vector3(-7.7f, -1.358f, 37.8f),
				["C"] = new Vector3(-7.7f, 2.177f, 37.8f),
				["D"] = new Vector3(-7.7f, 5.711f, 37.8f),
				["E"] = new Vector3(-7.7f, 9.246f, 37.8f),

				["F"] = new Vector3(-2.488f, -8.425f, 37.8f),
				["G"] = new Vector3(-2.488f, -4.893f, 37.8f),
				["H"] = new Vector3(-2.488f, -1.358f, 37.8f),
				["I"] = new Vector3(-2.488f, 2.177f, 37.8f),
				["J"] = new Vector3(-2.488f, 5.711f, 37.8f),
				["K"] = new Vector3(-2.488f, 9.246f, 37.8f),
				["L"] = new Vector3(-2.488f, 12.784f, 37.8f),

				["M"] = new Vector3(3.467f, -11.959f, 37.8f),
				["N"] = new Vector3(3.467f, -8.425f, 37.8f),
				["O"] = new Vector3(3.467f, -4.893f, 37.8f),
				["P"] = new Vector3(3.467f, -1.358f, 37.8f),
				["#"] = new Vector3(3.467f, 2.177f, 37.8f),
				["Q"] = new Vector3(3.467f, 5.711f, 37.8f),
				["R"] = new Vector3(3.467f, 9.246f, 37.8f),
				["S"] = new Vector3(3.467f, 12.784f, 37.8f),
				["T"] = new Vector3(3.467f, 16.314f, 37.8f),

				["1"] = new Vector3(8.403f, -11.959f, 37.8f),
				["2"] = new Vector3(8.403f, -8.425f, 37.8f),
				["3"] = new Vector3(8.403f, -4.893f, 37.8f),
				["4"] = new Vector3(8.403f, -1.358f, 37.8f),
				["5"] = new Vector3(8.403f, 2.177f, 37.8f),
				["6"] = new Vector3(8.403f, 5.711f, 37.8f),
				["7"] = new Vector3(8.403f, 9.246f, 37.8f),
				["8"] = new Vector3(8.403f, 12.784f, 37.8f),
				["9"] = new Vector3(8.403f, 16.314f, 37.8f),

				["U"] = new Vector3(14.292f, -8.425f, 37.8f),
				["V"] = new Vector3(14.292f, -4.893f, 37.8f),
				["W"] = new Vector3(14.292f, -1.358f, 37.8f),
				["0"] = new Vector3(14.292f, 2.177f, 37.8f),
				["X"] = new Vector3(14.292f, 5.711f, 37.8f),
				["Y"] = new Vector3(14.292f, 9.246f, 37.8f),
				["Z"] = new Vector3(14.292f, 12.784f, 37.8f),

				["*"] = new Vector3(-8.8f, 20.5f, 37.3f),
				["@"] = new Vector3(-4.2f, 20.5f, 37.3f),

				["IRIS"] = new Vector3(9.5f, -22.6f, 35.1f),
				["INSTANT"] = new Vector3(4.8f, -25f, 35.1f),

				["FAST"] = new Vector3(-9f, -29f, 37.4f),
				["SLOW"] = new Vector3(-9f, -16f, 37.4f),
			};

		protected override Vector3 ButtonPositionsOffset => new(0, 0, -0.5f);

		public DhdAtlantis()
		{
			Data = new("peg", "dhd.atlantis.press", "dhd.press_dial");
			DialIsLock = true;
		}

		public void CreateSingleButton(
			string model,
			string action,
			bool disabled = false,
			Transform? localTransformOverride = null,
			int glyphBodyGroup = 0
		) // visible model of buttons that turn on/off and animate
		{
			var button_object = new GameObject();
			button_object.Name = $"Button ({action})";
			button_object.Transform.World = GameObject.Transform.World;
			button_object.SetParent(GameObject);

			if (localTransformOverride == null)
			{
				localTransformOverride = new Transform();
			}

			button_object.Transform.Local = (Transform)localTransformOverride;

			var button_component = button_object.Components.Create<DhdButton>();
			var renderer = button_object.Components.Create<Superglyph>();
			renderer.Model = Model.Load(model);
			renderer.SetBodyGroup("glyph", glyphBodyGroup);

			renderer.GlyphEnabled = false;
			renderer.BrightnessTimeDelta = 12;

			if (action.Length == 1)
			{
				renderer.GlyphNumber = StargateRingPegasus.RingSymbols.IndexOf(action);
			}

			var collider = button_object.Components.Create<ModelCollider>();
			collider.Model = renderer.Model;
			renderer.Enabled = false;
			renderer.Enabled = true;

			button_component.Action = action;
			button_component.Disabled = disabled;
		}

		public override void CreateButtons() // visible models of buttons that turn on/off and animate
		{
			// SYMBOL BUTTONS
			var center = global::Transform.Zero.WithPosition(new Vector3(3.235f, 2.18f, 37.45f));
			var mdl = "models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_dynamic.vmdl";

			var stepHorizontal = 3.6f;
			var stepVertical = 5.6f;

			var symbolIndex = 0;
			int[] horizontalOffsets = new[] { 0, 1, 2, 2, 1 };
			for (var iVertical = -2; iVertical <= 2; iVertical++)
			{
				var horizontalOffset = horizontalOffsets[iVertical + 2];
				for (
					var iHorizontal = -2 - horizontalOffset;
					iHorizontal <= 2 + horizontalOffset;
					iHorizontal++
				)
				{
					if (iHorizontal == 0 && iVertical == 0)
						continue;

					var shouldRotate = symbolIndex % 2 != 0;
					if (iVertical == -1 || iVertical == 1 || (iVertical == 0 && iHorizontal > 0))
						shouldRotate = !shouldRotate;

					var action = ButtonSymbols[symbolIndex++].ToString();
					var buttonTransform = center
						.WithPosition(
							center.Position
								+ center.Rotation.Left * (stepHorizontal * iHorizontal)
								+ center.Rotation.Forward * (stepVertical * iVertical)
						)
						.WithRotation(
							center.Rotation.RotateAroundAxis(Vector3.Up, shouldRotate ? 180 : 0)
						);
					CreateSingleButton(
						mdl,
						action,
						localTransformOverride: buttonTransform,
						glyphBodyGroup: shouldRotate ? 2 : 1
					);
				}
			}

			// CENTER DIAL BUTTON
			// CreateSingleButton( "models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_37.vmdl", "DIAL" );
			CreateSingleButton(mdl, "DIAL", localTransformOverride: center);

			CreateSingleButton(
				"models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_extra_1.vmdl",
				"@"
			);
			CreateSingleButton(
				"models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_extra_2.vmdl",
				"*"
			);
			CreateSingleButton(
				"models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_extra_3.vmdl",
				"IRIS"
			);
			CreateSingleButton(
				"models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_extra_4.vmdl",
				"INSTANT"
			);
			CreateSingleButton(
				"models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_extra_5.vmdl",
				"FAST"
			);
			CreateSingleButton(
				"models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_extra_6.vmdl",
				"SLOW"
			);
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			if (DhdModel.IsValid() && DhdModel.SceneObject is SceneObject so)
			{
				so.Attributes.Set("selfillumscale", 1);
			}
		}
	}
}

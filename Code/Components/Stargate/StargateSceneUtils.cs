using Sandbox.Components.Stargate.Rings;

namespace Sandbox.Components.Stargate
{
	public static class StargateSceneUtils
	{
		public static readonly int[] ChevronAngles = { 40, 80, 120, 240, 280, 320, 0, 160, 200 };

		// Gates
		public static Stargate SpawnGateMilkyWay(Vector3 pos, Rotation rot)
		{
			var gate = new GameObject();
			gate.Name = "Stargate (Milky Way)";
			gate.WorldPosition = pos + Vector3.Up * 90;
			gate.WorldRotation = rot;

			var mdl = gate.Components.Create<ModelRenderer>();
			mdl.Model = Model.Load("models/sbox_stargate/sg_mw/sg_mw_gate.vmdl");

			var phy = gate.Components.Create<ModelCollider>();
			phy.Model = mdl.Model;

			var gate_component = gate.Components.Create<StargateMilkyWay>();

			for (var i = 0; i < 9; i++)
			{
				var chev = new GameObject();
				chev.Name = $"Chevron {i + 1}";
				chev.SetParent(gate);
				chev.WorldPosition = gate.WorldPosition;
				chev.WorldRotation = gate.WorldRotation;
				chev.LocalRotation = chev.LocalRotation.RotateAroundAxis(
					chev.LocalRotation.Forward,
					-ChevronAngles[i]
				);

				var chev_component = chev.Components.Create<Chevron>();
				chev_component.Number = i + 1;
				chev_component.ChevronModel = chev.Components.Create<SkinnedModelRenderer>();
				chev_component.ChevronModel.Model = Model.Load(
					"models/sbox_stargate/sg_mw/sg_mw_chevron.vmdl"
				);
				gate_component.Chevrons.Add(chev_component);

				var att = chev_component.ChevronModel.GetAttachment("light");
				if (att is Transform t)
				{
					var chev_light = new GameObject();
					chev_light.Name = "Chevron Light";
					chev_light.SetParent(chev);
					chev_light.WorldPosition = t.Position;
					chev_light.WorldRotation = t.Rotation;

					chev_component.ChevronLight = chev_light.Components.Create<PointLight>();
					chev_component.ChevronLight.LightColor = Color
						.Parse("#FF6A00")
						.GetValueOrDefault();
					chev_component.ChevronLight.Radius = 12f;
					chev_component.ChevronLight.Enabled = false;
				}
			}

			var ring = new GameObject();
			ring.Name = "Ring";
			ring.SetParent(gate, false);

			var ring_component = ring.Components.Create<StargateRingMilkyWay>();
			ring_component.RingModel = ring_component.Components.Create<ModelRenderer>();
			ring_component.RingModel.Model = Model.Load(
				"models/sbox_stargate/sg_mw/sg_mw_ring.vmdl"
			);

			return gate_component;
		}

		public static Stargate SpawnGateMovie(Vector3 pos, Rotation rot)
		{
			var gate = new GameObject();
			gate.Name = "Stargate (Movie)";
			gate.WorldPosition = pos + Vector3.Up * 90;
			gate.WorldRotation = rot;

			var mdl = gate.Components.Create<ModelRenderer>();
			mdl.Model = Model.Load("models/sbox_stargate/sg_mw/sg_mw_gate.vmdl");

			var phy = gate.Components.Create<ModelCollider>();
			phy.Model = mdl.Model;

			var gate_component = gate.Components.Create<StargateMovie>();

			for (var i = 0; i < 9; i++)
			{
				var chev = new GameObject();
				chev.Name = $"Chevron {i + 1}";
				chev.SetParent(gate);
				chev.WorldPosition = gate.WorldPosition;
				chev.WorldRotation = gate.WorldRotation;
				chev.LocalRotation = chev.LocalRotation.RotateAroundAxis(
					chev.LocalRotation.Forward,
					-ChevronAngles[i]
				);

				var chev_component = chev.Components.Create<Chevron>();
				chev_component.Number = i + 1;
				chev_component.ChevronModel = chev.Components.Create<SkinnedModelRenderer>();
				chev_component.ChevronModel.Model = Model.Load(
					"models/sbox_stargate/sg_mw/sg_mw_chevron.vmdl"
				);
				gate_component.Chevrons.Add(chev_component);

				var att = chev_component.ChevronModel.GetAttachment("light");
				if (att is Transform t)
				{
					var chev_light = new GameObject();
					chev_light.Name = "Chevron Light";
					chev_light.SetParent(chev);
					chev_light.WorldPosition = t.Position;
					chev_light.WorldRotation = t.Rotation;

					chev_component.ChevronLight = chev_light.Components.Create<PointLight>();
					chev_component.ChevronLight.LightColor = Color
						.Parse("#FF6A00")
						.GetValueOrDefault();
					chev_component.ChevronLight.Radius = 12f;
					chev_component.ChevronLight.Enabled = false;
				}
			}

			var ring = new GameObject();
			ring.Name = "Ring";
			ring.SetParent(gate, false);

			var ring_component = ring.Components.Create<StargateRingMovie>();
			ring_component.RingModel = ring_component.Components.Create<ModelRenderer>();
			ring_component.RingModel.Model = Model.Load(
				"models/sbox_stargate/sg_mw/sg_mw_ring.vmdl"
			);

			return gate_component;
		}

		public static Stargate SpawnGatePegasus(Vector3 pos, Rotation rot)
		{
			var gate = new GameObject();
			gate.Name = "Stargate (Pegasus)";
			gate.WorldPosition = pos + Vector3.Up * 90;
			gate.WorldRotation = rot;

			var mdl = gate.Components.Create<ModelRenderer>();
			mdl.Model = Model.Load("models/sbox_stargate/sg_peg/sg_peg_gate.vmdl");

			var phy = gate.Components.Create<ModelCollider>();
			phy.Model = mdl.Model;

			var gate_component = gate.Components.Create<StargatePegasus>();

			for (var i = 0; i < 9; i++)
			{
				var chev = new GameObject();
				chev.Name = $"Chevron {i + 1}";
				chev.SetParent(gate);
				chev.WorldPosition = gate.WorldPosition;
				chev.WorldRotation = gate.WorldRotation;
				chev.LocalRotation = chev.LocalRotation.RotateAroundAxis(
					chev.LocalRotation.Forward,
					-ChevronAngles[i]
				);

				var chev_component = chev.Components.Create<Chevron>();
				chev_component.Number = i + 1;
				chev_component.ChevronModel = chev.Components.Create<SkinnedModelRenderer>();
				chev_component.ChevronModel.Model = Model.Load(
					"models/sbox_stargate/sg_peg/sg_peg_chevron.vmdl"
				);
				chev_component.ChevronModel.Enabled = false;
				chev_component.ChevronModel.Enabled = true;
				gate_component.Chevrons.Add(chev_component);

				var att = chev_component.ChevronModel.GetAttachment("light");
				if (att is Transform t)
				{
					var chev_light = new GameObject();
					chev_light.Name = "Chevron Light";
					chev_light.SetParent(chev);
					chev_light.WorldPosition = t.Position;
					chev_light.WorldRotation = t.Rotation;

					chev_component.ChevronLight = chev_light.Components.Create<PointLight>();
					chev_component.ChevronLight.LightColor = Color
						.Parse("#FF6A00")
						.GetValueOrDefault();
					chev_component.ChevronLight.Radius = 12f;
					chev_component.ChevronLight.Enabled = false;
				}
			}

			var ring = new GameObject();
			ring.Name = "Ring";
			ring.SetParent(gate, false);

			var ring_component = ring.Components.Create<StargateRingPegasus>();
			ring_component.RingModel = ring_component.Components.Create<ModelRenderer>();
			ring_component.RingModel.Model = Model.Load(
				"models/sbox_stargate/sg_peg/sg_peg_ring.vmdl"
			);
			ring_component.RingModel.Enabled = false;
			ring_component.RingModel.Enabled = true;

			// var sym_part1 = ring.Components.Create<SkinnedModelRenderer>();
			// sym_part1.Model = Model.Load( "models/sbox_stargate/sg_peg/sg_peg_glyphs_1_18.vmdl" );
			// ring_component.SymbolParts.Add( sym_part1 );

			// var sym_part2 = ring.Components.Create<SkinnedModelRenderer>();
			// sym_part2.Model = Model.Load( "models/sbox_stargate/sg_peg/sg_peg_glyphs_19_36.vmdl" );
			// ring_component.SymbolParts.Add( sym_part2 );

			ring_component.CreateGlyphs();
			ring_component.ResetSymbols();
			// ring_component.SetRingState( true );

			return gate_component;
		}

		public static Stargate SpawnGateUniverse(Vector3 pos, Rotation rot)
		{
			var gate = new GameObject();
			gate.Name = "Stargate (Universe)";
			gate.WorldPosition = pos + Vector3.Up * 90;
			gate.WorldRotation = rot;

			var phy = gate.Components.Create<ModelCollider>();
			phy.Model = Model.Load("models/sbox_stargate/gate_universe/gate_universe.vmdl");

			var gate_component = gate.Components.Create<StargateUniverse>();

			var ring = new GameObject();
			ring.Name = "Ring";
			ring.SetParent(gate, false);

			var ring_component = ring.Components.Create<StargateRingUniverse>();
			ring_component.RingModel = ring_component.Components.Create<ModelRenderer>();
			ring_component.RingModel.Model = Model.Load(
				"models/sbox_stargate/gate_universe/gate_universe.vmdl"
			);

			var sym_part1 = ring.Components.Create<SkinnedModelRenderer>();
			sym_part1.Model = Model.Load(
				"models/sbox_stargate/gate_universe/gate_universe_symbols_1_18.vmdl"
			);
			// ring_component.SymbolParts.Add( sym_part1 );

			var sym_part2 = ring.Components.Create<SkinnedModelRenderer>();
			sym_part2.Model = Model.Load(
				"models/sbox_stargate/gate_universe/gate_universe_symbols_19_36.vmdl"
			);
			// ring_component.SymbolParts.Add( sym_part2 );

			ring_component.ResetSymbols();

			var chev = new GameObject();
			chev.Name = "Chevrons";
			chev.SetParent(ring);
			chev.WorldPosition = ring.WorldPosition;
			chev.WorldRotation = ring.WorldRotation;

			var chev_component = chev.Components.Create<Chevron>();
			chev_component.Number = 1;
			chev_component.ChevronModel = chev.Components.Create<SkinnedModelRenderer>();
			chev_component.ChevronModel.Model = Model.Load(
				"models/sbox_stargate/gate_universe/chevrons_universe.vmdl"
			);
			gate_component.Chevrons.Add(chev_component);

			return gate_component;
		}

		// Gate Prefabs
		public static Stargate SpawnGatePrefab(Vector3 pos, Rotation rot, string prefabPath)
		{
			try
			{
				var go = SceneUtility
					.GetPrefabScene(ResourceLibrary.Get<PrefabFile>(prefabPath))
					.Clone(pos + Vector3.Up * 90, rot);
				go.BreakFromPrefab();

				var gate = go.Components.Get<Stargate>();
				gate.GateAddress = Stargate.GenerateGateAddress(gate.GateGroup);
				return gate;
			}
			catch
			{
				Log.Warning("Problem creating stargate from prefab");
				return null;
			}
		}

		// DHD's
		public static void SpawnDhdMilkyWay(Vector3 pos, Rotation rot)
		{
			var dhd_object = new GameObject();
			dhd_object.Name = "Dhd (Milky Way)";
			dhd_object.WorldPosition = pos + new Vector3(0, 0, -3);
			dhd_object.WorldRotation = rot.RotateAroundAxis(Vector3.Right, -15);

			var dhd_component = dhd_object.Components.Create<DhdMilkyWay>();
			var renderer = dhd_component.Components.Create<SkinnedModelRenderer>();
			renderer.Model = Model.Load("models/sbox_stargate/dhd/dhd.vmdl");
			var collider = dhd_component.Components.Create<ModelCollider>();
			collider.Model = renderer.Model;

			dhd_component.CreateButtons();
		}

		public static void SpawnDhdPegasus(Vector3 pos, Rotation rot)
		{
			var dhd_object = new GameObject();
			dhd_object.Name = "Dhd (Pegasus)";
			dhd_object.WorldPosition = pos + new Vector3(0, 0, -3);
			dhd_object.WorldRotation = rot.RotateAroundAxis(Vector3.Right, -15);

			var dhd_component = dhd_object.Components.Create<DhdPegasus>();
			var renderer = dhd_component.Components.Create<SkinnedModelRenderer>();
			renderer.Model = Model.Load("models/sbox_stargate/dhd/dhd.vmdl");
			renderer.MaterialGroup = "peg";
			var collider = dhd_component.Components.Create<ModelCollider>();
			collider.Model = renderer.Model;

			dhd_component.CreateButtons();
		}

		public static void SpawnDhdAtlantis(Vector3 pos, Rotation rot)
		{
			var dhd_object = new GameObject();
			dhd_object.Name = "Dhd (Atlantis)";
			dhd_object.WorldPosition = pos;
			dhd_object.WorldRotation = rot;

			var dhd_component = dhd_object.Components.Create<DhdAtlantis>();
			var renderer = dhd_component.Components.Create<SkinnedModelRenderer>(false);
			renderer.Model = Model.Load("models/sbox_stargate/dhd_atlantis/dhd_atlantis.vmdl");
			renderer.Enabled = true;
			var collider = dhd_component.Components.Create<ModelCollider>();
			collider.Model = renderer.Model;

			dhd_component.CreateButtons();
		}

		// DHD Prefabs
		public static Dhd SpawnDhdPrefab(Vector3 pos, Rotation rot, string prefabPath)
		{
			try
			{
				var go = SceneUtility
					.GetPrefabScene(ResourceLibrary.Get<PrefabFile>(prefabPath))
					.Clone(pos, rot);
				go.BreakFromPrefab();

				var dhd = go.Components.Get<Dhd>();
				return dhd;
			}
			catch
			{
				Log.Warning("Problem creating dhd from prefab");
				return null;
			}
		}
	}
}

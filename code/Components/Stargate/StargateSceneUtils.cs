using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Components.Stargate
{
	public static class StargateSceneUtils
	{
		public static readonly int[] ChevronAngles = { 40, 80, 120, 240, 280, 320, 0, 160, 200 };

		public static Transform ToTransform(this GameTransform gt) {
			return Transform.Zero.WithPosition(gt.Position).WithRotation(gt.Rotation).WithScale(gt.Scale);
		}

		public static void SpawnGateMilkyWay(Vector3 pos, Rotation rot)
		{
			var gate = new GameObject();
			gate.Name = "Stargate (Milky Way)";
			gate.Transform.Position = pos;
			gate.Transform.Rotation = rot;

			var mdl = gate.Components.Create<ModelRenderer>();
			mdl.Model = Model.Load( "models/sbox_stargate/sg_mw/sg_mw_gate.vmdl" );

			var phy = gate.Components.Create<ModelCollider>();
			phy.Model = mdl.Model;

			var gate_component = gate.Components.Create<StargateMilkyWay>();

			for ( var i = 0; i < 9; i++ )
			{
				var chev = new GameObject();
				chev.Name = $"Chevron {i + 1}";
				chev.SetParent( gate );
				chev.Transform.Position = gate.Transform.Position;
				chev.Transform.Rotation = gate.Transform.Rotation;
				chev.Transform.LocalRotation = chev.Transform.LocalRotation.RotateAroundAxis( chev.Transform.LocalRotation.Forward, -ChevronAngles[i] );

				var chev_component = chev.Components.Create<Chevron>();
				chev_component.Number = i + 1;
				chev_component.Gate = gate.Components.Get<Stargate>();
				chev_component.ChevronModel = chev.Components.Create<SkinnedModelRenderer>();
				chev_component.ChevronModel.Model = Model.Load( "models/sbox_stargate/sg_mw/sg_mw_chevron.vmdl" );
				gate_component.Chevrons.Add(chev_component);

				var att = chev_component.ChevronModel.GetAttachment( "light" );
				if ( att is Transform t ) {
					var chev_light = new GameObject();
					chev_light.Name = "Chevron Light";
					chev_light.SetParent( chev );
					chev_light.Transform.Position = t.Position;
					chev_light.Transform.Rotation = t.Rotation;

					chev_component.ChevronLight = chev_light.Components.Create<PointLight>();
					chev_component.ChevronLight.LightColor = Color.Parse( "#FF6A00" ).GetValueOrDefault();
					chev_component.ChevronLight.Radius = 12f;
					chev_component.ChevronLight.Enabled = false;
				}
			}

			var ring = new GameObject();
			ring.Name = "Ring";
			ring.SetParent( gate, false );

			var ring_component = ring.Components.Create<StargateRingMilkyWay>();
			ring_component.RingModel = ring_component.Components.Create<ModelRenderer>();
			ring_component.RingModel.Model = Model.Load( "models/sbox_stargate/sg_mw/sg_mw_ring.vmdl" );
			gate_component.Ring = ring_component;
			ring_component.Gate = gate_component;
		}

		public static void SpawnGateMovie(Vector3 pos, Rotation rot)
		{
			var gate = new GameObject();
			gate.Name = "Stargate (Movie)";
			gate.Transform.Position = pos;
			gate.Transform.Rotation = rot;

			var mdl = gate.Components.Create<ModelRenderer>();
			mdl.Model = Model.Load( "models/sbox_stargate/sg_mw/sg_mw_gate.vmdl" );

			var phy = gate.Components.Create<ModelCollider>();
			phy.Model = mdl.Model;

			var gate_component = gate.Components.Create<StargateMovie>();

			for ( var i = 0; i < 9; i++ )
			{
				var chev = new GameObject();
				chev.Name = $"Chevron {i + 1}";
				chev.SetParent( gate );
				chev.Transform.Position = gate.Transform.Position;
				chev.Transform.Rotation = gate.Transform.Rotation;
				chev.Transform.LocalRotation = chev.Transform.LocalRotation.RotateAroundAxis( chev.Transform.LocalRotation.Forward, -ChevronAngles[i] );

				var chev_component = chev.Components.Create<Chevron>();
				chev_component.Number = i + 1;
				chev_component.Gate = gate.Components.Get<Stargate>();
				chev_component.ChevronModel = chev.Components.Create<SkinnedModelRenderer>();
				chev_component.ChevronModel.Model = Model.Load( "models/sbox_stargate/sg_mw/sg_mw_chevron.vmdl" );
				gate_component.Chevrons.Add(chev_component);

				var att = chev_component.ChevronModel.GetAttachment( "light" );
				if ( att is Transform t ) {
					var chev_light = new GameObject();
					chev_light.Name = "Chevron Light";
					chev_light.SetParent( chev );
					chev_light.Transform.Position = t.Position;
					chev_light.Transform.Rotation = t.Rotation;

					chev_component.ChevronLight = chev_light.Components.Create<PointLight>();
					chev_component.ChevronLight.LightColor = Color.Parse( "#FF6A00" ).GetValueOrDefault();
					chev_component.ChevronLight.Radius = 12f;
					chev_component.ChevronLight.Enabled = false;
				}
			}

			var ring = new GameObject();
			ring.Name = "Ring";
			ring.SetParent( gate, false );

			var ring_component = ring.Components.Create<StargateRingMovie>();
			ring_component.RingModel = ring_component.Components.Create<ModelRenderer>();
			ring_component.RingModel.Model = Model.Load( "models/sbox_stargate/sg_mw/sg_mw_ring.vmdl" );
			gate_component.Ring = ring_component;
			ring_component.Gate = gate_component;
		}

		public static void SpawnGatePegasus( Vector3 pos, Rotation rot )
		{
			var gate = new GameObject();
			gate.Name = "Stargate (Pegasus)";
			gate.Transform.Position = pos;
			gate.Transform.Rotation = rot;

			var mdl = gate.Components.Create<ModelRenderer>();
			mdl.Model = Model.Load( "models/sbox_stargate/sg_peg/sg_peg_gate.vmdl" );

			var phy = gate.Components.Create<ModelCollider>();
			phy.Model = mdl.Model;

			var gate_component = gate.Components.Create<StargatePegasus>();

			for ( var i = 0; i < 9; i++ )
			{
				var chev = new GameObject();
				chev.Name = $"Chevron {i + 1}";
				chev.SetParent( gate );
				chev.Transform.Position = gate.Transform.Position;
				chev.Transform.Rotation = gate.Transform.Rotation;
				chev.Transform.LocalRotation = chev.Transform.LocalRotation.RotateAroundAxis( chev.Transform.LocalRotation.Forward, -ChevronAngles[i] );

				var chev_component = chev.Components.Create<Chevron>();
				chev_component.Number = i + 1;
				chev_component.Gate = gate.Components.Get<Stargate>();
				chev_component.ChevronModel = chev.Components.Create<SkinnedModelRenderer>();
				chev_component.ChevronModel.Model = Model.Load( "models/sbox_stargate/sg_peg/sg_peg_chevron.vmdl" );
				chev_component.ChevronModel.Enabled = false;
				chev_component.ChevronModel.Enabled = true;
				gate_component.Chevrons.Add(chev_component);

				var att = chev_component.ChevronModel.GetAttachment( "light" );
				if ( att is Transform t )
				{
					var chev_light = new GameObject();
					chev_light.Name = "Chevron Light";
					chev_light.SetParent( chev );
					chev_light.Transform.Position = t.Position;
					chev_light.Transform.Rotation = t.Rotation;

					chev_component.ChevronLight = chev_light.Components.Create<PointLight>();
					chev_component.ChevronLight.LightColor = Color.Parse( "#FF6A00" ).GetValueOrDefault();
					chev_component.ChevronLight.Radius = 12f;
					chev_component.ChevronLight.Enabled = false;
				}
			}

			var ring = new GameObject();
			ring.Name = "Ring";
			ring.SetParent( gate, false );

			var ring_component = ring.Components.Create<StargateRingPegasus>();
			ring_component.RingModel = ring_component.Components.Create<ModelRenderer>();
			ring_component.RingModel.Model = Model.Load( "models/sbox_stargate/sg_peg/sg_peg_ring.vmdl" );
			ring_component.RingModel.Enabled = false;
			ring_component.RingModel.Enabled = true;
			// gate_component.Ring = ring_component;
			// ring_component.Gate = gate_component;

			var sym_part1 = ring.Components.Create<SkinnedModelRenderer>();
			sym_part1.Model = Model.Load( "models/sbox_stargate/sg_peg/sg_peg_glyphs_1_18.vmdl" );
			ring_component.SymbolParts.Add(sym_part1);

			var sym_part2 = ring.Components.Create<SkinnedModelRenderer>();
			sym_part2.Model = Model.Load( "models/sbox_stargate/sg_peg/sg_peg_glyphs_19_36.vmdl" );
			ring_component.SymbolParts.Add(sym_part2);

			ring_component.ResetSymbols();
			ring_component.SetRingState(true);
		}
	}
}

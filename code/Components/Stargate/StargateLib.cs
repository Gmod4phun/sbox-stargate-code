﻿using System.Runtime.CompilerServices;
using System.Text;
using Sandbox.Components.Stargate.Ramps;

namespace Sandbox.Components.Stargate
{
	public partial class Stargate : Component
	{
		public const string Symbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789*#@"; // symbol * (Aquila) isnt on the DHD, so gates using that symbol cant be dialed without a computer/other means
		public const string SymbolsForAddress = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789*";
		public const string SymbolsForGroup = "@";
		public const char PointOfOrigin = '#';

		public const int AutoCloseTimerDuration = 5;

		public readonly int[] ChevronAngles = { 40, 80, 120, 240, 280, 320, 0, 160, 200 };

		private List<TimedTask> StargateActions = new();

		public enum DialType
		{
			SLOW,
			FAST,
			INSTANT,
			NOX,
			DHD,
			MANUAL
		}

		public enum GateState
		{
			IDLE,
			ACTIVE,
			DIALING,
			OPENING,
			OPEN,
			CLOSING
		}

		public enum GlyphType
		{
			MILKYWAY,
			PEGASUS,
			UNIVERSE
		}

		public enum TimedTaskCategory
		{
			GENERIC,
			DIALING,
			SYMBOL_ROLL_PEGASUS_DHD,
			SET_BUSY
		}

		// Utility funcs

		/// <summary>
		/// Generates a random 2 symbol Gate Group.
		/// </summary>
		/// <returns>Gate Group.</returns>
		public static string GenerateGateGroup()
		{
			StringBuilder symbolsCopy = new(SymbolsForAddress + SymbolsForGroup);

			string generatedGroup = "";
			for (int i = 0; i < 2; i++) // pick random symbols without repeating
			{
				var randomIndex = new Random().Int(0, symbolsCopy.Length - 1);
				generatedGroup += symbolsCopy[randomIndex];

				symbolsCopy = symbolsCopy.Remove(randomIndex, 1);
			}

			return generatedGroup;
		}

		/// <summary>
		/// Generates a random 6 symbol Gate Address, excluding characters from a Gate Group.
		/// </summary>
		/// <returns>Gate Adress.</returns>
		public static string GenerateGateAddress(string excludeGroup)
		{
			StringBuilder symbolsCopy = new(SymbolsForAddress);

			foreach (var c in excludeGroup) // remove group chars from symbols
			{
				if (symbolsCopy.ToString().Contains(c))
					symbolsCopy = symbolsCopy.Remove(symbolsCopy.ToString().IndexOf(c), 1);
			}

			string generatedAddress = "";
			for (int i = 0; i < 6; i++) // pick random symbols without repeating
			{
				var randomIndex = new Random().Int(0, symbolsCopy.Length - 1);
				generatedAddress += symbolsCopy[randomIndex];

				symbolsCopy = symbolsCopy.Remove(randomIndex, 1);
			}

			return generatedAddress;
		}

		public static string GetFullGateAddress(Stargate gate, bool only8chev = false)
		{
			if (!only8chev)
				return gate.GateAddress + gate.GateGroup + PointOfOrigin;

			return gate.GateAddress + gate.GateGroup[0] + PointOfOrigin;
		}

		/// <summary>
		/// Checks if the format of the input string is that of a valid full Stargate Address (Address + Group + Point of Origin).
		/// </summary>
		/// <param name="address">The gate address represented in the string.</param>
		/// <returns>True or False</returns>
		public static bool IsValidFullAddress(string address) // a valid address has an address, a group and a point of origin, with no repeating symbols
		{
			if (address.Length < 7 || address.Length > 9)
				return false; // only 7, 8 or 9 symbol addresses

			foreach (char sym in address)
			{
				if (!Symbols.Contains(sym))
					return false; // only valid symbols
				if (address.Count(c => c == sym) > 1)
					return false; // only one occurence
			}

			if (!address.EndsWith(PointOfOrigin))
				return false; // must end with point of origin

			return true;
		}

		/// <summary>
		/// Checks if the format of the input string is that of a valid Stargate Address (6 non-repeating valid symbols).
		/// </summary>
		/// <param name="address">The gate address represented in the string.</param>
		/// <returns>True or False</returns>
		public static bool IsValidAddressOnly(string address)
		{
			if (address.Length != 6)
				return false; // only 7, 8 or 9 symbol addresses

			foreach (char sym in address)
			{
				if (!SymbolsForAddress.Contains(sym))
					return false; // only valid symbols
				if (address.Count(c => c == sym) > 1)
					return false; // only one occurence
			}

			return true;
		}

		/// <summary>
		/// Checks if the format of the input string is that of a valid Stargate Group.
		/// </summary>
		/// <param name="group">The gate group represented in the string.</param>
		/// <returns>True or False</returns>
		public static bool IsValidGroup(string group) // a valid address has an address, a group and a point of origin, with no repeating symbols
		{
			if (group.Length != 2)
				return false; // only 2 symbol groups

			var validSyms = SymbolsForAddress + SymbolsForGroup;

			foreach (char sym in group)
			{
				if (!validSyms.Contains(sym))
					return false; // only valid symbols
				if (group.Count(c => c == sym) > 1)
					return false; // only one occurence
			}
			return true;
		}

		public static bool IsUniverseGate(Stargate gate)
		{
			if (!gate.IsValid())
				return false;
			return gate is StargateUniverse;
		}

		public static Stargate FindDestinationGateByDialingAddress(Stargate gate, string address)
		{
			var addrLen = address.Length;
			var otherAddress = address.Substring(0, 6);
			var otherGroup =
				(addrLen == 9)
					? address.Substring(6, 2)
					: (addrLen == 8)
						? address.Substring(6, 1)
						: "";

			Stargate target = null;

			if (addrLen == 9) // 9 chevron connection - universe connection
			{
				target = FindByFullAddress(address);
				if (target.IsValid())
				{
					// cant have 9 chevron connection between 2 universe or 2 non-universe gates
					if (!IsUniverseGate(gate) && !IsUniverseGate(target))
						target = null;
					if (IsUniverseGate(gate) && IsUniverseGate(target))
						target = null;

					if (gate.GateLocal || target.IsValid() && target.GateLocal)
						target = null;
				}
			}
			else if (addrLen == 8) // 8 chevron connection - different group
			{
				if (otherGroup[0] != gate.GateGroup[0])
					target = FindByAddress8Chev(address);
				if (target.IsValid())
				{
					if (gate.GateLocal || target.GateLocal)
						target = null;
					if (IsUniverseGate(gate) || IsUniverseGate(target))
						target = null; // make it invalid if for some reason we got a universe gate
				}
			}
			else // classic 7 chevron connection - must have same group, unless both are universe, they always use 7 symbols
			{
				target = FindByAddressAndGroup(address, gate.GateGroup);
				if (!IsUniverseGate(gate) && !IsUniverseGate(target)) // both arent universe gates
				{
					if (target.IsValid() && target.GateGroup != gate.GateGroup)
						target = null; // if found gate does not have same group, its not valid
				}
				else if (IsUniverseGate(gate) != IsUniverseGate(target)) // only one of them is universe gate and the other is not
				{
					target = null;
				}
			}

			return target;
		}

		/// <summary>
		/// Returns the gate if it finds it by a specified full address.
		/// </summary>
		/// <param name="address">The full gate address represented in the string.</param>
		/// <returns>A gate that matches the parameter.</returns>
		public static Stargate FindByFullAddress(string address)
		{
			// foreach ( Stargate gate in Entity.All.OfType<Stargate>() )
			foreach (Stargate gate in Game.ActiveScene.GetAllComponents<Stargate>())
			{
				if (GetFullGateAddress(gate) == address)
					return gate;
			}
			return null;
		}

		/// <summary>
		/// Returns the gate if it finds it by a specified address.
		/// </summary>
		/// <param name="address">The gate address represented in the string.</param>
		/// /// <param name="group">The gate group represented in the string.</param>
		/// <returns>A gate that matches the parameter.</returns>
		public static Stargate FindByAddressAndGroup(string address, string group)
		{
			foreach (Stargate gate in Game.ActiveScene.GetAllComponents<Stargate>())
			{
				if (gate.GateAddress + PointOfOrigin == address && gate.GateGroup == group)
					return gate;
			}
			return null;
		}

		/// <summary>
		/// Returns the gate if it finds it by a specified address.
		/// </summary>
		/// <param name="address">The gate address represented in the string.</param>
		/// <returns>A gate that matches the parameter.</returns>
		public static Stargate FindByAddress8Chev(string address)
		{
			foreach (Stargate gate in Game.ActiveScene.GetAllComponents<Stargate>())
			{
				if (gate.GateAddress + gate.GateGroup[0] + PointOfOrigin == address)
					return gate;
			}
			return null;
		}

		public static string GetSelfAddressBasedOnOtherAddress(Stargate gate, string otherAddress)
		{
			if (otherAddress.Length == 7)
			{
				return gate.GateAddress + PointOfOrigin;
			}
			else if (otherAddress.Length == 8)
			{
				return gate.GateAddress + gate.GateGroup[0] + PointOfOrigin;
			}
			else if (otherAddress.Length == 9)
			{
				return GetFullGateAddress(gate);
			}

			return "";
		}

		public static string GetOtherGateAddressForMenu(Stargate gate, Stargate otherGate)
		{
			var finalAddress = "";

			// return empty string if either of the gates is null/not valid
			if (!gate.IsValid() || !otherGate.IsValid())
				return finalAddress;

			if (!IsUniverseGate(gate) && !IsUniverseGate(otherGate)) // none of them are universe gates
			{
				if (gate.GateGroup == otherGate.GateGroup) // if groups are equal, return address
				{
					finalAddress = otherGate.GateAddress + PointOfOrigin;
				}
				else // if groups arent equal, return addres with first group symbol
				{
					finalAddress = otherGate.GateAddress + otherGate.GateGroup[0] + PointOfOrigin;
				}
			}
			else // one or both are universe gates
			{
				if (IsUniverseGate(gate) && IsUniverseGate(otherGate)) // both are universe gates
				{
					if (gate.GateGroup == otherGate.GateGroup) // they have same gate group
					{
						finalAddress = otherGate.GateAddress + PointOfOrigin;
					}
					else
					{
						finalAddress = otherGate.GateAddress + PointOfOrigin;
					}
				}
				else // only one is universe gate
				{
					finalAddress = GetFullGateAddress(otherGate);
				}
			}

			return finalAddress;
		}

		/// <summary>
		/// Return the random gate.
		/// </summary>
		/// <returns>A gate that matches the parameter.</returns>
		public static Stargate FindRandomGate()
		{
			var allGates = Game.ActiveScene.GetAllComponents<Stargate>().ToList();

			return allGates.Count is 0 ? null : new Random().FromList(allGates);
		}

		/// <summary>
		/// Return the random gate, this gate will never be the gate given in the argument
		/// </summary>
		/// <param name="ent">A gate that is eliminated with a random outcome.</param>
		/// <returns>A gate that matches the parameter.</returns>
		public static Stargate FindRandomGate(Stargate ent)
		{
			var allGates = Game.ActiveScene.GetAllComponents<Stargate>().ToList();
			allGates.Remove(ent); // it will always be in the list, since it is a stargate

			return allGates.Count is 0 ? null : new Random().FromList(allGates);
		}

		/// <summary>
		/// It finds the nearest gate from the entity. It returns that gate.
		/// </summary>
		/// <param name="ent">The entity that will be the first point of remoteness.</param>
		/// <param name="maxDistance">The maximum distance. No limit by default.</param>
		/// <param name="sameWorld">Whether or not the gate should be in the same world as the GameObject.</param>
		/// <returns>A gate that matches the parameter.</returns>
		public static Stargate FindNearestGate(
			GameObject ent,
			float maxDistance = -1,
			bool sameWorld = false
		)
		{
			var allGates = Game
				.ActiveScene.GetAllComponents<Stargate>()
				.Where(g => !sameWorld || MultiWorldSystem.AreObjectsInSameWorld(g, ent))
				.ToList();
			if (allGates.Count() is 0)
				return null;

			var distances = new float[allGates.Count()];
			for (int i = 0; i < allGates.Count(); i++)
			{
				distances[i] = ent.Transform.Position.Distance(allGates[i].Transform.Position);
			}

			if (maxDistance > 0 && distances.Min() > maxDistance)
				return null;

			return allGates[distances.ToList().IndexOf(distances.Min())];
		}

		/// <summary>
		/// It finds the furthest gate from the entity that is in the argument. It returns that gate.
		/// </summary>
		/// <param name="ent">The entity that will be the first point of remoteness.</param>
		/// <param name="maxDistance">The maximum distance. No limit by default.</param>
		/// <returns>A gate that matches the parameter.</returns>
		public static Stargate FindFarthestGate(GameObject ent, float maxDistance = -1)
		{
			var allGates = Game
				.ActiveScene.GetAllComponents<Stargate>()
				.Where(g => MultiWorldSystem.AreObjectsInSameWorld(g, ent))
				.ToList();
			if (allGates.Count() is 0)
				return null;

			var distances = new float[allGates.Count()];
			for (int i = 0; i < allGates.Count(); i++)
				distances[i] = ent.Transform.Position.Distance(allGates[i].Transform.Position);

			if (maxDistance > 0 && distances.Min() > maxDistance)
				return null;

			return allGates[distances.ToList().IndexOf(distances.Max())];
		}

		/// <summary>
		/// Adds an Iris or Atlantis Gate Shield to the target Stargate if it does not have one yet.
		/// </summary>
		/// <returns>The just created, or already existing Iris.</returns>
		public static StargateIris AddIris(
			Stargate gate,
			StargateIris.IrisType irisType = StargateIris.IrisType.Standard
		)
		{
			if (!gate.HasIris())
			{
				var iris_object = new GameObject();
				iris_object.Name = "Iris";
				iris_object.Transform.Position = gate.Transform.Position;
				iris_object.Transform.Rotation = gate.Transform.Rotation;
				iris_object.Transform.Scale = gate.Transform.Scale;
				iris_object.SetParent(gate.GameObject);
				iris_object.Tags.Add("no_decal");

				var iris_component = irisType switch
				{
					StargateIris.IrisType.Atlantis
						=> iris_object.Components.Create<StargateIrisAtlantis>(),
					StargateIris.IrisType.Goauld
						=> iris_object.Components.Create<StargateIrisGoauld>(),
					_ => iris_object.Components.Create<StargateIris>()
				};

				iris_component.IrisModel = iris_object.Components.Create<SkinnedModelRenderer>();
				iris_component.IrisModel.Model = Model.Load(
					irisType switch
					{
						StargateIris.IrisType.Atlantis
							=> "models/sbox_stargate/iris_atlantis/iris_atlantis.vmdl",
						StargateIris.IrisType.Goauld
							=> "models/sbox_stargate/iris_goauld/iris_goauld.vmdl",
						_ => "models/sbox_stargate/iris/iris.vmdl"
					}
				);

				iris_component.IrisCollider = iris_object.Components.Create<ModelCollider>();
				iris_component.IrisCollider.Model = iris_component.IrisModel.Model;
				iris_component.IrisCollider.Enabled = false;

				if (irisType == StargateIris.IrisType.Goauld)
				{
					iris_component.IrisModel = null;
					var shield = iris_object.Components.Create<Shield>();
					shield.Radius = 32;
					shield.ShieldColor = Color.FromBytes(255, 100, 0);
					shield.Renderer.MaterialOverride = Material.Load(
						"materials/sbox_stargate/force_field_simple.vmat"
					);
					shield.ImpactMaterial = Material.Load(
						"materials/sbox_stargate/force_field_impact.vmat"
					);
				}
			}
			return gate.Iris;
		}

		/// <summary>
		/// Attempts to remove the Iris from the target Stargate.
		/// </summary>
		/// <returns>Whether or not the Iris was removed succesfully.</returns>
		public static bool RemoveIris(Stargate gate)
		{
			if (gate.HasIris())
			{
				gate.Iris.Destroy();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Adds a Gate Bearing to the target Universe Stargate if it does not have one yet.
		/// </summary>
		/// <returns>The just created, or already existing Bearing.</returns>
		public static GateBearing AddBearing(Stargate gate)
		{
			if (!gate.HasBearing() && gate is StargateUniverse)
			{
				var bearing_object = new GameObject();
				bearing_object.Name = "Bearing";
				bearing_object.Transform.Position =
					gate.Transform.Position + gate.Transform.Rotation.Up * 135.5f;
				bearing_object.Transform.Rotation = gate.Transform.Rotation;
				bearing_object.Transform.Scale = gate.Transform.Scale;
				bearing_object.SetParent(gate.GameObject);

				var bearing_component = bearing_object.Components.Create<GateBearing>();
				bearing_component.BearingModel =
					bearing_object.Components.Create<SkinnedModelRenderer>();
				bearing_component.BearingModel.Model = Model.Load(
					"models/sbox_stargate/universe_bearing/universe_bearing.vmdl"
				);
			}

			return gate.Bearing;
		}

		/// <summary>
		/// Attempts to remove the Bearing from the target Stargate.
		/// </summary>
		/// <returns>Whether or not the Bearing was removed succesfully.</returns>
		public static bool RemoveBearing(Stargate gate)
		{
			if (gate.HasBearing())
			{
				gate.Bearing.GameObject.Destroy();
				return true;
			}
			return false;
		}

		// sounds

		[Broadcast]
		public static void PlaySoundBroadcast(Guid gameObjectId, string name, float delay = 0)
		{
			var go = Game.ActiveScene.GetAllObjects(true).FirstOrDefault(x => x.Id == gameObjectId);
			if (go.IsValid())
			{
				PlaySound(go, name, delay);
			}
		}

		public static void PlaySound(Component comp, string name, float delay = 0)
		{
			PlaySound(comp.GameObject, name, delay);
		}

		public static async void PlaySound(GameObject gameObject, string name, float delay = 0)
		{
			if (delay > 0)
			{
				await GameTask.DelaySeconds(delay);
			}

			var worldIndex = MultiWorldSystem.GetWorldIndexOfObject(gameObject);
			MultiWorldSound.Play(name, gameObject.Transform.Position, worldIndex);
		}

		public static async void PlaySound(
			int worldIndex,
			Vector3 position,
			string name,
			float delay = 0
		)
		{
			if (delay > 0)
			{
				await GameTask.DelaySeconds(delay);
			}

			MultiWorldSound.Play(name, position, worldIndex);
		}

		public static MultiWorldSound PlayFollowingSound(GameObject followObject, string name)
		{
			return MultiWorldSound.Play(name, followObject, true);
		}

		/// <summary>
		/// Attempts to position a Stargate onto a Ramp.
		/// </summary>
		/// <returns>Whether or not the Gate was positioned on the Ramp succesfully.</returns>
		public static bool PutGateOnRamp(Stargate gate, GateRamp ramp)
		{
			if (gate.IsValid() && ramp.IsValid()) // gate ramps
			{
				if (ramp.HasFreeSlot())
				{
					gate.Transform.Position = ramp.Transform.World.PointToWorld(
						ramp.StargatePositionOffset
					);
					gate.Transform.Rotation = ramp.Transform.World.RotationToWorld(
						ramp.StargateRotationOffset.ToRotation()
					);
					gate.GameObject.SetParent(ramp.GameObject);
					return true;
				}
			}

			return false;
		}

		[Event("trace.prepare")] // sbox plus legacy code
		public static void OnTracePrepare(
			SceneTrace trace,
			GameObject ent,
			Action<SceneTrace> returnFn
		)
		{
			returnFn(GetAdjustedTraceForClipping(ent, trace));
		}

		public static SceneTrace GetAdjustedTraceForClipping(GameObject ent, SceneTrace trace)
		{
			if (ent.Tags.Has(StargateTags.BehindGate))
				trace = trace.WithoutTags(StargateTags.InBufferFront);

			if (ent.Tags.Has(StargateTags.BeforeGate))
				trace = trace.WithoutTags(StargateTags.InBufferBack);

			return trace;
		}

		public static TagSet GetAdjustedIgnoreTagsForClipping(GameObject ent, TagSet tags)
		{
			tags.Set(StargateTags.InBufferFront, ent.Tags.Has(StargateTags.BehindGate));
			tags.Set(StargateTags.InBufferBack, ent.Tags.Has(StargateTags.BeforeGate));

			return tags;
		}

		public static bool IsPointBehindEventHorizon(Vector3 point, Stargate gate)
		{
			if (!gate.IsValid())
				return false;

			var eh = gate.EventHorizon;
			if (!eh.IsValid())
				return false;

			return (point - eh.Transform.Position).Dot(eh.Transform.Rotation.Forward) < 0;
		}

		public static bool IsAllowedForGateTeleport(GameObject e)
		{
			if (!e.IsValid())
				return false;

			if (e.Tags.Has("player"))
				return true;

			// if our parent isnt the scene, we are a child of something else
			// if ( e.Parent is not Scene _ )
			//     return false;

			// if our parent does not have a multiworld component, we are not the root object, so we are not allowed
			if (!MultiWorldSystem.IsObjectRootInWorld(e))
				return false;

			if (e.Components.Get<Rigidbody>() is null)
				return false;

			// if ( e is StargateIris || e is GateBearing || e is PickupTrigger || e is EventHorizonTrigger || e is EventHorizonCollider )
			//     return false;

			// if ( e is Water || e is WaterController || e is WaterFunc )
			//     return false;

			return true;
		}

		public static List<GameObject> GetAllChildrenRecursive(GameObject e)
		{
			var l = new List<GameObject>();

			foreach (var c1 in e.Children)
			{
				l.Add(c1);

				if (c1.Children.Count > 0)
				{
					foreach (var c2 in GetAllChildrenRecursive(c1))
					{
						l.Add(c2);
					}
				}
			}

			return l;
		}

		public static List<GameObject> GetSelfWithAllChildrenRecursive(GameObject e)
		{
			var l = GetAllChildrenRecursive(e);
			l.Add(e);
			return l;
		}

		/*
		[Event( "weapon.shootbullet" )]
		public static void OnShootBullet( ShootBulletParams param )
		{
		    var tr = param.tr;
		    if ( tr.Entity is EventHorizon eh )
		    {
		        param.preventDefault = true;
		        eh.PlayTeleportSound();

		        if ( !eh.IsFullyFormed )
		            return;

		        var isInbound = eh.Gate.Inbound;
		        var otherEH = eh.GetOther();
		        var otherIrisClosed = otherEH.Gate.IsIrisClosed();
		        var fromBehind = eh.IsPointBehindEventHorizon( tr.HitPosition );

		        if ( !isInbound && !fromBehind && otherIrisClosed )
		            otherEH.Gate.Iris.PlayHitSound();

		        if ( isInbound || fromBehind || otherIrisClosed )
		            return;

		        var newCoords = eh.CalcExitPointAndDir( tr.HitPosition, tr.Direction );
		        var newPos = newCoords.Item1;
		        var newDir = newCoords.Item2;

		        //if ( Game.IsClient )
		        //{
		        //	DebugOverlay.Line( tr.StartPosition, tr.EndPosition, 4 );
		        //	DebugOverlay.Line( newPos + newDir * 2, newPos + newDir * 5000, 4 );
		        //}

		        // shoot a bullet from the other EH, new pos will be offset forward to avoid hitting itself
		        var offset = newDir * 0.5f;
		        Weapon weapon = param.weapon;
		        var spread = param.spread;
		        var force = param.force;
		        var damage = param.damage;
		        var bulletSize = param.bulletSize;
		        weapon.ShootBullet( newPos + offset, newDir, spread, force, damage, bulletSize );
		        eh.GetOther().PlayTeleportSound();
		    }
		}
		*/

		// WIP tasks testing

		public void AddTask(float time, Action task, TimedTaskCategory category)
		{
			// if ( IsProxy ) return;
			StargateActions.Add(new TimedTask(time, task, category));
		}

		public void ClearTasks()
		{
			// if ( IsProxy ) return;
			StargateActions.Clear();
		}

		public void ClearTasksByCategory(TimedTaskCategory category)
		{
			// if ( IsProxy ) return;
			var rem = StargateActions.RemoveAll(task => task.TaskCategory == category);
		}

		// [GameEvent.Tick.Server]
		private void TaskThink() // dont mind the retarded checks, it prevents ArgumentOutOfRangeException if actions get deleted while the loop runs, probably thread related stuff
		{
			if (StargateActions.Count > 0)
			{
				for (var i = StargateActions.Count - 1; i >= 0; i--)
				{
					if (StargateActions.Count > i)
					{
						var task = StargateActions[i];
						if (Time.Now >= task.TaskTime)
						{
							if (!task.TaskFinished)
							{
								task.Execute();
								if (StargateActions.Count > i)
									StargateActions.RemoveAt(i);
							}
						}
					}
				}
			}
		}

		// Timed Tasks
		public struct TimedTask
		{
			public TimedTask(float time, Action action, TimedTaskCategory category)
			{
				TaskTime = time;
				TaskAction = action;
				TaskCategory = category;
				TaskFinished = false;
			}

			public TimedTaskCategory TaskCategory { get; }
			public float TaskTime { get; }
			public bool TaskFinished { get; private set; }
			private Action TaskAction { get; }

			public void Execute()
			{
				TaskAction();
				TaskFinished = true;
			}
		}
	}
}

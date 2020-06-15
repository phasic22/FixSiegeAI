using System;
using System.Diagnostics;
using System.IO;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection;

// version 1.0.0
namespace FixSiegeAI
{
	// Main class, Harmony patch classes must be at this level
	public class Main : MBSubModuleBase
	{

		// This runs at game main menu
		protected override void OnBeforeInitialModuleScreenSetAsRoot()
		{
			try { File.Delete(@"C:\Users\Shadow\Desktop\FixSiegeAI_Log.txt"); } catch { };
			try
			{
				Main.Log("FixSiegeAI module v1.0.0 loaded.", true);
				var harmony = new Harmony("fixsiegeai");
				harmony.PatchAll();
				Main.Log("Harmony patches loaded.");
			}
			catch (Exception e) { Main.Log(e.Message); };
		}

		// This runs when module is loaded.
		protected override void OnSubModuleLoad()
		{
			//	todo hotkeys
		}

		// Stops ai from reticking if the original first order is not followed through, DOES NOT prevent first AI order
		public override void OnMissionBehaviourInitialize(Mission mission)
		{
			Main.Log(Environment.NewLine + " /////////////////////// MISSION LOADED /////////////////////// ");
			foreach (SiegeWeapon sw in Mission.Current.ActiveMissionObjects.FindAllWithType<SiegeWeapon>())
			{
				if (sw != null && sw.Side == Mission.Current.PlayerTeam.Side)
				{
					sw.ForcedUse = false;
					Main.Log(sw.Side.ToString() + " " + sw.GetSiegeEngineType().ToString() + " forced use haulted.");
					// TODO: attempt to fix siege angle restrictions
					//if (sw is RangedSiegeWeapon)
					//{
					//	Main.Log(sw.Side.ToString() + " " + sw.GetSiegeEngineType().ToString() + " release angles: " + (sw as RangedSiegeWeapon).TopReleaseAngleRestriction.ToString());
					//	Main.Log(sw.Side.ToString() + " " + sw.GetSiegeEngineType().ToString() + " release angles: " + (sw as RangedSiegeWeapon).BottomReleaseAngleRestriction.ToString());
					//	if (sw is Trebuchet)
					//	{
					//		(sw as RangedSiegeWeapon).BottomReleaseAngleRestriction = 0.1f;
					//	}
					//	Main.Log(sw.Side.ToString() + " " + sw.GetSiegeEngineType().ToString() + " release angles widened.");
					//}
				}
			}
		}

		// Utilities
		public static void Log(string s, bool ingameonly = false)
		{
			bool debugging = true; // change this to true for helpful debugging info in-game and to log below
			if (ingameonly || debugging)
			{
				InformationManager.DisplayMessage(new InformationMessage(s));
			}
			if (debugging)
			{
				string log_path = @"C:\Users\Shadow\Desktop\FixSiegeAI_Log.txt"; // change where you want the log to save
				try
				{
					using (System.IO.StreamWriter file = new System.IO.StreamWriter(log_path, true))
					{ file.WriteLine(s); };
				}
				catch { };
			}
		}

		public static void St()
		{
			StackTrace stackTrace = new StackTrace(true);
			//StringBuilder sb = new StringBuilder();
			foreach (StackFrame frame in stackTrace.GetFrames())
			{
				Main.Log(" Method Name: " + frame.GetMethod().Name + " File Name:" + frame.GetMethod().Module.Name + " Line No: " + frame.GetFileLineNumber());
			}
		}

		public static bool IsPIC(Formation formation, bool debug = false)
		{
			bool isPG = formation.Team.IsPlayerGeneral;
			if (debug) { Main.Log("Is player general of formation?: " + isPG.ToString()); }
			bool isPS = formation.Team.IsPlayerSergeant;
			if (debug) { Main.Log("Is player sergeant of formation?: " + isPS.ToString()); }
			bool isAIC = formation.IsAIControlled;
			if (debug) { Main.Log("Is formation AI controlled?: " + isAIC.ToString()); }
			bool isPA = Mission.Current.MainAgent != null && Mission.Current.MainAgent.IsActive();
			if (debug) { Main.Log("Is player alive?: " + isPA.ToString()); }
			bool isPIC = isPA && (isPG | isPS) & !isAIC;
			if (debug) { Main.Log("Is player in charge of formation?: " + isPIC.ToString()); }
			if (isPIC) { return true; } else { return false; }
		}

		public static bool IsOrderNear(Formation formation, SiegeWeapon sw, float thresh, bool debug = false)
		{
			Vec2 mo_loc = formation.MovementOrder.GetPosition(formation).AsVec2; // get where they're told to go
			Vec2 sw_loc = sw.GameEntity.GlobalPosition.AsVec2;
			float distance = sw_loc.Distance(mo_loc);
			bool isNear = distance <= thresh;
			if (debug)
			{
				Main.Log("Distance to " + sw.GetSiegeEngineType().ToString() + " from order placement: " + distance.ToString());
				Main.Log("Checking distance to: " + sw.GetSiegeEngineType().ToString());
				Main.Log("Siege Weapon Location: " + sw_loc.ToString());
				Main.Log("Order placement is near: " + isNear);
			}
			return isNear;
		}

	}

	// Harmony patch to prevent AI splitting formations
	[HarmonyPatch(typeof(MissionOrderVM), "DeployFormationsOfPlayer")]
	public class Patch_DeployFormationsOfPlayer
	{
		private static bool Prefix(MissionOrderVM __instance)
		{
			foreach (Formation formation in Mission.Current.PlayerTeam.Formations)
			{
				formation.IsAIControlled = !Main.IsPIC(formation);
			}
			Mission.Current.AllowAiTicking = true;
			Mission.Current.ForceTickOccasionally = true;
			Mission.Current.PlayerTeam.ResetTactic();
			bool isTeleportingAgents = Mission.Current.IsTeleportingAgents;
			Mission.Current.IsTeleportingAgents = true;
			Mission.Current.PlayerTeam.Tick(0f);
			Mission.Current.IsTeleportingAgents = isTeleportingAgents;
			Mission.Current.AllowAiTicking = false;
			Mission.Current.ForceTickOccasionally = false;
			return false;
		}
	}

	// Harmony patch to remove auto take-over of team by AI after "Begin Assault" button is pressed
	[HarmonyPatch(typeof(SiegeMissionController), "OnPlayerDeploymentFinish")]
	public static class Patch_OnPlayerDeploymentFinish
	{
		public static void Postfix()
		{
			if (!Mission.Current.IsFieldBattle && Mission.Current.PlayerTeam.IsPlayerGeneral)
			{
				Main.Log(Environment.NewLine + " /////////////////////// BEGIN ASSAULT BUTTON PRESSED /////////////////////// ");
				foreach (Formation formation in Mission.Current.PlayerTeam.FormationsIncludingSpecial)
				{
					formation.IsAIControlled = false;
					Main.Log("AI control disabled.");
				}
				Main.Log(" /////////////////////// PLAYERTEAM DEPLOYMENT POSTFIX FINISHED /////////////////////// ");
			}
		}
	}

	// Harmony patch that stops troops from getting stuck using ladders
	[HarmonyPatch(typeof(SiegeLadder), "OnTick")]
	public static class Patch_OnTick
	{
		public static void Postfix(SiegeLadder __instance)
		{
			(__instance as SiegeWeapon).ForcedUse = false;
		}
	}

	// Harmony patch to have troops use siege engines near movement commands
	[HarmonyPatch(typeof(MovementOrder), "OnApply")]
	public static class Patch_OnApply
	{
		public static bool Prefix(this Formation formation)
		{
			if (formation.CountOfUnits < 1) { return true; }
			if (!Main.IsPIC(formation)) { return true; }
			//if (Mission.Current.GetMissionBehaviour<SiegeDeploymentHandler>() != null) { return true; } // allows blue-gearing siege engines during deployment
			Main.Log("Movement order given: " + formation.MovementOrder.OrderType.ToString());
			if (formation.MovementOrder.OrderType == OrderType.FollowEntity) { return true; };
			//if (formation.MovementOrder.OrderType == OrderType.Charge)
			//	{
			//		foreach (MissionObject gatemo in Mission.Current.MissionObjects)
			//			{
			//				if (gatemo is CastleGate && gatemo.GameEntity.HasTag("inner_gate"))
			//				{
			//					CastleGate ig = gatemo as CastleGate;
			//				}
			//				if (gatemo is CastleGate && gatemo.GameEntity.HasTag("outer_gate"))
			//				{
			//					CastleGate og = gatemo as CastleGate;
			//				}
			//			}
			//	if (og.IsDestroyed() && !ig.IsDestroyed())
			//	{
			//		Traverse.Create(formation).Method("FormAttackEntityDetachment", ig.GameEntity).GetValue();
			//	}
			//	return true;
			//	}

			foreach (SiegeWeapon sw in Mission.Current.ActiveMissionObjects.FindAllWithType<SiegeWeapon>())
			{
				if (sw.Side == Mission.Current.PlayerTeam.Side) // only works when player gives order for player's side's siegeweapon
				{
					bool start = false; bool stop = false; bool cont = false;
					bool isUsing = formation.IsUsingMachine(sw);
					bool isOrderNear = Main.IsOrderNear(formation, sw, 12f);
					//bool isOrderFar = !Main.IsOrderNear(formation, sw, 45f);
					if (formation.MovementOrder.OrderType == OrderType.StandYourGround & isUsing) { stop = true; goto eval; };
					//if (!isOrderFar & !isOrderNear & isUsing) { stop = true; goto eval; };
					//if (isOrderFar & isUsing) { cont = true; goto eval; };
					if (isOrderNear & isUsing) { cont = true; goto eval; }
					if (isOrderNear & !isUsing) { start = true; goto eval; }
					if (!isOrderNear & !isUsing) { continue; }
				eval:
					string swStr = sw.GetSiegeEngineType().ToString();
					if (start)
					{
						Main.Log("Formation starting using: " + swStr, true);
						if (sw is BatteringRam | sw is SiegeTower)
						{
							MovementOrder mo = formation.MovementOrder;
							Traverse.Create(mo).Method("MovementOrderFollowEntity", sw.GameEntity).GetValue();
						}
						sw.ForcedUse = true;
						Traverse.Create(formation).Method("JoinDetachment", (sw as UsableMachine)).GetValue();
						//Traverse.Create(formation).Method("OrderPosition", (sw as UsableMachine)).GetValue();
						//formation.SetPositioning(null);
						return false;
						//continue;
					}
					if (stop)
					{
						Main.Log("Formation stopping using: " + swStr, true);
						sw.ForcedUse = false;
						Traverse.Create(formation).Method("LeaveDetachment", (sw as UsableMachine)).GetValue();
						continue;
					}
					if (cont)
					{
						Main.Log("Formation continuing to use: " + swStr, true);
						continue;
					}
				}
			}
			return true;
		}
	}

	//Harmony patch to get units to attack inner gate when outer gate's down
	[HarmonyPatch(typeof(CastleGate), "OnDestroyed")]
	public static class Patch_OnDestroyed
	{
		public static void Postfix(CastleGate __instance)
		{
			TeamAISiegeComponent aic = Mission.Current.AttackerTeam.TeamAI as TeamAISiegeComponent;
			if (__instance.GameEntity.HasTag("inner_gate"))
			{
				Main.Log("Inner gate destroyed!", true);
				foreach (Formation f in Mission.Current.PlayerTeam.FormationsIncludingSpecial)
				{
					Traverse.Create(f).Method("DisbandAttackEntityDetachment").GetValue();
				}
			}
			if (__instance.GameEntity.HasTag("outer_gate"))
			{
				Main.Log("Outer gate destroyed!", true);
				foreach (MissionObject mo in Mission.Current.ActiveMissionObjects)
				{
					foreach (Formation f in Mission.Current.PlayerTeam.Formations)
					{
						if (f.IsUsingMachine(mo as BatteringRam) && !aic.InnerGate.IsDestroyed)
						{
							if (Mission.Current.PlayerTeam.IsAttacker) { Main.Log("Sending detachment to attack inner gate!", true); }
							Traverse.Create(f).Method("FormAttackEntityDetachment", aic.InnerGate.GameEntity).GetValue();
						}
					}
				}
			}
			if (Mission.Current.PlayerTeam.IsAttacker && aic.InnerGate.IsDestroyed && aic.OuterGate.IsDestroyed)
			{ Main.Log("Charge when ready!", true); }
		}
	}

	// Override charge
	[HarmonyPatch(typeof(OrderController), "GetChargeOrderSubstituteForSiege")]
	public static class Patch_GetChargeOrderSubstituteForSiege
	{
		public static bool Prefix(OrderController __instance, ref MovementOrder __result, Formation formation)
		{
			Main.Log("Charge command overridden.");
			__result = MovementOrder.MovementOrderCharge;
			return true;
		}
	}

	// infinite ammo
	[HarmonyPatch(typeof(RangedSiegeWeapon), "ConsumeAmmo")]
	public static class Patch_ConsumeAmmo
	{
		public static bool Prefix()
		{
			Main.Log("Consuming ammo");
			return false;
		}
	}

}

///////////////////////////////////////////////////////////////////////
// 1.0.1
// handled castle gate edge case
// added infinite ammo

// new todo
//send detachments rather than whole formations, only works with weapons, e.g.catapults, not engines, e.g.towers and rams, when using native blue gear option
//make battering ram detachment attack gate, rather than a new group
//stop ram detachment from shuffling positions when reaching gate
//patch for making formations follow when using ram/tower(only works sometimes), will be easier if devs release the FollowEntity movement order, for now formations must be manually advanced
//lower the minimum range for siege weapons, especially trebuchets
//ensure defender/attacker balancing, likely by encouraging towns/castles to emphasize fire onagers if possible
//improve onager/trebuchet reload pathfinding, perhaps add extra StandingPoints
//have troops take better cover, it seems horse archers aren't allowed to use cover like archers are, but not sure
//known crash involving using gear option to re-use siege weapons/use ammo piles, issue on dev end, reported
//allow rams and towers to be blue-geared before arriving at walls(works when at walls)
//get troops to attack second gate without grey-gearing
//change attacking priority so troops will take the path of least resistance, i.e. if the gate is destroyed or if siege towers are fully deployed
//increase ammo pile size, give defenders infinite/ludicrous ammo
//manage how troops are commanded so that siege weapons are not stopped using if a new order is given
//stop ladders from still being forced used in certain conditions
//settings menu to enable/disable settings
//give a report when using the blue-gear / grey-gear command
//fix so ram is deactivated/disabled to no longer give blue-gear icon after first gate is down
//fix so grey-gear inner gate works from outside walls
//stop defender ai from falling off walls
//verify mod works as sergeant

// useful trash
//// Old harmony patch that failed to completely prevent dumb AI behavior
//[HarmonyPatch(typeof(ModuleExtensions), "StartUsingMachine")]
//public static class Prefix_StartUsingMachine
//{
//	public static bool Prefix(this Formation formation, UsableMachine usable, bool isPlayerOrder) // variable names must match exactly
//	{
//		if (formation.CountOfUnits < 1) { return true; }
//		if (!Main.IsPIC(formation)) { return true; }
//		SiegeWeapon sw = (usable as SiegeWeapon);
//		Main.Log("Formation told to start using: " + sw.GetSiegeEngineType().ToString());
//		Main.Log("Is it forced used?:" + sw.ForcedUse.ToString());
//		Main.Log("Is it a player order?: " + isPlayerOrder.ToString());
//		return true;
//	}
//}
//if (isAIC) { Main.Log("Player in charge, but AI in command. Allowing order."); return true; } // prevents any ai movement orders if player is alive and in charge
//if (formation.IsUsingMachine(usable)) { Main.Log("Already using."); return false; } // prevent redundant orders
//Mission.Current.MainAgent.MakeVoice(SkinVoiceManager.VoiceType.UseSiegeWeapon, SkinVoiceManager.CombatVoiceNetworkPredictionType.Prediction);
//foreach (Formation fs in formation.Team.FormationsIncludingSpecial) { };
//Main.Log("Num of formations with special: "+formation.Team.FormationsIncludingSpecial.Count().ToString());
//Main.Log("Num of formations without special: " + formation.Team.Formations.Count().ToString());
//Main.Log("Num of formations without special: " + formation.Team.Formations.Count().ToString());
//Main.Log("Num of units including detachments: " + formation.CountOfUnits.ToString());
//Main.Log("Num of units not including detachments: " + formation.CountOfUnitsWithoutDetachedOnes.ToString());
//Environment.NewLine + Environment.NewLine + 
//formation.MovementOrder = MovementOrder.MovementOrderFollow;
//formation.Team.ResetTactic();
//formation.ReleaseFormationFromAI();
//BehaviorAssaultWalls baw = BehaviorAssaultWalls;
//AIBehaviorComponent;
//	formation.AI.ResetBehaviorWeights
//	formation.AI.Can
//BehaviorComponent sdfsd = new BehaviorStop(formation);
//formation.AI.AddBehavior(sdfsd);
//formation.AI.ResetBehaviorWeights();
//(usable as SiegeLadder).HasCompletedAction
//(usable as SiegeLadder).State.
//(usable as SiegeLadder).State.Equals(SiegeLadder.LadderState.OnLand

// var f_loc = formation.QuerySystem.MedianPosition.Position;
//BehaviorUseSiegeMachines busm = new BehaviorUseSiegeMachines(formation);
//							if (sw is SiegeTower) { formation.AI.AddBehavior(busm);}
//							formation.AI.
//						}
//OrderController oc = Mission.Current.PlayerTeam.GetOrderControllerOf(formation.PlayerOwner);
//Mission.Current.MainAgent.MakeVoice(SkinVoiceManager.VoiceType.UseSiegeWeapon, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
//bool ordIsForward = (formation.MovementOrder.OrderType == OrderType.Advance);
//bool swMoves = (sw is SiegeLadder || sw is SiegeTower || sw is BatteringRam);
//if (ordIsForward & swMoves & !isUsing) { start = true; goto eval; }
//if (ordIsForward & swMoves & isUsing) { cont = true; goto eval; }

//Mission.Current.ActiveMissionObjects.FindAllWithType<CastleGate>();

// Override charge
//[HarmonyPatch(typeof(OrderController), "GetChargeOrderSubstituteForSiege")]
//public static class Patch_GetChargeOrderSubstituteForSiege
//{
//	public static bool Prefix(OrderController __instance, ref MovementOrder __result, Formation formation)
//	{
//		Team team = formation.Team;
//		TeamAISiegeComponent teamAISiegeComponent = ((team != null) ? team.TeamAI : null) as TeamAISiegeComponent;
//		if (Mission.Current.IsTeleportingAgents || teamAISiegeComponent == null || teamAISiegeComponent.IsAnyLaneThroughWallsOpen())
//		{
//			__result = MovementOrder.MovementOrderCharge;
//			return false;
//		}
//		if (!formation.Team.IsAttacker)
//		{
//			CastleGate castleGate = (!teamAISiegeComponent.InnerGate.IsGateOpen) ? teamAISiegeComponent.InnerGate : teamAISiegeComponent.OuterGate;
//			if (castleGate != null && !formation.IsUsingMachine(castleGate))
//			{
//				formation.StartUsingMachine(castleGate, true);
//			}
//			__result = MovementOrder.MovementOrderCharge;
//			return false;
//		}
//		if (teamAISiegeComponent.Ladders.Count > 0)
//		{
//			SiegeLadder siegeLadder = teamAISiegeComponent.Ladders.MinBy((SiegeLadder l) => l.WaitFrame.origin.DistanceSquared(formation.QuerySystem.MedianPosition.GetNavMeshVec3()));
//			if (!formation.IsUsingMachine(siegeLadder))
//			{
//				formation.StartUsingMachine(siegeLadder, true);
//			}
//			__result = MovementOrder.MovementOrderMove(siegeLadder.WaitFrame.origin.ToWorldPosition());
//			return false;
//		}
//		__result = MovementOrder.MovementOrderCharge;
//		return false;
//	}
//}
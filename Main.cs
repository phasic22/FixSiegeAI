using System;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using HarmonyLib;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using System.IO;
using System.Linq.Expressions;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using System.Diagnostics;
using System.Runtime;
using System.Threading;

namespace FixSiegeAI
{
	// Main class, Harmony patch classes must be at this level
	public class Main : MBSubModuleBase
	{
		// Print to log in-game and desktop
		public static void Log(string s)
		{
			bool debugging = false; // change this to true for helpful debugging info in-game and to log below
			if (debugging)
			{
				InformationManager.DisplayMessage(new InformationMessage(s));
				string log_path = @"C:\Users\Shadow\Desktop\Log.txt"; // change where you want the log to save
				try
				{
					using (System.IO.StreamWriter file = new System.IO.StreamWriter(log_path, true))
					{ file.WriteLine(s); };
				}	catch { };
			}
		}

		// This runs at game main menu
		protected override void OnBeforeInitialModuleScreenSetAsRoot()
		{
			try { File.Delete(@"C:\Users\Shadow\Desktop\Log.txt"); } catch { };
			InformationManager.DisplayMessage(new InformationMessage("FixSiegeAI module loaded."));
			var harmony = new Harmony("fixsiegeai");
			harmony.PatchAll();
			Main.Log("Harmony patches loaded.");
		}

		// Stops ai from reticking if the original first order is not followed through, DOES NOT prevent first AI order
		public override void OnMissionBehaviourInitialize(Mission mission)
		{
			Main.Log(Environment.NewLine+" /////////////////////// MISSION LOADED /////////////////////// ");
			foreach (SiegeWeapon sw in Mission.Current.ActiveMissionObjects.FindAllWithType<SiegeWeapon>())
			{
				if (sw.Side == Mission.Current.PlayerTeam.Side)
				{
					sw.ForcedUse = false;
					Main.Log(sw.Side.ToString()+" "+sw.GetSiegeEngineType().ToString() + " forced use haulted."); 
				}
			}
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
					Main.Log(Environment.NewLine + "AI control disabled.");
					Main.Log("Haulting formation.");
					formation.MovementOrder = MovementOrder.MovementOrderStop;
				}
				Main.Log(" /////////////////////// PLAYERTEAM DEPLOYMENT POSTFIX FINISHED /////////////////////// ");
			}
		}
	}

	// Harmony patch to control when a siege engine is used, prevents AI from taking control ever, including first tick
	[HarmonyPatch(typeof(ModuleExtensions), "StartUsingMachine")]
	public static class Prefix_StartUsingMachine
	{
		public static bool Prefix(this Formation formation, UsableMachine usable, bool isPlayerOrder) // variable names must match exactly
		{
			bool isAIC = formation.IsAIControlled;
			bool isPG = formation.Team.IsPlayerGeneral;
			bool isPS = formation.Team.IsPlayerSergeant;
			bool isPT = formation.Team.IsPlayerTeam;
			bool isPA = Mission.Current.MainAgent != null && Mission.Current.MainAgent.IsActive(); // is player alive
			bool is_dep = (Mission.Current.GetMissionBehaviour<SiegeDeploymentHandler>() != null); // is during deployment

			if (!isPA || !isPG || !isPT || !isPS) { return true; } // if not entirely player controlled
			if (formation.IsUsingMachine(usable)) { Main.Log("Already using."); return false; } // prevent redundant orders
			Main.Log(Environment.NewLine + "Formation told to start using: " + (usable as SiegeWeapon).GetSiegeEngineType().ToString());
			if (isAIC) { Main.Log("Prevented team AI from ordering start using machine.");	return false;	}
			else { Main.Log("Allowing use of machine."); return true;}
		}
	}

	// Harmony patch to force units to stop using siege engines if given any order, and use if nearby
	[HarmonyPatch(typeof(MovementOrder), "OnApply")]
	public static class Patch_OnApply
	{
		public static bool Prefix(this Formation formation)
		{
			if (formation.CountOfUnits < 1)	{	return true; } // this line cost me hours to figure out
			bool isAIC = formation.IsAIControlled;
			bool isPG = formation.Team.IsPlayerGeneral;
			bool isPT = formation.Team.IsPlayerTeam;
			bool isPS = formation.Team.IsPlayerSergeant;
			bool isPA = Mission.Current.MainAgent != null && Mission.Current.MainAgent.IsActive(); // is player alive
			bool is_dep = (Mission.Current.GetMissionBehaviour<SiegeDeploymentHandler>() != null); // is during deployment

			if (isPA && isPT && (isPG || isPS))
			{
				Main.Log(Environment.NewLine + "Movement order applied: " + formation.MovementOrder.OrderType.ToString());
				Main.Log("Is this during deployment?: " + is_dep);
				Main.Log("Is formation AI controlled?: " + formation.IsAIControlled.ToString());
				if (isAIC && is_dep) { Main.Log("Order blocked."); return false; } // prevents any ai movement orders during deployment
				if (is_dep) { return true; } // allows using siege engines during deployment
				var fo_loc = formation.MovementOrder.GetPosition(formation).AsVec2; // get where they're told to go
				Main.Log("Order Location: " + fo_loc.ToString());
				if (formation.MovementOrder.OrderType == OrderType.StandYourGround) { Main.Log("Stop order issued"); }
				foreach (SiegeWeapon sw in Mission.Current.ActiveMissionObjects.FindAllWithType<SiegeWeapon>())
				{
					if (!isAIC && sw.Side == Mission.Current.PlayerTeam.Side && !is_dep) // only works when player gives order for player's side's siegeweapon
					{
						var sw_loc = sw.GameEntity.GlobalPosition.AsVec2;
						double distance = sw_loc.Distance(fo_loc);
						Main.Log("Checking distance to: " + sw.GetSiegeEngineType().ToString());
						Main.Log("Siege Weapon Location: " + sw_loc.ToString());
						Main.Log("Distance: " + distance.ToString());
						if (formation.MovementOrder.OrderType == OrderType.StandYourGround && formation.IsUsingMachine(sw)) 
						{
							formation.StopUsingMachine(sw, true);
							Main.Log("Formation stopping using: " + sw.GetSiegeEngineType().ToString());
							InformationManager.DisplayMessage(new InformationMessage("Formation stopping using machine: " + sw.GetSiegeEngineType().ToString()));
						}
						if (distance <= 10 && !formation.IsUsingMachine(sw))
						{
							formation.StartUsingMachine(sw, true);
							Main.Log("Formation starting using new machine: " + sw.GetSiegeEngineType().ToString());
							InformationManager.DisplayMessage(new InformationMessage("Formation starting using new machine: " + sw.GetSiegeEngineType().ToString()));
						}
						else if (distance > 10 && formation.IsUsingMachine(sw))
						{
							formation.StopUsingMachine(sw, true);
							Main.Log("Formation stopping using: " + sw.GetSiegeEngineType().ToString());
							InformationManager.DisplayMessage(new InformationMessage("Formation stopping using machine: " + sw.GetSiegeEngineType().ToString()));
						}
						else
						{
							Main.Log("Same as previous order");
						}
					}
				}
			}
			return true;
		}
	}
}

// 0.1.1
// added report for when using/not using siege weapon
// fixed log to account for different paths

// breaks
// if (sw.Users != null) { formation.MovementOrder = MovementOrder.MovementOrderFollow(sw.Users.First()); }

// useful
// var f_loc = formation.QuerySystem.MedianPosition.Position;
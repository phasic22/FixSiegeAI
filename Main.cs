using System;
using System.Diagnostics;
using System.IO;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace FixSiegeAI
{
	// Main class
	public class Main : MBSubModuleBase
	{
		// This runs at game main menu
		protected override void OnBeforeInitialModuleScreenSetAsRoot()
		{
			try { File.Delete(@"C:\Users\Shadow\Desktop\FixSiegeAI_Log.txt"); } catch { };
			try
			{
				Main.Log("FixSiegeAI v2.0.2 loaded.",true);
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
			Main.Log(Environment.NewLine + "Mission loaded. ");
			foreach (SiegeWeapon sw in Mission.Current.ActiveMissionObjects.FindAllWithType<SiegeWeapon>())
			{
				if (sw != null && sw.Side == Mission.Current.PlayerTeam.Side && Mission.Current.PlayerTeam.IsPlayerGeneral)
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
		public static void Log(string s, bool ingame = false)
		{
			bool debugging = true; // change this to true for helpful debugging info in-game and to log below
			if (ingame || debugging)
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
			foreach (StackFrame frame in stackTrace.GetFrames())
			{
				Main.Log(" Method Name: " + frame.GetMethod().Name + " File Name:" + frame.GetMethod().Module.Name + " Line No: " + frame.GetFileLineNumber());
			}
		}
		public static bool IsPIC(Formation formation, bool debug = false)
		{
			bool isPG = formation.Team.IsPlayerGeneral;
			if (debug) { Main.Log(Environment.NewLine + "Is player general of formation?: " + isPG.ToString()); }
			bool isPS = formation.Team.IsPlayerSergeant;
			if (debug) { Main.Log("Is player sergeant of formation?: " + isPS.ToString()); }
			bool isPA = Mission.Current.MainAgent != null && Mission.Current.MainAgent.IsActive();
			if (debug) { Main.Log("Is player alive?: " + isPA.ToString()); }
			bool isPIC = isPA && (isPG | isPS);
			if (debug) { Main.Log("Is player in charge of formation?: " + isPIC.ToString()); }
			if (isPIC) { return true; } else { return false; }
		}

	}

}
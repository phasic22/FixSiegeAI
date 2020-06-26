using System;
using HarmonyLib;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection;

namespace FixSiegeAI
{
	// Prevent AI splitting formations at onset of deployment
	[HarmonyPatch(typeof(MissionOrderVM), "DeployFormationsOfPlayer")]
	public static class Patch_DeployFormationsOfPlayer
	{
		private static bool Prefix()
		{
			if (!Mission.Current.PlayerTeam.IsPlayerGeneral) { return true; }
			Main.Log(Environment.NewLine + "Deployment started. ");
			foreach (Formation formation in Mission.Current.PlayerTeam.Formations)
			{
				if (Main.IsPIC(formation))
				{
					formation.IsAIControlled = false;
					Main.Log("AI control disabled.");
				}
				else { formation.IsAIControlled = true; }
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

	// Remove auto take-over of team by AI after "Begin Assault" button is pressed
	[HarmonyPatch(typeof(SiegeMissionController), "OnPlayerDeploymentFinish")]
	public static class Patch_OnPlayerDeploymentFinish
	{
		public static void Postfix()
		{
			if (!Mission.Current.PlayerTeam.IsPlayerGeneral) { return; }
			Main.Log(Environment.NewLine + "Begin assault button pressed. ");
			if (!Mission.Current.IsFieldBattle)
			{
				foreach (Formation formation in Mission.Current.PlayerTeam.FormationsIncludingSpecial)
				{
					if (Main.IsPIC(formation))
					{
						formation.IsAIControlled = false;
						Main.Log("AI control disabled again.");
					}
				}
				Main.Log("Post deployment patch finished. ");
			}
		}
	}

	//// Stops troops from getting stuck using ladders if within a certain range
	//[HarmonyPatch(typeof(SiegeLadder), "OnTick")]
	//public static class Patch_SiegeLadder_OnTick
	//{
	//	public static void Postfix(SiegeLadder __instance)
	//	{
	//		//if (!Mission.Current.PlayerTeam.IsPlayerGeneral) { return; }
	//		if (!__instance.IsUsed) 
	//		{
	//			(__instance as SiegeWeapon).ForcedUse = false; 
	//		}
	//		else { (__instance as SiegeWeapon).ForcedUse = true; }
	//	}
	//}


}
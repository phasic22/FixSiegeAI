using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace FixSiegeAI
{
	[HarmonyPatch(typeof(LadderQueueManager), "AddAgentToQueue")]
	public static class Patch_AddAgentToQueue
	{
		public static bool Prefix(LadderQueueManager __instance, Agent agent)
		{
			//int ql = Traverse.Create(__instance).Field("_queuedAgentCount").GetValue<int>();
			//if (ql > 20)
			//{
			//	Main.Log("Add to queue blocked.");
			//	return false;
			//}
			return true;
			//float d = __instance.GameEntity.GlobalPosition.Distance(agent.Position);
			//Main.Log(Environment.NewLine + "Agent added to queue.");
			//Main.Log("Entity: " + __instance.GameEntity.Name);
			//Main.Log("Location: " + __instance.GameEntity.GlobalPosition.ToString());
			//Main.Log("Distance from agent to entity: " + d.ToString());

			//Main.Log("Spacing: " + Traverse.Create(__instance).Field("_agentSpacing").GetValue());
			//Main.Log("Max users: " + Traverse.Create(__instance).Field("_maxUserCount").GetValue());
			//Main.Log("Queue begin distance: " + Traverse.Create(__instance).Field("_queueBeginDistance").GetValue());
			//Main.Log("Queue row size: " + Traverse.Create(__instance).Field("_queueRowSize").GetValue());
			//Main.Log("Update period: " + Traverse.Create(__instance).Field("_updatePeriod").GetValue());
			//Main.Log("Distance to stop using: " + Traverse.Create(__instance).Field("_distanceToStopUsing2d").GetValue());

			//Main.Log("Queued agent count: " + Traverse.Create(__instance).Field("_queuedAgentCount").GetValue());
			//var qagents = Traverse.Create(__instance).Field("_queuedAgents").GetValue<List<Agent>>();
			//Main.Log("Number of queued agents: " + qagents.Count);
			//var uagents = Traverse.Create(__instance).Field("_userAgents").GetValue<List<Agent>>();
			//Main.Log("Number of users: " + uagents.Count);

			//var mc = agent?.GetComponent<MoraleAgentComponent>();
			//if (mc != null)
			//{
			//	mc.Morale = 100;
			//	mc.StopRetreating();
			//}
			//return true;

		}
	}

	[HarmonyPatch(typeof(LadderQueueManager), "RemoveAgentFromQueueAtIndex")]
	public static class Patch_RemoveAgentFromQueueAtIndex
	{
		public static bool Prefix(LadderQueueManager __instance, int queueIndex)
		{
			return true; 
			//var qagents = Traverse.Create(__instance).Field("_queuedAgents").GetValue<List<Agent>>();
			//float d = __instance.GameEntity.GlobalPosition.Distance(qagents[queueIndex].Position);
			//Main.Log(Environment.NewLine + "Agent removed from queue.");
			//Main.Log("Entity: " + __instance.GameEntity.Name);
			//Main.Log("Location: " + __instance.GameEntity.GlobalPosition.ToString());
			//Main.Log("Distance from agent to entity: " + d.ToString());

			//Main.Log("Spacing: " + Traverse.Create(__instance).Field("_agentSpacing").GetValue());
			//Main.Log("Max users: " + Traverse.Create(__instance).Field("_maxUserCount").GetValue());
			//Main.Log("Queue begin distance: " + Traverse.Create(__instance).Field("_queueBeginDistance").GetValue());
			//Main.Log("Queue row size: " + Traverse.Create(__instance).Field("_queueRowSize").GetValue());
			//Main.Log("Update period: " + Traverse.Create(__instance).Field("_updatePeriod").GetValue());
			//Main.Log("Distance to stop using: " + Traverse.Create(__instance).Field("_distanceToStopUsing2d").GetValue());

			//Main.Log("Queued agent count: " + Traverse.Create(__instance).Field("_queuedAgentCount").GetValue());
			//qagents = Traverse.Create(__instance).Field("_queuedAgents").GetValue<List<Agent>>();
			//Main.Log("Number of queued agents: " + qagents.Count);
			//var uagents = Traverse.Create(__instance).Field("_userAgents").GetValue<List<Agent>>();
			//Main.Log("Number of users: " + uagents.Count);

		}
	}

	[HarmonyPatch(typeof(SiegeTower), "OnInit")]
	public static class Patch_ST_OnInit
	{
		public static void Postfix(SiegeTower __instance)
		{
			var lqms = Traverse.Create(__instance).Field("_queueManagers").GetValue<List<LadderQueueManager>>();
			foreach (LadderQueueManager lqm in lqms)
			{
				int mdfid = (int)Traverse.Create(lqm).Field("ManagedNavigationFaceId").GetValue();
				GameEntity cs = (GameEntity)Traverse.Create(__instance).Field("_cleanState").GetValue();
				cs.Scene.SetAbilityOfFacesWithId(mdfid, false);
				lqm.IsDeactivated = false;
			}
		}
	}

	[HarmonyPatch(typeof(LadderQueueManager), "OnTick")]
	public static class Patch_LQM_OnTick
	{
		public static bool Prefix(out bool __state, LadderQueueManager __instance, float dt)
		{
			__state = true;
			//return false;
			Traverse.Create(__instance).Field("_blockUsage").SetValue(false);
			//Traverse.Create(__instance).Field("_updatePeriod").SetValue(5f);
			Traverse.Create(__instance).Field("_maxUserCount").SetValue(12);
			//Traverse.Create(__instance).Field("_distanceToStopUsing2d").SetValue(.1f);
			Traverse.Create(__instance).Field("_queueRowSize").SetValue(1f); // 1 default
			Traverse.Create(__instance).Field("_queueBeginDistance").SetValue(3f); // 2 default
			Traverse.Create(__instance).Field("_agentSpacing").SetValue(1f); // .8 st def
			Traverse.Create(__instance).Field("_arcAngle").SetValue(3f); // st 0.7853982f
			foreach (var mc in Traverse.Create(__instance).Field("_queuedAgents").GetValue<List<Agent>>()
						.Select(a => a?.GetComponent<MoraleAgentComponent>())
						.Where(c => c != null))
			{
				mc.Morale = 100;
				mc.StopRetreating();
			}
			return true;
			//__state = true;
			//if (DateTime.Now.Millisecond % 500f != 0) { __state = false; return true; }
			//Main.Log(Environment.NewLine + "Pre");
			//Main.Log("Entity: " + __instance.GameEntity.Name);
			//Main.Log("Location: " + __instance.GameEntity.GlobalPosition.ToString());
			//Main.Log("Spacing: " + Traverse.Create(__instance).Field("_agentSpacing").GetValue());
			//Main.Log("Max users: " + Traverse.Create(__instance).Field("_maxUserCount").GetValue());
			//Main.Log("Queue begin distance: " + Traverse.Create(__instance).Field("_queueBeginDistance").GetValue());
			//Main.Log("Queue row size: " + Traverse.Create(__instance).Field("_queueRowSize").GetValue());
			//Main.Log("Update period: " + Traverse.Create(__instance).Field("_updatePeriod").GetValue());
			//Main.Log("Distance to stop using: " + Traverse.Create(__instance).Field("_distanceToStopUsing2d").GetValue());

			//Main.Log("Queued agent count: " + Traverse.Create(__instance).Field("_queuedAgentCount").GetValue());
			//var qagents = Traverse.Create(__instance).Field("_queuedAgents").GetValue<List<Agent>>();
			//Main.Log("Number of queued agents: " + qagents.Count);
			//var uagents = Traverse.Create(__instance).Field("_userAgents").GetValue<List<Agent>>();
			//Main.Log("Number of users: " + uagents.Count);

			
			//return true;
		}

		public static void Postfix(bool __state, LadderQueueManager __instance)
		{
			//Traverse.Create(__instance).Field("_updatePeriod").SetValue(5f);
			//Traverse.Create(__instance).Field("_maxUserCount").SetValue(3);

			//if (!__state) { return; }

			//Main.Log("Post");
			//Main.Log("Entity: "+__instance.GameEntity.Name);
			//Main.Log("Location: " + __instance.GameEntity.GlobalPosition.ToString());
			//Main.Log("Spacing: " + Traverse.Create(__instance).Field("_agentSpacing").GetValue());
			//Main.Log("Max users: " + Traverse.Create(__instance).Field("_maxUserCount").GetValue());
			//Main.Log("Queue begin distance: " + Traverse.Create(__instance).Field("_queueBeginDistance").GetValue());
			//Main.Log("Queue row size: " + Traverse.Create(__instance).Field("_queueRowSize").GetValue());
			//Main.Log("Update period: " + Traverse.Create(__instance).Field("_updatePeriod").GetValue());
			//Main.Log("Distance to stop using: " + Traverse.Create(__instance).Field("_distanceToStopUsing2d").GetValue());

			//Main.Log("Queued agent count: " + Traverse.Create(__instance).Field("_queuedAgentCount").GetValue());
			//var qagents = Traverse.Create(__instance).Field("_queuedAgents").GetValue<List<Agent>>();
			//Main.Log("Number of queued agents: " + qagents.Count);
			//var uagents = Traverse.Create(__instance).Field("_userAgents").GetValue<List<Agent>>();
			//Main.Log("Number of users: " + uagents.Count);
		}
	}

	//[HarmonyPatch(typeof(SiegeLadder), "OnTick")]
	//public static class Patch_SL_OnTick
	//{
	//	public static void Prefix(SiegeLadder __instance, float dt)
	//	{
	//		if (DateTime.Now.Millisecond % 100f != 0)
	//		{
	//			Main.Log("Spacing: " + Traverse.Create(__instance).Field("_agentSpacing").GetValue());
	//		}
	//	}
	//}
}

// trash
// float d = __instance.GameEntity.GlobalPosition.Distance(agent.Position);
//int ql = Traverse.Create(__instance).Field("_queuedAgentCount").GetValue<int>();
//if (qagents.Count > 20)
//{
//	Main.Log("Add to queue blocked.");
//	return false;
//}
//[HarmonyPatch(typeof(LadderQueueManager), "Initialize")]
//	public static class Patch_LQM_Init
//	{
//		public static void Postfix(LadderQueueManager __instance)
//		{
//			//Traverse.Create(__instance).Field("_updatePeriod").SetValue(5f);
//			//Traverse.Create(__instance).Field("_maxUserCount").SetValue(3);
//			//Traverse.Create(__instance).Field("_blockUsage").SetValue(false);
//			//Traverse.Create(__instance).Field("_distanceToStopUsing2d").SetValue(15f);
//		}
//	}

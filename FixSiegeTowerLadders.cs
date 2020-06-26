//using System.Collections;
//using HarmonyLib;
//using TaleWorlds.MountAndBlade;

//namespace FixSiegeAI
//{
//	[HarmonyPatch(typeof(LadderQueueManager), "AddAgentToQueue")]
//	public static class Patch_AddAgentToQueue
//	{
//		public static bool Prefix(LadderQueueManager __instance, Agent agent)
//		{

//			int ql = Traverse.Create(__instance).Field("_queuedAgentCount").GetValue<int>();
//			if (ql > 10) { return false; }
//			else { Main.Log("Agent added to queue."); return true; }
//		}
//	}

//	[HarmonyPatch(typeof(LadderQueueManager), "OnTick")]
//	public static class Patch_LQM_OnTick
//	{
//		public static bool Prefix(LadderQueueManager __instance, float dt)
//		{
//			Traverse.Create(__instance).Field("_updatePeriod").SetValue(3f);
//			//Main.Log(__instance.
//			return true;
//		}
//	}

//	[HarmonyPatch(typeof(LadderQueueManager), "Initialize")]
//	public static class Patch_LQM_Init
//	{
//		public static void Postfix(LadderQueueManager __instance)
//		{
//			Traverse.Create(__instance).Field("_updatePeriod").SetValue(3f);
//			Traverse.Create(__instance).Field("_maxUserCount").SetValue(2);
//			Traverse.Create(__instance).Field("_blockUsage").SetValue(false);
//			Traverse.Create(__instance).Field("_distanceToStopUsing2d").SetValue(1f);

//		}
//	}

//	[HarmonyPatch(typeof(LadderQueueManager), "RemoveAgentFromQueueAtIndex")]
//	public static class Patch_RemoveAgentFromQueueAtIndex
//	{
//		public static void Prefix(LadderQueueManager __instance, int queueIndex)
//		{
//			Main.Log("Agent removed from queue index:" + queueIndex.ToString());
//		}
//	}
//}

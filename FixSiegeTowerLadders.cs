using System.Collections;
using HarmonyLib;
using TaleWorlds.MountAndBlade;

namespace FixSiegeAI
{
	//[HarmonyPatch(typeof(LadderQueueManager), "AddAgentToQueue")]
	//public static class Patch_AddAgentToQueue
	//{
	//	public static bool Prefix(LadderQueueManager __instance, Agent agent)
	//	{
			
	//		int ql = Traverse.Create(__instance).Field("_queuedAgentCount").GetValue<int>();
	//		if (ql > 10) { return false; }
	//		else { Main.Log("Agent added to queue."); return true; }
	//	}
	//}

	//[HarmonyPatch(typeof(LadderQueueManager), "OnTick")]
	//public static class Patch_LQM_OnTick
	//{
	//	public static bool Prefix(LadderQueueManager __instance, float dt)
	//	{
	//	 Traverse.Create(__instance).Field("_updatePeriod").SetValue(2f);
	//		return true;
	//	}
	//}

	//[HarmonyPatch(typeof(LadderQueueManager), "RemoveAgentFromQueueAtIndex")]
	//public static class Patch_RemoveAgentFromQueueAtIndex
	//{
	//	public static void Prefix(LadderQueueManager __instance, int queueIndex)
	//	{
	//		Main.Log("Agent removed from queue index:"+queueIndex.ToString());
	//	}
	//}
}

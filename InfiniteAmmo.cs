using HarmonyLib;
using TaleWorlds.MountAndBlade;

namespace FixSiegeAI
{
	// Just cancels the call to consume ammo :D
	[HarmonyPatch(typeof(RangedSiegeWeapon), "ConsumeAmmo")]
	public static class InfiniteAmmo
	{
		public static bool Prefix()
		{
			return false;
		}
	}
}
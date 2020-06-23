using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace FixSiegeAI
{
	// fixes what toggling the blue gear does, quite a clever workaround
	[HarmonyPatch(typeof(OrderController), "ToggleSideOrderUse")]
	public static class Patch_ToggleSideOrderUse
	{
		public static bool Prefix(IEnumerable<Formation> formations, UsableMachine usable)
		{
			// if every formation is not using, have every formation start
			if (formations.All(i => i.IsUsingMachine(usable) == false))
			{
				foreach (Formation f in formations) { f.StartUsingMachine(usable, true); };
				return false;
			}

			// if not every formation is using, have every formation start
			if (formations.Any(i => i.IsUsingMachine(usable) == true) && formations.Any(i => i.IsUsingMachine(usable) == false))
			{
				foreach (Formation f in formations) { f.StartUsingMachine(usable, true); };
				return false;
			}

			if (usable is SiegeTower || usable is BatteringRam)
			{
				// if every formation is following, have every formation stop
				if (formations.All(i => i.MovementOrder.TargetEntity == usable.WaitEntity))
				{
					foreach (Formation f in formations)
					{
						f.StopUsingMachine(usable, true);
						f.MovementOrder = MovementOrder.MovementOrderMove(f.OrderPosition);
					};
					return false;
				}
				// if every formation is using, have every formation follow
				if (formations.All(i => i.IsUsingMachine(usable) == true))
				{
					foreach (Formation f in formations)
					{
						var mo = Traverse.Create<MovementOrder>().Method("MovementOrderFollowEntity", usable.WaitEntity).GetValue<MovementOrder>();
						Main.Log("Formation following: " + (usable as SiegeWeapon).GetSiegeEngineType(), true);
						f.MovementOrder = mo;
					}
					return false;
				}
			}
			else
			{
				// if every formation is using, have every formation stop
				if (formations.All(i => i.IsUsingMachine(usable) == true))
				{
					foreach (Formation f in formations)
					{
						f.StopUsingMachine(usable, true);
					};
					return false;
				}
			}
			return false;
		}
	}

	// Fix ram blue gear
	[HarmonyPatch(typeof(BatteringRam), "GetOrder")]
	public static class Patch_BatteringRam_GetOrder
	{
		public static bool Prefix(ref OrderType __result, BatteringRam __instance, BattleSideEnum side)
		{
			if (side == BattleSideEnum.Defender)
			{
				__result = OrderType.AttackEntity;
				return false;
			}
			if (__instance.HasCompletedAction() | __instance.IsDestroyed | __instance.IsDeactivated | __instance.IsDisabled)
			{ __result = OrderType.None; return false; }
			__result = OrderType.Use;
			return false;
		}
	}

	// Fix siege tower blue gear
	[HarmonyPatch(typeof(SiegeTower), "GetOrder")]
	public static class Patch_SiegeTower_GetOrder
	{
		public static bool Prefix(ref OrderType __result, SiegeTower __instance, BattleSideEnum side)
		{
			if (side == BattleSideEnum.Defender)
			{
				__result = OrderType.AttackEntity;
				return false;
			}
			if ( __instance.IsDestroyed) 
				{ __result = OrderType.None; return false; }
			__result = OrderType.Use;
			return false;
		}
	}
}
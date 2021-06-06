using HarmonyLib;
using TaleWorlds.MountAndBlade;

namespace FixSiegeAI
{
	// Start using machine
	[HarmonyPatch(typeof(ModuleExtensions), "StartUsingMachine")]
	public static class Prefix_StartUsingMachine
	{
		public static bool Prefix(this Formation formation, UsableMachine usable, bool isPlayerOrder) // variable names must match exactly
		{
			if (formation.CountOfUnits < 1) { return true; }
			if (!Main.IsPIC(formation) | formation.IsAIControlled) { return true; }
			SiegeWeapon sw = (usable as SiegeWeapon);
			Traverse.Create(formation).Method("JoinDetachment", usable).GetValue();
			Main.Log("Formation starting using: " + sw.GetSiegeEngineType().ToString(), true);
			return false;
		}
	}

	// Stop using machine
	[HarmonyPatch(typeof(ModuleExtensions), "StopUsingMachine")]
	public static class Prefix_StopUsingMachine
	{
		public static bool Prefix(this Formation formation, UsableMachine usable, bool isPlayerOrder) // variable names must match exactly
		{
			if (formation.CountOfUnits < 1) { return true; }
			if (!Main.IsPIC(formation) | formation.IsAIControlled) { return true; }
			SiegeWeapon sw = (usable as SiegeWeapon);
			Main.Log("Formation stopping using: " + sw.GetSiegeEngineType().ToString(), true);
			Traverse.Create(formation).Method("LeaveDetachment", usable).GetValue();
			return false;
		}
	}

	// Stop troops using/following siege weapons
	[HarmonyPatch(typeof(MovementOrder), "OnApply")]
	public static class Patch_OnApply
	{
		public static bool Prefix(this Formation formation)
		{
			if (formation.CountOfUnits < 1) { return true; }
			if (!Main.IsPIC(formation) | formation.IsAIControlled) { return true; }
			foreach (SiegeWeapon sw in Mission.Current.ActiveMissionObjects.FindAllWithType<SiegeWeapon>())
			{
				if (sw.Side == Mission.Current.PlayerTeam.Side)
				{
					bool isUsing = formation.IsUsingMachine(sw);
					if (formation.MovementOrder.OrderType == OrderType.StandYourGround & isUsing)
					{
						sw.ForcedUse = false;
						formation.StopUsingMachine(sw, true);
						continue;
					};
				}
			}
			return true;
		}
	}
}
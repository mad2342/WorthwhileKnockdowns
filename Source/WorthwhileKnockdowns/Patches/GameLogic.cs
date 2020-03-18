using Harmony;
using BattleTech;

namespace WorthwhileKnockdowns.Patches
{
    class GameLogic
    {
        // Sprint
        [HarmonyPatch(typeof(Mech), nameof(Mech.CanSprint), MethodType.Getter)]
        public static class Mech_CanSprint_Patch
        {
            public static bool Prepare()
            {
                return WorthwhileKnockdowns.Settings.KnockdownPreventsSprinting;
            }

            public static void Postfix(Mech __instance, ref bool __result)
            {
                bool MechStoodUpThisRound = __instance.StoodUpThisRound;

                if (__result == true)
                {
                    // Additional check
                    if (MechStoodUpThisRound)
                    {
                        Logger.Info("[Mech_CanSprint_POSTFIX] Mech COULD sprint BUT just stood up, thus returning false.");
                        __result = false;
                    }
                }
            }
        }



        // Jump
        [HarmonyPatch(typeof(AbstractActor), nameof(AbstractActor.WorkingJumpjets), MethodType.Getter)]
        public static class AbstractActor_WorkingJumpjets_Patch
        {
            public static bool Prepare()
            {
                return WorthwhileKnockdowns.Settings.KnockdownPreventsJumping;
            }

            public static void Postfix(AbstractActor __instance, ref int __result)
            {
                bool ActorStoodUpThisRound = __instance.StoodUpThisRound;

                if (__result > 0)
                {
                    // Additional check
                    if (ActorStoodUpThisRound)
                    {
                        Logger.Info("[AbstractActor_WorkingJumpjets_POSTFIX] Mech HAS working jumpjets installed BUT just stood up, reporting zero working jumpjets to temporarily disable his jumping capability.");
                        __result = 0;
                    }
                }
            }
        }
    }
}

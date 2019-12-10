using System.Reflection;
using Harmony;
using BattleTech;
using System.IO;
using BattleTech.UI;
using Localize;
using System;



namespace WorthwhileKnockdowns
{
    public class WorthwhileKnockdowns
    {
        public static string LogPath;
        public static string ModDirectory;

        // BEN: Debug (0: nothing, 1: errors, 2:all)
        internal static int DebugLevel = 1;

        public static void Init(string directory, string settings)
        {
            ModDirectory = directory;

            LogPath = Path.Combine(ModDirectory, "WorthwhileKnockdowns.log");
            File.CreateText(WorthwhileKnockdowns.LogPath);

            var harmony = HarmonyInstance.Create("de.mad.WorthwhileKnockdowns");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(Mech), nameof(Mech.CanSprint), MethodType.Getter)]
    public static class Mech_CanSprint_Patch
    {
        public static void Postfix(Mech __instance, ref bool __result)
        {
            bool MechStoodUpThisRound = __instance.StoodUpThisRound;

            if (__result == true)
            {
                // Additional check
                if (MechStoodUpThisRound)
                {
                    //Logger.LogLine("[Mech_CanSprint_POSTFIX] Mech COULD sprint BUT just stood up, thus returning false.");
                    __result = false;
                }
            }
        }
    }

    /*
    [HarmonyPatch(typeof(Mech), nameof(Mech.JumpDistance), MethodType.Getter)]
    public static class Mech_JumpDistance_Patch
    {
        public static void Postfix(Mech __instance, ref float __result)
        {
            bool MechStoodUpThisRound = __instance.StoodUpThisRound;

            if (__result > 0f)
            {
                // Additional check
                if (MechStoodUpThisRound)
                {
                    __result = 0f;
                }
            }
        }
    }
    */

    [HarmonyPatch(typeof(AbstractActor), nameof(AbstractActor.WorkingJumpjets), MethodType.Getter)]
    public static class AbstractActor_WorkingJumpjets_Patch
    {
        public static void Postfix(AbstractActor __instance, ref int __result)
        {
            bool ActorStoodUpThisRound = __instance.StoodUpThisRound;

            if (__result > 0)
            {
                // Additional check
                if (ActorStoodUpThisRound)
                {
                    //Logger.LogLine("[AbstractActor_WorkingJumpjets_POSTFIX] Mech HAS working jumpjets installed BUT just stood up, reporting zero working jumpjets to temporarily disable his jumping capability.");
                    __result = 0;
                }
            }
        }
    }

    [HarmonyPatch(typeof(CombatHUDSidePanelHoverElement), "InitForSelectionState")]
    public static class CombatHUDSidePanelHoverElement_InitForSelectionState_Patch
    {
        public static void Postfix(CombatHUDSidePanelHoverElement __instance, SelectionType SelectionType, AbstractActor actor)
        {
            try
            {
                Logger.LogLine("----------------------------------------------------------------------------------------------------");
                Logger.LogLine("[CombatHUDSidePanelHoverElement_InitForSelectionState_POSTFIX] SelectionType: " + SelectionType);

                // THROWS EXCEPTION if there any empty ability slots(SelectionType.None) in MWTray!
                // Thus returning early here for all non-relevant SelectionTypes
                if (!(SelectionType == SelectionType.Sprint || SelectionType == SelectionType.Jump))
                {
                    return;
                }

                bool ActorIsProne = actor.IsProne;
                Logger.LogLine("[CombatHUDSidePanelHoverElement_InitForSelectionState_POSTFIX] ActorIsProne: " + ActorIsProne);
                bool ActorStoodUpThisRound = actor.StoodUpThisRound;
                Logger.LogLine("[CombatHUDSidePanelHoverElement_InitForSelectionState_POSTFIX] ActorStoodUpThisRound: " + ActorStoodUpThisRound);

                // Reset Text
                __instance.WarningText = new Text();

                if (SelectionType == SelectionType.Sprint)
                {
                    Mech mech = actor as Mech;
                    Logger.LogLine("[CombatHUDSidePanelHoverElement_InitForSelectionState_POSTFIX] mech.IsLegged: " + mech.IsLegged);
                    Logger.LogLine("[CombatHUDSidePanelHoverElement_InitForSelectionState_POSTFIX] mech.IsUnsteady: " + mech.IsUnsteady);

                    // Added check for prone as warning doesn't make sense if unit is still down. At that moment buttons are still disabled anyway...
                    if (mech != null && !actor.CanSprint && !ActorIsProne)
                    {
                        // First condition wins
                        if (mech.IsLegged)
                        {
                            __instance.WarningText = new Text("UNAVAILABLE - DESTROYED LEG PREVENTS SPRINTING", new object[0]);
                        }
                        // BEN: Added
                        else if (ActorStoodUpThisRound)
                        {
                            __instance.WarningText = new Text("UNAVAILABLE - RECENTLY KNOCKED DOWN UNITS CANNOT SPRINT", new object[0]);
                        }
                        // :NEB
                        else if (mech.IsUnsteady)
                        {
                            __instance.WarningText = new Text("UNAVAILABLE - UNSTEADY UNITS CANNOT SPRINT", new object[0]);
                        }
                    }
                }
                else if (SelectionType == SelectionType.Jump)
                {
                    // Only add warning text if it makes sense. For mechs that doesn't even have JJs installed a warning is not sensible
                    bool ActorHasJumpjetsInstalled = actor.jumpjets.Count > 0;
                    Logger.LogLine("[CombatHUDSidePanelHoverElement_InitForSelectionState_POSTFIX] ActorHasJumpjetsInstalled: " + ActorHasJumpjetsInstalled);

                    // Added check for prone as warning doesn't make sense if unit is still down. At that moment buttons are still disabled anyway...
                    if (ActorHasJumpjetsInstalled && !ActorIsProne)
                    {
                        if (ActorStoodUpThisRound)
                        {
                            __instance.WarningText = new Text("UNAVAILABLE - RECENTLY KNOCKED DOWN UNITS CANNOT JUMP", new object[0]);
                        }
                        else if (actor.WorkingJumpjets == 0)
                        {
                            __instance.WarningText = new Text("UNAVAILABLE - NO OPERATIONAL JUMPJETS LEFT", new object[0]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }
}


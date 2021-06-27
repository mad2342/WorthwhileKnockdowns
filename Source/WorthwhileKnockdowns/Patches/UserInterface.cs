using Harmony;
using BattleTech;
using BattleTech.UI;
using Localize;
using System;

namespace WorthwhileKnockdowns.Patches
{
    class UserInterface
    {
        internal static string SprintDescriptionHint = "\nCannot sprint if LEGGED, UNSTEADY, or just recovered from a KNOCKDOWN.";
        internal static string JumpDescriptionHint = "\nCannot jump with all jumpjets destroyed or if just recovered from a KNOCKDOWN.";

        [HarmonyPatch(typeof(CombatHUDSidePanelHoverElement), "InitForSelectionState")]
        public static class CombatHUDSidePanelHoverElement_InitForSelectionState_Patch
        {
            public static void Postfix(CombatHUDSidePanelHoverElement __instance, SelectionType SelectionType, AbstractActor actor)
            {
                try
                {
                    // THROWS EXCEPTION if there any empty ability slots(SelectionType.None) in MWTray!
                    // Thus returning early here for all non-relevant SelectionTypes
                    if (!(SelectionType == SelectionType.Sprint || SelectionType == SelectionType.Jump))
                    {
                        return;
                    }
                    // All of the following only concerns Sprint or Jump

                    Logger.Info("[CombatHUDSidePanelHoverElement_InitForSelectionState_POSTFIX] SelectionType: " + SelectionType);
                    Logger.Info("[CombatHUDSidePanelHoverElement_InitForSelectionState_POSTFIX] actor.IsProne: " + actor.IsProne);

                    // Always change description according to settings
                    if (SelectionType == SelectionType.Sprint && WorthwhileKnockdowns.Settings.KnockdownPreventsSprinting)
                    {
                        //__instance.Description = new Text(SprintDescription, new object[0]);
                        __instance.Description.Append(SprintDescriptionHint, new object[0]);
                    }
                    if (SelectionType == SelectionType.Jump && WorthwhileKnockdowns.Settings.KnockdownPreventsJumping)
                    {
                        //__instance.Description = new Text(JumpDescription, new object[0]);
                        __instance.Description.Append(JumpDescriptionHint, new object[0]);
                    }

                    // Always clear warnings
                    __instance.WarningText = new Text();

                    // Return early if actor is currently prone/shutdown as warning doesn't make sense if unit is still down. At that moment buttons are still disabled anyway...
                    if (actor.IsProne || actor.IsShutDown)
                    {
                        return;
                    }


                    if (SelectionType == SelectionType.Sprint && (actor is Mech mech))
                    {
                        Logger.Info("[CombatHUDSidePanelHoverElement_InitForSelectionState_POSTFIX] mech.IsLegged: " + mech.IsLegged);
                        Logger.Info("[CombatHUDSidePanelHoverElement_InitForSelectionState_POSTFIX] mech.IsUnsteady: " + mech.IsUnsteady);
                        Logger.Info("[CombatHUDSidePanelHoverElement_InitForSelectionState_POSTFIX] actor.StoodUpThisRound: " + actor.StoodUpThisRound);

                        if (!actor.CanSprint)
                        {
                            // Rearranged conditions, see original method...
                            if (mech.IsLegged)
                            {
                                __instance.WarningText = new Text("UNAVAILABLE - DESTROYED LEG PREVENTS SPRINTING", new object[0]);
                            }
                            // MAD: Added
                            else if (actor.StoodUpThisRound && WorthwhileKnockdowns.Settings.KnockdownPreventsSprinting)
                            {
                                __instance.WarningText = new Text("UNAVAILABLE - RECENTLY KNOCKED DOWN UNITS CANNOT SPRINT", new object[0]);
                            }
                            // :MAD
                            else if (mech.IsUnsteady)
                            {
                                __instance.WarningText = new Text("UNAVAILABLE - UNSTEADY UNITS CANNOT SPRINT", new object[0]);
                            }
                        }
                    }
                    else if (SelectionType == SelectionType.Jump)
                    {
                        bool actorHasJumpjetsInstalled = actor.jumpjets.Count > 0;
                        Logger.Info("[CombatHUDSidePanelHoverElement_InitForSelectionState_POSTFIX] actorHasJumpjetsInstalled: " + actorHasJumpjetsInstalled);
                        Logger.Info("[CombatHUDSidePanelHoverElement_InitForSelectionState_POSTFIX] actor.StoodUpThisRound: " + actor.StoodUpThisRound);

                        // Only add warning text if it makes any sense. For mechs that don't even have JJs installed a warning is not sensible...
                        if (actorHasJumpjetsInstalled)
                        {
                            // Note that condition order may not be switched as actor.WorkingJumpjets == 0 is also reported if actor.StoodUpThisRound (See GameLogic.cs)
                            if (actor.StoodUpThisRound && WorthwhileKnockdowns.Settings.KnockdownPreventsJumping)
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
                    Logger.Error(e);
                }
            }
        }
    }
}

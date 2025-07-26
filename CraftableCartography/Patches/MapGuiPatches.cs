﻿using HarmonyLib;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using static CraftableCartography.Lib.CCConstants;
using static CraftableCartography.Lib.ItemChecks;

namespace CraftableCartography.Patches
{
    [HarmonyPatch]
    internal static class MapGuiPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GuiDialogWorldMap), nameof(GuiDialogWorldMap.OnGuiOpened))]
        public static void OnMapOpened(GuiDialogWorldMap __instance)
        {
            Traverse traverse = Traverse.Create(__instance);

            ICoreClientAPI capi = traverse.Field("capi").GetValue<ICoreClientAPI>();

            /*
            MapChecker mapChecker = capi.ModLoader.GetModSystem<MapChecker>();

            if (mapChecker != null)
            {
                if (__instance.DialogType == EnumDialogType.HUD)
                {
                    if (!mapChecker.IsMinimapAllowed())
                    {
                        __instance.TryClose();
                    }
                } else if (__instance.DialogType == EnumDialogType.Dialog)
                {
                    if (!mapChecker.IsMapAllowed())
                    {
                        __instance.TryClose();
                    }
                }
            }
            */

            GuiElementMap elemMap = __instance.SingleComposer.GetElement("mapElem") as GuiElementMap;

            SavedPositions saved = capi.ModLoader.GetModSystem<CraftableCartographyModSystem>().LoadMapPos();
            
            elemMap.ZoomLevel = saved.zoomLevel;
            elemMap.CenterMapTo(saved.pos);

            //capi.ShowChatMessage("Loaded centre: " + pos.ToString() + " (" + pos.SubCopy(capi.World.DefaultSpawnPosition.AsBlockPos).ToString() + ")\nZoom level: " + zoom);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GuiElementMap), nameof(GuiElementMap.PostRenderInteractiveElements))]
        public static void StopMapTrackingPlayer(GuiElementMap __instance)
        {
            if (__instance != null)
            {
                Traverse traverse = Traverse.Create(__instance);

                if (traverse != null)
                {
                    ICoreClientAPI capi = __instance.Api;
                    if (capi != null)
                    {
                        if (capi.World != null)
                        {
                            if (capi.World.Player != null)
                            {
                                if (capi.World.Player.Entity != null)
                                {
                                    MapChecker mapChecker = capi.ModLoader.GetModSystem<MapChecker>();

                                    if (!HasJPS(capi.World.Player))
                                    {
                                        Vec3d playerPos = capi.World.Player.Entity.Pos.XYZ;
                                        traverse.Field("prevPlayerPos").GetValue<Vec3d>().Set(playerPos.X, playerPos.Y, playerPos.Z);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GuiElementMap), nameof(GuiElementMap.OnKeyDown))]
        public static bool SpaceBarPressedCheck(GuiElementMap __instance, ICoreClientAPI api, KeyEvent args)
        {
            if (args.KeyCode == 51)
            {
                if (!HasJPS(api.World.Player)) return false;
            }
            return true;
        }
    }
}
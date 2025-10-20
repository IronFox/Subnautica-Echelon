
using HarmonyLib;
using System.Collections;
using UnityEngine;


namespace Subnautica_Echelon
{

    [HarmonyPatch(typeof(VehicleUpgradeConsoleInput))]
    class VehicleUpgradeConsoleInputPatcher
    {
        const float openDuration = 0.5f;
        static float timeUntilClose = 0f;
        static Coroutine? closeDoorCor = null;
        public static IEnumerator closeDoorSoon(EchelonControl control)
        {
            while (timeUntilClose > 0)
            {
                timeUntilClose -= Time.deltaTime;
                yield return null;
            }
            if (control == null)
            {
                PLog.Warn($"Control is null in {nameof(closeDoorSoon)}");
            }
            else
            {
                PLog.Write($"Timeout passed. Closing");
                control.openUpgradeCover = false;
            }
            closeDoorCor = null;
            yield break;
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(VehicleUpgradeConsoleInput.OnHandHover))]
        public static void VehicleUpgradeConsoleInputOnHandHoverPostfix(VehicleUpgradeConsoleInput __instance, Sequence ___sequence)
        {
            if (__instance.GetComponentInParent<Echelon>() != null)
            {
                __instance.GetComponentInParent<EchelonControl>().openUpgradeCover = true;
                timeUntilClose = openDuration;
                if (closeDoorCor == null)
                {
                    closeDoorCor = UWE.CoroutineHost.StartCoroutine(closeDoorSoon(__instance.GetComponentInParent<EchelonControl>()));
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OpenPDA")]
        public static void VehicleUpgradeConsoleInputOpenPDAPostfix(VehicleUpgradeConsoleInput __instance, Sequence ___sequence)
        {
            if (__instance.GetComponentInParent<EchelonControl>() != null)
            {
                UWE.CoroutineHost.StopCoroutine(closeDoorCor);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnClosePDA")]
        public static void VehicleUpgradeConsoleInputOnClosePDAPostfix(VehicleUpgradeConsoleInput __instance, Sequence ___sequence)
        {
            if (__instance.GetComponentInParent<EchelonControl>() != null)
            {
                closeDoorCor = UWE.CoroutineHost.StartCoroutine(closeDoorSoon(__instance.GetComponentInParent<EchelonControl>()));
            }
        }
    }
}
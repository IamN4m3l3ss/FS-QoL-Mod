using HarmonyLib;
using UnityEngine;

namespace FS_FovChanger.Patches
{
    [HarmonyPatch(typeof(FPSController), "Update")]
    public static class CameraPatches
    {
        private static float _currentTilt;

        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(FPSController __instance)
        {
            if (__instance == null || __instance.playerCamera == null) return;

            // Handle Camera Tilt
            float target = Mechanics.AdvancedMovementPatch.targetTilt;
            _currentTilt = Mathf.Lerp(_currentTilt, target, Time.deltaTime * 8f);
            
            Vector3 euler = __instance.playerCamera.transform.localEulerAngles;
            __instance.playerCamera.transform.localRotation = Quaternion.Euler(euler.x, euler.y, _currentTilt);
        }
    }
}
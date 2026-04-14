using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace FS_FovChanger.Patches
{
    // --- THE ABSOLUTE LOCK ---
    // This patches the 'Setter' for FOV inside the Unity Engine itself.
    // This stops ANY script in the game from changing the FOV.
    [HarmonyPatch(typeof(Camera), nameof(Camera.fieldOfView), MethodType.Setter)]
    public static class UniversalFovLock
    {
        [HarmonyPrefix]
        public static void Prefix(ref float value)
        {
            // Whenever ANY code tries to set fov, we overwrite the value 
            // with our menu setting before it's applied.
            value = Config.SavedFov.Value;
        }
    }

    // --- LOGIC & SENSITIVITY ---
    [HarmonyPatch(typeof(FPSController), "Update")]
    public static class CameraLogicPatch
    {
        private static float _currentTilt;

        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)] // Run after the game's Update wipes our rotation
        public static void Postfix(FPSController __instance)
        {
            if (__instance == null || __instance.playerCamera == null) return;

            // Handle Camera Tilt (Re-applying it because the game wipes it every frame)
            float target = Mechanics.AdvancedMovementPatch.targetTilt;
            _currentTilt = Mathf.Lerp(_currentTilt, target, Time.deltaTime * 8f);
            
            Vector3 euler = __instance.playerCamera.transform.localEulerAngles;
            __instance.playerCamera.transform.localRotation = Quaternion.Euler(euler.x, euler.y, _currentTilt);
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var sensMethod = AccessTools.Method(typeof(CameraLogicPatch), nameof(GetSens));
            foreach (var code in instructions)
            {
                if (code.opcode == OpCodes.Ldc_R4 && (float)code.operand == 2f)
                    yield return new CodeInstruction(OpCodes.Call, sensMethod);
                else
                    yield return code;
            }
        }
        public static float GetSens() => Config.SavedSens.Value;
    }
}
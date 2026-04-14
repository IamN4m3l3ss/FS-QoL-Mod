using UnityEngine;
using HarmonyLib;

namespace FS_FovChanger.Mechanics
{
    [HarmonyPatch(typeof(FPSController), "Update")]
    public class AdvancedMovementPatch
    {
        public static bool canDoubleJump = true;
        public static bool isGrounded = true;
        public static float wallRunTimer = 0f;
        public static readonly float MaxWallRunTime = 1.5f;
        public static float targetTilt = 0f;

        public static int WallRunsRemaining = 2;
        public static bool IsWallRunning => _isWallRunning; // Public property for _isWallRunning
        private static bool _isWallRunning;
        private static bool _wasWallRunningLastFrame;
        private static float _airTime;

        [HarmonyPostfix]
        public static void Postfix(FPSController __instance)
        {
            if (__instance == null) return;
            var traverse = Traverse.Create(__instance);
            var charController = __instance.GetComponent<CharacterController>();
            if (charController == null) return;

            Vector3 currentMoveDir = traverse.Field("moveDirection").GetValue<Vector3>();
            isGrounded = charController.isGrounded;

            if (isGrounded)
            {
                canDoubleJump = true;
                _isWallRunning = false;
                _wasWallRunningLastFrame = false;
                _airTime = 0f;
                targetTilt = 0f;
                wallRunTimer = MaxWallRunTime;
                WallRunsRemaining = 2;
            }
            else
            {
                _airTime += Time.deltaTime;
                Transform camTransform = __instance.playerCamera.transform;
                Vector3 origin = __instance.transform.position + (Vector3.up * 1.0f);
                
                int layerMask = 1 << 0;
                bool wallLeft = Physics.Raycast(origin, -camTransform.right, out RaycastHit hitLeft, 1.2f, layerMask);
                bool wallRight = Physics.Raycast(origin, camTransform.right, out RaycastHit hitRight, 1.2f, layerMask);

                bool isValidWallLeft = wallLeft && Mathf.Abs(hitLeft.normal.y) < 0.1f && hitLeft.collider.gameObject.tag != "Tree";
                bool isValidWallRight = wallRight && Mathf.Abs(hitRight.normal.y) < 0.1f && hitRight.collider.gameObject.tag != "Tree";

                bool jumpInput = Input.GetButtonDown("Jump");

                if (Config.EnableWallRun.Value && (isValidWallLeft || isValidWallRight) && Input.GetKey(KeyCode.W) && wallRunTimer > 0f && WallRunsRemaining > 0)
                {
                    _isWallRunning = true;
                    canDoubleJump = true; // Reset double jump when wallrunning
                    currentMoveDir.y = -0.5f;
                    targetTilt = isValidWallLeft ? -15f : 15f;
                    wallRunTimer -= Time.deltaTime;

                    // Consume stamina while wallrunning
                    float currentStam = traverse.Field("currentStamina").GetValue<float>();
                    float runStaminaCost = __instance.runStaminaCost; // Access runStaminaCost from instance
                    currentStam -= runStaminaCost * Time.deltaTime;
                    if (currentStam <= 0.0f)
                    {
                        currentStam = 0.0f;
                        // Optionally, stop wallrunning if stamina runs out
                        // _isWallRunning = false; 
                    }
                    traverse.Field("currentStamina").SetValue(currentStam);


                    traverse.Field("wallJumpCooldownRemaining").SetValue(0.1f);

                    if (jumpInput)
                    {
                        WallRunsRemaining--;
                        wallRunTimer = MaxWallRunTime;
                        
                        currentMoveDir.y = __instance.jumpPower;
                        Vector3 pushDir = isValidWallLeft ? camTransform.right : -camTransform.right;
                        currentMoveDir += pushDir * 7f;

                        traverse.Field("wallJumpCooldownRemaining").SetValue(__instance.wallJumpCooldown);
                        
                        canDoubleJump = false;
                        jumpInput = false;

                        PlayJumpSound(__instance);
                    }
                }
                else
                {
                    _isWallRunning = false;
                    targetTilt = 0f;
                }

                if (_wasWallRunningLastFrame && !_isWallRunning && WallRunsRemaining > 0)
                {
                    WallRunsRemaining--;
                    wallRunTimer = MaxWallRunTime;
                }

                _wasWallRunningLastFrame = _isWallRunning;

                if (Config.EnableDoubleJump.Value && jumpInput && canDoubleJump && !_isWallRunning && _airTime > 0.2f)
                {
                    float currentStam = traverse.Field("currentStamina").GetValue<float>();
                    if (currentStam >= __instance.jumpStaminaCost)
                    {
                        currentMoveDir.y = __instance.jumpPower;
                        traverse.Field("currentStamina").SetValue(currentStam - __instance.jumpStaminaCost);
                        canDoubleJump = false;
                        PlayJumpSound(__instance);
                    }
                }
            }

            traverse.Field("moveDirection").SetValue(currentMoveDir);
        }

        private static void PlayJumpSound(FPSController instance)
        {
            AudioSource audioSource = instance.GetComponent<AudioSource>();
            if (audioSource != null && instance.jumpSound != null)
                audioSource.PlayOneShot(instance.jumpSound);
        }
    }
}
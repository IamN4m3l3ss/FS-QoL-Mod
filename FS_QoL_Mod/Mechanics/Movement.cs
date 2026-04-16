using UnityEngine;
using HarmonyLib;

namespace FS_FovChanger.Mechanics
{
    [HarmonyPatch(typeof(FPSController), "Update")]
    public class AdvancedMovementPatch
    {
        public static int _jumpsPerformedInAir;
        public static bool isGrounded;
        public static float wallRunTimer;
        public static readonly float MaxWallRunTime = 2.0f;
        public static float targetTilt;
        public static int WallRunsRemaining;
        public static bool IsWallRunning => _isWallRunning;
        
        private static bool _isWallRunning;
        private static bool _wasWallRunningLastFrame;
        private static float _airTime;
        private static float _wallrunCooldown;
        private static int _lastFrameJumped;

        [HarmonyPostfix]
        public static void Postfix(FPSController __instance)
        {
            if (__instance == null) return;
            var traverse = Traverse.Create(__instance);

            if (Config.DisableStamina.Value)
            {
                traverse.Field("currentStamina").SetValue(__instance.maxStamina);
            }

            if (Config.EnableDoubleJump.Value)
            {
                traverse.Field("wallJumpCooldownRemaining").SetValue(0.1f);
            }

            var charController = __instance.GetComponent<CharacterController>();
            if (charController == null) return;

            Vector3 currentMoveDir = traverse.Field("moveDirection").GetValue<Vector3>();
            isGrounded = charController.isGrounded;

            if (_wallrunCooldown > 0) _wallrunCooldown -= Time.deltaTime;

            if (isGrounded)
            {
                _jumpsPerformedInAir = 0;
                WallRunsRemaining = Config.WallrunCharges.Value;
                _isWallRunning = false;
                _wasWallRunningLastFrame = false;
                _airTime = 0f;
                targetTilt = 0f;
                wallRunTimer = MaxWallRunTime;
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
                bool jumpConsumed = false;

                if (Config.EnableWallRun.Value && (isValidWallLeft || isValidWallRight) && Input.GetKey(KeyCode.W) && wallRunTimer > 0f && WallRunsRemaining > 0 && _wallrunCooldown <= 0)
                {
                    _isWallRunning = true;
                    _jumpsPerformedInAir = 0;
                    currentMoveDir.y = -0.5f;
                    targetTilt = isValidWallLeft ? -15f : 15f;
                    wallRunTimer -= Time.deltaTime;

                    if (!Config.DisableStamina.Value)
                    {
                        float currentStam = traverse.Field("currentStamina").GetValue<float>();
                        currentStam -= __instance.runStaminaCost * Time.deltaTime;
                        if (currentStam <= 0.0f) {
                            currentStam = 0.0f;
                            _isWallRunning = false; 
                        }
                        traverse.Field("currentStamina").SetValue(currentStam);
                    }

                    if (jumpInput)
                    {
                        WallRunsRemaining--;
                        wallRunTimer = MaxWallRunTime;
                        _wallrunCooldown = 0.5f;
                        currentMoveDir.y = __instance.jumpPower;
                        Vector3 pushDir = isValidWallLeft ? camTransform.right : -camTransform.right;
                        currentMoveDir += pushDir * 10f;
                        jumpConsumed = true;
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

                if (Config.EnableDoubleJump.Value && jumpInput && !jumpConsumed && _jumpsPerformedInAir < Config.DoubleJumpCharges.Value && !_isWallRunning && _airTime > 0.2f)
                {
                    if (Time.frameCount == _lastFrameJumped) return;
                    
                    if (Config.DisableStamina.Value || traverse.Field("currentStamina").GetValue<float>() >= __instance.jumpStaminaCost)
                    {
                        _lastFrameJumped = Time.frameCount;
                        currentMoveDir.y = __instance.jumpPower;
                        if (!Config.DisableStamina.Value)
                        {
                            traverse.Field("currentStamina").SetValue(traverse.Field("currentStamina").GetValue<float>() - __instance.jumpStaminaCost);
                        }
                        _jumpsPerformedInAir++;
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
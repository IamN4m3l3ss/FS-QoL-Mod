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

        public static bool isUpdating;
        public static Vector3 _velocity;

        [HarmonyPrefix]
        public static void Prefix(FPSController __instance)
        {
            isUpdating = true;
            if (Config.EnableBhop.Value && Config.DisableStamina.Value)
            {
                var traverse = Traverse.Create(__instance);
                traverse.Field("currentStamina").SetValue(__instance.maxStamina);
            }
        }

        [HarmonyPostfix]
        public static void Postfix(FPSController __instance)
        {
            isUpdating = false;

            if (__instance == null) return;
            var traverse = Traverse.Create(__instance);
            var charController = __instance.GetComponent<CharacterController>();
            if (charController == null) return;

            isGrounded = charController.isGrounded;

            if (!Config.EnableBhop.Value)
            {
                _velocity = Vector3.zero;
            }

            if (Config.EnableBhop.Value)
            {
                if (Config.EnableDoubleJump.Value)
                {
                    traverse.Field("wallJumpCooldownRemaining").SetValue(0.1f);
                }
                
                if (!isGrounded && Config.EnableDoubleJump.Value && Input.GetButtonDown("Jump") && _jumpsPerformedInAir < Config.DoubleJumpCharges.Value)
                {
                    if (Time.frameCount != _lastFrameJumped)
                    {
                        _lastFrameJumped = Time.frameCount;
                        Vector3 gameMoveDir = traverse.Field("moveDirection").GetValue<Vector3>();
                        gameMoveDir.y = __instance.jumpPower;
                        traverse.Field("moveDirection").SetValue(gameMoveDir);
                        _jumpsPerformedInAir++;
                        PlayJumpSound(__instance);
                    }
                }
                return; 
            }

            // ============================================
            // Normal (Non-Bhop) Advanced Movement Logic
            // ============================================
            
            if (Config.DisableStamina.Value)
            {
                traverse.Field("currentStamina").SetValue(__instance.maxStamina);
            }

            if (Config.EnableDoubleJump.Value)
            {
                traverse.Field("wallJumpCooldownRemaining").SetValue(0.1f);
            }

            Vector3 currentMoveDir = traverse.Field("moveDirection").GetValue<Vector3>();

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

    [HarmonyPatch(typeof(CharacterController), "Move")]
    public class CharacterControllerMovePatch
    {
        [HarmonyPrefix]
        public static void Prefix(CharacterController __instance, ref Vector3 motion)
        {
            if (!AdvancedMovementPatch.isUpdating || !Config.EnableBhop.Value) return;

            var fpsController = __instance.GetComponent<FPSController>();
            if (fpsController == null) return;

            // 1. Extract the vanilla intended horizontal movement directly from what the game passed to Move()
            // This perfectly preserves shield debuffs, walk/run states, and external speed modifiers!
            Vector3 vanillaVel = motion / Time.deltaTime;
            Vector3 vanillaHoriz = new Vector3(vanillaVel.x, 0, vanillaVel.z);
            
            float wishSpeed = vanillaHoriz.magnitude;
            Vector3 wishDir = wishSpeed > 0 ? vanillaHoriz.normalized : Vector3.zero;

            bool isGrounded = __instance.isGrounded;
            bool isJumping = Input.GetButton("Jump");

            if (isGrounded && !isJumping)
            {
                AdvancedMovementPatch._jumpsPerformedInAir = 0;

                // Ground Friction
                float speed = AdvancedMovementPatch._velocity.magnitude;
                if (speed != 0)
                {
                    float drop = speed * 8f * Time.deltaTime;
                    AdvancedMovementPatch._velocity *= Mathf.Max(speed - drop, 0) / speed;
                }

                // Accelerate up to vanilla speed
                if (wishSpeed > 0)
                {
                    Accelerate(wishDir, wishSpeed, 10f * wishSpeed);
                }

                // STRICT CLAMP to prevent runaway ground speed.
                // If you miss a bhop and stay on the ground, your speed is instantly capped to your normal walk/run speed.
                if (AdvancedMovementPatch._velocity.magnitude > wishSpeed)
                {
                    AdvancedMovementPatch._velocity = AdvancedMovementPatch._velocity.normalized * wishSpeed;
                }
            }
            else
            {
                // Air Strafing Physics - Build up momentum via mouse synchronization
                float airWishSpeed = 1.5f; 
                if (wishSpeed > 0)
                {
                    Accelerate(wishDir, airWishSpeed, 150f * airWishSpeed);
                }
            }

            // Absolute max speed cap
            if (AdvancedMovementPatch._velocity.magnitude > 50f)
            {
                AdvancedMovementPatch._velocity = AdvancedMovementPatch._velocity.normalized * 50f;
            }

            // Replace horizontal motion with our fluid momentum physics
            motion.x = AdvancedMovementPatch._velocity.x * Time.deltaTime;
            motion.z = AdvancedMovementPatch._velocity.z * Time.deltaTime;
        }

        [HarmonyPostfix]
        public static void Postfix(CharacterController __instance)
        {
            if (!AdvancedMovementPatch.isUpdating || !Config.EnableBhop.Value) return;

            // 2. Sync our momentum with the physical collision result
            // This ensures hitting walls correctly kills your speed instead of sliding infinitely
            Vector3 actualVel = __instance.velocity;
            AdvancedMovementPatch._velocity.x = actualVel.x;
            AdvancedMovementPatch._velocity.z = actualVel.z;
        }

        private static void Accelerate(Vector3 wishDir, float wishSpeed, float accel)
        {
            float currentSpeed = Vector3.Dot(AdvancedMovementPatch._velocity, wishDir);
            float addSpeed = wishSpeed - currentSpeed;
            if (addSpeed <= 0) return;

            float accelSpeed = accel * Time.deltaTime;
            if (accelSpeed > addSpeed) accelSpeed = addSpeed;

            AdvancedMovementPatch._velocity += wishDir * accelSpeed;
        }
    }
}
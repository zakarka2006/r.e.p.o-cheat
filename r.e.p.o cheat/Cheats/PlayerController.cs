using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;

namespace r.e.p.o_cheat
{
    static class PlayerController
    {
        public static object playerControllerInstance;
        public static Type playerControllerType = Type.GetType("PlayerController, Assembly-CSharp");

        private static float desiredDelayMultiplier = 1f;
        private static float desiredRateMultiplier = 1f;

        private static void InitializePlayerController()
        {
            if (playerControllerType == null)
            {
                Hax2.Log1("PlayerController type not found.");
                return;
            }

            playerControllerInstance = GameHelper.FindObjectOfType(playerControllerType);
            if (playerControllerInstance == null)
            {
                Hax2.Log1("PlayerController instance not found in current scene.");
            }
            else
            {
                Hax2.Log1("PlayerController instance updated successfully.");
            }
        }

        public static void SetFlashlightIntensity(float value)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var playerAvatarScriptField = playerControllerType.GetField("playerAvatarScript", BindingFlags.Public | BindingFlags.Instance);
            if (playerAvatarScriptField != null)
            {
                var playerAvatarScriptInstance = playerAvatarScriptField.GetValue(playerControllerInstance);
                if (playerAvatarScriptInstance != null)
                {
                    var flashlightControllerField = playerAvatarScriptInstance.GetType().GetField("flashlightController", BindingFlags.Public | BindingFlags.Instance);
                    if (flashlightControllerField != null)
                    {
                        var flashlightControllerInstance = flashlightControllerField.GetValue(playerAvatarScriptInstance);
                        if (flashlightControllerInstance != null)
                        {
                            var baseIntensityField = flashlightControllerInstance.GetType().GetField("baseIntensity", BindingFlags.Public | BindingFlags.Instance);
                            if (baseIntensityField != null)
                            {
                                baseIntensityField.SetValue(flashlightControllerInstance, value);
                                Hax2.Log1($"BaseIntensity set to {value}");
                            }
                            else
                            {
                                Hax2.Log1("baseIntensity field not found in flashlightController.");
                            }
                        }
                        else
                        {
                            Hax2.Log1("flashlightController instance not found in playerAvatarScript.");
                        }
                    }
                    else
                    {
                        Hax2.Log1("flashlightController field not found in playerAvatarScript.");
                    }
                }
                else
                {
                    Hax2.Log1("playerAvatarScript instance is null.");
                }
            }
            else
            {
                Hax2.Log1("playerAvatarScript field not found in PlayerController.");
            }
        }

        public static void SetCrouchDelay(float value)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var crouchTimeMinField = playerControllerType.GetField("CrouchTimeMin", BindingFlags.NonPublic | BindingFlags.Instance);
            if (crouchTimeMinField != null)
            {
                crouchTimeMinField.SetValue(playerControllerInstance, value);
                Hax2.Log1($"CrouchTimeMin set to {value}");
            }
            else
            {
                Hax2.Log1("CrouchTimeMin field not found in PlayerController.");
            }
        }

        public static void SetJumpForce(float value)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var jumpForceField = playerControllerType.GetField("JumpForce", BindingFlags.NonPublic | BindingFlags.Instance);
            if (jumpForceField != null)
            {
                jumpForceField.SetValue(playerControllerInstance, value);
                Hax2.Log1($"JumpForce set to {value}");
            }
            else
            {
                Hax2.Log1("JumpForce field not found in PlayerController.");
            }
        }

        public static void SetExtraJumps(int value)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var jumpExtraField = playerControllerType.GetField("JumpExtra", BindingFlags.NonPublic | BindingFlags.Instance);
            if (jumpExtraField != null)
            {
                jumpExtraField.SetValue(playerControllerInstance, value);
                Hax2.Log1($"JumpExtra set to {value}");
            }
            else
            {
                Hax2.Log1("JumpExtra field not found in PlayerController.");
            }
        }

        public static void SetCustomGravity(float value)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var customGravityField = playerControllerType.GetField("CustomGravity", BindingFlags.NonPublic | BindingFlags.Instance);
            if (customGravityField != null)
            {
                customGravityField.SetValue(playerControllerInstance, value);
                Hax2.Log1($"CustomGravity set to {value}");
            }
            else
            {
                Hax2.Log1("CustomGravity field not found in PlayerController.");
            }
        }

        public static void GodMode()
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var playerAvatarScriptField = playerControllerType.GetField("playerAvatarScript", BindingFlags.Public | BindingFlags.Instance);
            if (playerAvatarScriptField != null)
            {
                var playerAvatarScriptInstance = playerAvatarScriptField.GetValue(playerControllerInstance);

                var playerHealthField = playerAvatarScriptInstance.GetType().GetField("playerHealth", BindingFlags.Public | BindingFlags.Instance);
                if (playerHealthField != null)
                {
                    var playerHealthInstance = playerHealthField.GetValue(playerAvatarScriptInstance);

                    var godModeField = playerHealthInstance.GetType().GetField("godMode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (godModeField != null)
                    {
                        bool currentGodMode = (bool)godModeField.GetValue(playerHealthInstance);
                        bool newGodModeState = !currentGodMode;
                        godModeField.SetValue(playerHealthInstance, newGodModeState);

                        Hax2.godModeActive = !newGodModeState;
                        Hax2.Log1("God Mode " + (newGodModeState ? "enabled" : "disabled"));
                    }
                    else
                    {
                        Hax2.Log1("godMode field not found in playerHealth.");
                    }
                }
                else
                {
                    Hax2.Log1("playerHealth field not found in playerAvatarScript.");
                }
            }
            else
            {
                Hax2.Log1("playerAvatarScript field not found in PlayerController.");
            }
        }

        public static void DecreaseStaminaRechargeDelay(float delayMultiplier, float rateMultiplier = 1f)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            desiredDelayMultiplier = delayMultiplier;
            desiredRateMultiplier = rateMultiplier;

            var sprintRechargeTimeField = playerControllerType.GetField("sprintRechargeTime", BindingFlags.NonPublic | BindingFlags.Instance);
            if (sprintRechargeTimeField != null)
            {
                float newRechargeTime = 1f * delayMultiplier;
                sprintRechargeTimeField.SetValue(playerControllerInstance, newRechargeTime);
                Hax2.Log1($"sprintRechargeTime set to {newRechargeTime} (multiplier: {delayMultiplier})");
            }
            else
            {
                Hax2.Log1("sprintRechargeTime field not found in PlayerController.");
            }

            var sprintRechargeAmountField = playerControllerType.GetField("sprintRechargeAmount", BindingFlags.NonPublic | BindingFlags.Instance);
            if (sprintRechargeAmountField != null)
            {
                float newRechargeAmount = 2f * rateMultiplier;
                sprintRechargeAmountField.SetValue(playerControllerInstance, newRechargeAmount);
                Hax2.Log1($"sprintRechargeAmount set to {newRechargeAmount} (multiplier: {rateMultiplier})");
            }
            else
            {
                Hax2.Log1("sprintRechargeAmount field not found in PlayerController.");
            }
        }
    }
}

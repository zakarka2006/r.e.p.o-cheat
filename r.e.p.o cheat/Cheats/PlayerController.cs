using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;

namespace r.e.p.o_cheat
{
     class PlayerController
    {
        public static object playerSpeedInstance;
        public static object reviveInstance;
        public static object enemyDirectorInstance;
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

        public static void GodMode()
        {
            var playerControllerType = Type.GetType("PlayerController, Assembly-CSharp");
            if (playerControllerType != null)
            {
                Hax2.Log1("PlayerController found.");

                var playerControllerInstance = GameHelper.FindObjectOfType(playerControllerType);
                if (playerControllerInstance != null)
                {
                    var playerAvatarScriptField = playerControllerInstance.GetType().GetField("playerAvatarScript", BindingFlags.Public | BindingFlags.Instance);
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
                else
                {
                    Hax2.Log1("playerControllerInstance not found.");
                }
            }
            else
            {
                Hax2.Log1("PlayerController type not found.");
            }
        }

public static void RemoveSpeed(float sliderValue)
{
    if (sliderValue < 0.1f) sliderValue = 0.1f;  // Prevents invalid values
    if (playerSpeedInstance == null)
    {
        Hax2.Log1("Player speed instance is null, skipping modification.");
        return;
    }

    var moveSpeedField = playerSpeedInstance.GetType().GetField("MoveSpeed", BindingFlags.Public | BindingFlags.Instance);
    if (moveSpeedField != null)
    {
        moveSpeedField.SetValue(playerSpeedInstance, sliderValue);
        Hax2.Log1("MoveSpeed set to " + sliderValue);
    }
    else
    {
        Hax2.Log1("MoveSpeed field not found in PlayerController.");
    }
}

public static void MaxStamina()
{
    var playerControllerType = Type.GetType("PlayerController, Assembly-CSharp");
    if (playerControllerType == null)
    {
        Hax2.Log1("PlayerController type not found.");
        return;
    }

    var playerControllerInstance = GameHelper.FindObjectOfType(playerControllerType);
    if (playerControllerInstance == null)
    {
        Hax2.Log1("PlayerController instance not found.");
        return;
    }

    var energyCurrentField = playerControllerInstance.GetType().GetField("EnergyCurrent", BindingFlags.Public | BindingFlags.Instance);
    if (energyCurrentField == null)
    {
        Hax2.Log1("EnergyCurrent field not found in PlayerController.");
        return;
    }

    int newStaminaValue = Hax2.stamineState ? 999999 : 40;
    energyCurrentField.SetValue(playerControllerInstance, newStaminaValue);
    Hax2.Log1("EnergyCurrent set to " + newStaminaValue);
}

        public static void DecreaseStaminaRechargeDelay(float delayMultiplier, float rateMultiplier = 1f)
{
    InitializePlayerController();
    if (playerControllerInstance == null)
    {
        Hax2.Log1("PlayerController instance is null, cannot modify stamina recharge.");
        return;
    }

    var sprintRechargeTimeField = playerControllerType.GetField("sprintRechargeTime", BindingFlags.NonPublic | BindingFlags.Instance);
    if (sprintRechargeTimeField != null)
    {
        sprintRechargeTimeField.SetValue(playerControllerInstance, Mathf.Clamp(delayMultiplier, 0.1f, 10f));
        Hax2.Log1($"sprintRechargeTime set to {delayMultiplier}");
    }
}

        public static void ReapplyStaminaSettings()
        {
            InitializePlayerController();
            if (playerControllerInstance != null)
            {
                DecreaseStaminaRechargeDelay(desiredDelayMultiplier, desiredRateMultiplier);
                Hax2.Log1("Reapplied stamina settings after scene change.");
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
                            var baseIntensityField = flashlightControllerInstance.GetType().GetField("baseIntensity", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (baseIntensityField != null)
                            {
                                baseIntensityField.SetValue(flashlightControllerInstance, value);
                                Hax2.Log1($"Flashlight BaseIntensity set to {value}");
                            }
                        }
                    }
                }
            }
        }

        public static void SetCrouchDelay(float value)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var crouchTimeMinField = playerControllerType.GetField("CrouchTimeMin", BindingFlags.Public | BindingFlags.Instance);
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
        public static void SetCrouchSpeed(float value)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var crouchTimeMinField = playerControllerType.GetField("CrouchSpeed", BindingFlags.Public | BindingFlags.Instance);
            if (crouchTimeMinField != null)
            {
                crouchTimeMinField.SetValue(playerControllerInstance, value);
                Hax2.Log1($"CrouchSpeed set to {value}");
            }
            else
            {
                Hax2.Log1("CrouchSpeed field not found in PlayerController.");
            }
        }

        public static void SetJumpForce(float value)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var jumpForceField = playerControllerType.GetField("JumpForce", BindingFlags.Public | BindingFlags.Instance);
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

            var customGravityField = playerControllerType.GetField("CustomGravity", BindingFlags.Public | BindingFlags.Instance);
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

        public static void SetCrawlDelay(float crawlDelay)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            // Aplicar localmente
            var crouchTimeMinField = playerControllerType.GetField("CrouchTimeMin", BindingFlags.Public | BindingFlags.Instance);
            if (crouchTimeMinField != null)
            {
                crouchTimeMinField.SetValue(playerControllerInstance, crawlDelay);
                Hax2.Log1($"CrouchTimeMin set locally to {crawlDelay}");
            }
            else
            {
                Hax2.Log1("CrouchTimeMin field not found in PlayerController.");
                return;
            }

            if (PhotonNetwork.IsConnected)
            {
                var photonViewField = playerControllerType.GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                PhotonView photonView = photonViewField != null ? (PhotonView)photonViewField.GetValue(playerControllerInstance) : null;

                if (photonView == null)
                {
                    var playerAvatarScriptField = playerControllerType.GetField("playerAvatarScript", BindingFlags.Public | BindingFlags.Instance);
                    if (playerAvatarScriptField != null)
                    {
                        var playerAvatarScriptInstance = playerAvatarScriptField.GetValue(playerControllerInstance);
                        photonView = playerAvatarScriptInstance?.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(playerAvatarScriptInstance) as PhotonView;
                    }
                }

                if (photonView != null)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        photonView.RPC("SetCrawlDelayRPC", RpcTarget.AllBuffered, crawlDelay);
                        Hax2.Log1($"Master Client set crawl delay to {crawlDelay} and synced via RPC.");
                    }
                    else
                    {
                        photonView.RPC("SetCrawlDelayRPC", RpcTarget.MasterClient, crawlDelay);
                        Hax2.Log1($"Requested Master Client to set crawl delay to {crawlDelay} via RPC.");
                    }
                }
                else
                {
                    Hax2.Log1("PhotonView not found for crawl delay synchronization.");
                }
            }
            else
            {
                Hax2.Log1("Not connected to Photon, crawl delay applied only locally.");
            }
        }

    }
}

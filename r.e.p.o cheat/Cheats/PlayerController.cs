using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Photon.Pun;
namespace r.e.p.o_cheat
{
    static class PlayerController
    {
        public static object playerSpeedInstance;
        static private object reviveInstance;
        static private object enemyDirectorInstance;
        private static object playerControllerInstance; 
        private static Type playerControllerType = Type.GetType("PlayerController, Assembly-CSharp");

        // Inicializar o cache na primeira chamada
        private static void InitializePlayerController()
        {
            if (playerControllerType == null)
            {
                Hax2.Log1("PlayerController type not found.");
                return;
            }
            if (playerControllerInstance == null)
            {
                playerControllerInstance = GameHelper.FindObjectOfType(playerControllerType);
                if (playerControllerInstance == null)
                {
                    Hax2.Log1("PlayerController instance not found.");
                }
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
                    var playerAvatarScriptField = playerControllerInstance.GetType().GetField("playerAvatarScript", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (playerAvatarScriptField != null)
                    {
                        var playerAvatarScriptInstance = playerAvatarScriptField.GetValue(playerControllerInstance);

                        var playerHealthField = playerAvatarScriptInstance.GetType().GetField("playerHealth", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (playerHealthField != null)
                        {
                            var playerHealthInstance = playerHealthField.GetValue(playerAvatarScriptInstance);

                            var godModeField = playerHealthInstance.GetType().GetField("godMode", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
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
            var playerInSpeedType = Type.GetType("PlayerController, Assembly-CSharp"); 
            if (playerInSpeedType != null)
            {
                Hax2.Log1("playerInSpeedType n é null");
                playerSpeedInstance = GameHelper.FindObjectOfType(playerInSpeedType);
                if (playerSpeedInstance != null)
                {
                    Hax2.Log1("playerSpeedInstance n é null");
                }
                else
                {
                    Hax2.Log1("playerSpeedInstance null");
                }
            }
            else
            {
                Hax2.Log1("playerInSpeedType null");
            }
            if (playerSpeedInstance != null)
            {
                Hax2.Log1("playerSpeedInstance n é null");

                var playerControllerType = playerSpeedInstance.GetType();

                var moveSpeedField1 = playerControllerType.GetField("MoveSpeed", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (moveSpeedField1 != null)
                {
                    moveSpeedField1.SetValue(playerSpeedInstance, sliderValue);
                    Hax2.Log1("MoveSpeed value set to " + sliderValue);
                }
                else
                {
                    Hax2.Log1("MoveSpeed field not found in PlayerController.");
                }
            }
            Thread.Sleep(1);
        }
        /* revive that works
        public static void Revive()
        {
            var enemyDirectorType = Type.GetType("EnemyDirector, Assembly-CSharp");
            if (enemyDirectorType != null)
            {
                enemyDirectorInstance = GameHelper.FindObjectOfType(enemyDirectorType);
                if (enemyDirectorInstance != null)
                {
                    Hax2.Log1("EnemyDirector encontrado.");

                    var setInvestigateMethod = enemyDirectorType.GetMethod("SetInvestigate");
                    if (setInvestigateMethod != null)
                    {
                        Vector3 spawnPosition = new Vector3(0f, 1f, 0f);
                        setInvestigateMethod.Invoke(enemyDirectorInstance, new object[] { spawnPosition, 999f });
                        Hax2.Log1("SetInvestigate chamado com sucesso.");
                    }
                    else
                    {
                        Hax2.Log1("Método SetInvestigate não encontrado.");
                    }
                }
                else
                {
                    Hax2.Log1("enemyDirectorInstance é null.");
                    return;
                }
            }
            else
            {
                Hax2.Log1("EnemyDirector não encontrado.");
                return;
            }

            var semiFuncType = Type.GetType("SemiFunc, Assembly-CSharp");
            if (semiFuncType != null)
            {
                var playerGetListMethod = semiFuncType.GetMethod("PlayerGetList", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (playerGetListMethod != null)
                {
                    var playerList = playerGetListMethod.Invoke(null, null) as IEnumerable<object>; 
                    if (playerList != null)
                    {
                        foreach (var playerAvatar in playerList)
                        {
                            var playerDeathHeadField = playerAvatar.GetType().GetField("playerDeathHead", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            if (playerDeathHeadField != null)
                            {
                                var playerDeathHeadInstance = playerDeathHeadField.GetValue(playerAvatar);
                                if (playerDeathHeadInstance != null)
                                {
                                    var inExtractionPointField = playerDeathHeadInstance.GetType().GetField("inExtractionPoint", System.Reflection.BindingFlags.NonPublic  | System.Reflection.BindingFlags.Instance);
                                    if (inExtractionPointField != null)
                                    {
                                        inExtractionPointField.SetValue(playerDeathHeadInstance, true);
                                        Hax2.Log1("inExtractionPoint definido para true.");
                                    }
                                    else
                                    {
                                        Hax2.Log1("Campo inExtractionPoint não encontrado.");
                                    }

                                    var reviveMethod = playerDeathHeadInstance.GetType().GetMethod("Revive");
                                    if (reviveMethod != null)
                                    {
                                        reviveMethod.Invoke(playerDeathHeadInstance, null);
                                        Hax2.Log1("Player revivido com sucesso.");
                                    }
                                    else
                                    {
                                        Hax2.Log1("Método Revive não encontrado.");
                                    }
                                }
                                else
                                {
                                    Hax2.Log1("playerDeathHeadInstance é null.");
                                }
                            }
                            else
                            {
                                Hax2.Log1("Campo playerDeathHead não encontrado.");
                            }
                        }
                    }
                    else
                    {
                        Hax2.Log1("PlayerGetList retornou null.");
                    }
                }
                else
                {
                    Hax2.Log1("Método PlayerGetList não encontrado.");
                }
            }
            else
            {
                Hax2.Log1("SemiFunc não encontrado.");
            }
        }*/

        public static void MaxStamina()
        {
            var playerControllerType = Type.GetType("PlayerController, Assembly-CSharp");
            if (playerControllerType != null)
            {
                Hax2.Log1("PlayerController found.");

                var playerControllerInstance = GameHelper.FindObjectOfType(playerControllerType);
                if (playerControllerInstance != null)
                {
                        var energyCurrentField = playerControllerInstance.GetType().GetField("EnergyCurrent", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (energyCurrentField != null)
                        {
                        if (Hax2.stamineState)
                        {
                            energyCurrentField.SetValue(playerControllerInstance, 999999);
                        }
                        else if (!Hax2.stamineState)
                        {
                            energyCurrentField.SetValue(playerControllerInstance, 40);
                        }

                            Hax2.Log1("EnergyCurrent set to " + 999999);
                        }
                        else
                        {
                            Hax2.Log1("EnergyCurrent field not found in playerAvatarScript.");
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

        public static void MaxStrength()
        {
            var playerControllerType = Type.GetType("PlayerController, Assembly-CSharp");
            if (playerControllerType == null)
            {
                Hax2.Log1("PlayerController type not found.");
                return;
            }
            Hax2.Log1("PlayerController type found.");

            var playerControllerInstance = GameHelper.FindObjectOfType(playerControllerType);
            if (playerControllerInstance == null)
            {
                Hax2.Log1("PlayerController instance not found.");
                return;
            }
            Hax2.Log1("PlayerController instance found.");

            var playerAvatarScriptField = playerControllerType.GetField("playerAvatarScript", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (playerAvatarScriptField == null)
            {
                Hax2.Log1("playerAvatarScript field not found in PlayerController.");
                return;
            }

            var playerAvatarScriptInstance = playerAvatarScriptField.GetValue(playerControllerInstance);
            if (playerAvatarScriptInstance == null)
            {
                Hax2.Log1("playerAvatarScript instance is null.");
                return;
            }
            Hax2.Log1("playerAvatarScript instance found.");

            var physGrabberField = playerAvatarScriptInstance.GetType().GetField("physGrabber", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (physGrabberField == null)
            {
                Hax2.Log1("physGrabber field not found in PlayerAvatarScript.");
                return;
            }
            var physGrabberInstance = physGrabberField.GetValue(playerAvatarScriptInstance);
            if (physGrabberInstance == null)
            {
                Hax2.Log1("physGrabber instance is null.");
                return;
            }
            Hax2.Log1("physGrabber instance found.");

            var steamIDField = playerAvatarScriptInstance.GetType().GetField("steamID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (steamIDField == null)
            {
                Hax2.Log1("steamID field not found in PlayerAvatarScript.");
                return;
            }
            string steamID = steamIDField.GetValue(playerAvatarScriptInstance) as string;
            if (string.IsNullOrEmpty(steamID))
            {
                Hax2.Log1("steamID is null or empty.");
                return;
            }
            Hax2.Log1($"steamID found: {steamID}");

            var grabStrengthField = physGrabberInstance.GetType().GetField("grabStrength", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (grabStrengthField != null)
            {
                grabStrengthField.SetValue(physGrabberInstance, Hax2.sliderValueStrength);
                Hax2.Log1($"grabStrength set locally to {Hax2.sliderValueStrength}");
            }
            else
            {
                Hax2.Log1("grabStrength field not found in PhysGrabber.");
            }

            var statsManagerType = Type.GetType("StatsManager, Assembly-CSharp");
            if (statsManagerType != null)
            {
                var statsManagerInstanceField = statsManagerType.GetField("instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var statsManagerInstance = statsManagerInstanceField?.GetValue(null);
                if (statsManagerInstance != null)
                {
                    var playerUpgradeStrengthField = statsManagerInstance.GetType().GetField("playerUpgradeStrength", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    var playerUpgradeStrength = playerUpgradeStrengthField?.GetValue(statsManagerInstance) as System.Collections.Generic.Dictionary<string, int>;
                    if (playerUpgradeStrength != null)
                    {
                        int newUpgradeValue = Mathf.FloorToInt(Hax2.sliderValueStrength / 0.2f);
                        playerUpgradeStrength[steamID] = newUpgradeValue;
                        Hax2.Log1($"playerUpgradeStrength[{steamID}] set to {newUpgradeValue}");
                    }
                    else
                    {
                        Hax2.Log1("playerUpgradeStrength dictionary not found in StatsManager.");
                    }
                }
                else
                {
                    Hax2.Log1("StatsManager instance not found.");
                }
            }
            else
            {
                Hax2.Log1("StatsManager type not found.");
            }

            var punManagerType = Type.GetType("PunManager, Assembly-CSharp");
            if (punManagerType != null)
            {
                var punManagerInstanceField = punManagerType.GetField("instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var punManagerInstance = punManagerInstanceField?.GetValue(null);
                if (punManagerInstance != null)
                {
                    var upgradeMethod = punManagerType.GetMethod("UpgradePlayerGrabStrength", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (upgradeMethod != null)
                    {
                        upgradeMethod.Invoke(punManagerInstance, new object[] { steamID });
                        Hax2.Log1($"Called PunManager.UpgradePlayerGrabStrength for steamID: {steamID}");
                    }
                    else
                    {
                        Hax2.Log1("UpgradePlayerGrabStrength method not found in PunManager.");
                    }
                }
                else
                {
                    Hax2.Log1("PunManager instance not found.");
                }
            }
            else
            {
                Hax2.Log1("PunManager type not found.");
            }
        }

        public static void DecreaseStaminaRechargeDelay(float delayMultiplier, float rateMultiplier = 1f)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            Hax2.Log1("Attempting to decrease stamina recharge delay.");

            var sprintRechargeTimeField = playerControllerType.GetField("sprintRechargeTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (sprintRechargeTimeField != null)
            {
                float defaultRechargeTime = 1f;
                float newRechargeTime = defaultRechargeTime * delayMultiplier;
                sprintRechargeTimeField.SetValue(playerControllerInstance, newRechargeTime);
                Hax2.Log1($"sprintRechargeTime set to {newRechargeTime} (multiplier: {delayMultiplier})");
            }
            else
            {
                Hax2.Log1("sprintRechargeTime field not found in PlayerController.");
            }

            var sprintRechargeAmountField = playerControllerType.GetField("sprintRechargeAmount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (sprintRechargeAmountField != null)
            {
                float defaultRechargeAmount = 2f;
                float newRechargeAmount = defaultRechargeAmount * rateMultiplier;
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

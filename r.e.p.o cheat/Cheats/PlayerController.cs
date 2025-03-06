using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading;
namespace r.e.p.o_cheat
{
    static class PlayerController
    {
        public static object playerSpeedInstance;
        static private object reviveInstance;
        static private object enemyDirectorInstance;

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
        public static void SendFirstPlayerToVoid()
        {
            var playerController = Type.GetType("PlayerController, Assembly-CSharp");
            if (playerController != null)
            {
                var playerControllerInstance = GameHelper.FindObjectOfType(playerController);
                {
                    if (playerControllerInstance != null)
                    {
                        Hax2.Log1("reviveInstance n é null.");
                        var damageMethod1 = playerControllerInstance.GetType().GetMethod("Revive");
                        if (damageMethod1 != null)
                        {
                            Vector3 spawnPosition = new Vector3(-9999f, -9999f, -99999f);
                            damageMethod1.Invoke(spawnPosition, new object[] { });
                            Hax2.Log1("Player Sent to Void");
                        } else
                        {
                            Hax2.Log1("piroquinha null");
                        }
                    }
                }
            }
            else
            {
                Hax2.Log1("reviveInstance null");
            }
        }
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
                            energyCurrentField.SetValue(playerControllerInstance, 999999);
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

    }
}

using System;
using System.Collections.Generic;
using UnityEngine;


namespace r.e.p.o_cheat
{
    static class Health_Player
    {
        static public object playerHealthInstance;
        static public object playerMaxHealthInstance;

        public static void HealPlayer(int healAmount)
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

                            var healMethod = playerHealthInstance.GetType().GetMethod("Heal");
                            if (healMethod != null)
                            {
                                healMethod.Invoke(playerHealthInstance, new object[] { healAmount, true });
                                Hax2.Log1("Healed player with " + healAmount + " HP.");
                            }
                            else
                            {
                                Hax2.Log1("Heal method not found in playerHealth.");
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


        public static void DamagePlayer(int damageAmount)
        {
            var playerControllerType = Type.GetType("PlayerController, Assembly-CSharp");
            if (playerControllerType != null)
            {
                Hax2.Log1("PlayerController found.");
                var playerControllerInstance = GameHelper.FindObjectOfType(playerControllerType);
                if (playerControllerInstance != null)
                {
                    Hax2.Log1("playerControllerInstance found.");
                    var playerAvatarScriptField = playerControllerInstance.GetType().GetField("playerAvatarScript", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (playerAvatarScriptField != null)
                    {
                        Hax2.Log1("playerAvatarScriptField found.");
                        var playerAvatarScriptInstance = playerAvatarScriptField.GetValue(playerControllerInstance);
                        var playerHealthField = playerAvatarScriptInstance.GetType().GetField("playerHealth", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (playerHealthField != null)
                        {
                            Hax2.Log1("playerHealthField found.");
                            var playerHealthInstance = playerHealthField.GetValue(playerAvatarScriptInstance);
                            var damageMethod = playerHealthInstance.GetType().GetMethod("Hurt");
                            if (damageMethod != null)
                            {
                                damageMethod.Invoke(playerHealthInstance, new object[] { damageAmount, true, -1 });
                                Hax2.Log1("Damaged player with " + damageAmount + " HP.");
                            }
                            else
                            {
                                Hax2.Log1("Hurt method not found in playerHealth.");
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

        public static void MaxHealth()
        {
            var playerControllerType = Type.GetType("PlayerController, Assembly-CSharp");
            if (playerControllerType != null)
            {
                Hax2.Log1("PlayerController found.");
                var playerControllerInstance = GameHelper.FindObjectOfType(playerControllerType);
                if (playerControllerInstance != null)
                {
                    Hax2.Log1("playerControllerInstance found.");
                    var playerAvatarScriptField = playerControllerInstance.GetType().GetField("playerAvatarScript", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (playerAvatarScriptField != null)
                    {
                        Hax2.Log1("playerAvatarScriptField found.");
                        var playerAvatarScriptInstance = playerAvatarScriptField.GetValue(playerControllerInstance);
                        var playerHealthField = playerAvatarScriptInstance.GetType().GetField("playerHealth", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (playerHealthField != null)
                        {
                            Hax2.Log1("playerHealthField found.");
                            var playerHealthInstance = playerHealthField.GetValue(playerAvatarScriptInstance);
                            var damageMethod = playerHealthInstance.GetType().GetMethod("UpdateHealthRPC");
                            if (damageMethod != null)
                            {
                                damageMethod.Invoke(playerHealthInstance, new object[] { 999999, 100, true });
                                Hax2.Log1("Damaged player with " + 999999 + " HP.");
                            }
                            else
                            {
                                Hax2.Log1("Hurt method not found in playerHealth.");
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

    }
}

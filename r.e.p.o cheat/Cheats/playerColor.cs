using System;
using System.Reflection;
using UnityEngine;
using Photon.Pun;

namespace r.e.p.o_cheat
{
    internal class playerColor
    {
        public static bool isRandomizing = false;
        private static float lastColorChangeTime = 0f;
        private static float changeInterval = 0.1f;

        private static Type colorControllerType;
        private static object colorControllerInstance;
        private static MethodInfo playerSetColorMethod;
        private static bool isInitialized = false;

        private static void Initialize()
        {
            if (isInitialized) return;

            colorControllerType = Type.GetType("PlayerAvatar, Assembly-CSharp");
            if (colorControllerType == null)
            {
                Hax2.Log1("colorControllerType (PlayerAvatar) not found.");
                return;
            }

            Hax2.Log1("colorControllerType (PlayerAvatar) found.");


            colorControllerInstance = null;
            var photonViews = UnityEngine.Object.FindObjectsOfType<PhotonView>();
            Hax2.Log1($"Found {photonViews.Length} PhotonViews in scene.");
            foreach (var photonView in photonViews)
            {
                if (photonView != null && photonView.IsMine)
                {
                    var playerAvatar = photonView.gameObject.GetComponent(colorControllerType);
                    if (playerAvatar != null)
                    {
                        colorControllerInstance = playerAvatar;
                        Hax2.Log1($"Local PlayerAvatar found: {photonView.gameObject.name}, Owner: {photonView.Owner?.NickName}");
                        break;
                    }
                }
            }

            if (colorControllerInstance == null)
            {
                Hax2.Log1("No local PlayerAvatar found for this client.");
                return;
            }

            playerSetColorMethod = colorControllerType.GetMethod("PlayerAvatarSetColor", BindingFlags.Public | BindingFlags.Instance);
            if (playerSetColorMethod == null)
            {
                Hax2.Log1("PlayerAvatarSetColor method not found in PlayerAvatar.");
                return;
            }

            isInitialized = true;
            Hax2.Log1("playerColor initialized successfully for local player.");
        }

        public static void colorRandomizer()
        {
            Initialize();

            if (!isInitialized || colorControllerInstance == null || playerSetColorMethod == null)
            {
                Hax2.Log1("Randomizer skipped: Initialization failed or instance/method missing.");
                return;
            }

            if (isRandomizing && Time.time - lastColorChangeTime >= changeInterval)
            {
                var colorIndex = new System.Random().Next(0, 30);
                try
                {
                    playerSetColorMethod.Invoke(colorControllerInstance, new object[] { colorIndex });
                    lastColorChangeTime = Time.time;
                    Hax2.Log1($"Local player color changed to index: {colorIndex}");
                }
                catch (Exception e)
                {
                    Hax2.Log1($"Error invoking PlayerAvatarSetColor: {e.Message}");
                }
            }
        }

        public static void Reset()
        {
            isInitialized = false;
            colorControllerType = null;
            colorControllerInstance = null;
            playerSetColorMethod = null;
            Hax2.Log1("playerColor reset.");
        }
    }
}
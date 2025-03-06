//player color function from https://github.com/svind1er
using System;
using System.Reflection;
using UnityEngine;

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
                Hax2.Log1("colorControllerType not found.");
                return;
            }

            Hax2.Log1("colorControllerType found.");
            colorControllerInstance = GameHelper.FindObjectOfType(colorControllerType);
            if (colorControllerInstance == null)
            {
                Hax2.Log1("colorControllerInstance not found.");
                return;
            }

            Hax2.Log1("colorControllerInstance found.");
            playerSetColorMethod = colorControllerType.GetMethod("PlayerAvatarSetColor");
            if (playerSetColorMethod == null)
            {
                Hax2.Log1("PlayerAvatarSetColor method not found in PlayerAvatar.");
                return;
            }

            isInitialized = true;
            Hax2.Log1("playerColor initialized successfully.");
        }

        public static void colorRandomizer()
        {
            Initialize();

            if (!isInitialized || colorControllerInstance == null || playerSetColorMethod == null)
            {
                return;
            }

            if (isRandomizing && Time.time - lastColorChangeTime >= changeInterval)
            {
                var colorIndex = new System.Random().Next(0, 30);
                playerSetColorMethod.Invoke(colorControllerInstance, new object[] { colorIndex });
                lastColorChangeTime = Time.time;
                Hax2.Log1($"Color changed to index: {colorIndex}");
            }
        }

        public static void Reset()
        {
            isInitialized = false;
            colorControllerType = null;
            colorControllerInstance = null;
            playerSetColorMethod = null;
        }
    }
}
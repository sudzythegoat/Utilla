using BepInEx;
using System;
using GorillaExtensions;
using UnityEngine;
using Utilla.HarmonyPatches;
using Utilla.Tools;
using Utilla.Utils;
using Utilla.Behaviours;

namespace Patcher
{
    public class Modifiers
    {
        public static void LoadModifiers()
        {
            GorillaLocomotion.Player.Instance.jumpMultiplier = 2.5f;
            GorillaLocomotion.Player.Instance.maxJumpSpeed = 4.5f;
            Player.Instance.transform.localScale += new Vector3(0.02f, 0.02f, 0.02f);
        }
    }
}

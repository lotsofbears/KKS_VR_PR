using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using KK_VR.Features;
using KK_VR.Fixes;
using KK_VR.Settings;
using KKAPI;
using KKAPI.MainGame;
using UnityEngine;
using VRGIN.Core;
using VRGIN.Helpers;

namespace KK_VR
{
    [BepInPlugin(GUID, Name, Version)]
    [BepInProcess(KoikatuAPI.StudioProcessName)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    public class VRPlugin : BaseUnityPlugin
    {
        public const string GUID = "kk.vr.studio";
        public const string Name = "StudioVR";
        public const string Version = Constants.Version;

        internal static new ManualLogSource Logger;

        private void Awake()
        {
            Logger = base.Logger;

            var settings = StudioSettings.Create(Config);

            if (Environment.CommandLine.Contains("--vr") || SteamVRDetector.IsRunning)
            {
                BepInExVrLogBackend.ApplyYourself();
                StartCoroutine(LoadDevice(settings));
            }
            CrossFader.Initialize(Config, enabled);
        }

        private IEnumerator LoadDevice(VRSettings settings)
        {
            //yield return new WaitUntil(() => Manager.Scene. initialized);
            //yield return new WaitUntil(() => Manager.Scene.initialized && Manager.Scene.LoadSceneName == "Title");
            var openVR = "OpenVR";
            if (UnityEngine.VR.VRSettings.loadedDeviceName != openVR)
            {
                UnityEngine.VR.VRSettings.LoadDeviceByName(openVR);
                yield return null;
            }
            UnityEngine.VR.VRSettings.enabled = true;
            while (UnityEngine.VR.VRSettings.loadedDeviceName != openVR)
            {
                yield return null;
            }
            while (true)
            {
                var rect = VRGIN.Native.WindowManager.GetClientRect();
                if (rect.Right - rect.Left > 0)
                {
                    break;
                }
                //VRLog.Info("waiting for the window rect to be non-empty");
                yield return null;
            }

            new Harmony(GUID).PatchAll(typeof(VRPlugin).Assembly);
            VRManager.Create<Interpreters.KoikStudioInterpreter>(new KoikContext(settings));

            VR.Manager.SetMode<StudioStandingMode>();

            VRFade.Create();
            GraphicRaycasterPatches.Initialize();

            // It's been reported in #28 that the game window defocues when
            // the game is under heavy load. We disable window ghosting in
            // an attempt to counter this.
            NativeMethods.DisableProcessWindowsGhosting();

            //if (SettingsManager.EnableBoop.Value)
            //{
            //    VRBoop.Initialize();
            //}
            Logger.LogInfo("Finished loading into VR mode!");
        }

        private static class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern void DisableProcessWindowsGhosting();
        }


    }
}

using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;
using VRGIN.Core;

namespace KK_VR.Settings
{
    public class StudioSettings
    {
        private const string SectionGeneral = "General";

        public static ConfigEntry<float> NearClipPlane { get; private set; }



        public static VRSettings Create(ConfigFile config)
        {
            var settings = new VRSettings();

            var ipdScale = config.Bind(SectionGeneral, "IPD Scale", 0.9f,
                new ConfigDescription(
                    "Scale of the camera. The lesser the bigger the world around appears",
                    new AcceptableValueRange<float>(0.25f, 4f)));
            Tie(ipdScale, v => settings.IPDScale = v);
        
            return settings;
        }

        private static void Tie<T>(ConfigEntry<T> entry, Action<T> set)
        {
            set(entry.Value);
            entry.SettingChanged += (_, _1) => set(entry.Value);
        }
    }
}

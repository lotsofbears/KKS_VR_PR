using System;
using System.Collections.Generic;
using UnityEngine;

namespace KKSCharaStudioVR
{
    public class KKSCharaStudioVRGUI : MonoBehaviour
    {
        private int windowID = 8731;

        private Rect windowRect = new Rect(Screen.width - 150, Screen.height - 100, 150f, 100f);

        private string windowTitle = "KKS_CharaStudioVR";

        private Texture2D windowBG = new Texture2D(1, 1, TextureFormat.ARGB32, false);

        private Dictionary<string, GUIStyle> styleBackup = new Dictionary<string, GUIStyle>();

        private void OnGUI()
        {
        }

        private void FuncWindowGUI(int winID)
        {
            styleBackup = new Dictionary<string, GUIStyle>();
            BackupGUIStyle("Button");
            BackupGUIStyle("Label");
            BackupGUIStyle("Toggle");
            try
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    GUI.FocusControl("");
                    GUI.FocusWindow(winID);
                }

                GUI.enabled = true;
                var style = GUI.skin.GetStyle("Button");
                style.normal.textColor = Color.white;
                style.alignment = TextAnchor.MiddleCenter;
                var style2 = GUI.skin.GetStyle("Label");
                style2.normal.textColor = Color.white;
                style2.alignment = TextAnchor.MiddleLeft;
                style2.wordWrap = false;
                var style3 = GUI.skin.GetStyle("Toggle");
                style3.normal.textColor = Color.white;
                style3.onNormal.textColor = Color.white;
                GUILayout.BeginVertical();
                GUILayout.EndVertical();
                GUI.DragWindow();
            }
            catch (Exception value)
            {
                Console.WriteLine(value);
            }
            finally
            {
                RestoreGUIStyle("Button");
                RestoreGUIStyle("Label");
                RestoreGUIStyle("Toggle");
            }
        }

        private void BackupGUIStyle(string name)
        {
            var value = new GUIStyle(GUI.skin.GetStyle(name));
            styleBackup.Add(name, value);
        }

        private void RestoreGUIStyle(string name)
        {
            if (styleBackup.ContainsKey(name))
            {
                var gUIStyle = styleBackup[name];
                var style = GUI.skin.GetStyle(name);
                style.normal.textColor = gUIStyle.normal.textColor;
                style.alignment = gUIStyle.alignment;
                style.wordWrap = gUIStyle.wordWrap;
            }
        }
    }
}
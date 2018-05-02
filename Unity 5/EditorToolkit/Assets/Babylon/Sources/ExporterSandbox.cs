using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEditor;
using UnityEngine;

namespace Unity3D2Babylon
{
    public class ExporterSandbox : EditorWindow
    {
        SandboxType sandboxType = SandboxType.Retail;

        bool keepGeneratorOpen = true;

        List<string> logs = new List<string>();

        Vector2 scrollPosLog;

        public void OnInitialize()
        {
            maxSize = new Vector2(500, 400);
            minSize = this.maxSize;
        }

        void OnEnable()
        {
            titleContent = new GUIContent("Switch Local Windows Sandbox");
        }

        public void OnGUI()
        {
            EditorGUILayout.Space();

            sandboxType = (SandboxType)EditorGUILayout.EnumPopup("Windows Sandbox Type:", sandboxType, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(sandboxType == SandboxType.Retail);
            ExporterWindow.exportationOptions.CustomWindowsSandbox = EditorGUILayout.TextField("Custom Sandbox Name", ExporterWindow.exportationOptions.CustomWindowsSandbox);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();

            keepGeneratorOpen = EditorGUILayout.Toggle("Keep Generator Open:", keepGeneratorOpen);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUILayout.Label("Important: You Must Run Unity As Administrator", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.Space();
            Tools.DrawGuiBox(new Rect(2, 115, 496, 252), Color.white);
            scrollPosLog = EditorGUILayout.BeginScrollView(scrollPosLog, GUILayout.ExpandWidth(true), GUILayout.Height(250));
            foreach (var log in this.logs)
            {
                var bold = log.StartsWith("*");
                GUILayout.Label(bold ? log.Remove(0, 1) : log, bold ? (EditorStyles.boldLabel) : EditorStyles.label);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();
            if (GUILayout.Button("Switch Local Windows Sandbox"))
            {
                SwitchSandbox();
            }
            EditorGUILayout.Space();
        }

        void SwitchSandbox()
        {
            if (sandboxType == SandboxType.Custom && String.IsNullOrEmpty(ExporterWindow.exportationOptions.CustomWindowsSandbox))
            {
                ExporterWindow.ShowMessage("You must enter a custom sandbox.", "Babylon.js");
                return;
            }
            this.logs.Clear();
            ExporterWindow.ReportProgress(1, "Switching windows sandbox... This may take a while.");
            string sandbox = (sandboxType == SandboxType.Custom) ? ExporterWindow.exportationOptions.CustomWindowsSandbox : "RETAIL";
            string command = "\"" + Path.Combine(Application.dataPath, "Babylon/Plugins/Windows/SwitchSandbox.cmd") + "\"";
            this.logs.Add("Switching windows sandbox to: " + sandbox);
            int result = Tools.ExecuteProcess(command, sandbox, ref this.logs);
            EditorUtility.ClearProgressBar();
            if (result != 0)
            {
                ExporterWindow.ShowMessage("Failed to switch windows sandbox.", "Babylon.js");
            }
            if (this.keepGeneratorOpen == false)
            {
                this.Close();
            }
        }

        public void OnInspectorUpdate()
        {
            this.Repaint();
        }
    }
}
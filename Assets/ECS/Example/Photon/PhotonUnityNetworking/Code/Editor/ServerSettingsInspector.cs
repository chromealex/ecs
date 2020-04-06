// ----------------------------------------------------------------------------
// <copyright file="ServerSettingsInspector.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   This is a custom editor for the ServerSettings scriptable object.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------

using System;
using UnityEditor;
using UnityEngine;

using Photon.Pun;

using ExitGames.Client.Photon;

[CustomEditor(typeof (ServerSettings))]
public class ServerSettingsInspector : Editor
{
    private string versionPhoton;

    private string[] regionsPrefsList;

    private string prefLabel;
    private const string notAvailableLabel = "n/a";

    private string rpcCrc;
    private bool showRpcs;

    public void Awake()
    {
        this.versionPhoton = System.Reflection.Assembly.GetAssembly(typeof(PhotonPeer)).GetName().Version.ToString();
    }


    public override void OnInspectorGUI()
    {
        SerializedObject sObj = new SerializedObject(this.target);
        ServerSettings settings = this.target as ServerSettings;


        EditorGUI.BeginChangeCheck();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(new GUIContent("Version", "Version of PUN and Photon3Unity3d.dll."));
        EditorGUILayout.LabelField("Pun: " + PhotonNetwork.PunVersion + " Photon lib: " + this.versionPhoton);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(PhotonGUI.HelpIcon, GUIStyle.none))
        {
            Application.OpenURL(PhotonEditor.UrlPunSettings);
        }
        EditorGUILayout.EndHorizontal();



        SerializedProperty showSettingsProp = this.serializedObject.FindProperty("ShowSettings");
        bool showSettings = EditorGUILayout.Foldout(showSettingsProp.boolValue, new GUIContent("Settings", "Core Photon Server/Cloud settings."));
        if (showSettings != settings.ShowSettings)
        {
            showSettingsProp.boolValue = showSettings;
        }

        if (showSettingsProp.boolValue)
        {
            SerializedProperty settingsSp = this.serializedObject.FindProperty("AppSettings");

            EditorGUI.indentLevel++;

            //Realtime APP ID
            this.BuildAppIdField(settingsSp.FindPropertyRelative("AppIdRealtime"));

            if (PhotonEditorUtils.HasChat)
            {
                this.BuildAppIdField(settingsSp.FindPropertyRelative("AppIdChat"));
            }
            if (PhotonEditorUtils.HasVoice)
            {
                this.BuildAppIdField(settingsSp.FindPropertyRelative("AppIdVoice"));
            }

            EditorGUILayout.PropertyField(settingsSp.FindPropertyRelative("AppVersion"));
            EditorGUILayout.PropertyField(settingsSp.FindPropertyRelative("UseNameServer"), new GUIContent("Use Name Server", "Photon Cloud requires this checked.\nUncheck for Photon Server SDK (OnPremise)."));
            EditorGUILayout.PropertyField(settingsSp.FindPropertyRelative("FixedRegion"), new GUIContent("Fixed Region", "Photon Cloud setting, needs a Name Server.\nDefine one region to always connect to.\nLeave empty to use the best region from a server-side region list."));
            EditorGUILayout.PropertyField(settingsSp.FindPropertyRelative("Server"), new GUIContent("Server", "Typically empty for Photon Cloud.\nFor Photon OnPremise, enter your host name or IP. Also uncheck \"Use Name Server\" for older Photon OnPremise servers."));
            EditorGUILayout.PropertyField(settingsSp.FindPropertyRelative("Port"), new GUIContent("Port","Use 0 for Photon Cloud.\nOnPremise uses 5055 for UDP and 4530 for TCP."));
            EditorGUILayout.PropertyField(settingsSp.FindPropertyRelative("Protocol"), new GUIContent("Protocol", "Use UDP where possible.\nWSS works on WebGL and Xbox exports.\nDefine WEBSOCKET for use on other platforms."));
            EditorGUILayout.PropertyField(settingsSp.FindPropertyRelative("EnableLobbyStatistics"), new GUIContent("Enable Lobby Statistics", "When using multiple room lists (lobbies), the server can send info about their usage."));
            EditorGUILayout.PropertyField(settingsSp.FindPropertyRelative("NetworkLogging"), new GUIContent("Network Logging", "Log level for the Photon libraries."));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("PunLogging"), new GUIContent("PUN Logging", "Log level for the PUN layer."));
        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("EnableSupportLogger"), new GUIContent("Enable Support Logger", "Logs additional info for debugging.\nUse this when you submit bugs to the Photon Team."));
        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("RunInBackground"), new GUIContent("Run In Background", "Enables apps to keep the connection without focus. Android and iOS ignore this."));
        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("StartInOfflineMode"), new GUIContent("Start In Offline Mode", "Simulates an online connection.\nPUN can be used as usual."));


        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(new GUIContent("Best Region Preference", "Clears the Best Region of the editor.\n.Best region is used if Fixed Region is empty."));

        if (!string.IsNullOrEmpty(PhotonNetwork.BestRegionSummaryInPreferences))
        {
            this.regionsPrefsList = PhotonNetwork.BestRegionSummaryInPreferences.Split(';');
            if (this.regionsPrefsList == null || this.regionsPrefsList.Length == 0 || string.IsNullOrEmpty(this.regionsPrefsList[0]))
            {
                this.prefLabel = notAvailableLabel;
            }
            else
            {
                this.prefLabel = string.Format("'{0}' ping:{1}ms ", this.regionsPrefsList[0], this.regionsPrefsList[1]);
            }
        }
        else
        {
            this.prefLabel = notAvailableLabel;
        }

        GUILayout.Label(this.prefLabel, GUILayout.ExpandWidth(false));

        if (GUILayout.Button("Reset", EditorStyles.miniButton))
        {
            ServerSettings.ResetBestRegionCodeInPreferences();
        }

        if (GUILayout.Button("Edit WhiteList", EditorStyles.miniButton))
        {
            Application.OpenURL("https://dashboard.photonengine.com/en-US/App/RegionsWhitelistEdit/" + PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime);

        }

        EditorGUILayout.EndHorizontal();



        this.showRpcs = EditorGUILayout.Foldout(this.showRpcs, new GUIContent("RPCs", "RPC shortcut list."));

        if (this.showRpcs)
        {
            // first time check to get the rpc has proper
            if (string.IsNullOrEmpty(this.rpcCrc))
            {
                this.rpcCrc = this.RpcListHashCode().ToString("X");
            }


            EditorGUI.indentLevel++;


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("List CRC");

            EditorGUI.indentLevel--;
            if (GUILayout.Button(PhotonGUI.CopyIcon, GUIStyle.none,GUILayout.ExpandWidth(false)))
            {
                Debug.Log("RPC-List HashCode copied into your ClipBoard: " + this.rpcCrc + ". Make sure clients that send each other RPCs have the same RPC-List.");
                EditorGUIUtility.systemCopyBuffer = this.rpcCrc;
            }
            EditorGUILayout.SelectableLabel(this.rpcCrc, GUILayout.MaxHeight(16),GUILayout.ExpandWidth(false));


            EditorGUI.indentLevel++;
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("List Tools");
            if (GUILayout.Button("Refresh RPCs", EditorStyles.miniButton))
            {
                PhotonEditor.UpdateRpcList();
                this.Repaint();
            }

            if (GUILayout.Button("Clear RPCs", EditorStyles.miniButton))
            {
                PhotonEditor.ClearRpcList();
            }

            EditorGUILayout.EndHorizontal();


            SerializedProperty sRpcs = sObj.FindProperty("RpcList");
            EditorGUILayout.PropertyField(sRpcs, true);

            EditorGUI.indentLevel--;
        }


        if (EditorGUI.EndChangeCheck())
        {
            sObj.ApplyModifiedProperties();
            this.serializedObject.ApplyModifiedProperties();

            // cache the rpc hash
            this.rpcCrc = this.RpcListHashCode().ToString("X");
        }
    }


    private int RpcListHashCode()
    {
        // this is a hashcode generated to (more) easily compare this Editor's RPC List with some other
        int hashCode = PhotonNetwork.PhotonServerSettings.RpcList.Count + 1;
        foreach (string s in PhotonNetwork.PhotonServerSettings.RpcList)
        {
            int h1 = s.GetHashCode();
            hashCode = ((h1 << 5) + h1) ^ hashCode;
        }

        return hashCode;
    }


    private void BuildAppIdField(SerializedProperty property)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(property);
        string appId = property.stringValue;
        string url = "https://dashboard.photonengine.com/en-US/PublicCloud";
        if (!string.IsNullOrEmpty(appId))
        {
            url = string.Format("https://dashboard.photonengine.com/en-US/App/Manage/{0}", appId);
        }
        if (GUILayout.Button("Dashboard", EditorStyles.miniButton, GUILayout.Width(70)))
        {
            Application.OpenURL(url);
        }
        EditorGUILayout.EndHorizontal();
    }
}
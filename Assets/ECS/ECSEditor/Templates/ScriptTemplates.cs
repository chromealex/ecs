using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using System.Collections.Generic;

namespace ME.ECSEditor {

    public static class ScriptTemplates {

        public const int CREATE_MENU_PRIORITY = 90;
        private const int CREATE_PROJECT_PRIORITY = ScriptTemplates.CREATE_MENU_PRIORITY + 0;
        private const int CREATE_MODULE_PRIORITY = ScriptTemplates.CREATE_MENU_PRIORITY + 1;
        private const int CREATE_SYSTEM_PRIORITY = ScriptTemplates.CREATE_MENU_PRIORITY + 2;
        private const int CREATE_ENTITY_PRIORITY = ScriptTemplates.CREATE_MENU_PRIORITY + 3;
        private const int CREATE_COMPONENT_PRIORITY = ScriptTemplates.CREATE_MENU_PRIORITY + 4;
        private const int CREATE_COMPONENT_ONCE_PRIORITY = ScriptTemplates.CREATE_MENU_PRIORITY + 5;
        private const int CREATE_COMPONENT_SHARED_PRIORITY = ScriptTemplates.CREATE_MENU_PRIORITY + 6;
        private const int CREATE_COMPONENT_SHARED_ONCE_PRIORITY = ScriptTemplates.CREATE_MENU_PRIORITY + 7;
        private const int CREATE_MARKER_PRIORITY = ScriptTemplates.CREATE_MENU_PRIORITY + 8;
        private const int CREATE_FEATURE_PRIORITY = ScriptTemplates.CREATE_MENU_PRIORITY + 9;

        internal class DoCreateScriptAsset : EndNameEditAction {

            private static UnityEngine.Object CreateScriptAssetFromTemplate(string pathName, string resourceFile) {

                var str1 = resourceFile.Replace("#NOTRIM#", "");
                var withoutExtension = System.IO.Path.GetFileNameWithoutExtension(pathName);
                var str2 = str1.Replace("#NAME#", withoutExtension);
                var str3 = withoutExtension.Replace(" ", "");
                var str4 = str2.Replace("#SCRIPTNAME#", str3);
                string templateContent;
                if (char.IsUpper(str3, 0)) {
                    var newValue = char.ToLower(str3[0]).ToString() + str3.Substring(1);
                    templateContent = str4.Replace("#SCRIPTNAME_LOWER#", newValue);
                } else {
                    var newValue = "my" + (object)char.ToUpper(str3[0]) + str3.Substring(1);
                    templateContent = str4.Replace("#SCRIPTNAME_LOWER#", newValue);
                }

                return DoCreateScriptAsset.CreateScriptAssetWithContent(pathName, templateContent);

            }

            private static string SetLineEndings(string content, LineEndingsMode lineEndingsMode) {

                string replacement;
                switch (lineEndingsMode) {
                    case LineEndingsMode.OSNative:
                        replacement = Application.platform != RuntimePlatform.WindowsEditor ? "\n" : "\r\n";
                        break;

                    case LineEndingsMode.Unix:
                        replacement = "\n";
                        break;

                    case LineEndingsMode.Windows:
                        replacement = "\r\n";
                        break;

                    default:
                        replacement = "\n";
                        break;
                }

                content = System.Text.RegularExpressions.Regex.Replace(content, "\\r\\n?|\\n", replacement);
                return content;

            }

            private static UnityEngine.Object CreateScriptAssetWithContent(string pathName, string templateContent) {

                templateContent = DoCreateScriptAsset.SetLineEndings(templateContent, EditorSettings.lineEndingsForNewScripts);
                var fullPath = System.IO.Path.GetFullPath(pathName);
                var utF8Encoding = new System.Text.UTF8Encoding(true);
                System.IO.File.WriteAllText(fullPath, templateContent, (System.Text.Encoding)utF8Encoding);
                AssetDatabase.ImportAsset(pathName);
                return AssetDatabase.LoadAssetAtPath(pathName, typeof(UnityEngine.Object));

            }

            public override void Action(int instanceId, string pathName, string resourceFile) {

                ProjectWindowUtil.ShowCreatedAsset(DoCreateScriptAsset.CreateScriptAssetFromTemplate(pathName, resourceFile));

            }

        }

        private static Texture2D scriptIcon = EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;

        internal static void Create(string fileName, string templateName, System.Collections.Generic.Dictionary<string, string> customDefines = null, bool allowRename = true) {

            var obj = Selection.activeObject;
            var path = AssetDatabase.GetAssetPath(obj);
            if (System.IO.File.Exists(path) == true) {
                path = System.IO.Path.GetDirectoryName(path);
            }

            if (string.IsNullOrEmpty(path) == true) {
                path = "Assets/";
            }
            
            ScriptTemplates.Create(path, fileName, templateName, customDefines, allowRename);
            
        }
        
        internal static void Create(string path, string fileName, string templateName, System.Collections.Generic.Dictionary<string, string> customDefines = null, bool allowRename = true) {

            var stateTypeStr = "StateClassType";
            var type = typeof(ME.ECS.IStateBase);
            var types = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => type.IsAssignableFrom(p)).ToArray();
            if (types.Length > 0) {

                var stateType = types[0];
                stateTypeStr = stateType.Name;

            }

            var templatePath = Resources.Load<TextAsset>(templateName);
            if (templatePath == null) {
                
                Debug.LogError("Template was not found at path " + templateName);
                return;

            }

            var content = templatePath.text;
            if (customDefines != null) {

                foreach (var def in customDefines) {

                    content = content.Replace("#" + def.Key + "#", def.Value);

                }

            }
            
            content = content.Replace(@"#NAMESPACE#", path.Replace("Assets/", "").Replace("/", "."));
            content = content.Replace(@"#STATENAME#", stateTypeStr);

            if (allowRename == true) {

                var defaultNewFileName = fileName;
                var image = ScriptTemplates.scriptIcon;
                ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<DoCreateScriptAsset>(), defaultNewFileName, image,
                                                                        content);

            } else {

                var fullDir = path + "/" + fileName;
                System.IO.File.WriteAllText(fullDir, content);
                AssetDatabase.ImportAsset(fullDir);
                
            }

        }

        internal static void CreateEmptyDirectory(string path, string dir) {

            var fullDir = path + "/" + dir;
            System.IO.Directory.CreateDirectory(fullDir);
            System.IO.File.WriteAllText(fullDir + "/.dummy", string.Empty);
            AssetDatabase.ImportAsset(fullDir);

        }

        internal static void CreatePrefab(string path, string name, string guid) {
            
            var prefabPath = path + "/" + name + ".prefab";
            
            var content = @"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &6009573824331432541
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 6971210752520958191}
  - component: {fileID: 4958016069329523290}
  m_Layer: 0
  m_Name: GameObject
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &6971210752520958191
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 6009573824331432541}
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: -0.114168406, y: 0, z: 2.8584266}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_Children: []
  m_Father: {fileID: 0}
  m_RootOrder: 0
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!114 &4958016069329523290
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 6009573824331432541}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: #GUID#, type: 3}
  m_Name: 
  m_EditorClassIdentifier:
";
            
            System.IO.File.WriteAllText(prefabPath, content.Replace("#GUID#", guid));
            AssetDatabase.ImportAsset(prefabPath);
            
        }

        [UnityEditor.MenuItem("Assets/Create/ME.ECS/Initialize Project", priority = ScriptTemplates.CREATE_PROJECT_PRIORITY)]
        public static void CreateProject() {

            var obj = Selection.activeObject;
            var path = AssetDatabase.GetAssetPath(obj);
            if (System.IO.File.Exists(path) == true) {
                path = System.IO.Path.GetDirectoryName(path);
            }

            if (string.IsNullOrEmpty(path) == true) {
                path = "Assets";
            }

            var projectName = System.IO.Path.GetFileName(path);
            projectName = projectName.Replace(".", "");
            projectName = projectName.Replace(" ", "");
            projectName = projectName.Replace("_", "");
            var stateName = projectName + "State";
            var defines = new Dictionary<string, string>() { { "STATENAME", stateName }, { "PROJECTNAME", projectName } };

            ScriptTemplates.CreateEmptyDirectory(path, "Modules");
            ScriptTemplates.CreateEmptyDirectory(path, "Systems");
            ScriptTemplates.CreateEmptyDirectory(path, "Components");
            ScriptTemplates.CreateEmptyDirectory(path, "Markers");
            ScriptTemplates.CreateEmptyDirectory(path, "Features");
            ScriptTemplates.CreateEmptyDirectory(path, "Entities");
            ScriptTemplates.CreateEmptyDirectory(path, "Views");

            ScriptTemplates.Create(path, projectName + "State.cs", "00-StateTemplate", defines, allowRename: false);
            ScriptTemplates.Create(path, projectName + "Initializer.cs", "00-InitializerTemplate", defines, allowRename: false);
            
            ScriptTemplates.Create(path, "Modules/FPSModule.cs", "00-FPSModuleTemplate", defines, allowRename: false);
            ScriptTemplates.Create(path, "Modules/NetworkModule.cs", "00-NetworkModuleTemplate", defines, allowRename: false);
            ScriptTemplates.Create(path, "Modules/StatesHistoryModule.cs", "00-StatesHistoryModuleTemplate", defines, allowRename: false);

            var guid = AssetDatabase.AssetPathToGUID(path + "/" + projectName + "Initializer.cs");
            if (string.IsNullOrEmpty(guid) == false) ScriptTemplates.CreatePrefab(path, projectName + "Initializer", guid);

        }

        [UnityEditor.MenuItem("Assets/Create/ME.ECS/Module", priority = ScriptTemplates.CREATE_MODULE_PRIORITY)]
        public static void CreateModule() {

            ScriptTemplates.Create("New Module.cs", "01-ModuleTemplate");

        }

        [UnityEditor.MenuItem("Assets/Create/ME.ECS/System", priority = ScriptTemplates.CREATE_SYSTEM_PRIORITY)]
        public static void CreateSystem() {

            ScriptTemplates.Create("New System.cs", "11-SystemTemplate");

        }

        [UnityEditor.MenuItem("Assets/Create/ME.ECS/Entity", priority = ScriptTemplates.CREATE_ENTITY_PRIORITY)]
        public static void CreateEntity() {

            ScriptTemplates.Create("New Entity.cs", "21-EntityTemplate");

        }

        [UnityEditor.MenuItem("Assets/Create/ME.ECS/Component", priority = ScriptTemplates.CREATE_COMPONENT_PRIORITY)]
        public static void CreateComponent() {

            ScriptTemplates.Create("New Component.cs", "31-ComponentTemplate");

        }

        [UnityEditor.MenuItem("Assets/Create/ME.ECS/Component (Once)", priority = ScriptTemplates.CREATE_COMPONENT_ONCE_PRIORITY)]
        public static void CreateComponentOnce() {

            ScriptTemplates.Create("New Component.cs", "41-ComponentOnceTemplate");

        }

        [UnityEditor.MenuItem("Assets/Create/ME.ECS/Shared Component", priority = ScriptTemplates.CREATE_COMPONENT_SHARED_PRIORITY)]
        public static void CreateComponentShared() {

            ScriptTemplates.Create("New Shared Component.cs", "32-ComponentSharedTemplate");

        }

        [UnityEditor.MenuItem("Assets/Create/ME.ECS/Shared Component (Once)", priority = ScriptTemplates.CREATE_COMPONENT_SHARED_ONCE_PRIORITY)]
        public static void CreateComponentSharedOnce() {

            ScriptTemplates.Create("New Shared Component.cs", "42-ComponentOnceSharedTemplate");

        }

        [UnityEditor.MenuItem("Assets/Create/ME.ECS/Marker", priority = ScriptTemplates.CREATE_MARKER_PRIORITY)]
        public static void CreateMarker() {

            ScriptTemplates.Create("New Marker.cs", "51-MarkerTemplate");

        }

        [UnityEditor.MenuItem("Assets/Create/ME.ECS/Feature", priority = ScriptTemplates.CREATE_FEATURE_PRIORITY)]
        public static void CreateFeature() {

            ScriptTemplates.Create("New Feature.cs", "61-FeatureTemplate");

        }

    }

}
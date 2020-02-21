using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;

namespace ME.ECSEditor {

    public static class ScriptTemplates {

        public const int CREATE_MENU_PRIORITY = 90;
        private const int CREATE_MODULE_PRIORITY = ScriptTemplates.CREATE_MENU_PRIORITY + 1;
        private const int CREATE_SYSTEM_PRIORITY = ScriptTemplates.CREATE_MENU_PRIORITY + 2;
        private const int CREATE_ENTITY_PRIORITY = ScriptTemplates.CREATE_MENU_PRIORITY + 3;
        private const int CREATE_COMPONENT_PRIORITY = ScriptTemplates.CREATE_MENU_PRIORITY + 4;
        private const int CREATE_COMPONENT_ONCE_PRIORITY = ScriptTemplates.CREATE_MENU_PRIORITY + 5;
        private const int CREATE_COMPONENT_SHARED_PRIORITY = ScriptTemplates.CREATE_MENU_PRIORITY + 6;
        private const int CREATE_COMPONENT_SHARED_ONCE_PRIORITY = ScriptTemplates.CREATE_MENU_PRIORITY + 7;
        private const int CREATE_MARKER_PRIORITY = ScriptTemplates.CREATE_MENU_PRIORITY + 8;

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

        internal static void Create(string fileName, string templateName) {

            var obj = Selection.activeObject;
            var path = AssetDatabase.GetAssetPath(obj);
            if (System.IO.File.Exists(path) == true) {
                path = System.IO.Path.GetDirectoryName(path);
            }

            if (string.IsNullOrEmpty(path) == true) {
                path = "Assets/";
            }

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
            content = content.Replace(@"#NAMESPACE#", path.Replace("Assets/", "").Replace("/", "."));
            content = content.Replace(@"#STATENAME#", stateTypeStr);
            var defaultNewFileName = fileName;
            var image = ScriptTemplates.scriptIcon;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, (EndNameEditAction)ScriptableObject.CreateInstance<DoCreateScriptAsset>(), defaultNewFileName, image,
                                                                    content);

        }

        [UnityEditor.MenuItem("Assets/Create/ME.ECS/Module", priority = ScriptTemplates.CREATE_MODULE_PRIORITY)]
        public static void CreateModuleClass() {

            ScriptTemplates.Create("New Module.cs", "01-ModuleTemplate");

        }

        [UnityEditor.MenuItem("Assets/Create/ME.ECS/System", priority = ScriptTemplates.CREATE_SYSTEM_PRIORITY)]
        public static void CreateSystemClass() {

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

    }

}
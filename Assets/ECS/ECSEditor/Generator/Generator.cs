using System.Collections.Generic;
using UnityEditor;

namespace ME.ECSEditor {

    public class Generator : AssetPostprocessor {

        private static string MENU_ITEM_AUTO;
        private static string CONTENT_ITEM;
        private static string CONTENT_ITEM2;
        private static string CONTENT_ITEM3;
        private static string FILE_NAME;
        private static string TEMPLATE;
        private static System.Type SEARCH_TYPE;
        private static string PREFS_KEY;
        private static string DIRECTORY_CONTAINS;
        private static bool AUTO_COMPILE_DEFAULT;

        protected static void Set(string MENU_ITEM_AUTO, string CONTENT_ITEM, string FILE_NAME, string TEMPLATE, System.Type SEARCH_TYPE, string PREFS_KEY,
                                  string DIRECTORY_CONTAINS, bool AUTO_COMPILE_DEFAULT, string CONTENT_ITEM2 = null, string CONTENT_ITEM3 = null) {

            Generator.MENU_ITEM_AUTO = MENU_ITEM_AUTO;
            Generator.CONTENT_ITEM = CONTENT_ITEM;
            Generator.CONTENT_ITEM2 = CONTENT_ITEM2;
            Generator.CONTENT_ITEM3 = CONTENT_ITEM3;
            Generator.FILE_NAME = FILE_NAME;
            Generator.TEMPLATE = TEMPLATE;
            Generator.SEARCH_TYPE = SEARCH_TYPE;
            Generator.PREFS_KEY = PREFS_KEY;
            Generator.DIRECTORY_CONTAINS = DIRECTORY_CONTAINS;
            Generator.AUTO_COMPILE_DEFAULT = AUTO_COMPILE_DEFAULT;

        }

        protected static void OnPostprocessAllAssetsGen(string[] importedAssets, string[] deletedAssets,
                                                        string[] movedAssets, string[] movedFromAssetPaths) {

            foreach (var importedAsset in deletedAssets) {

                if (importedAsset.EndsWith(".cs") == true && importedAsset.EndsWith(Generator.FILE_NAME) == false && importedAsset.Contains(Generator.DIRECTORY_CONTAINS) == true) {

                    Generator.OnAfterAssemblyReload(true);
                    return;

                }

            }

        }

        protected static void OnAfterAssemblyReload(bool delete) {

            if (Generator.AutoGenerateValidate() == false) {
                return;
            }

            var asms = UnityEditor.AssetDatabase.FindAssets("t:asmdef");
            foreach (var asm in asms) {

                var asmPath = UnityEditor.AssetDatabase.GUIDToAssetPath(asm);
                var asmNamePath = System.IO.Path.GetDirectoryName(asmPath);
                if (System.IO.Directory.Exists(asmNamePath) == false) continue;

                if (delete == true) {

                    var fullDir = asmNamePath + "/" + Generator.FILE_NAME;
                    if (System.IO.File.Exists(fullDir) == true) {

                        AssetDatabase.DeleteAsset(fullDir);
                        AssetDatabase.ImportAsset(fullDir, ImportAssetOptions.ForceUpdate);
                        AssetDatabase.Refresh();

                    }

                } else {

                    Generator.CompileDirectory(asmNamePath);

                }

            }

        }

        private static bool AutoGenerateValidate() {

            Menu.SetChecked(Generator.MENU_ITEM_AUTO, EditorPrefs.GetBool(Generator.PREFS_KEY, Generator.AUTO_COMPILE_DEFAULT));
            return EditorPrefs.GetBool(Generator.PREFS_KEY, Generator.AUTO_COMPILE_DEFAULT);

        }

        protected static void AutoGenerateCheck() {

            EditorPrefs.SetBool(Generator.PREFS_KEY, !Generator.AutoGenerateValidate());
            Menu.SetChecked(Generator.MENU_ITEM_AUTO, Generator.AutoGenerateValidate());

        }

        public static bool IsValidToCompile() {
            
            var obj = Selection.activeObject;
            if (obj == null) return false;
            
            string path = null;
            if (obj != null) {

                path = AssetDatabase.GetAssetPath(obj);
                if (System.IO.File.Exists(path) == true) {
                    path = System.IO.Path.GetDirectoryName(path);
                }

            }

            if (string.IsNullOrEmpty(path) == true) {
                path = "Assets/";
            }
            
            var asms = UnityEditor.AssetDatabase.FindAssets("t:asmdef", new[] { path });
            if (asms.Length == 0) return false;

            return true;

        }

        public static void Compile() {

            string path = null;
            var obj = Selection.activeObject;
            if (obj != null) {

                path = AssetDatabase.GetAssetPath(obj);
                if (System.IO.File.Exists(path) == true) {
                    path = System.IO.Path.GetDirectoryName(path);
                }

            }

            if (string.IsNullOrEmpty(path) == true) {
                path = "Assets/";
            }

            Generator.CompileDirectory(path);

        }

        private static void CompileDirectory(string dir) {

            if (System.IO.Directory.Exists(dir) == false) return;
            
            var itemStr = Generator.CONTENT_ITEM;
            var itemStr2 = Generator.CONTENT_ITEM2;
            var itemStr3 = Generator.CONTENT_ITEM3;

            var listEntities = new List<System.Type>();
            var listComponents = new List<System.Type>();
            var asms = UnityEditor.AssetDatabase.FindAssets("t:asmdef", new[] { dir });
            foreach (var asm in asms) {

                var output = string.Empty;
                var output2 = string.Empty;
                var output3 = string.Empty;
                listEntities.Clear();
                listComponents.Clear();

                var asmPath = UnityEditor.AssetDatabase.GUIDToAssetPath(asm);
                var asmNamePath = System.IO.Path.GetDirectoryName(asmPath);
                if (System.IO.Directory.Exists(asmNamePath) == false) continue;

                var splitted = asmNamePath.Split('/');
                var asmName = splitted[splitted.Length - 1];

                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies) {

                    if (assembly.GetName().Name != "ECSAssembly") {

                        var allTypes = assembly.GetTypes();
                        foreach (var type in allTypes) {

                            var interfaces = type.GetInterfaces();
                            foreach (var @interface in interfaces) {

                                if (@interface.IsAssignableFrom(Generator.SEARCH_TYPE) == true) {

                                    if (listEntities.Contains(type) == false) listEntities.Add(type);

                                }

                                if (@interface.IsAssignableFrom(typeof(ME.ECS.IComponent)) == true) {

                                    if (listComponents.Contains(type) == false) listComponents.Add(type);

                                }

                            }

                        }

                        //break;

                    }

                }

                foreach (var type in listComponents) {
                    
                    if (itemStr3 != null) {

                        var resItem3 = itemStr3;
                        resItem3 = resItem3.Replace("#PROJECTNAME#", asmName);
                        resItem3 = resItem3.Replace("#STATENAME#", asmName + "State");
                        resItem3 = resItem3.Replace("#TYPENAME#", type.FullName);
                        resItem3 = resItem3.Replace("#ISTAG#", "false");

                        output3 += resItem3;

                    }

                }

                foreach (var entityType in listEntities) {

                    var hasFields = (entityType.GetFields(System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).Length > 0);
                    
                    var resItem = itemStr;
                    resItem = resItem.Replace("#PROJECTNAME#", asmName);
                    resItem = resItem.Replace("#STATENAME#", asmName + "State");
                    resItem = resItem.Replace("#TYPENAME#", entityType.FullName);
                    resItem = resItem.Replace("#ISTAG#", hasFields == true ? "false" : "true");

                    output += resItem;

                    if (itemStr2 != null) {

                        var resItem2 = itemStr2;
                        resItem2 = resItem2.Replace("#PROJECTNAME#", asmName);
                        resItem2 = resItem2.Replace("#STATENAME#", asmName + "State");
                        resItem2 = resItem2.Replace("#TYPENAME#", entityType.FullName);
                        resItem2 = resItem2.Replace("#ISTAG#", hasFields == true ? "false" : "true");

                        output2 += resItem2;

                    }

                    if (itemStr3 != null) {

                        var resItem3 = itemStr3;
                        resItem3 = resItem3.Replace("#PROJECTNAME#", asmName);
                        resItem3 = resItem3.Replace("#STATENAME#", asmName + "State");
                        resItem3 = resItem3.Replace("#TYPENAME#", entityType.FullName);
                        resItem3 = resItem3.Replace("#ISTAG#", hasFields == true ? "false" : "true");

                        output3 += resItem3;

                    }

                }

                ME.ECSEditor.ScriptTemplates.Create(asmNamePath, Generator.FILE_NAME, Generator.TEMPLATE, new Dictionary<string, string>() { { "CONTENT", output }, { "CONTENT2", output2 }, { "CONTENT3", output3 } }, false);
                
            }

        }

    }

}
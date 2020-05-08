using System.Collections.Generic;
using UnityEditor;

namespace ME.ECSEditor {

    public class AOTGenerator : AssetPostprocessor {

        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {

            foreach (var importedAsset in deletedAssets) {

                if (importedAsset.EndsWith(".cs") == true && importedAsset.EndsWith("aot.compiler.gen.cs") == false && importedAsset.Contains("/Entities/") == true) {

                    AOTGenerator.OnAfterAssemblyReload(delete: true);
                    return;

                }

            }
            
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded() {
            
            AOTGenerator.OnAfterAssemblyReload(delete: false);
            
        }

        private static void OnAfterAssemblyReload(bool delete) {

            var asms = UnityEditor.AssetDatabase.FindAssets("t:asmdef");
            foreach (var asm in asms) {
                
                var asmPath = UnityEditor.AssetDatabase.GUIDToAssetPath(asm);
                var asmNamePath = System.IO.Path.GetDirectoryName(asmPath);

                if (delete == true) {
                    
                    var fullDir = asmNamePath + "/aot.compiler.gen.cs";
                    if (System.IO.File.Exists(fullDir) == true) {
                        
                        AssetDatabase.DeleteAsset(fullDir);
                        AssetDatabase.Refresh();

                    }
                    
                } else {

                    AOTGenerator.CompileDirectory(asmNamePath);

                }

            }

        }

        [UnityEditor.MenuItem("ME.ECS/Compile AOT Helper")]
        public static void CompileAOT() {

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

            AOTGenerator.CompileDirectory(path);

        }

        public static void CompileDirectory(string dir) {

            var itemStr = @"
new ME.ECS.Views.ViewsModule<#STATENAME#, #PROJECTNAME#.Entities.#ENTITYNAME#>();
ME.ECS.PoolModules.Spawn<ME.ECS.Views.ViewsModule<#STATENAME#, #PROJECTNAME#.Entities.#ENTITYNAME#>>();
ME.ECS.Worlds<#STATENAME#>.currentWorld.AddComponent<#PROJECTNAME#.Entities.#ENTITYNAME#, ME.ECS.Views.ViewComponent<#STATENAME#, #PROJECTNAME#.Entities.#ENTITYNAME#>>(new ME.ECS.Entity());
ME.ECS.Worlds<#STATENAME#>.currentWorld.AddComponent<#PROJECTNAME#.Entities.#ENTITYNAME#, ME.ECS.Views.CreateViewComponentRequest<#STATENAME#, #PROJECTNAME#.Entities.#ENTITYNAME#>>(new ME.ECS.Entity());
ME.ECS.Worlds<#STATENAME#>.currentWorld.AddComponent<#PROJECTNAME#.Entities.#ENTITYNAME#, ME.ECS.Views.DestroyViewComponentRequest<#STATENAME#, #PROJECTNAME#.Entities.#ENTITYNAME#>>(new ME.ECS.Entity());
ME.ECS.Worlds<#STATENAME#>.currentWorld.AddComponent<#PROJECTNAME#.Entities.#ENTITYNAME#, ME.ECS.Views.CreateViewComponentRequest<#STATENAME#, #PROJECTNAME#.Entities.#ENTITYNAME#>, ME.ECS.Views.IViewComponentRequest<#STATENAME#, #PROJECTNAME#.Entities.#ENTITYNAME#>>(new ME.ECS.Entity());
ME.ECS.Worlds<#STATENAME#>.currentWorld.AddComponent<#PROJECTNAME#.Entities.#ENTITYNAME#, ME.ECS.Views.DestroyViewComponentRequest<#STATENAME#, #PROJECTNAME#.Entities.#ENTITYNAME#>, ME.ECS.Views.IViewComponentRequest<#STATENAME#, #PROJECTNAME#.Entities.#ENTITYNAME#>>(new ME.ECS.Entity());
ME.ECS.Worlds<#STATENAME#>.currentWorld.AddOrGetComponent<#PROJECTNAME#.Entities.#ENTITYNAME#, ME.ECS.Views.ViewComponent<#STATENAME#, #PROJECTNAME#.Entities.#ENTITYNAME#>>(new ME.ECS.Entity());
ME.ECS.Worlds<#STATENAME#>.currentWorld.GetComponent<#PROJECTNAME#.Entities.#ENTITYNAME#, ME.ECS.Views.ViewComponent<#STATENAME#, #PROJECTNAME#.Entities.#ENTITYNAME#>>(new ME.ECS.Entity());
ME.ECS.Worlds<#STATENAME#>.currentWorld.ForEachEntity<#PROJECTNAME#.Entities.#ENTITYNAME#>(out ME.ECS.Collections.RefList<#PROJECTNAME#.Entities.#ENTITYNAME#> _);
ME.ECS.Worlds<#STATENAME#>.currentWorld.ForEachComponent<#PROJECTNAME#.Entities.#ENTITYNAME#, ME.ECS.Views.ViewComponent<#STATENAME#, #PROJECTNAME#.Entities.#ENTITYNAME#>>(new ME.ECS.Entity());
ME.ECS.Worlds<#STATENAME#>.currentWorld.GetEntityData(new ME.ECS.Entity(), out #PROJECTNAME#.Entities.#ENTITYNAME# _);
ME.ECS.Worlds<#STATENAME#>.currentWorld.RemoveComponentsPredicate<ME.ECS.Views.ViewComponent<#STATENAME#, #PROJECTNAME#.Entities.#ENTITYNAME#>, ME.ECS.Views.RemoveComponentViewPredicate<#STATENAME#, #PROJECTNAME#.Entities.#ENTITYNAME#>, #PROJECTNAME#.Entities.#ENTITYNAME#>(new ME.ECS.Entity(), new ME.ECS.Views.RemoveComponentViewPredicate<#STATENAME#, #PROJECTNAME#.Entities.#ENTITYNAME#>());
ME.ECS.Worlds<#STATENAME#>.currentWorld.RemoveComponents<#PROJECTNAME#.Entities.#ENTITYNAME#, ME.ECS.Views.IViewComponentRequest<#STATENAME#, #PROJECTNAME#.Entities.#ENTITYNAME#>>(new ME.ECS.Entity());
";
            
            //var templatePath = UnityEngine.Resources.Load<UnityEngine.TextAsset>("00-types-item");
            //var itemTypesStr = templatePath.text;
            
            var listEntities = new List<System.Type>();
            var asms = UnityEditor.AssetDatabase.FindAssets("t:asmdef", new[] { dir });
            foreach (var asm in asms) {

                var output = string.Empty;
                var outputTypes = string.Empty;
                listEntities.Clear();

                var asmPath = UnityEditor.AssetDatabase.GUIDToAssetPath(asm);
                var asmNamePath = System.IO.Path.GetDirectoryName(asmPath);

                var splitted = asmNamePath.Split('/');
                var asmName = splitted[splitted.Length - 1];

                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies) {

                    if (assembly.GetName().Name == asmName) {

                        var allTypes = assembly.GetTypes();
                        foreach (var type in allTypes) {

                            var interfaces = type.GetInterfaces();
                            foreach (var @interface in interfaces) {

                                if (@interface == typeof(ME.ECS.IEntity)) {

                                    listEntities.Add(type);

                                }

                            }

                        }

                        break;

                    }

                }

                foreach (var entityType in listEntities) {

                    var resItem = itemStr;
                    resItem = resItem.Replace("#PROJECTNAME#", asmName);
                    resItem = resItem.Replace("#STATENAME#", asmName + "State");
                    resItem = resItem.Replace("#ENTITYNAME#", entityType.Name);

                    output += resItem;

                    /*resItem = itemTypesStr;
                    resItem = resItem.Replace("#PROJECTNAME#", asmName);
                    resItem = resItem.Replace("#STATENAME#", asmName + "State");
                    resItem = resItem.Replace("#ENTITYNAME#", entityType.Name);

                    outputTypes += resItem;*/

                }

                if (string.IsNullOrEmpty(output) == false) ME.ECSEditor.ScriptTemplates.Create(asmNamePath, "aot.compiler.gen.cs", "00-aot", new Dictionary<string, string>() { { "CONTENT", output } }, allowRename: false);
                //if (string.IsNullOrEmpty(outputTypes) == false) ME.ECSEditor.ScriptTemplates.Create(asmNamePath, "types.compiler.gen.cs", "00-types", new Dictionary<string, string>() { { "CONTENT", outputTypes } }, allowRename: false);

            }

        }

    }

}
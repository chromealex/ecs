using System.Collections.Generic;
using UnityEditor;

namespace ME.ECSEditor {

    public class AOTGenerator : Generator {

        private const string FILE_NAME = "gen/compiler.gen.aot.cs";
        private const string MENU_ITEM = "ME.ECS/Generators/AOT/Compile...";
        private const string MENU_ITEM_AUTO = "ME.ECS/Generators/AOT/Auto Compile";
        private const string PREFS_KEY = "ME.ECS.Compiler.Gen.AOT.Auto";
        private const string TEMPLATE = "00-aot";
        private const string DIRECTORY_CONTAINS = "/Components/";
        private static readonly System.Type SEARCH_TYPE = typeof(ME.ECS.IStructComponent);
        private const string CONTENT_ITEM = @"
            new ME.ECS.StructComponents<#TYPENAME#>();";
        private const bool AUTO_COMPILE_DEFAULT = false;

        public static void Init() {
            
            Generator.Set(
                AOTGenerator.MENU_ITEM_AUTO,
                AOTGenerator.CONTENT_ITEM,
                AOTGenerator.FILE_NAME,
                AOTGenerator.TEMPLATE,
                AOTGenerator.SEARCH_TYPE,
                AOTGenerator.PREFS_KEY,
                AOTGenerator.DIRECTORY_CONTAINS,
                AOTGenerator.AUTO_COMPILE_DEFAULT);
            
        }

        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {

            AOTGenerator.Init();
            Generator.OnPostprocessAllAssetsGen(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded() {
            
            AOTGenerator.Init();
            Generator.OnAfterAssemblyReload(false);
            
        }

        [UnityEditor.MenuItem(AOTGenerator.MENU_ITEM_AUTO)]
        public static void AutoGenerate() {
            
            AOTGenerator.Init();
            Generator.AutoGenerateCheck();
            
        }

        [UnityEditor.MenuItem(AOTGenerator.MENU_ITEM, true)]
        public static bool CompileMenuValidate() {
            
            AOTGenerator.Init();
            return Generator.IsValidToCompile();

        }

        [UnityEditor.MenuItem(AOTGenerator.MENU_ITEM)]
        public static void CompileMenu() {
            
            AOTGenerator.Init();
            Generator.Compile();
            
        }

    }

}
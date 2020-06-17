using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.ECSEditor {

    public static class AllGenerators {

        private const string MENU_ITEM = "ME.ECS/Generators/Compile All...";
        
        [UnityEditor.MenuItem(AllGenerators.MENU_ITEM, true)]
        public static bool CompileMenuValidate() {

            StructComponentsGenerator.Init();
            var isValid = Generator.IsValidToCompile();
            return isValid;

        }

        [UnityEditor.MenuItem(AllGenerators.MENU_ITEM)]
        public static void CompileMenu() {

            StructComponentsGenerator.Init();
            Generator.Compile();

        }

    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.ECSEditor {

    public static class AllGenerators {

        private const string MENU_ITEM = "ME.ECS/Generators/Compile All...";
        
        [UnityEditor.MenuItem(AllGenerators.MENU_ITEM, true)]
        public static bool CompileMenuValidate() {

            AOTGenerator.Init();
            var isValid = Generator.IsValidToCompile();
            if (isValid == true) {
                
                StructComponentsGenerator.Init();
                isValid = Generator.IsValidToCompile();
                if (isValid == true) {

                    return true;

                }
                
            }

            return false;

        }

        [UnityEditor.MenuItem(AllGenerators.MENU_ITEM)]
        public static void CompileMenu() {

            AOTGenerator.Init();
            Generator.Compile();

            StructComponentsGenerator.Init();
            Generator.Compile();

        }

    }

}
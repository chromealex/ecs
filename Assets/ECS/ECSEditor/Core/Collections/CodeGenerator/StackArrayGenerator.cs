using System.Collections.Generic;
using UnityEditor;

namespace ME.ECSEditor {

    public class StackArrayGenerator : AssetPostprocessor {

        [UnityEditor.MenuItem("ME.ECS/Generators/Compile Stack Array...")]
        public static void CompileStackArray() {

            var templateItemPath = UnityEngine.Resources.Load<UnityEngine.TextAsset>("StackArrayElement");
            var contentItemStr = templateItemPath.text;

            var templateItemVarPath = UnityEngine.Resources.Load<UnityEngine.TextAsset>("StackArrayElementVar");
            var contentItemVarStr = templateItemVarPath.text;

            
            var templateStruct = UnityEngine.Resources.Load<UnityEngine.TextAsset>("StackArrayStruct");
            var contentStruct = templateStruct.text;

            var templateStructCaseGet = UnityEngine.Resources.Load<UnityEngine.TextAsset>("StackArrayStructCaseGet");
            var contentStructCaseGet = templateStructCaseGet.text;

            var templateStructCaseSet = UnityEngine.Resources.Load<UnityEngine.TextAsset>("StackArrayStructCaseSet");
            var contentStructCaseSet = templateStructCaseSet.text;

            var asms = UnityEditor.AssetDatabase.FindAssets("t:asmdef ECSAssembly");
            foreach (var asm in asms) {

                var asset = UnityEditor.AssetDatabase.GUIDToAssetPath(asm);
                var dir = System.IO.Path.GetDirectoryName(asset) + "/Core/Collections/CodeGenerator";
                if (System.IO.Directory.Exists(dir) == false) continue;

                const int maxLength = 1000;
                var lengths = new int[] {
                    10, 20, 30, 40, 50, 1000
                };
                
                var structs = string.Empty;
                var output = string.Empty;
                for (int i = 0; i < lengths.Length; ++i) {

                    var length = lengths[i];
                    var vars = string.Empty;
                    var casesGet = string.Empty;
                    var casesSet = string.Empty;
                    for (int j = 0; j < length; ++j) {

                        vars += contentItemVarStr.Replace("#INDEX#", j.ToString()) + (j == length - 1 ? string.Empty : ",");

                        casesGet += contentStructCaseGet.Replace("#INDEX#", j.ToString());
                        casesSet += contentStructCaseSet.Replace("#INDEX#", j.ToString());

                    }

                    output += contentItemStr.Replace("#LENGTH#", length.ToString()).Replace("#VARS#", vars);

                    if (maxLength != length) {

                        var itemStruct = contentStruct;
                        itemStruct = itemStruct.Replace("#LENGTH#", length.ToString());
                        itemStruct = itemStruct.Replace("#CASES_GET#", casesGet);
                        itemStruct = itemStruct.Replace("#CASES_SET#", casesSet);
                        structs += itemStruct;

                    }

                }

                if (string.IsNullOrEmpty(output) == false) ME.ECSEditor.ScriptTemplates.Create(dir, "stackarray.compiler.gen.cs", "StackArray", new Dictionary<string, string>() { { "CONTENT", output }, { "STRUCTS", structs }, { "MAX_LENGTH", maxLength.ToString() } }, allowRename: false);
                
            }

        }

    }

}
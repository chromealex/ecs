using System.Collections.Generic;
using UnityEditor;

namespace ME.ECSEditor {

    public class StackArrayGenerator : AssetPostprocessor {

        [UnityEditor.MenuItem("ME.ECS/Compile Stack Array")]
        public static void CompileStackArray() {

            var templateItemPath = UnityEngine.Resources.Load<UnityEngine.TextAsset>("StackArrayElement");
            var contentItemStr = templateItemPath.text;

            var templateItemVarPath = UnityEngine.Resources.Load<UnityEngine.TextAsset>("StackArrayElementVar");
            var contentItemVarStr = templateItemVarPath.text;

            var asms = UnityEditor.AssetDatabase.FindAssets("t:asmdef ECSAssembly");
            foreach (var asm in asms) {

                var asset = UnityEditor.AssetDatabase.GUIDToAssetPath(asm);
                var dir = System.IO.Path.GetDirectoryName(asset) + "/Core/Collections/CodeGenerator";

                const int maxLength = 1000;
                var output = string.Empty;
                var i = maxLength;
                //for (int i = 1; i <= maxLength; ++i) {

                    var vars = string.Empty;
                    for (int j = 0; j < i; ++j) {

                        vars += contentItemVarStr.Replace("#INDEX#", j.ToString()) + (j == i - 1 ? string.Empty : ",");

                    }

                    output += contentItemStr.Replace("#LENGTH#", i.ToString()).Replace("#VARS#", vars);

                //}

                if (string.IsNullOrEmpty(output) == false) ME.ECSEditor.ScriptTemplates.Create(dir, "stackarray.compiler.gen.cs", "StackArray", new Dictionary<string, string>() { { "CONTENT", output }, { "MAX_LENGTH", maxLength.ToString() } }, allowRename: false);
                
            }

        }

    }

}
using System.Collections.Generic;
using UnityEditor;

namespace ME.ECSEditor {

    public class GlobalCSCGenerator : AssetPostprocessor {

        [UnityEditor.MenuItem("ME.ECS/Generators/Reset Global CSC...")]
        public static void ResetGlobalCSC() {
            
            var dir = "Assets";
            ME.ECSEditor.ScriptTemplates.Create(dir, "csc.gen.rsp", "00-csc-gen-default.rsp", new Dictionary<string, string>(), allowRename: false);
            
        }

    }

}
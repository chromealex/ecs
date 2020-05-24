
using UnityEngine.UIElements;

namespace ME.ECSEditor {

    using UnityEditor;
    
    [InitializeOnLoad]
    public static class InspectorAddComponent {

        static InspectorAddComponent() {
        
            Selection.selectionChanged += InspectorAddComponent.OnSelectionChanged;
            InspectorAddComponent.OnSelectionChanged();
            InspectorAddComponent.OnSelectionChangedWait();

        }

        private static void OnSelectionChangedWait() {

            EditorApplication.update += () => {
                InspectorAddComponent.OnSelectionChanged();
            };

        }

        private static void OnSelectionChanged() {

            var newObjects = Selection.objects;
            if (newObjects.Length == 1) {

                var obj = newObjects[0];
                if (obj is UnityEngine.GameObject go) {

                    var comp = go.GetComponent<ME.ECS.InitializerBase>();
                    if (comp != null) {

                        InspectorAddComponent.HideButton();
                        return;

                    }
                    
                    var c = go.GetComponent<ME.ECS.EntityDebugComponent>();
                    if (c != null) {

                        InspectorAddComponent.HideButton();
                        return;

                    }

                }

            }
            
            InspectorAddComponent.DrawButton();

        }

        private static void DrawButton() {
            
            // Nothing to do
            
        }

        private static void HideButton() {
            
            var windows = UnityEngine.Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var ew in windows) {
                
                if (ew.GetType().FullName == "UnityEditor.InspectorWindow") {

                    //var asm = ew.GetType().Assembly;
                    //var type = asm.GetType("UnityEditor.InspectorWindow+Styles");
                    //var buttonStyle = type.GetField("addComponentButtonStyle", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    var addComponentButton = ew.rootVisualElement.Q(className: "unity-inspector-add-component-button");
                    if (addComponentButton != null) {

                        addComponentButton.Clear();
                        
                    }
                    
                }
                
            }
            
        }
    
    }

}

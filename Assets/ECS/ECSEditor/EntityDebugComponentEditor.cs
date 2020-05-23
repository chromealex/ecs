namespace ME.ECSEditor {

    using UnityEngine;
    using UnityEditor;
    using ME.ECS;
    
    [UnityEditor.CustomEditor(typeof(EntityDebugComponent), true)]
    public class EntityDebugComponentEditor : Editor {

        private static readonly System.Collections.Generic.Dictionary<IWorldBase, WorldsViewerEditor.WorldEditor> worldEditors = new System.Collections.Generic.Dictionary<IWorldBase, WorldsViewerEditor.WorldEditor>();
        
        public override void OnInspectorGUI() {

            var target = this.target as EntityDebugComponent;
            if (target.world != null) {

                if (EntityDebugComponentEditor.worldEditors.TryGetValue(target.world, out var worldEditor) == false) {

                    worldEditor = new WorldsViewerEditor.WorldEditor();
                    worldEditor.world = target.world;
                    EntityDebugComponentEditor.worldEditors.Add(target.world, worldEditor);

                }

                WorldsViewerEditor.DrawEntity(target.entity, worldEditor, worldEditor.GetEntitiesStorage(), worldEditor.GetStructComponentsStorage(), worldEditor.GetComponentsStorage(), worldEditor.GetModules());
                this.Repaint();

            }

        }

    }

}

#if DRAWMESH_VIEWS_MODULE_SUPPORT
using UnityEditor;
using ME.ECS.Views.Providers;
using ME.ECS;

namespace ME.ECSEditor {

    [UnityEditor.CustomEditor(typeof(DrawMeshViewSourceBase), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class DrawMeshViewSourceBaseEditor : ViewBaseEditor {

        public override void OnInspectorGUI() {

            this.DrawInspectorGUI(((DrawMeshViewSourceBase)this.target).GetSource().entity, drawDefaultInspector: false);

        }

    }

    [ViewProviderCustomEditor(typeof(UnityDrawMeshProvider))]
    public class UnityDrawMeshProviderJobsEditor : IJobsViewGUIEditor<InitializerBase> {

        public InitializerBase target { get; set; }

        public T GetTarget<T>() {

            return (T)(object)this.target;

        }

        public bool OnDrawGUI() {

            var isDirty = false;

            EditorGUI.BeginDisabledGroup(this.target.worldSettings.useJobsForViews == false);
            var disabled = this.target.worldSettings.viewsSettings.unityDrawMeshProviderDisableJobs;
            GUILayoutExt.ToggleLeft(
                ref disabled,
                ref isDirty,
                "Disable <b>Unity Graphics View Provider</b> jobs",
                "Note: If checked <b>ApplyStateJob</b> will be called in main thread");
            EditorGUI.EndDisabledGroup();
            this.target.worldSettings.viewsSettings.unityDrawMeshProviderDisableJobs = disabled;
            
            return isDirty;

        }

    }

}
#endif
#if GAMEOBJECT_VIEWS_MODULE_SUPPORT
using UnityEditor;
using ME.ECS.Views.Providers;
using ME.ECS;

namespace ME.ECSEditor {

    [UnityEditor.CustomEditor(typeof(MonoBehaviourViewBase), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class MonoBehaviourViewBaseEditor : ViewBaseEditor {

        public override void OnInspectorGUI() {
            
            this.DrawInspectorGUI(((MonoBehaviourView)this.target).entity, drawDefaultInspector: true);
            
        }

    }

    [ViewProviderCustomEditor(typeof(UnityGameObjectProvider))]
    public class UnityGameObjectProviderEditor : IDebugViewGUIEditor<InitializerBase> {

        public InitializerBase target { get; set; }

        public T GetTarget<T>() {

            return (T)(object)this.target;

        }

        public bool OnDrawGUI() {

            var isDirty = false;

            EditorGUI.BeginDisabledGroup(this.target.worldDebugSettings.showViewsOnScene == false);
            GUILayoutExt.ToggleLeft(
                ref this.target.worldDebugSettings.viewsSettings.unityGameObjectProviderShowOnScene,
                ref isDirty,
                "Unity GameObject View Provider",
                string.Empty);
            EditorGUI.EndDisabledGroup();
            
            return isDirty;

        }

    }

    [ViewProviderCustomEditor(typeof(UnityGameObjectProvider))]
    public class UnityGameObjectProviderJobsEditor : IJobsViewGUIEditor<InitializerBase> {

        public InitializerBase target { get; set; }

        public T GetTarget<T>() {

            return (T)(object)this.target;

        }

        public bool OnDrawGUI() {

            var isDirty = false;

            EditorGUI.BeginDisabledGroup(this.target.worldSettings.useJobsForViews == false);
            var disabled = this.target.worldSettings.viewsSettings.unityGameObjectProviderDisableJobs;
            GUILayoutExt.ToggleLeft(
                ref disabled,
                ref isDirty,
                "Disable <b>Unity GameObject View Provider</b> jobs",
                "Note: If checked <b>ApplyStateJob</b> will not call");
            EditorGUI.EndDisabledGroup();
            this.target.worldSettings.viewsSettings.unityGameObjectProviderDisableJobs = disabled;
            
            return isDirty;

        }

    }

}
#endif
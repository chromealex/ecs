using UnityEditor;
using ME.ECS.Views.Providers;
using ME.ECS;

namespace ME.ECSEditor {

    [ViewProviderCustomEditor(typeof(NoViewProvider))]
    public class UnityNoViewProviderJobsEditor : IJobsViewGUIEditor<InitializerBase> {

        public InitializerBase target { get; set; }

        public T GetTarget<T>() {

            return (T)(object)this.target;

        }

        public bool OnDrawGUI() {

            var isDirty = false;

            EditorGUI.BeginDisabledGroup(this.target.worldSettings.useJobsForViews == false);
            var disabled = this.target.worldSettings.viewsSettings.unityNoViewProviderDisableJobs;
            GUILayoutExt.ToggleLeft(
                ref disabled,
                ref isDirty,
                "Disable <b>No View Provider</b> jobs",
                "Note: If checked <b>ApplyStateJob</b> will be called in main thread");
            EditorGUI.EndDisabledGroup();
            this.target.worldSettings.viewsSettings.unityNoViewProviderDisableJobs = disabled;
            
            return isDirty;

        }

    }

}
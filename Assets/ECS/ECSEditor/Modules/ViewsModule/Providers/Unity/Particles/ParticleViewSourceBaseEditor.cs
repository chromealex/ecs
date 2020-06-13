#if PARTICLES_VIEWS_MODULE_SUPPORT
using UnityEditor;
using ME.ECS.Views.Providers;
using ME.ECS;

namespace ME.ECSEditor {

    [UnityEditor.CustomEditor(typeof(ParticleViewSourceBase), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class ParticleViewSourceBaseEditor : ViewBaseEditor {

        public override void OnInspectorGUI() {

            this.DrawInspectorGUI(((ParticleViewSourceBase)this.target).GetSource().entity, drawDefaultInspector: false);

        }

    }

    [ViewProviderCustomEditor(typeof(UnityParticlesProvider))]
    public class UnityParticlesProviderEditor : IDebugViewGUIEditor<InitializerBase> {

        public InitializerBase target { get; set; }

        public T GetTarget<T>() {

            return (T)(object)this.target;

        }

        public bool OnDrawGUI() {

            var isDirty = false;

            EditorGUI.BeginDisabledGroup(this.target.worldDebugSettings.showViewsOnScene == false);
            GUILayoutExt.ToggleLeft(
                ref this.target.worldDebugSettings.viewsSettings.unityParticlesProviderShowOnScene,
                ref isDirty,
                "Unity Particles View Provider",
                string.Empty);
            EditorGUI.EndDisabledGroup();
            
            return isDirty;

        }

    }

    [ViewProviderCustomEditor(typeof(UnityParticlesProvider))]
    public class UnityParticlesProviderJobsEditor : IJobsViewGUIEditor<InitializerBase> {

        public InitializerBase target { get; set; }

        public T GetTarget<T>() {

            return (T)(object)this.target;

        }

        public bool OnDrawGUI() {

            var isDirty = false;

            EditorGUI.BeginDisabledGroup(this.target.worldSettings.useJobsForViews == false);
            var disabled = this.target.worldSettings.viewsSettings.unityParticlesProviderDisableJobs;
            GUILayoutExt.ToggleLeft(
                ref disabled,
                ref isDirty,
                "Disable <b>Unity Particles View Provider</b> jobs",
                "Note: If checked <b>ApplyStateJob</b> will be called in main thread");
            EditorGUI.EndDisabledGroup();
            this.target.worldSettings.viewsSettings.unityParticlesProviderDisableJobs = disabled;
            
            return isDirty;

        }

    }

}
#endif
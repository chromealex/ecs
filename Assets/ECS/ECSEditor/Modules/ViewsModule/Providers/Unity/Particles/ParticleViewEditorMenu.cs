#if PARTICLES_VIEWS_MODULE_SUPPORT
namespace ME.ECSEditor {

    public static class ParticleViewEditorMenu {

        private const int CREATE_PARTICLES_PRIORITY = ScriptTemplates.CREATE_MENU_PRIORITY - 5;

        [UnityEditor.MenuItem("Assets/Create/ME.ECS/Views/Particles View", priority = ParticleViewEditorMenu.CREATE_PARTICLES_PRIORITY)]
        public static void CreateView() {

            ScriptTemplates.Create("New View.cs", "62-ViewParticlesTemplate");

        }

    }

}
#endif
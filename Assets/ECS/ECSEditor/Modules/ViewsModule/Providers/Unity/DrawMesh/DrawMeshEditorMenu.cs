#if DRAWMESH_VIEWS_MODULE_SUPPORT
namespace ME.ECSEditor {

    public static class DrawMeshEditorMenu {

        private const int CREATE_DRAWMESH_PRIORITY = ScriptTemplates.CREATE_MENU_PRIORITY - 5;

        [UnityEditor.MenuItem("Assets/Create/ME.ECS/Views/Draw Mesh (Graphics) View", priority = DrawMeshEditorMenu.CREATE_DRAWMESH_PRIORITY)]
        public static void CreateView() {

            ScriptTemplates.Create("New View.cs", "63-ViewGraphicsTemplate");

        }

    }

}
#endif
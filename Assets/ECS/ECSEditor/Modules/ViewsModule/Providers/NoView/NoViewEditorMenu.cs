namespace ME.ECSEditor {

    public static class NoViewEditorMenu {

        private const int CREATE_NOVIEW_PRIORITY = ScriptTemplates.CREATE_MENU_PRIORITY - 5;

        [UnityEditor.MenuItem("Assets/Create/ME.ECS/Views/No View", priority = NoViewEditorMenu.CREATE_NOVIEW_PRIORITY)]
        public static void CreateView() {

            ScriptTemplates.Create("New View.cs", "60-NoViewTemplate");

        }

    }

}
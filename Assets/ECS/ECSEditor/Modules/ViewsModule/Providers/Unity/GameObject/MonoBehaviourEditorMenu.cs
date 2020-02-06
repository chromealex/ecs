#if GAMEOBJECT_VIEWS_MODULE_SUPPORT
namespace ME.ECSEditor {

    public static class MonoBehaviourEditorMenu {

        private const int CREATE_MONOBEHAVIOUR_PRIORITY = ScriptTemplates.CREATE_MENU_PRIORITY - 5;

        [UnityEditor.MenuItem("Assets/Create/ME.ECS/Views/MonoBehaviour View", priority = MonoBehaviourEditorMenu.CREATE_MONOBEHAVIOUR_PRIORITY)]
        public static void CreateView() {

            ScriptTemplates.Create("New View.cs", "61-ViewMonoBehaviourTemplate");

        }

    }

}
#endif
namespace ME.ECSEditor {

    using ME.ECS;
    
    [CustomEditor(typeof(ME.ECS.Name.Name))]
    public class NameComponentEditor : ME.ECSEditor.IGUIEditor<ME.ECS.Name.Name> {

        public ME.ECS.Name.Name target { get; set; }

        public T GetTarget<T>() {

            return (T)(object)this.target;

        }

        void IGUIEditorBase.OnDrawGUI() {

            var target = this.target;
            target.value = UnityEditor.EditorGUILayout.TextField("Name", target.value);
            this.target = target;

        }

    }

}
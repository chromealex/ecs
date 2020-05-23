namespace ME.ECSEditor {

    using ME.ECS;
    
    [ComponentCustomEditor(typeof(ME.ECS.Name.Name), order: -100)]
    public class NameComponentEditor : ME.ECSEditor.IGUIEditor<ME.ECS.Name.Name> {

        public ME.ECS.Name.Name target { get; set; }

        public T GetTarget<T>() {

            return (T)(object)this.target;

        }

        bool IGUIEditorBase.OnDrawGUI() {

            var target = this.target;
            target.value = UnityEditor.EditorGUILayout.TextField("Name", target.value);
            var isDirty = (target.value != this.target.value);
            this.target = target;

            return isDirty;

        }

    }

}
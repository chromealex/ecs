namespace ME.ECSEditor {

    using ME.ECS;
    
    [CustomEditor(typeof(ME.ECS.Transform.Position))]
    public class TransformPositionComponentEditor : ME.ECSEditor.IGUIEditor<ME.ECS.Transform.Position> {

        public ME.ECS.Transform.Position target { get; set; }

        public T GetTarget<T>() {

            return (T)(object)this.target;

        }

        void IGUIEditorBase.OnDrawGUI() {
            
            var target = this.target;
            target = UnityEditor.EditorGUILayout.Vector3Field("Position", target.ToVector3()).ToPositionStruct();
            this.target = target;

        }

    }

    [CustomEditor(typeof(ME.ECS.Transform.Rotation))]
    public class TransformRotationComponentEditor : ME.ECSEditor.IGUIEditor<ME.ECS.Transform.Rotation> {

        public ME.ECS.Transform.Rotation target { get; set; }

        public T GetTarget<T>() {

            return (T)(object)this.target;

        }

        void IGUIEditorBase.OnDrawGUI() {
            
            var target = this.target;
            target = UnityEngine.Quaternion.Euler(UnityEditor.EditorGUILayout.Vector3Field("Rotation", target.ToQuaternion().eulerAngles)).ToRotationStruct();
            this.target = target;

        }

    }

    [CustomEditor(typeof(ME.ECS.Transform.Scale))]
    public class TransformScaleComponentEditor : ME.ECSEditor.IGUIEditor<ME.ECS.Transform.Scale> {

        public ME.ECS.Transform.Scale target { get; set; }

        public T GetTarget<T>() {

            return (T)(object)this.target;

        }

        void IGUIEditorBase.OnDrawGUI() {
            
            var target = this.target;
            target = UnityEditor.EditorGUILayout.Vector3Field("Scale", target.ToVector3()).ToScaleStruct();
            this.target = target;

        }

    }

}
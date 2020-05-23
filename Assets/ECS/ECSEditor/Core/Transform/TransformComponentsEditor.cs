namespace ME.ECSEditor {

    using ME.ECS;
    
    [ComponentCustomEditor(typeof(ME.ECS.Transform.Position), order: -50)]
    public class TransformPositionComponentEditor : ME.ECSEditor.IGUIEditor<ME.ECS.Transform.Position> {

        public ME.ECS.Transform.Position target { get; set; }

        public T GetTarget<T>() {

            return (T)(object)this.target;

        }

        bool IGUIEditorBase.OnDrawGUI() {
            
            var target = this.target;
            target = UnityEditor.EditorGUILayout.Vector3Field("Position", target.ToVector3()).ToPositionStruct();
            var isDirty = (target.ToVector3() != this.target.ToVector3());
            this.target = target;

            return isDirty;

        }

    }

    [ComponentCustomEditor(typeof(ME.ECS.Transform.Rotation), order: -49)]
    public class TransformRotationComponentEditor : ME.ECSEditor.IGUIEditor<ME.ECS.Transform.Rotation> {

        public ME.ECS.Transform.Rotation target { get; set; }

        public T GetTarget<T>() {

            return (T)(object)this.target;

        }

        bool IGUIEditorBase.OnDrawGUI() {
            
            var target = this.target;
            target = UnityEngine.Quaternion.Euler(UnityEditor.EditorGUILayout.Vector3Field("Rotation", target.ToQuaternion().eulerAngles)).ToRotationStruct();
            var isDirty = (target.ToQuaternion() != this.target.ToQuaternion());
            this.target = target;
            
            return isDirty;

        }

    }

    [ComponentCustomEditor(typeof(ME.ECS.Transform.Scale), order: -48)]
    public class TransformScaleComponentEditor : ME.ECSEditor.IGUIEditor<ME.ECS.Transform.Scale> {

        public ME.ECS.Transform.Scale target { get; set; }

        public T GetTarget<T>() {

            return (T)(object)this.target;

        }

        bool IGUIEditorBase.OnDrawGUI() {
            
            var target = this.target;
            target = UnityEditor.EditorGUILayout.Vector3Field("Scale", target.ToVector3()).ToScaleStruct();
            var isDirty = (target.ToVector3() != this.target.ToVector3());
            this.target = target;

            return isDirty;

        }

    }

}
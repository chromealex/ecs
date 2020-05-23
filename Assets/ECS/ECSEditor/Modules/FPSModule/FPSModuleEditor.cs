#if FPS_MODULE_SUPPORT
namespace ME.ECSEditor {

    using ME.ECS;
    
    [ComponentCustomEditor(typeof(IFPSModuleBase))]
    public class FPSModuleEditor : IGUIEditor<IFPSModuleBase> {

        public IFPSModuleBase target { get; set; }

        public T GetTarget<T>() {

            return (T)(object)this.target;

        }

        bool IGUIEditorBase.OnDrawGUI() {

            var style = new UnityEngine.GUIStyle(UnityEngine.GUI.skin.label);
            style.richText = true;

            UnityEngine.GUILayout.Label("<b>FPS:</b> " + this.target.fps.ToString(), style);
            UnityEngine.GUILayout.Label("<b>Max FPS:</b> " + this.target.maxFps.ToString(), style);
            UnityEngine.GUILayout.Label("<b>Min FPS:</b> " + this.target.minFps.ToString(), style);
            UnityEngine.GUILayout.Label("<b>Target FPS:</b> " + this.target.targetFps.ToString(), style);

            return false;

        }

    }

}
#endif
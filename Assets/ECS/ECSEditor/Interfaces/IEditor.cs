namespace ME.ECSEditor {

    public interface IGUIEditorBase {
        
        void OnDrawGUI();
        T GetTarget<T>();

    }

    public interface IGUIEditor<T> : IGUIEditorBase {

        T target { get; set; }

    }

}
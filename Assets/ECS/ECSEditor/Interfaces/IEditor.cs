namespace ME.ECSEditor {

    public interface IGUIEditorBase {
        
        void OnDrawGUI();

    }

    public interface IGUIEditor<T> : IGUIEditorBase {

        T target { get; set; }

    }

}
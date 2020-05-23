namespace ME.ECSEditor {

    public interface IGUIEditorBase {
        
        bool OnDrawGUI();
        T GetTarget<T>();

    }

    public interface IGUIEditor<T> : IGUIEditorBase {

        T target { get; set; }

    }
    
    public interface IDebugViewGUIEditor<T> : IGUIEditor<T> {
    }

    public interface IJobsViewGUIEditor<T> : IGUIEditor<T> {
    }

}
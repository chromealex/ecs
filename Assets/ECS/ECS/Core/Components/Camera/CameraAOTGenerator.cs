
namespace ME.ECS {

    public static class CameraAOTCompileHelper<TState> where TState : class, IState<TState>, new() {
    
        public static void IL2CPP() {
    
            new ME.ECS.StructComponents<TState, ME.ECS.Camera.Camera>();

        }
    
    }

}

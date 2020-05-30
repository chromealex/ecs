
namespace ME.ECS {

    public static class TransformAOTCompileHelper<TState> where TState : class, IState<TState>, new() {
    
        public static void IL2CPP() {
    
            new ME.ECS.StructComponents<TState, ME.ECS.Transform.Childs>();
            new ME.ECS.StructComponents<TState, ME.ECS.Transform.Container>();

        }
    
    }

}

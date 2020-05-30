
namespace ME.ECS {

    public static class NameAOTCompileHelper<TState> where TState : class, IState<TState>, new() {
    
        public static void IL2CPP() {
    
            new ME.ECS.StructComponents<TState, ME.ECS.Transform.Position>();
            new ME.ECS.StructComponents<TState, ME.ECS.Transform.Rotation>();
            new ME.ECS.StructComponents<TState, ME.ECS.Transform.Scale>();
            new ME.ECS.StructComponents<TState, ME.ECS.Transform.Childs>();
            new ME.ECS.StructComponents<TState, ME.ECS.Transform.Container>();

        }
    
    }

}

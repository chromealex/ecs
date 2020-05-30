
namespace ME.ECS {

    public static class Transform3DAOTCompileHelper<TState> where TState : class, IState<TState>, new() {
    
        public static void IL2CPP() {
    
            new ME.ECS.StructComponents<TState, ME.ECS.Transform.Position>();
            new ME.ECS.StructComponents<TState, ME.ECS.Transform.Rotation>();
            new ME.ECS.StructComponents<TState, ME.ECS.Transform.Scale>();

        }
    
    }

}

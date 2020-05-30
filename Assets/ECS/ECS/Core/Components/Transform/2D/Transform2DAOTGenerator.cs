
namespace ME.ECS {

    public static class Transform2DAOTCompileHelper<TState> where TState : class, IState<TState>, new() {
    
        public static void IL2CPP() {
    
            new ME.ECS.StructComponents<TState, ME.ECS.Transform.Position2D>();
            new ME.ECS.StructComponents<TState, ME.ECS.Transform.Rotation2D>();
            new ME.ECS.StructComponents<TState, ME.ECS.Transform.Scale2D>();

        }
    
    }

}

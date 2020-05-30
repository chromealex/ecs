
namespace ME.ECS {

    public static class Transform2DComponentsInitializer<TState> where TState : class, IState<TState>, new() {
    
        public static void Init(ref ME.ECS.StructComponentsContainer<TState> structComponentsContainer) {
    
            structComponentsContainer.Validate<ME.ECS.Transform.Position2D>();
            structComponentsContainer.Validate<ME.ECS.Transform.Rotation2D>();
            structComponentsContainer.Validate<ME.ECS.Transform.Scale2D>();

        }
    
    }

}


namespace ME.ECS {

    public static class Transform3DComponentsInitializer<TState> where TState : class, IState<TState>, new() {
    
        public static void Init(ref ME.ECS.StructComponentsContainer<TState> structComponentsContainer) {
    
            structComponentsContainer.Validate<ME.ECS.Transform.Position>();
            structComponentsContainer.Validate<ME.ECS.Transform.Rotation>();
            structComponentsContainer.Validate<ME.ECS.Transform.Scale>();

        }
    
    }

}

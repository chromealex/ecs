
namespace ME.ECS {

    public static class TransformComponentsInitializer<TState> where TState : class, IState<TState>, new() {
    
        public static void Init(ref ME.ECS.StructComponentsContainer<TState> structComponentsContainer) {
    
            structComponentsContainer.Validate<ME.ECS.Transform.Position>();
            structComponentsContainer.Validate<ME.ECS.Transform.Rotation>();
            structComponentsContainer.Validate<ME.ECS.Transform.Scale>();
            structComponentsContainer.Validate<ME.ECS.Transform.Childs>();
            structComponentsContainer.Validate<ME.ECS.Transform.Container>();

        }
    
    }

}

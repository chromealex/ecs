
namespace ME.ECS {

    public static class TransformComponentsInitializer<TState> where TState : class, IState<TState>, new() {
    
        public static void Init(ref ME.ECS.StructComponentsContainer<TState> structComponentsContainer) {
    
            structComponentsContainer.Validate<ME.ECS.Transform.Childs>();
            structComponentsContainer.Validate<ME.ECS.Transform.Container>();

            Transform2DComponentsInitializer<TState>.Init(ref structComponentsContainer);
            Transform3DComponentsInitializer<TState>.Init(ref structComponentsContainer);

        }
    
    }

}

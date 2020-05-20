
namespace ME.ECS {

    public static class CoreComponentsInitializer<TState> where TState : class, IState<TState>, new() {

        public static void Init(ref ME.ECS.StructComponentsContainer<TState> structComponentsContainer) {
            
            TransformComponentsInitializer<TState>.Init(ref structComponentsContainer);
            NameComponentsInitializer<TState>.Init(ref structComponentsContainer);
            
        }

    }

}
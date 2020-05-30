
namespace ME.ECS {

    public static class NameComponentsInitializer<TState> where TState : class, IState<TState>, new() {
    
        public static void Init(ref ME.ECS.StructComponentsContainer<TState> structComponentsContainer) {
    
            structComponentsContainer.Validate<ME.ECS.Name.Name>();

        }
    
    }

}

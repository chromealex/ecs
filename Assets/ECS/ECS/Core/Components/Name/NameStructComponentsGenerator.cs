
namespace ME.ECS {

    public static class NameComponentsInitializer {
    
        public static void Init(ref ME.ECS.StructComponentsContainer structComponentsContainer) {
    
            structComponentsContainer.Validate<ME.ECS.Name.Name>();

        }
    
    }

}

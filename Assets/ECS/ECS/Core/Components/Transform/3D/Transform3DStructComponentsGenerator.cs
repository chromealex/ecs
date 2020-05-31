
namespace ME.ECS {

    public static class Transform3DComponentsInitializer {
    
        public static void Init(ref ME.ECS.StructComponentsContainer structComponentsContainer) {
    
            structComponentsContainer.Validate<ME.ECS.Transform.Position>();
            structComponentsContainer.Validate<ME.ECS.Transform.Rotation>();
            structComponentsContainer.Validate<ME.ECS.Transform.Scale>();

        }
    
    }

}

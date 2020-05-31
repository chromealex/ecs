
namespace ME.ECS {

    public static class Transform3DAOTCompileHelper {
    
        public static void IL2CPP() {
    
            new ME.ECS.StructComponents<ME.ECS.Transform.Position>();
            new ME.ECS.StructComponents<ME.ECS.Transform.Rotation>();
            new ME.ECS.StructComponents<ME.ECS.Transform.Scale>();

        }
    
    }

}

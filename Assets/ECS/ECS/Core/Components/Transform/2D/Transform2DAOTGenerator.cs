
namespace ME.ECS {

    public static class Transform2DAOTCompileHelper {
    
        public static void IL2CPP() {
    
            new ME.ECS.StructComponents<ME.ECS.Transform.Position2D>();
            new ME.ECS.StructComponents<ME.ECS.Transform.Rotation2D>();
            new ME.ECS.StructComponents<ME.ECS.Transform.Scale2D>();

        }
    
    }

}

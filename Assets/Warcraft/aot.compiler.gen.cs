namespace Warcraft {

    public static class AOTCompileHelper {
    
        public static void IL2CPP() {
    
    
ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.CameraEntity, ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.CameraEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.CameraEntity, ME.ECS.Views.CreateViewComponentRequest<WarcraftState, Warcraft.Entities.CameraEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.CameraEntity, ME.ECS.Views.DestroyViewComponentRequest<WarcraftState, Warcraft.Entities.CameraEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.CameraEntity, ME.ECS.Views.CreateViewComponentRequest<WarcraftState, Warcraft.Entities.CameraEntity>, ME.ECS.Views.IViewComponentRequest<WarcraftState, Warcraft.Entities.CameraEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.CameraEntity, ME.ECS.Views.DestroyViewComponentRequest<WarcraftState, Warcraft.Entities.CameraEntity>, ME.ECS.Views.IViewComponentRequest<WarcraftState, Warcraft.Entities.CameraEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddOrGetComponent<Warcraft.Entities.CameraEntity, ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.CameraEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.GetComponent<Warcraft.Entities.CameraEntity, ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.CameraEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.ForEachEntity<Warcraft.Entities.CameraEntity>(out ME.ECS.Collections.RefList<Warcraft.Entities.CameraEntity> _);
ME.ECS.Worlds<WarcraftState>.currentWorld.ForEachComponent<Warcraft.Entities.CameraEntity, ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.CameraEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.GetEntityData(new ME.ECS.Entity(), out Warcraft.Entities.CameraEntity _);
ME.ECS.Worlds<WarcraftState>.currentWorld.RemoveComponentsPredicate<ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.CameraEntity>, ME.ECS.Views.RemoveComponentViewPredicate<WarcraftState, Warcraft.Entities.CameraEntity>, Warcraft.Entities.CameraEntity>(new ME.ECS.Entity(), new ME.ECS.Views.RemoveComponentViewPredicate<WarcraftState, Warcraft.Entities.CameraEntity>());
ME.ECS.Worlds<WarcraftState>.currentWorld.RemoveComponents<Warcraft.Entities.CameraEntity, ME.ECS.Views.IViewComponentRequest<WarcraftState, Warcraft.Entities.CameraEntity>>(new ME.ECS.Entity());

ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.DebugEntity, ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.DebugEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.DebugEntity, ME.ECS.Views.CreateViewComponentRequest<WarcraftState, Warcraft.Entities.DebugEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.DebugEntity, ME.ECS.Views.DestroyViewComponentRequest<WarcraftState, Warcraft.Entities.DebugEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.DebugEntity, ME.ECS.Views.CreateViewComponentRequest<WarcraftState, Warcraft.Entities.DebugEntity>, ME.ECS.Views.IViewComponentRequest<WarcraftState, Warcraft.Entities.DebugEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.DebugEntity, ME.ECS.Views.DestroyViewComponentRequest<WarcraftState, Warcraft.Entities.DebugEntity>, ME.ECS.Views.IViewComponentRequest<WarcraftState, Warcraft.Entities.DebugEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddOrGetComponent<Warcraft.Entities.DebugEntity, ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.DebugEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.GetComponent<Warcraft.Entities.DebugEntity, ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.DebugEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.ForEachEntity<Warcraft.Entities.DebugEntity>(out ME.ECS.Collections.RefList<Warcraft.Entities.DebugEntity> _);
ME.ECS.Worlds<WarcraftState>.currentWorld.ForEachComponent<Warcraft.Entities.DebugEntity, ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.DebugEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.GetEntityData(new ME.ECS.Entity(), out Warcraft.Entities.DebugEntity _);
ME.ECS.Worlds<WarcraftState>.currentWorld.RemoveComponentsPredicate<ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.DebugEntity>, ME.ECS.Views.RemoveComponentViewPredicate<WarcraftState, Warcraft.Entities.DebugEntity>, Warcraft.Entities.DebugEntity>(new ME.ECS.Entity(), new ME.ECS.Views.RemoveComponentViewPredicate<WarcraftState, Warcraft.Entities.DebugEntity>());
ME.ECS.Worlds<WarcraftState>.currentWorld.RemoveComponents<Warcraft.Entities.DebugEntity, ME.ECS.Views.IViewComponentRequest<WarcraftState, Warcraft.Entities.DebugEntity>>(new ME.ECS.Entity());

ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.PlayerEntity, ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.PlayerEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.PlayerEntity, ME.ECS.Views.CreateViewComponentRequest<WarcraftState, Warcraft.Entities.PlayerEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.PlayerEntity, ME.ECS.Views.DestroyViewComponentRequest<WarcraftState, Warcraft.Entities.PlayerEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.PlayerEntity, ME.ECS.Views.CreateViewComponentRequest<WarcraftState, Warcraft.Entities.PlayerEntity>, ME.ECS.Views.IViewComponentRequest<WarcraftState, Warcraft.Entities.PlayerEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.PlayerEntity, ME.ECS.Views.DestroyViewComponentRequest<WarcraftState, Warcraft.Entities.PlayerEntity>, ME.ECS.Views.IViewComponentRequest<WarcraftState, Warcraft.Entities.PlayerEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddOrGetComponent<Warcraft.Entities.PlayerEntity, ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.PlayerEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.GetComponent<Warcraft.Entities.PlayerEntity, ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.PlayerEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.ForEachEntity<Warcraft.Entities.PlayerEntity>(out ME.ECS.Collections.RefList<Warcraft.Entities.PlayerEntity> _);
ME.ECS.Worlds<WarcraftState>.currentWorld.ForEachComponent<Warcraft.Entities.PlayerEntity, ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.PlayerEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.GetEntityData(new ME.ECS.Entity(), out Warcraft.Entities.PlayerEntity _);
ME.ECS.Worlds<WarcraftState>.currentWorld.RemoveComponentsPredicate<ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.PlayerEntity>, ME.ECS.Views.RemoveComponentViewPredicate<WarcraftState, Warcraft.Entities.PlayerEntity>, Warcraft.Entities.PlayerEntity>(new ME.ECS.Entity(), new ME.ECS.Views.RemoveComponentViewPredicate<WarcraftState, Warcraft.Entities.PlayerEntity>());
ME.ECS.Worlds<WarcraftState>.currentWorld.RemoveComponents<Warcraft.Entities.PlayerEntity, ME.ECS.Views.IViewComponentRequest<WarcraftState, Warcraft.Entities.PlayerEntity>>(new ME.ECS.Entity());

ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.SelectionEntity, ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.SelectionEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.SelectionEntity, ME.ECS.Views.CreateViewComponentRequest<WarcraftState, Warcraft.Entities.SelectionEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.SelectionEntity, ME.ECS.Views.DestroyViewComponentRequest<WarcraftState, Warcraft.Entities.SelectionEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.SelectionEntity, ME.ECS.Views.CreateViewComponentRequest<WarcraftState, Warcraft.Entities.SelectionEntity>, ME.ECS.Views.IViewComponentRequest<WarcraftState, Warcraft.Entities.SelectionEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.SelectionEntity, ME.ECS.Views.DestroyViewComponentRequest<WarcraftState, Warcraft.Entities.SelectionEntity>, ME.ECS.Views.IViewComponentRequest<WarcraftState, Warcraft.Entities.SelectionEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddOrGetComponent<Warcraft.Entities.SelectionEntity, ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.SelectionEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.GetComponent<Warcraft.Entities.SelectionEntity, ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.SelectionEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.ForEachEntity<Warcraft.Entities.SelectionEntity>(out ME.ECS.Collections.RefList<Warcraft.Entities.SelectionEntity> _);
ME.ECS.Worlds<WarcraftState>.currentWorld.ForEachComponent<Warcraft.Entities.SelectionEntity, ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.SelectionEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.GetEntityData(new ME.ECS.Entity(), out Warcraft.Entities.SelectionEntity _);
ME.ECS.Worlds<WarcraftState>.currentWorld.RemoveComponentsPredicate<ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.SelectionEntity>, ME.ECS.Views.RemoveComponentViewPredicate<WarcraftState, Warcraft.Entities.SelectionEntity>, Warcraft.Entities.SelectionEntity>(new ME.ECS.Entity(), new ME.ECS.Views.RemoveComponentViewPredicate<WarcraftState, Warcraft.Entities.SelectionEntity>());
ME.ECS.Worlds<WarcraftState>.currentWorld.RemoveComponents<Warcraft.Entities.SelectionEntity, ME.ECS.Views.IViewComponentRequest<WarcraftState, Warcraft.Entities.SelectionEntity>>(new ME.ECS.Entity());

ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.SelectionRectEntity, ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.SelectionRectEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.SelectionRectEntity, ME.ECS.Views.CreateViewComponentRequest<WarcraftState, Warcraft.Entities.SelectionRectEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.SelectionRectEntity, ME.ECS.Views.DestroyViewComponentRequest<WarcraftState, Warcraft.Entities.SelectionRectEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.SelectionRectEntity, ME.ECS.Views.CreateViewComponentRequest<WarcraftState, Warcraft.Entities.SelectionRectEntity>, ME.ECS.Views.IViewComponentRequest<WarcraftState, Warcraft.Entities.SelectionRectEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.SelectionRectEntity, ME.ECS.Views.DestroyViewComponentRequest<WarcraftState, Warcraft.Entities.SelectionRectEntity>, ME.ECS.Views.IViewComponentRequest<WarcraftState, Warcraft.Entities.SelectionRectEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddOrGetComponent<Warcraft.Entities.SelectionRectEntity, ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.SelectionRectEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.GetComponent<Warcraft.Entities.SelectionRectEntity, ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.SelectionRectEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.ForEachEntity<Warcraft.Entities.SelectionRectEntity>(out ME.ECS.Collections.RefList<Warcraft.Entities.SelectionRectEntity> _);
ME.ECS.Worlds<WarcraftState>.currentWorld.ForEachComponent<Warcraft.Entities.SelectionRectEntity, ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.SelectionRectEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.GetEntityData(new ME.ECS.Entity(), out Warcraft.Entities.SelectionRectEntity _);
ME.ECS.Worlds<WarcraftState>.currentWorld.RemoveComponentsPredicate<ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.SelectionRectEntity>, ME.ECS.Views.RemoveComponentViewPredicate<WarcraftState, Warcraft.Entities.SelectionRectEntity>, Warcraft.Entities.SelectionRectEntity>(new ME.ECS.Entity(), new ME.ECS.Views.RemoveComponentViewPredicate<WarcraftState, Warcraft.Entities.SelectionRectEntity>());
ME.ECS.Worlds<WarcraftState>.currentWorld.RemoveComponents<Warcraft.Entities.SelectionRectEntity, ME.ECS.Views.IViewComponentRequest<WarcraftState, Warcraft.Entities.SelectionRectEntity>>(new ME.ECS.Entity());

ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.UnitEntity, ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.UnitEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.UnitEntity, ME.ECS.Views.CreateViewComponentRequest<WarcraftState, Warcraft.Entities.UnitEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.UnitEntity, ME.ECS.Views.DestroyViewComponentRequest<WarcraftState, Warcraft.Entities.UnitEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.UnitEntity, ME.ECS.Views.CreateViewComponentRequest<WarcraftState, Warcraft.Entities.UnitEntity>, ME.ECS.Views.IViewComponentRequest<WarcraftState, Warcraft.Entities.UnitEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddComponent<Warcraft.Entities.UnitEntity, ME.ECS.Views.DestroyViewComponentRequest<WarcraftState, Warcraft.Entities.UnitEntity>, ME.ECS.Views.IViewComponentRequest<WarcraftState, Warcraft.Entities.UnitEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.AddOrGetComponent<Warcraft.Entities.UnitEntity, ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.UnitEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.GetComponent<Warcraft.Entities.UnitEntity, ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.UnitEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.ForEachEntity<Warcraft.Entities.UnitEntity>(out ME.ECS.Collections.RefList<Warcraft.Entities.UnitEntity> _);
ME.ECS.Worlds<WarcraftState>.currentWorld.ForEachComponent<Warcraft.Entities.UnitEntity, ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.UnitEntity>>(new ME.ECS.Entity());
ME.ECS.Worlds<WarcraftState>.currentWorld.GetEntityData(new ME.ECS.Entity(), out Warcraft.Entities.UnitEntity _);
ME.ECS.Worlds<WarcraftState>.currentWorld.RemoveComponentsPredicate<ME.ECS.Views.ViewComponent<WarcraftState, Warcraft.Entities.UnitEntity>, ME.ECS.Views.RemoveComponentViewPredicate<WarcraftState, Warcraft.Entities.UnitEntity>, Warcraft.Entities.UnitEntity>(new ME.ECS.Entity(), new ME.ECS.Views.RemoveComponentViewPredicate<WarcraftState, Warcraft.Entities.UnitEntity>());
ME.ECS.Worlds<WarcraftState>.currentWorld.RemoveComponents<Warcraft.Entities.UnitEntity, ME.ECS.Views.IViewComponentRequest<WarcraftState, Warcraft.Entities.UnitEntity>>(new ME.ECS.Entity());

    
        }
    
    }
    
}
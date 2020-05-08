namespace Prototype {

    public static class AOTCompileHelper {
    
        public static void IL2CPP() {
    

new ME.ECS.Views.ViewsModule<PrototypeState, Prototype.Entities.Map>();
ME.ECS.PoolModules.Spawn<ME.ECS.Views.ViewsModule<PrototypeState, Prototype.Entities.Map>>();
ME.ECS.Worlds<PrototypeState>.currentWorld.AddComponent<Prototype.Entities.Map, ME.ECS.Views.ViewComponent<PrototypeState, Prototype.Entities.Map>>(new ME.ECS.Entity());
ME.ECS.Worlds<PrototypeState>.currentWorld.AddComponent<Prototype.Entities.Map, ME.ECS.Views.CreateViewComponentRequest<PrototypeState, Prototype.Entities.Map>>(new ME.ECS.Entity());
ME.ECS.Worlds<PrototypeState>.currentWorld.AddComponent<Prototype.Entities.Map, ME.ECS.Views.DestroyViewComponentRequest<PrototypeState, Prototype.Entities.Map>>(new ME.ECS.Entity());
ME.ECS.Worlds<PrototypeState>.currentWorld.AddComponent<Prototype.Entities.Map, ME.ECS.Views.CreateViewComponentRequest<PrototypeState, Prototype.Entities.Map>, ME.ECS.Views.IViewComponentRequest<PrototypeState, Prototype.Entities.Map>>(new ME.ECS.Entity());
ME.ECS.Worlds<PrototypeState>.currentWorld.AddComponent<Prototype.Entities.Map, ME.ECS.Views.DestroyViewComponentRequest<PrototypeState, Prototype.Entities.Map>, ME.ECS.Views.IViewComponentRequest<PrototypeState, Prototype.Entities.Map>>(new ME.ECS.Entity());
ME.ECS.Worlds<PrototypeState>.currentWorld.AddOrGetComponent<Prototype.Entities.Map, ME.ECS.Views.ViewComponent<PrototypeState, Prototype.Entities.Map>>(new ME.ECS.Entity());
ME.ECS.Worlds<PrototypeState>.currentWorld.GetComponent<Prototype.Entities.Map, ME.ECS.Views.ViewComponent<PrototypeState, Prototype.Entities.Map>>(new ME.ECS.Entity());
ME.ECS.Worlds<PrototypeState>.currentWorld.ForEachEntity<Prototype.Entities.Map>(out ME.ECS.Collections.RefList<Prototype.Entities.Map> _);
ME.ECS.Worlds<PrototypeState>.currentWorld.ForEachComponent<Prototype.Entities.Map, ME.ECS.Views.ViewComponent<PrototypeState, Prototype.Entities.Map>>(new ME.ECS.Entity());
ME.ECS.Worlds<PrototypeState>.currentWorld.GetEntityData(new ME.ECS.Entity(), out Prototype.Entities.Map _);
ME.ECS.Worlds<PrototypeState>.currentWorld.RemoveComponentsPredicate<ME.ECS.Views.ViewComponent<PrototypeState, Prototype.Entities.Map>, ME.ECS.Views.RemoveComponentViewPredicate<PrototypeState, Prototype.Entities.Map>, Prototype.Entities.Map>(new ME.ECS.Entity(), new ME.ECS.Views.RemoveComponentViewPredicate<PrototypeState, Prototype.Entities.Map>());
ME.ECS.Worlds<PrototypeState>.currentWorld.RemoveComponents<Prototype.Entities.Map, ME.ECS.Views.IViewComponentRequest<PrototypeState, Prototype.Entities.Map>>(new ME.ECS.Entity());

new ME.ECS.Views.ViewsModule<PrototypeState, Prototype.Entities.Player>();
ME.ECS.PoolModules.Spawn<ME.ECS.Views.ViewsModule<PrototypeState, Prototype.Entities.Player>>();
ME.ECS.Worlds<PrototypeState>.currentWorld.AddComponent<Prototype.Entities.Player, ME.ECS.Views.ViewComponent<PrototypeState, Prototype.Entities.Player>>(new ME.ECS.Entity());
ME.ECS.Worlds<PrototypeState>.currentWorld.AddComponent<Prototype.Entities.Player, ME.ECS.Views.CreateViewComponentRequest<PrototypeState, Prototype.Entities.Player>>(new ME.ECS.Entity());
ME.ECS.Worlds<PrototypeState>.currentWorld.AddComponent<Prototype.Entities.Player, ME.ECS.Views.DestroyViewComponentRequest<PrototypeState, Prototype.Entities.Player>>(new ME.ECS.Entity());
ME.ECS.Worlds<PrototypeState>.currentWorld.AddComponent<Prototype.Entities.Player, ME.ECS.Views.CreateViewComponentRequest<PrototypeState, Prototype.Entities.Player>, ME.ECS.Views.IViewComponentRequest<PrototypeState, Prototype.Entities.Player>>(new ME.ECS.Entity());
ME.ECS.Worlds<PrototypeState>.currentWorld.AddComponent<Prototype.Entities.Player, ME.ECS.Views.DestroyViewComponentRequest<PrototypeState, Prototype.Entities.Player>, ME.ECS.Views.IViewComponentRequest<PrototypeState, Prototype.Entities.Player>>(new ME.ECS.Entity());
ME.ECS.Worlds<PrototypeState>.currentWorld.AddOrGetComponent<Prototype.Entities.Player, ME.ECS.Views.ViewComponent<PrototypeState, Prototype.Entities.Player>>(new ME.ECS.Entity());
ME.ECS.Worlds<PrototypeState>.currentWorld.GetComponent<Prototype.Entities.Player, ME.ECS.Views.ViewComponent<PrototypeState, Prototype.Entities.Player>>(new ME.ECS.Entity());
ME.ECS.Worlds<PrototypeState>.currentWorld.ForEachEntity<Prototype.Entities.Player>(out ME.ECS.Collections.RefList<Prototype.Entities.Player> _);
ME.ECS.Worlds<PrototypeState>.currentWorld.ForEachComponent<Prototype.Entities.Player, ME.ECS.Views.ViewComponent<PrototypeState, Prototype.Entities.Player>>(new ME.ECS.Entity());
ME.ECS.Worlds<PrototypeState>.currentWorld.GetEntityData(new ME.ECS.Entity(), out Prototype.Entities.Player _);
ME.ECS.Worlds<PrototypeState>.currentWorld.RemoveComponentsPredicate<ME.ECS.Views.ViewComponent<PrototypeState, Prototype.Entities.Player>, ME.ECS.Views.RemoveComponentViewPredicate<PrototypeState, Prototype.Entities.Player>, Prototype.Entities.Player>(new ME.ECS.Entity(), new ME.ECS.Views.RemoveComponentViewPredicate<PrototypeState, Prototype.Entities.Player>());
ME.ECS.Worlds<PrototypeState>.currentWorld.RemoveComponents<Prototype.Entities.Player, ME.ECS.Views.IViewComponentRequest<PrototypeState, Prototype.Entities.Player>>(new ME.ECS.Entity());

new ME.ECS.Views.ViewsModule<PrototypeState, Prototype.Entities.Unit>();
ME.ECS.PoolModules.Spawn<ME.ECS.Views.ViewsModule<PrototypeState, Prototype.Entities.Unit>>();
ME.ECS.Worlds<PrototypeState>.currentWorld.AddComponent<Prototype.Entities.Unit, ME.ECS.Views.ViewComponent<PrototypeState, Prototype.Entities.Unit>>(new ME.ECS.Entity());
ME.ECS.Worlds<PrototypeState>.currentWorld.AddComponent<Prototype.Entities.Unit, ME.ECS.Views.CreateViewComponentRequest<PrototypeState, Prototype.Entities.Unit>>(new ME.ECS.Entity());
ME.ECS.Worlds<PrototypeState>.currentWorld.AddComponent<Prototype.Entities.Unit, ME.ECS.Views.DestroyViewComponentRequest<PrototypeState, Prototype.Entities.Unit>>(new ME.ECS.Entity());
ME.ECS.Worlds<PrototypeState>.currentWorld.AddComponent<Prototype.Entities.Unit, ME.ECS.Views.CreateViewComponentRequest<PrototypeState, Prototype.Entities.Unit>, ME.ECS.Views.IViewComponentRequest<PrototypeState, Prototype.Entities.Unit>>(new ME.ECS.Entity());
ME.ECS.Worlds<PrototypeState>.currentWorld.AddComponent<Prototype.Entities.Unit, ME.ECS.Views.DestroyViewComponentRequest<PrototypeState, Prototype.Entities.Unit>, ME.ECS.Views.IViewComponentRequest<PrototypeState, Prototype.Entities.Unit>>(new ME.ECS.Entity());
ME.ECS.Worlds<PrototypeState>.currentWorld.AddOrGetComponent<Prototype.Entities.Unit, ME.ECS.Views.ViewComponent<PrototypeState, Prototype.Entities.Unit>>(new ME.ECS.Entity());
ME.ECS.Worlds<PrototypeState>.currentWorld.GetComponent<Prototype.Entities.Unit, ME.ECS.Views.ViewComponent<PrototypeState, Prototype.Entities.Unit>>(new ME.ECS.Entity());
ME.ECS.Worlds<PrototypeState>.currentWorld.ForEachEntity<Prototype.Entities.Unit>(out ME.ECS.Collections.RefList<Prototype.Entities.Unit> _);
ME.ECS.Worlds<PrototypeState>.currentWorld.ForEachComponent<Prototype.Entities.Unit, ME.ECS.Views.ViewComponent<PrototypeState, Prototype.Entities.Unit>>(new ME.ECS.Entity());
ME.ECS.Worlds<PrototypeState>.currentWorld.GetEntityData(new ME.ECS.Entity(), out Prototype.Entities.Unit _);
ME.ECS.Worlds<PrototypeState>.currentWorld.RemoveComponentsPredicate<ME.ECS.Views.ViewComponent<PrototypeState, Prototype.Entities.Unit>, ME.ECS.Views.RemoveComponentViewPredicate<PrototypeState, Prototype.Entities.Unit>, Prototype.Entities.Unit>(new ME.ECS.Entity(), new ME.ECS.Views.RemoveComponentViewPredicate<PrototypeState, Prototype.Entities.Unit>());
ME.ECS.Worlds<PrototypeState>.currentWorld.RemoveComponents<Prototype.Entities.Unit, ME.ECS.Views.IViewComponentRequest<PrototypeState, Prototype.Entities.Unit>>(new ME.ECS.Entity());

    
        }
    
    }
    
}
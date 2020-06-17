#if UNITY_EDITOR
namespace ME.ECS {

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public sealed partial class World {

        private System.Collections.Generic.Dictionary<Entity, EntityDebugComponent> debugEntities = new System.Collections.Generic.Dictionary<Entity, EntityDebugComponent>();

        private float updateNamesTimer = 0f;

        public System.Collections.Generic.Dictionary<Entity, EntityDebugComponent> GetDebugEntities() {

            return this.debugEntities;

        }
        
        private string GetName(Entity entity, out bool hasName) {

            hasName = entity.HasData<ME.ECS.Name.Name>();
            var name = hasName == true ? entity.GetData<ME.ECS.Name.Name>().value : "Unnamed";
            return string.Format("{0} ({1})", name, entity);
            
        }

        private System.Collections.Generic.HashSet<Entity> used;
        partial void BeginRestoreEntitiesPlugin5() {

            if (this.debugSettings.createGameObjectsRepresentation == true) {
                
                this.used = PoolHashSet<Entity>.Spawn(100);
                
            }

        }

        partial void EndRestoreEntitiesPlugin5() {
            
            if (this.debugSettings.createGameObjectsRepresentation == true) {

                var unused = PoolList<Entity>.Spawn(100);
                foreach (var entKv in this.debugEntities) {

                    if (this.used.Contains(entKv.Key) == false) {
                        
                        unused.Add(entKv.Key);
                        
                    }
                    
                }

                for (int i = 0; i < unused.Count; ++i) {

                    this.DestroyEntityPlugin5(unused[i]);
                    

                }
                
                PoolList<Entity>.Recycle(ref unused);
                PoolHashSet<Entity>.Recycle(ref this.used);

            }
            
        }
        
        partial void CreateEntityPlugin5(Entity entity) {
            
            if (this.debugSettings.createGameObjectsRepresentation == true) {

                if (this.debugEntities.ContainsKey(entity) == false) {

                    var debug = new UnityEngine.GameObject(this.GetName(entity, out var hasName), typeof(EntityDebugComponent));
                    var comp = debug.GetComponent<EntityDebugComponent>();
                    comp.transform.hideFlags = UnityEngine.HideFlags.HideInInspector;
                    comp.entity = entity;
                    comp.world = this;
                    comp.hasName = hasName;

                    this.debugEntities.Add(entity, comp);

                } else {

                    var comp = this.debugEntities[entity];
                    comp.entity = entity;

                }
                
                if (this.used != null) this.used.Add(entity);

            }

        }

        partial void DestroyEntityPlugin5(Entity entity) {
            
            if (this.debugSettings.createGameObjectsRepresentation == true) {

                if (this.debugEntities.TryGetValue(entity, out var comp) == true) {
                    
                    UnityEngine.GameObject.Destroy(comp.gameObject);
                    this.debugEntities.Remove(entity);
                    
                }

            }

        }

        private void UpdateNames() {

            foreach (var kv in this.debugEntities) {

                var entity = kv.Key;
                if (kv.Value.hasName == false) {

                    kv.Value.gameObject.name = this.GetName(entity, out var hasName);
                    kv.Value.hasName = hasName;

                }

            }
            
        }

        partial void PlayPlugin5ForTick(Tick tick) {

            if (this.debugSettings.createGameObjectsRepresentation == true) {

                this.updateNamesTimer += this.tickTime;
                if (this.updateNamesTimer >= 1f) {

                    this.updateNamesTimer = 0f;
                    this.UpdateNames();

                }

            }

        }

    }

}
#endif
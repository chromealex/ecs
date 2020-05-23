#if UNITY_EDITOR
namespace ME.ECS {

    public partial class World<TState> {

        private System.Collections.Generic.Dictionary<Entity, EntityDebugComponent> debugEntities = new System.Collections.Generic.Dictionary<Entity, EntityDebugComponent>();

        private float updateNamesTimer = 0f;

        private string GetName(Entity entity, out bool hasName) {

            hasName = entity.HasData<ME.ECS.Name.Name>();
            var name = hasName == true ? entity.GetData<ME.ECS.Name.Name>().value : "Unnamed";
            return string.Format("{0} ({1})", name, entity);
            
        }
        
        partial void CreateEntityPlugin5(Entity entity) {
            
            if (this.debugSettings.createGameObjectsRepresentation == true) {

                var debug = new UnityEngine.GameObject(this.GetName(entity, out var hasName), typeof(EntityDebugComponent));
                var comp = debug.GetComponent<EntityDebugComponent>();
                comp.transform.hideFlags = UnityEngine.HideFlags.HideInInspector;
                comp.entity = entity;
                comp.world = this;
                comp.hasName = hasName;

                this.debugEntities.Add(entity, comp);

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
                if (this.updateNamesTimer >= 3f) {

                    this.updateNamesTimer = 0f;
                    this.UpdateNames();

                }

            }

        }

    }

}
#endif
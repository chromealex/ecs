using ME.ECS;

namespace Prototype.Features.Units.Views {
    
    using ME.ECS.Views.Providers;
    using TEntity = Prototype.Entities.Unit;
    using Prototype.Features.Units.Components;
    
    public class UnitView : MonoBehaviourView<TEntity> {

        public UnityEngine.Animator animator;
        public UnityEngine.Transform bulletPoint;

        [System.Serializable]
        public struct PerPlayerMaterial {

            public int playerIndex;
            public UnityEngine.Material material;

            public void Apply(UnityEngine.Renderer[] renderers) {

                for (int i = 0; i < renderers.Length; ++i) {

                    renderers[i].sharedMaterial = this.material;

                }

            }

        }

        public UnityEngine.Renderer[] renderers;
        public PerPlayerMaterial[] perPlayerMaterials;
        
        private TEntity data;
        
        public override void OnInitialize(in TEntity data) {

            var player = data.entity.GetData<Owner>().value;
            var world = Worlds<PrototypeState>.currentWorld;
            ref var playerData = ref world.GetEntityDataRef<Prototype.Entities.Player>(player);
            var playerIndex = playerData.actorId - 1;

            for (int i = 0; i < this.perPlayerMaterials.Length; ++i) {

                if (this.perPlayerMaterials[i].playerIndex == playerIndex) {

                    this.perPlayerMaterials[i].Apply(this.renderers);

                }

            }

        }
        
        public override void OnDeInitialize(in TEntity data) {
            
        }
        
        public override void ApplyState(in TEntity data, float deltaTime, bool immediately) {

            //this.transform.position = data.entity.GetPosition();
            //this.transform.rotation = data.entity.GetRotation();
            this.data = data;

        }

        public void LateUpdate() {
            
            this.animator.SetFloat("Speed", this.data.entity.GetData<Speed>().value / this.data.entity.GetData<MaxSpeed>().value);
            this.animator.SetBool("Attack", this.data.entity.HasData<IsAttack>());
            this.animator.SetBool("Death", this.data.entity.HasData<IsDead>());
            
            this.transform.position = this.data.entity.GetPosition();
            this.transform.rotation = this.data.entity.GetRotation();

        }

    }
    
}
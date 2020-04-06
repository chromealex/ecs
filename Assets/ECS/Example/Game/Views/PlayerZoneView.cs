using ME.ECS;

namespace ME.Example.Game.Views {
    
    using ME.ECS.Views.Providers;
    using TEntity = Entities.PlayerZone;
    
    public class PlayerZoneView : ParticleViewSource<ApplyPlayerZoneViewStateParticle> { }
    
    [System.Serializable]
    public class ApplyPlayerZoneViewStateParticle : ParticleView<TEntity> {
        
        public override void ApplyState(in TEntity data, float deltaTime, bool immediately) {
            
            ref var rootData = ref this.GetRootData();

            rootData.position = data.position;
            rootData.startColor = data.color;
            rootData.startSize3D = data.scale;
            
            this.SetRootData(ref rootData);
            
        }
        
    }
    
}
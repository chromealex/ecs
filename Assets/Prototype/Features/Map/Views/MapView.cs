using ME.ECS;

namespace Prototype.Features.Map.Views {
    
    using ME.ECS.Views.Providers;
    using TEntity = Entities.Map;
    
    public class MapView : MonoBehaviourView<TEntity> {

        [System.Serializable]
        public struct StartPoint {

            public int playerIndex;
            public UnityEngine.Transform position;

            [System.Serializable]
            public struct UnitInfo {

                public Prototype.Features.Units.Data.SquadData unitData;
                public UnityEngine.Transform position;

            }

            public UnitInfo[] onSpawnUnits;

        }

        public StartPoint[] startPoints;

        public override void OnInitialize(in TEntity data) {
            
            this.transform.position = UnityEngine.Vector3.zero;
            this.transform.rotation = UnityEngine.Quaternion.identity;

        }
        
        public override void OnDeInitialize(in TEntity data) {
            
        }
        
        public override void ApplyState(in TEntity data, float deltaTime, bool immediately) {

        }
        
    }
    
}
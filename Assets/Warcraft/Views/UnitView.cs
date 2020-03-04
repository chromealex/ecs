using ME.ECS;
using UnityEngine;

namespace Warcraft.Views {
    
    using ME.ECS.Views.Providers;
    using Warcraft.Entities;

    public class UnitView : MonoBehaviourView<UnitEntity> {

        public Sprite[] buildingProgressSprites;

        public Color invalidColor = new Color(1f, 0f, 0f, 0.5f);
        public SpriteRenderer spriteRenderer;
        public Vector2Int size;

        public Vector2Int halfSize {
            get {
                return new Vector2Int(this.size.x / 2, this.size.y / 2);
            }
        }
        protected Transform tr;
        
        public override void OnInitialize(in UnitEntity data) {
            
            this.tr = this.transform;
            
        }
        
        public override void OnDeInitialize(in UnitEntity data) {

            this.tr = null;

        }
        
        public override void ApplyState(in UnitEntity data, float deltaTime, bool immediately) {

            var world = Worlds<WarcraftState>.currentWorld;
            
            if (world.HasComponent<UnitEntity, Warcraft.Components.UnitHiddenView>(data.entity) == true) {

                this.tr.position = data.position;
                this.tr.rotation = data.rotation;
                this.tr.localScale = Vector3.zero;
                return;

            }

            var ghosterComponent = world.GetComponent<Warcraft.Entities.UnitEntity, Warcraft.Components.UnitGhosterComponent>(data.entity);
            if (ghosterComponent != null && ghosterComponent.isValid == false) {

                this.spriteRenderer.color = this.invalidColor;

            } else {

                this.spriteRenderer.color = Color.white;

            }

            var progress = world.GetComponent<UnitEntity, Warcraft.Components.UnitBuildingProgress>(data.entity);
            if (progress != null && progress.time > 0f && this.buildingProgressSprites.Length > 0) {

                var value = Mathf.FloorToInt(progress.progress / progress.time * this.buildingProgressSprites.Length);
                var sprite = this.buildingProgressSprites[value];
                this.spriteRenderer.sprite = sprite;

            } else {
                
                if (this.buildingProgressSprites.Length > 0) this.spriteRenderer.sprite = this.buildingProgressSprites[this.buildingProgressSprites.Length - 1];
                
            }

            this.tr.localScale = Vector3.one;
            
            if (immediately == true) {

                this.tr.position = data.position;
                this.tr.rotation = data.rotation;

            } else {
                
                this.tr.position = Vector3.Lerp(this.tr.position, data.position, 10f * deltaTime);
                this.tr.rotation = Quaternion.Lerp(this.tr.rotation, data.rotation, 10f * deltaTime);

            }

        }

    }
    
}
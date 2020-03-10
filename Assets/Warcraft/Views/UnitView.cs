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

        protected IWorld<WarcraftState> world;
        protected Warcraft.Features.PlayersFeature playersFeature;
        protected Warcraft.Features.FogOfWarFeature fowFeature;
        protected Warcraft.Features.MapFeature mapFeature;
        protected Transform tr;
        
        private MaterialPropertyBlock propertyBlock;
        private int targetColorIndexId;
        private int mainTextureId;
        
        public override void OnInitialize(in UnitEntity data) {
            
            this.tr = this.transform;
            this.world = Worlds<WarcraftState>.currentWorld;
            this.mapFeature = this.world.GetFeature<Warcraft.Features.MapFeature>();
            this.fowFeature = this.world.GetFeature<Warcraft.Features.FogOfWarFeature>();
            this.playersFeature = this.world.GetFeature<Warcraft.Features.PlayersFeature>();

            this.propertyBlock = new MaterialPropertyBlock();
            this.targetColorIndexId = Shader.PropertyToID("_Target");
            this.mainTextureId = Shader.PropertyToID("_MainTex");

        }
        
        public override void OnDeInitialize(in UnitEntity data) {

            this.tr = null;

        }
        
        public override void ApplyState(in UnitEntity data, float deltaTime, bool immediately) {

            var world = Worlds<WarcraftState>.currentWorld;
            var unitPlayerOwnerComponent = world.GetComponent<UnitEntity, Warcraft.Components.UnitPlayerOwnerComponent>(data.entity);
            
            if (world.HasComponent<UnitEntity, Warcraft.Components.UnitGhosterComponent>(data.entity) == false &&
                (world.HasComponent<UnitEntity, Warcraft.Components.UnitHiddenView>(data.entity) == true ||
                 (this.playersFeature.IsNeutralPlayer(unitPlayerOwnerComponent.player) == false &&
                  this.fowFeature.IsVisibleAny(this.playersFeature.GetActivePlayer(), data.position, data.size) == false)
                 )) {

                this.tr.position = data.position;
                this.tr.rotation = data.rotation;
                this.tr.localScale = Vector3.zero;
                return;

            }

            world.GetEntityData(unitPlayerOwnerComponent.player, out PlayerEntity playerData);

            var ghosterComponent = world.GetComponent<Warcraft.Entities.UnitEntity, Warcraft.Components.UnitGhosterComponent>(data.entity);
            if (ghosterComponent != null && ghosterComponent.isValid == false) {

                this.spriteRenderer.color = this.invalidColor;

            } else {

                this.spriteRenderer.color = Color.white;

            }

            this.spriteRenderer.sortingOrder = this.mapFeature.GetOrder(data.position);

            this.spriteRenderer.GetPropertyBlock(this.propertyBlock, 0);
            this.propertyBlock.SetFloat(this.targetColorIndexId, playerData.colorIndex);
            this.propertyBlock.SetTexture(this.mainTextureId, this.spriteRenderer.sprite.texture);
            this.spriteRenderer.SetPropertyBlock(this.propertyBlock, 0);

            var progress = world.GetComponent<UnitEntity, Warcraft.Components.UnitBuildingProgress>(data.entity);
            if (progress != null && progress.time > 0f && progress.time < 1f && this.buildingProgressSprites.Length > 0) {

                var value = Mathf.FloorToInt(progress.progress / progress.time * (this.buildingProgressSprites.Length - 1));
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
using UnityEngine;

namespace ME.Example.Game.Views {

    using ME.ECS.Views.Providers;
    using ME.Example.Game.Entities;

    public class ApplyUnitStateToView : MonoBehaviourView<Unit> {

        public Renderer cubeRenderer;
        public float lerpSpeed = 3f;
        private MaterialPropertyBlock materialPropBlock;
        private Transform tr;

        public override void OnInitialize(in Unit data) {

            this.materialPropBlock = new MaterialPropertyBlock();
            
            this.tr = this.cubeRenderer.transform;
            this.tr.position = data.position;

        }

        public override void OnDeInitialize(in Unit data) {

            ApplyUnitStateToView.Destroy(this.cubeRenderer.sharedMaterial);

        }

        public override void ApplyState(in Unit data, float deltaTime, bool immediately) {

            this.materialPropBlock.SetColor("_Color", data.color);
            this.cubeRenderer.SetPropertyBlock(this.materialPropBlock);
            
            if (immediately == true) {

                this.tr.position = data.position;
                this.tr.rotation = data.rotation;
                this.tr.localScale = data.scale;

            } else {

                this.tr.position = Vector3.Lerp(this.tr.position, data.position, deltaTime * this.lerpSpeed);
                this.tr.rotation = Quaternion.Slerp(this.tr.rotation, data.rotation, deltaTime * this.lerpSpeed);
                this.tr.localScale = Vector3.Lerp(this.tr.localScale, data.scale, deltaTime * this.lerpSpeed);

            }

        }

    }

}
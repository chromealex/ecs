using UnityEngine;

namespace ME.Example.Game.Views {

    using ME.ECS.Views.Providers;
    using ME.Example.Game.Entities;

    public class ApplyUnitStateToView : MonoBehaviourView<Unit> {

        public Renderer cubeRenderer;
        public float lerpSpeed = 3f;

        public override void OnInitialize(in Unit data) {

            this.cubeRenderer.sharedMaterial = new Material(this.cubeRenderer.material);

            var tr = this.cubeRenderer.transform;
            tr.position = data.position;

        }

        public override void OnDeInitialize(in Unit data) {

            ApplyUnitStateToView.Destroy(this.cubeRenderer.sharedMaterial);

        }

        public override void ApplyState(in Unit data, float deltaTime, bool immediately) {

            var tr = this.cubeRenderer.transform;
            this.cubeRenderer.sharedMaterial.color = data.color;

            if (immediately == true) {

                tr.position = data.position;
                tr.rotation = data.rotation;
                tr.localScale = data.scale;

            } else {

                tr.position = Vector3.Lerp(tr.position, data.position, deltaTime * this.lerpSpeed);
                tr.rotation = Quaternion.Slerp(tr.rotation, data.rotation, deltaTime * this.lerpSpeed);
                tr.localScale = Vector3.Lerp(tr.localScale, data.scale, deltaTime * this.lerpSpeed);

            }

        }

    }

}
using UnityEngine;

namespace ME.Example.Game.Views {

    using ME.ECS.Views.Providers;
    using ME.Example.Game.Entities;

    public class ApplyPointStateToView : MonoBehaviourView<Point> {

        public TMPro.TMP_Text text;
        public Renderer cubeRenderer;
        public float lerpSpeed = 3f;

        private float prevUnitsCount;

        public override void OnInitialize(in Point data) {

            this.cubeRenderer.sharedMaterial = new Material(this.cubeRenderer.material);

            var tr = this.cubeRenderer.transform;
            tr.position = data.position;

        }

        public override void OnDeInitialize(in Point data) {

            Material.Destroy(this.cubeRenderer.sharedMaterial);

        }

        public override void ApplyState(in Point data, float deltaTime, bool immediately) {

            if (this.prevUnitsCount != data.unitsCount) {

                this.text.text = data.unitsCount.ToString();
                this.prevUnitsCount = data.unitsCount;

            }

            var tr = this.cubeRenderer.transform;
            this.cubeRenderer.sharedMaterial.color = data.color;

            if (immediately == true) {

                tr.position = data.position;
                tr.localScale = data.scale;

            } else {

                tr.position = Vector3.Lerp(tr.position, data.position, deltaTime * this.lerpSpeed);
                tr.localScale = data.scale;

            }

        }

    }

}
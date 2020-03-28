using UnityEngine;

namespace ME.Example.Game.Views {

    using ME.ECS.Views.Providers;
    using ME.Example.Game.Entities;

    public class ApplyPointStateToParticleView : ParticleViewSource<ApplyPointStateParticle> { }

    [System.Serializable]
    public class ApplyPointStateParticle : ParticleView<Point> {

        public float lerpSpeed = 3f;

        public override void ApplyState(in Point data, float deltaTime, bool immediately) {

            ref var rootData = ref this.GetRootData();
            rootData.startColor = data.color;

            if (immediately == true) {

                rootData.position = data.position;
                rootData.startSize3D = data.scale;

            } else {

                rootData.position = Vector3.Lerp(rootData.position, data.position, deltaTime * this.lerpSpeed);
                rootData.startSize3D = data.scale;

            }

            this.SetRootData(ref rootData);

        }

    }

}
using UnityEngine;

namespace ME.GameExample.Game.Views {

    using ME.ECS.Views.Providers;
    using ME.GameExample.Game.Entities;

    public class ApplyUnitProgressStateToParticleView : ParticleViewSource<ApplyUnitProgressStateParticle> { }

    [System.Serializable]
    public class ApplyUnitProgressStateParticle : ParticleView<Unit> {

        public float lerpSpeed = 3f;

        public override void ApplyState(in Unit data, float deltaTime, bool immediately) {

            ref var rootData = ref this.GetRootData();
            rootData.startColor = Color.white;

            var size = 6f;
            var scaleSize = 0.3f;
            var targetScale = new Vector3(data.lifes / data.maxLifes * size * scaleSize, scaleSize, scaleSize);
            
            if (immediately == true) {

                rootData.position = data.position + Vector3.up * 1.5f;
                rootData.rotation3D = data.rotation.eulerAngles;
                rootData.startSize3D = targetScale;

            } else {

                rootData.position = Vector3.Lerp(rootData.position, data.position + Vector3.up * 1.5f, deltaTime * this.lerpSpeed);
                rootData.rotation3D = Quaternion.Slerp(Quaternion.Euler(rootData.rotation3D), data.rotation, deltaTime * this.lerpSpeed).eulerAngles;
                rootData.startSize3D = Vector3.Lerp(rootData.startSize3D, targetScale, deltaTime * this.lerpSpeed);

            }

            this.SetRootData(ref rootData);

        }

    }

}
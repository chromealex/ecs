using UnityEngine;

namespace ME.Example.Game.Views {

    using ME.ECS.Views.Providers;
    using ME.Example.Game.Entities;

    public class ApplyUnitStateToParticleView : ParticleViewSource<ApplyUnitStateParticle> { }

    [System.Serializable]
    public class ApplyUnitStateParticle : ParticleView<Unit> {

        public float lerpSpeed = 3f;

        public override void ApplyState(in Unit data, float deltaTime, bool immediately) {

            ref var rootData = ref this.GetRootData();
            rootData.startColor = data.color;

            if (immediately == true) {

                rootData.position = data.position;
                rootData.rotation3D = data.rotation.eulerAngles;
                rootData.startSize3D = data.scale;

            } else {

                rootData.position = Vector3.Lerp(rootData.position, data.position, deltaTime * this.lerpSpeed);
                rootData.rotation3D = Quaternion.Slerp(Quaternion.Euler(rootData.rotation3D), data.rotation, deltaTime * this.lerpSpeed).eulerAngles;
                rootData.startSize3D = Vector3.Lerp(rootData.startSize3D, data.scale, deltaTime * this.lerpSpeed);

            }

            this.SetRootData(ref rootData);

        }

    }

}
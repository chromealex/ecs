using UnityEngine;

namespace ME.Example.Game.Views {

    using ME.ECS.Views.Providers;
    using ME.Example.Game.Entities;

    public class ApplyUnitStateToDrawMeshView : DrawMeshViewSource<ApplyUnitStateDrawMesh> { }

    [System.Serializable]
    public class ApplyUnitStateDrawMesh : DrawMeshView<Unit> {

        public float lerpSpeed = 3f;

        public override void ApplyState(in Unit data, float deltaTime, bool immediately) {

            var rootData = this.rootData;

            if (immediately == true) {

                rootData.position = data.position;
                rootData.rotation = data.rotation.eulerAngles;
                rootData.scale = data.scale;

            } else {

                rootData.position = Vector3.Lerp(rootData.position, data.position, deltaTime * this.lerpSpeed);
                rootData.rotation = Quaternion.Slerp(Quaternion.Euler(rootData.rotation), data.rotation, deltaTime * this.lerpSpeed).eulerAngles;
                rootData.scale = Vector3.Lerp(rootData.scale, data.scale, deltaTime * this.lerpSpeed);

            }

            this.rootData = rootData;

        }

    }

}
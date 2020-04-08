using UnityEngine;

namespace ME.Example.Game.Views {

    using ME.ECS.Views.Providers;
    using ME.Example.Game.Entities;

    public class ApplyPointStateToDrawMeshView : DrawMeshViewSource<ApplyPointStateDrawMesh> { }

    [System.Serializable]
    public class ApplyPointStateDrawMesh : DrawMeshView<ApplyPointStateDrawMesh, Point> {

        public float lerpSpeed = 3f;

        public override void ApplyState(in Point data, float deltaTime, bool immediately) {

            ref var rootData = ref this.GetRootData();

            if (immediately == true) {

                rootData.position = data.position;
                rootData.scale = data.scale;

            } else {

                rootData.position = Vector3.Lerp(rootData.position, data.position, deltaTime * this.lerpSpeed);
                rootData.scale = data.scale;

            }

            this.SetRootData(ref rootData);

        }

    }

}
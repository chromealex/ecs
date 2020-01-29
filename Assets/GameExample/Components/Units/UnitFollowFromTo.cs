using ME.ECS;

namespace ME.GameExample.Game.Components {

    using ME.GameExample.Game.Entities;

    public class UnitFollowFromTo : IComponent<GameState, Unit> {

        public Entity from;
        public Entity to;

        private System.Collections.Generic.List<UnityEngine.Vector3> pathPoints;
        private int index;

        public void AdvanceTick(GameState state, ref Unit data, float deltaTime, int index) {

            if (this.pathPoints != null) {

                if (this.index < this.pathPoints.Count) {

                    var toPos = this.pathPoints[this.index];
                    toPos.y = data.position.y;
                    var toRot = UnityEngine.Quaternion.LookRotation(toPos - data.position, UnityEngine.Vector3.up);
                    data.position = UnityEngine.Vector3.MoveTowards(data.position, toPos, data.speed * deltaTime);
                    data.rotation = UnityEngine.Quaternion.RotateTowards(data.rotation, toRot, data.speed * 20f * deltaTime);
                    data.position.y = toPos.y;
                    if ((toPos - data.position).sqrMagnitude <= 0.01f) {

                        ++this.index;

                    }

                } else {

                    PoolList<UnityEngine.Vector3>.Recycle(ref this.pathPoints);

                }

            } else {

                Point toData;
                if (this.GetEntityData(this.to.id, out toData) == true) {

                    var path = Pathfinding.ABPath.Construct(data.position, toData.position);
                    AstarPath.StartPath(path);
                    AstarPath.BlockUntilCalculated(path);

                    if (path.IsDone() == false || path.error == true) {

                        Pathfinding.PathPool.Pool(path);
                        PoolList<UnityEngine.Vector3>.Recycle(ref this.pathPoints);
                        this.index = 0;

                    } else {

                        this.pathPoints = PoolList<UnityEngine.Vector3>.Spawn(path.vectorPath.Count);
                        for (int i = 0, count = path.vectorPath.Count; i < count; ++i) {

                            this.pathPoints.Add(path.vectorPath[i]);

                        }

                        this.pathPoints.Add(toData.position);
                        this.index = 0;

                    }

                }

            }

        }

        void IPoolableRecycle.OnRecycle() {

            this.pathPoints = null;
            this.index = 0;

        }

        void IComponent<GameState, Unit>.CopyFrom(IComponent<GameState, Unit> other) {

            var otherUnit = ((UnitFollowFromTo)other);
            this.from = otherUnit.from;
            this.to = otherUnit.to;
            this.index = otherUnit.index;

            if (otherUnit.pathPoints == null) {

                this.pathPoints = null;

            } else {

                if (this.pathPoints != null) PoolList<UnityEngine.Vector3>.Recycle(ref this.pathPoints);
                this.pathPoints = PoolList<UnityEngine.Vector3>.Spawn(otherUnit.pathPoints.Count);
                for (int i = 0; i < otherUnit.pathPoints.Count; ++i) {

                    this.pathPoints.Add(otherUnit.pathPoints[i]);

                }

            }

        }

    }

}
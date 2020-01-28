using ME.ECS;

namespace ME.Example.Game.Components {

    using ME.Example.Game.Entities;

    public class UnitFollowFromTo : IComponent<State, Unit> {

        public Entity from;
        public Entity to;

        private UnityEngine.Vector3[] path;
        private int index;

        public void AdvanceTick(State state, ref Unit data, float deltaTime, int index) {

            Point toData;
            if (this.GetEntityData(this.to.id, out toData) == true) {

                if (this.path == null) {
                    
                    var path = new UnityEngine.AI.NavMeshPath();
                    if (UnityEngine.AI.NavMesh.CalculatePath(data.position, toData.position, -1, path) == true) {

                        UnityEngine.Debug.Log(data.entity.id + ". data.Position: " + data.position.x + ", " + data.position.y + ", " + data.position.z);
                        UnityEngine.Debug.Log(data.entity.id + ". toData.Position: " + toData.position.x + ", " + toData.position.y + ", " + toData.position.z);
                        this.path = path.corners;
                        this.index = 0;

                    }
    
                } else {

                    if (this.index < this.path.Length) {

                        var toPos = this.path[this.index];
                        data.position = UnityEngine.Vector3.MoveTowards(data.position, toPos, data.speed * deltaTime);
                        data.position.y = toPos.y;
                        if ((toPos - data.position).sqrMagnitude <= 0.01f) {

                            ++this.index;

                        }

                    } else {

                        this.path = null;

                    }

                }

            }

        }

        void IComponent<State, Unit>.CopyFrom(IComponent<State, Unit> other) {

            var otherUnit = ((UnitFollowFromTo)other);
            this.from = otherUnit.from;
            this.to = otherUnit.to;
            this.index = otherUnit.index;

            if (otherUnit.path == null) {

                this.path = null;

            } else {

                this.path = new UnityEngine.Vector3[otherUnit.path.Length];
                for (int i = 0; i < otherUnit.path.Length; ++i) {

                    this.path[i] = otherUnit.path[i];

                }

            }

        }

    }

}
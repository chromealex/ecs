using ME.ECS;
using UnityEngine;

namespace ME.GameExample.Game.Entities {

    public struct Unit : IEntity {

        public Entity entity { get; set; }

        public Vector3 position;
        public Color color;
        public Quaternion rotation;
        public Vector3 scale;
        public float speed;
        public Entity pointFrom;
        public Entity pointTo;
        public float maxLifes;
        public float lifes;

    }

}
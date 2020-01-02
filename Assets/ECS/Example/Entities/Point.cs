using ME.ECS;
using UnityEngine;

public struct Point : IEntity {

    public Entity entity { get; set; }

    public Vector3 position;
    public float increaseRate;
    public float unitsCount;
    
}

using ME.ECS;
using UnityEngine;

public struct Unit : IEntity {

    public Entity entity { get; set; }
    
    public Vector3 position;
    public float speed;
    public Entity pointFrom;
    public Entity pointTo;

}

using EntityId = System.Int32;
using Tick = System.UInt64;
using RandomState = UnityEngine.Random.State;

namespace ME.ECS {

    public interface IEntity {

        Entity entity { get; set; }

    }

}
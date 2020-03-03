using EntityId = System.Int32;
using Tick = System.UInt64;
#if UNITY_MATHEMATICS
using RandomState = System.UInt32;
#else
using RandomState = UnityEngine.Random.State;
#endif

namespace ME.ECS {

    public interface IStateBase {

        EntityId entityId { get; set; }
        Tick tick { get; set; }
        RandomState randomState { get; set; }

        int GetHash();

    }

    public interface IState<T> : IStateBase, IPoolableRecycle where T : class, IState<T> {

        void Initialize(IWorld<T> world, bool freeze, bool restore);
        void CopyFrom(T other);

    }

}
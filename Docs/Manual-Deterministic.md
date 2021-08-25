
# Deterministic Operations [![](Logo-Tiny.png)](/../../#glossary)
In ME.ECS there are several places where general logic must be replaced with deterministic logic.

### Random

As you know (I hope) all random values must be deterministic, so if you need to use some random logic, you can use one of these methods:
| Method | Replace |
| ------ | ------- |
| ```float UnityEngine.Random.Range(float, float)``` | ```float world.GetRandomRange(float, float)``` | |
| Returns random float value in range [min, max) |
| ```int UnityEngine.Random.Range(int, int)``` | ```int world.GetRandomRange(int, int)``` |
| Returns random float value in range [min, max) |
| ```Unity.Mathematics.Random::NextFloat(float, float)``` | ```float world.GetRandomRange(float, float)``` |
| Returns random float value in range [min, max) |
| ```Unity.Mathematics.Random::NextInt(int, int)``` | ```int world.GetRandomRange(int, int)``` |
| Returns random int value in range [min, max) |
| ```Unity.Mathematics.Random::NextFloat3(float3, float3)``` | ```Vector3 world.GetRandomInSphere(Vector3 center, float maxRadius)``` |
| Returns random point in sphere |
| ```UnityEngine.Random.value``` | ```float world.GetRandomValue()``` |
| Returns random float in range 0..1 |
| ```Unity.Mathematics.Random::NextFloat(0f, 1f)``` | ```float world.GetRandomValue()``` |
| Returns random float in range 0..1 |

> Note! We are recommend to use Unity.Mathematics package instead of UnityEngine.Random. To use Unity.Mathematics package you should set UNITY_MATHEMATICS define on.

> Note! Using world.GetRandom*() methods couldn't be called inside systems with **jobs** on. You can turn off this check by disabling WORLD_THREAD_CHECK.

### HashSet and Dictionary

In deterministic logic you couldn't use default HashSet and Dictionary with object instances because it calls GetHashCode() method on your instances.
You should override GetHashCode() method to be able to use these collection types.
     
### If you are Fixed-Point Math fan

ME.ECS has FPVector2, FPVector3, FPQuaternion, pfloat and FPMath implementations. If you really need to use fixed-point math you can use any of these structs.

### Burst

Now is 2021 and we are currently have no Burst deterministic functions. So in burst you can use Fixed-Point Math to be sure all calculations are identical on all platforms. It is recommend to use FloatingPoint.High btw.

### Mono/IL2CPP Builds

Unity Editor runs using mono, but all target builds must have IL2CPP compiler to works right with determinism. So you can use Mono builds with Unity Editor and IL2CPP builds with other IL2CPP builds, otherwise you've got some desync point.

[![](Footer.png)](/../../#glossary)

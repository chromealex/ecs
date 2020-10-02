using System.Linq;
using SerializerTestView = ECS.ECS.Serialization.Tests.Resources.SerializerTestView;

namespace ME.ECS.Serializer.Tests {

    using System.Linq;

    public class SerializerTests {

        public struct Person {

            public enum Sex {

                Male,
                Female,

            }

            public int age;
            public string firstName;
            public string lastName;
            public Sex sex;

        }
        
        public struct PerfStruct {

            public Person[] data;

        }

        [NUnit.Framework.TestAttribute]
        public void PerformanceTest() {

            var l = System.Linq.Enumerable.Range(1, 100).Select(x => new Person { age = x, firstName = "Windows", lastName = "Server", sex = Person.Sex.Female }).ToArray();
            var test = new PerfStruct() {
                data = l,
            };
            
            var serializersInternal = Serializer.GetInternalSerializers();
            var serializers = Serializer.GetDefaultSerializers();
            serializers.Add(serializersInternal);

            byte[] lastTest = null;
            for (int i = 0; i < 1000; ++i) {

                var bytes = Serializer.Pack(serializers, test);
                var testRes = Serializer.Unpack<PerfStruct>(serializers, bytes);
                lastTest = bytes;

            }
            
            UnityEngine.Debug.Log("Bytes length: " + lastTest.Length);

        }

        [NUnit.Framework.TestAttribute]
        public void BufferArraySerialization() {
            var test = new TestDataBufferArray {
                viewInfo = new ME.ECS.Views.ViewInfo(Entity.Empty, 12, 23),
                bufferComponents = new ME.ECS.Collections.BufferArray<ME.ECS.Views.ViewComponent>(new [] {
                    new ME.ECS.Views.ViewComponent { seed = 123u, viewInfo = new ME.ECS.Views.ViewInfo(Entity.Empty, 12, 23) }, 
                    null,
                    null,
                    null,
                    null,
                    new ME.ECS.Views.ViewComponent { seed = 123u, viewInfo = new ME.ECS.Views.ViewInfo(Entity.Empty, 12, 23) },
                    null,
                    null,
                }, 6),
                buffer = new ME.ECS.Collections.BufferArray<MyStruct>(new[] {
                    new MyStruct { bar = 1, foo = 2 },
                    new MyStruct { bar = 2, foo = 3 },
                    new MyStruct { bar = 4, foo = 5 },
                    new MyStruct { bar = 6, foo = 7 },
                    new MyStruct { bar = 8, foo = 9 }
                }, 5)
            };


            var ser = new Serializers();
            ser.Add(new BufferArraySerializer());

            var bytes = Serializer.Pack(test, ser);

            var testRes = Serializer.Unpack<TestDataBufferArray>(bytes, ser);
            for (int i = 0; i < test.buffer.Length; ++i) {
                
                NUnit.Framework.Assert.AreEqual(test.buffer.arr[i], testRes.buffer.arr[i]);
                
            }
            
        }

		void DictionarySerializationTest1()
		{
			var test = new TestDataDictionary
			{
				someDict = new System.Collections.Generic.Dictionary<object, object>
				{
					["hello"] = 123,
					[456] = "yo"
				}
			};

			var bytes = Serializer.Pack(test);

			var testRes = Serializer.Unpack<TestDataDictionary>(bytes);
			NUnit.Framework.Assert.AreEqual(test.someDict, testRes.someDict);
		}

		void DictionarySerializationTest2()
		{
			var dic1 = new System.Collections.Generic.Dictionary<ETestEnum, string>();

			dic1.Add(ETestEnum.Second, "Second");
			dic1.Add(ETestEnum.First, "First");

			var test_data = new TestDataDictionary2
			{
				someDict = dic1,
			};

			var bytes = Serializer.Pack(test_data);

			var res_data = Serializer.Unpack<TestDataDictionary2>(bytes);
			NUnit.Framework.Assert.AreEqual(test_data.someDict, res_data.someDict);
		}

		void DictionarySerializationTest3()
		{
			var test_dic = new System.Collections.Generic.Dictionary<ETestEnum, string>();

			test_dic.Add(ETestEnum.Second, "Second");
			test_dic.Add(ETestEnum.First, "First");

			var bytes = Serializer.Pack(test_dic);

			var res_dic = Serializer.Unpack<System.Collections.Generic.Dictionary<ETestEnum, string>>(bytes);
			NUnit.Framework.Assert.AreEqual(res_dic, test_dic);
		}

		[NUnit.Framework.TestAttribute]
        public void DictionarySerialization() {
			DictionarySerializationTest1();
			DictionarySerializationTest2();
			//DictionarySerializationTest3();
		}

        [NUnit.Framework.TestAttribute]
        public void StatesSerialization() {

            var comps = new Components();
            comps.Initialize(100);
            comps.Add(1, new ME.ECS.Views.ViewComponent() { seed = 123u, viewInfo = new ME.ECS.Views.ViewInfo(Entity.Empty, 12, 23) });

            var test = new TestState() {
                components = comps
            };
            
            var ser = new Serializers();
            ser.Add(new BufferArraySerializer());

            var bytes = Serializer.Pack(test, ser);

            var testRes = Serializer.Unpack<TestState>(bytes, ser);
            NUnit.Framework.Assert.AreEqual(test.components.GetData(1).Count, testRes.components.GetData(1).Count);
            
        }

        [NUnit.Framework.TestAttribute]
        public void ArraysSerialization() {
            var test = new TestDataArray {
                buffer = new object[] { 1, 3, 5, 7, 9 },
                buffer2 = new object[] {
                    new[] { 1, 3, 5, 7, 9 },
                    new[] { 0, 2, 4, 6 },
                    new[] { 11, 22 }
                },
                buffer3 = new[] {
                    new object[] { 1, 3, 5, 7, 9 },
                    new object[] { "123", "asdsad", 4, 6 },
                    new object[] { 11, 22 }
                },
                buffer4 = new object[,] { { 1, 2 }, { 3, 4 }, { 5, 6 }, { 7, 8 } }
            };


            var bytes   = Serializer.Pack(test);
            var testRes = Serializer.Unpack<TestDataArray>(bytes);

            NUnit.Framework.Assert.AreEqual(test.buffer, testRes.buffer);
            NUnit.Framework.Assert.AreEqual(test.buffer2, testRes.buffer2);
            NUnit.Framework.Assert.AreEqual(test.buffer3, testRes.buffer3);
            NUnit.Framework.Assert.AreEqual(test.buffer4,testRes.buffer4);
        }

        [NUnit.Framework.TestAttribute]
        public void WorldSerialization() {
            
            World CreateWorld() {

                World world = null;
                WorldUtilities.CreateWorld<TestState>(ref world, 0.033f);
                {
                    world.AddModule<TestStatesHistoryModule>();
                
                    world.SetState<TestState>(WorldUtilities.CreateState<TestState>());

                    //components
                    {
                        ref var sc = ref world.GetStructComponents();
                        ComponentsInitializerWorld.Setup(e => e.ValidateData<TestStructComponent>());
                        CoreComponentsInitializer.Init(ref sc);
                        sc.Validate<TestStructComponent>();
                    }
                    //settings
                    {
                        world.SetSettings(new WorldSettings {
                            useJobsForSystems = false,
                            useJobsForViews   = false,
                            turnOffViews      = false,
                            viewsSettings     = new WorldViewsSettings()
                        });
                        world.SetDebugSettings(WorldDebugSettings.Default);
                    }
                
                    var group = new SystemGroup(world, "GroupName");
                    group.AddSystem<TestSystem>();
                }
            
                var ent = new Entity("Test Entity");
                ent.SetPosition(UnityEngine.Vector3.zero);
                ent.SetData(new TestStructComponent());
            
                world.SaveResetState<TestState>();

                return world;

            }

            var sourceWorld = CreateWorld();
            {
                var dt = 2f;
                sourceWorld.SetFromToTicks(0, 10);
                //sourceWorld.PreUpdate(dt);
                sourceWorld.Update(dt);
                //sourceWorld.LateUpdate(dt);
            }
            var bytes = sourceWorld.GetState().Serialize<TestState>();

            var targetWorld = CreateWorld();
            targetWorld.GetState().Deserialize<TestState>(bytes);
            {

                // Restore new world
                targetWorld.ForEachEntity(out var allEntities);
                for (int j = allEntities.FromIndex, jCount = allEntities.SizeCount; j < jCount; ++j) {

                    var entity = allEntities[j];
                    if (entity.IsAlive() == true) {
                        
                        targetWorld.UpdateEntity(entity);
                        
                    }

                }

                // Test new world

            }
            
            UnityEngine.Debug.Log("Bytes: " + bytes.Length);

            WorldUtilities.ReleaseWorld<TestState>(ref sourceWorld);
            WorldUtilities.ReleaseWorld<TestState>(ref targetWorld);

        }

        public sealed class  TestStatesHistoryModule : ME.ECS.StatesHistory.StatesHistoryModule<TestState> {

            protected override uint GetQueueCapacity() => 10u;

            protected override uint GetTicksPerState() => 20u;

        }
        
        public class TestSystem : ISystemFilter {

            private ViewId viewId;
            
            public World world { get; set; }

            public void OnConstruct() {
                
                var res = UnityEngine.Resources.Load<SerializerTestView>("SerializerTestView");
                if (res == null) UnityEngine.Debug.Log("View is null");
                this.viewId = this.world.RegisterViewSource(res);
                
                UnityEngine.Debug.Log("OKAY");

            }

            public void OnDeconstruct() {
            }

            public bool jobs => false;

            public int jobsBatchCount => 64;

            public Filter filter { get; set; }

            public Filter CreateFilter() => Filter.Create("Filter-TestStructComponent").WithStructComponent<TestStructComponent>().Push();

            public void AdvanceTick(in Entity entity, in float deltaTime) {

                ref var data = ref entity.GetData<TestStructComponent>();
                ++data.f;
                
                var pos = entity.GetPosition();
                pos += UnityEngine.Vector3.one;
                entity.SetPosition(pos);
                
                entity.InstantiateView(this.viewId);
                
            }

        }
        
        public class TestState : State { }

        public struct TestStructComponent : IStructComponent {

            public int f;

        }

        public struct TestDataBufferArray {

            public ME.ECS.Views.ViewInfo viewInfo;
            public ME.ECS.Collections.BufferArray<MyStruct> buffer;
            public ME.ECS.Collections.BufferArray<ME.ECS.Views.ViewComponent> bufferComponents;
        }

        public struct TestDataArray {

            public object[]   buffer;
            public object[]   buffer2;
            public object[][] buffer3;
            public object[,] buffer4;
        }

		public enum ETestEnum: byte { First, Second, Last }
        
        public struct TestDataDictionary {
            public System.Collections.Generic.Dictionary<object, object> someDict;
        }

		public struct TestDataDictionary2
		{
			public System.Collections.Generic.Dictionary<ETestEnum, string> someDict;
		}

		public struct MyStruct : System.IEquatable<MyStruct> {

            public int bar;
            public int foo;

            public override string ToString() => $"{this.bar} {this.foo}";

            public bool Equals(MyStruct other) => this.bar == other.bar && this.foo == other.foo;

            public override bool Equals(object obj) => obj is MyStruct other && Equals(other);

            public override int GetHashCode() {
                unchecked {
                    return (this.bar * 397) ^ this.foo;
                }
            }

            public static bool operator ==(MyStruct left, MyStruct right) => left.Equals(right);

            public static bool operator !=(MyStruct left, MyStruct right) => !left.Equals(right);

        }

        public class FakeNetworkModule : ME.ECS.Network.NetworkModule<TestState> {

            protected override void OnInitialize() {
                var tr       = new FakeTransporter();
                var instance = (ME.ECS.Network.INetworkModuleBase)this;
                instance.SetTransporter(tr);
                instance.SetSerializer(new FakeSerializer());

                // this.photonTransporter = tr;
            }

            public class FakeReceiver {
                
            }
            
            public class FakeTransporter : ME.ECS.Network.ITransporter {
                public System.Collections.Generic.Queue<byte[]> sentData       = new System.Collections.Generic.Queue<byte[]>();
                public System.Collections.Generic.Queue<byte[]> sentSystemData = new System.Collections.Generic.Queue<byte[]>();

                public System.Collections.Generic.Queue<byte[]> receivedData       = new System.Collections.Generic.Queue<byte[]>();
                public System.Collections.Generic.Queue<byte[]> receivedSystemData = new System.Collections.Generic.Queue<byte[]>();

                private int sentCount;
                private int sentBytesCount;
                private int receivedCount;
                private int receivedBytesCount;
                
                private FakeReceiver fakeReceiver;

                public bool IsConnected() => true;

                public void Send(byte[] bytes) {
                    this.sentData.Enqueue(bytes);

                    this.sentBytesCount += bytes.Length;
                    ++this.sentCount;
        }

                public void SendSystem(byte[] bytes) {
                    this.sentSystemData.Enqueue(bytes);

                    this.sentBytesCount += bytes.Length;
                }

                public byte[] Receive() {
                    if (this.receivedData.Count == 0) {

                        if (this.receivedSystemData.Count == 0) return null;

                        var bytes = this.receivedSystemData.Dequeue();

                        this.receivedBytesCount += bytes.Length;

                        return bytes;

                    } else {

                        var bytes = this.receivedData.Dequeue();

                        ++this.receivedCount;
                        this.receivedBytesCount += bytes.Length;

                        return bytes;

                    }
    }

                public int GetEventsSentCount()          => this.sentCount;
                public int GetEventsBytesSentCount()     => this.sentBytesCount;
                public int GetEventsReceivedCount()      => this.receivedCount;
                public int GetEventsBytesReceivedCount() => this.receivedBytesCount;

            }

            public class FakeTransportBridge {

                //todo meta for delays, packets drop, etc
                public System.Collections.Generic.List<FakeTransporter> transporters;

                public void Step() {
                    foreach (var fakeTransporter in this.transporters) {
                        foreach (var otherTransporter in this.transporters.Where(t => t != fakeTransporter)) {
                            foreach (var data in fakeTransporter.sentData) {
                                otherTransporter.receivedData.Enqueue(data);
                            }

                            foreach (var data in fakeTransporter.sentSystemData) {
                                otherTransporter.receivedSystemData.Enqueue(data);
                            }
                        }
                    }
                }

            }

            public class FakeSerializer : ME.ECS.Network.ISerializer {

                public ME.ECS.StatesHistory.HistoryStorage DeserializeStorage(byte[] bytes) {
                    throw new System.NotImplementedException();
                }

                public byte[] SerializeStorage(ME.ECS.StatesHistory.HistoryStorage historyStorage) {
                    throw new System.NotImplementedException();
                }

                public byte[] Serialize(ME.ECS.StatesHistory.HistoryEvent historyEvent) => ME.ECS.Serializer.Serializer.Pack(historyEvent);
                public ME.ECS.StatesHistory.HistoryEvent Deserialize(byte[] bytes) => ME.ECS.Serializer.Serializer.Unpack<ME.ECS.StatesHistory.HistoryEvent>(bytes);
            }
        }
    }
}
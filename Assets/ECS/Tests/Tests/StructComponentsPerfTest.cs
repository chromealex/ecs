using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ME.ECS;
using Unity.PerformanceTesting;

namespace Tests
{

    public struct TestComponent : ME.ECS.IStructComponent { }

    public struct TestComponent2 : ME.ECS.IStructComponent { }

    public struct TestComponent3 : ME.ECS.IStructComponent { }

    public struct TestComponent4 : ME.ECS.IStructComponent { }

    public class TestState : IState<TestState> {

        public int entityId { get; set; }
        public Tick tick { get; set; }
        public uint randomState { get; set; }

        public FiltersStorage filters;
        public StructComponentsContainer<TestState> s;
        public Storage test;
        
        public int GetHash() {

            return 1;

        }

        public void Initialize(IWorld<TestState> world, bool freeze, bool restore) {
            
            world.Register(ref this.filters, freeze, restore);
            world.Register(ref this.s, freeze, restore);
            world.Register(ref this.test, freeze, restore);
            
        }
        public void CopyFrom(TestState other) { }

        public void OnRecycle() {
            
            
        }

    }

    public class PerfTests {

        [Test, Performance]
        public void FiltersPerfTestSimplePasses() {

            World<TestState> world = null;
            WorldUtilities.CreateWorld(ref world, 1f);
            world.SetState(WorldUtilities.CreateState<TestState>());

            for (int i = 0; i < 10; ++i) {

                Filter<TestState>.Create().WithStructComponent<TestComponent>().Push();

            }

            var f4 = Filter<TestState>.Create().WithStructComponent<TestComponent>().WithStructComponent<TestComponent2>().WithStructComponent<TestComponent3>().WithStructComponent<TestComponent4>().Push();
            var f3 = Filter<TestState>.Create().WithStructComponent<TestComponent>().WithStructComponent<TestComponent3>().WithStructComponent<TestComponent4>().Push();
            Filter<TestState>.Create().WithStructComponent<TestComponent>().WithStructComponent<TestComponent4>().Push();
            Filter<TestState>.Create().WithStructComponent<TestComponent>().WithStructComponent<TestComponent2>().Push();
            Filter<TestState>.Create().WithStructComponent<TestComponent2>().Push();
            Filter<TestState>.Create().WithStructComponent<TestComponent3>().Push();
            Filter<TestState>.Create().WithStructComponent<TestComponent4>().Push();

            IFilter<TestState> f = null;
            Filter<TestState>.Create().WithoutStructComponent<TestComponent>().Push(ref f);
            
            using (Measure.Scope("Set Component")) {

                var test = new TestComponent();
                for (int i = 0; i < 100000; ++i) {

                    var ent = new Entity(i, 0);
                    world.SetData(ent, test, updateFilters: false);
                    
                }

            }

            using (Measure.Scope("Get Filter")) {

                for (int i = 0; i < 100000; ++i) {

                    var ent = new Entity(i, 0);
                    var filter = world.GetFilter(1);
                    
                }

            }

            using (Measure.Scope("Add_INTERNAL")) {

                var filter = (IFilterInternal<TestState>)world.GetFilter(1);
                for (int i = 0; i < 100000; ++i) {

                    var ent = new Entity(i, 0);
                    filter.Add_INTERNAL(ent);

                }

            }

            using (Measure.Scope("Add Component")) {

                //var filter = (IFilterInternal<TestState>)world.GetFilter<Tests.Entities.TestEntity>(1);
                /*Debug.Log(ArchetypeEntities.Get(new Entity(1, 0)));
                Debug.Log((filter as Filter<TestState, Tests.Entities.TestEntity>).archetypeContains);
                Debug.Log((f3 as Filter<TestState, Tests.Entities.TestEntity>).archetypeContains);
                Debug.Log((f4 as Filter<TestState, Tests.Entities.TestEntity>).archetypeContains);*/
                var ent = new Entity(1, 0);
                for (int i = 0; i < 100000; ++i) {

                    world.AddComponentToFilter(ent);
                    
                }

            }

        }

        [Test, Performance]
        public void StructComponentsPerfTestSimplePasses() {

            var instance = new StructComponents<TestState, TestComponent>();

            using (Measure.Scope("Create Entity")) {

                for (int i = 0; i < 100000; ++i) {

                    var ent = new Entity(i, 0);

                }

            }

            using (Measure.Scope("Set")) {

                var test = new TestComponent();
                for (int i = 0; i < 100000; ++i) {

                    var ent = new Entity(i, 0);
                    instance.Set(in ent, in test);

                }

            }

            using (Measure.Scope("Get")) {

                for (int i = 0; i < 100000; ++i) {

                    var ent = new Entity(i, 0);
                    instance.Get(in ent);

                }

            }

            using (Measure.Scope("Has")) {

                var ent = new Entity(100, 0);
                for (int i = 0; i < 100000; ++i) {

                    instance.Has(in ent);

                }

            }

            using (Measure.Scope("Remove")) {

                for (int i = 0; i < 100000; ++i) {

                    var ent = new Entity(i, 0);
                    instance.Remove(in ent);

                }

            }

        }

    }
}

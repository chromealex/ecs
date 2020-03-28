using ME.ECS;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ME.Example.Tests {

    using ME.Example.Game;
    using ME.Example.Game.Systems;
    using ME.Example.Game.Modules;
    using ME.Example.Game.Components;
    using ME.Example.Game.Entities;
    using Unity.PerformanceTesting;
    using UnityEngine.TestTools.Constraints;
    using Is = UnityEngine.TestTools.Constraints.Is;
    
    public class CollectionTests {

        [Test]
        public void RefList() {

            var list = new ME.ECS.Collections.RefList<Entity>();
            var idx0 = list.Add(new Entity());
            var idx1 = list.Add(new Entity());
            var idx2 = list.Add(new Entity());

            Assert.True(list.SizeCount == 3);
            Assert.True(list.UsedCount == 3);

            list.RemoveAt(idx1);
            
            Assert.True(list.UsedCount == 2);
            Assert.True(list.SizeCount == 3);

        }

        public struct TestStruct {

            public int data;

        }

        [Test, Unity.PerformanceTesting.PerformanceAttribute]
        public void StackArrayTest() {
            
            var allocated = new SampleGroupDefinition("TotalAllocatedMemory", SampleUnit.Megabyte);
            var reserved = new SampleGroupDefinition("TotalReservedMemory", SampleUnit.Megabyte);

             var mem1 = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
             var mem2 = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong();
        
            Unity.PerformanceTesting.Measure.Method(() => {

                 ME.ECS.Collections.StackArray<TestStruct> arr2;
                 var arr = new ME.ECS.Collections.StackArray<TestStruct>(1000);
                 for (int i = 0; i < arr.Length; ++i) {
            
                     arr[i] = new TestStruct() { data = i };
            
                 }

                 arr2 = arr;
                 arr[0] = new TestStruct() { data = 10 };
                 for (int i = 0; i < arr.Length; ++i) {
            
                     Assert.True(i == arr2[i].data);
            
                 }
        
                 Assert.True(10 == arr[0].data);
                 
            }).WarmupCount(100)
               .MeasurementCount(100)
               .IterationsPerMeasurement(5)
               .GC()
               .Definition(sampleUnit: Unity.PerformanceTesting.SampleUnit.Microsecond)
               .Run();
            
            Assert.That(() => {

                ME.ECS.Collections.StackArray<TestStruct> arr2;
                var arr = new ME.ECS.Collections.StackArray<TestStruct>(1000);
                for (int i = 0; i < arr.Length; ++i) {
            
                    arr[i] = new TestStruct() { data = i };
            
                }

                arr2 = arr;
                arr[0] = new TestStruct() { data = 10 };
                
            }, Is.Not.AllocatingGCMemory());
            
            Measure.Custom(allocated, (UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() - mem1) / 1048576f);
            Measure.Custom(reserved, (UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong() - mem2) / 1048576f);
                 
        }

        [Test, Unity.PerformanceTesting.PerformanceAttribute]
        public void StackArrayTestCompare() {

            var allocated = new SampleGroupDefinition("TotalAllocatedMemory", SampleUnit.Megabyte);
            var reserved = new SampleGroupDefinition("TotalReservedMemory", SampleUnit.Megabyte);

            var mem1 = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
            var mem2 = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong();

            Unity.PerformanceTesting.Measure.Method(() => {

                var arr = new TestStruct[1000];
                for (int i = 0; i < arr.Length; ++i) {
                    
                    arr[i] = new TestStruct() { data = i };
                    
                }

                var arr2 = new TestStruct[1000];
                System.Array.Copy(arr, arr2, arr.Length);
                
                arr[0] = new TestStruct() { data = 10 };
                for (int i = 0; i < arr.Length; ++i) {
                    
                    Assert.True(i == arr2[i].data);
                    
                }
                
                Assert.True(10 == arr[0].data);
                    
            }).WarmupCount(100)
            .MeasurementCount(100)
            .IterationsPerMeasurement(5)
            .GC()
            .Definition(sampleUnit: Unity.PerformanceTesting.SampleUnit.Microsecond)
            .Run();

            Measure.Custom(allocated, (UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() - mem1) / 1048576f);
            Measure.Custom(reserved, (UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong() - mem2) / 1048576f);


        }

    }

}
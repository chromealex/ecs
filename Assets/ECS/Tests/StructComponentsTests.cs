using ME.ECS;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ME.Example.Tests {

    public class StructComponents {

        public struct TestComponent : IStructComponent {

            public int data1;
            public int data2;

        }

        [Test]
        public void SetGetRemove() {

            var entity = Entity.Empty;

            var container = new StructComponentsContainer();
            container.Set(entity, new TestComponent() { data1 = 1, data2 = 2 });
            Assert.True(container.Has<TestComponent>(entity));

            var data = container.Get<TestComponent>(entity);
            Assert.True(data.data1 == 1);
            Assert.True(data.data2 == 2);
            data.data1 = 3;
            data.data2 = 4;
            container.Set(entity, data);
            data = container.Get<TestComponent>(entity);
            Assert.True(data.data1 == 3);
            Assert.True(data.data2 == 4);
            
            container.Remove<TestComponent>(entity);
            Assert.False(container.Has<TestComponent>(entity));

        }

        [Test]
        public void CopyFrom() {

            var entity = Entity.Empty;

            var container = new StructComponentsContainer();
            container.Set(entity, new TestComponent() { data1 = 1, data2 = 2 });
            Assert.True(container.Has<TestComponent>(entity));

            var data = container.Get<TestComponent>(entity);
            Assert.True(data.data1 == 1);
            Assert.True(data.data2 == 2);
            
            var container2 = new StructComponentsContainer();
            container2.CopyFrom(container);

            var data2 = container2.Get<TestComponent>(entity);
            Assert.True(data2.data1 == 1);
            Assert.True(data2.data2 == 2);
            
            data.data1 = 3;
            data.data2 = 4;
            container.Set(entity, data);
            
            data2 = container2.Get<TestComponent>(entity);
            Assert.True(data2.data1 == 1);
            Assert.True(data2.data2 == 2);

            container.Remove<TestComponent>(entity);
            Assert.False(container.Has<TestComponent>(entity));
            Assert.True(container2.Has<TestComponent>(entity));
            
        }

    }

}
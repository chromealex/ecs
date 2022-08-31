# Creating Entities [![](Logo-Tiny.png)](/../../#glossary)

Entity – is an abstract idea of something that exists in a world. In context of ECS – an entity is something to represent a data container with no data attached. Basically, entity is an ID, and you can specify components (data) to be associated with that ID.

In ME.ECS – Entities are structs, that have a name (relevant in editor only), a generation and a version. They do not have anything by themselves, and it is your job to do something with them. Entities could be created by in several ways

```csharp
var entity = this.world.AddEntity(); 
var entity = new Entity("someName");
var entity = Entity.Create("someName");
```

> **Warning**
> If you are using ```new Entity()``` instead of ```this.world.AddEntity()``` you **must** provide a name and/or a flag.

Entities can have certain flags associated with them, controlling their lifetime. By default – if you do not provide a flag yourself, or choose None flag – entity's lifetime would be infinite, as in – you would have to manually remove entity from the world (entity would be returned to the pool upon Destroy() call).

| Flag | Description |
| ------ | ----- |
| ```EntityFlag.None``` | Entity would never been removed, you should destroy it manually. ```Default``` behaviour. |
| ```EntityFlag.OneShot``` | Entity would destroy automatically at the end of the tick. |
| ```EntityFlag.DestroyWithoutComponents``` | Entity would destroy automatically at the end of the tick if there are no components on it. |

You can have a OneShot entity, that would be (as a OneShotComponent) removed and returned to the pool at the end of a logic tick it was created in.

You can have a DestroyWithoutComponents entity, that would be automatically removed and returned to the pool if it has no components associated with it.

```csharp
var entity = this.world.AddEntity("someName"); // adds an entity with a name and a default flag
var entity = this.world.AddEntity(EntityFlag.DestroyWithoutComponents); // adds an entity with no name and a destroy flag 
var entity = this.world.AddEntity("someEntity", EntityFlag.OneShot); // adds an entity with a name and a OneShot flag 

var entity = new Entity("someEntity"); // adds an entity with a name and a default flag 
var entity = new Entity(EntityFlag.DestroyWithoutComponents); // adds an entity with no name and a destroy flag
var entity = new Entity("someName", EntityFlag.OneShot); // adds an entity with a name and a OneShot flag

var entity = Entity.Create("someName"); // adds an entity with a name and a default flag
var entity = Entity.Create(EntityFlag.None); // adds an entity with no name and a default flag
var entity = Entity.Create("someName", EntityFlag.OneShot); // adds an entity with no name and a OneShot flag
```

You can remove an entity at any given time during AdvanceTick() or in any RPC, by calling entity.Destroy().
> **Warning**
> This should be done at the end of the logic call, or the loop should be continued or broken, to exclude any errors.
```csharp
void ISystemFilter.AdvanceTick(in Entity entity, in float deltaTime) {

	if (someCondition) {
        
		entity.Destroy();
		
	}
	
}
```

## Entity Methods
| Method | Description |
| ------ | ----- |
| ```SetAs<TComponent>(dataConfig)``` | Set entity's component state as in data config. |
| ```SetAs<TComponent>(entity)``` | Set entity's component state as in source entity. |
| <code>CopyFrom(entity,&#160;[copyHierarchy])</code> | Clear target entity and add all components as defined on source entity. ```copyHierarchy``` is optional and by default is ```true```. |

## What are the generation and version for?
Entities in ME.ECS are pooled for better performance, and every entity is generation 0 while it is dormant, but as soon as you create it from the pool – it’s generation goes up, indicating, that this entity has already been created, processed and recycled.
Versions are almost the same, but they indicate entity’s changes throughout it’s lifetime (within one generation). Entity’s version is changed every time a component is added, removed, or component’s data is changed.

You can access different entities properties
```csharp
var id = entity.id; // returns you entity’s id
var generation = entity.generation; // returns entity’s generation
var version = entity.GetVersion(); // returns current version of the entity
```
You can also check if the entity is alive or if the entity was created or referenced at all
```csharp
var alive = entity.IsAlive(); // returns whether the entity is in the world or was returned to the pool
var exists = entity.IsEmpty(); // returns whether the entity was created at all
var existsWithinBounds = entity.IsAliveWithBoundsCheck(); // returns whether the entity is alive with some checks.
```

> **Warning**
> ```IsAliveWithBoundsCheck()``` should be called from UI scripts as they might reference a sate that has been changed.

Also, there are several methods to help you debug the entity

```csharp
var noVersion = entity.ToStringNoVersion() ; // returns a formatted string with Id and  Generation
var full = entity.ToString(); // returns a formatted string, with an Id, generation and version
var small = entity.ToSmallString(); // same as above, but formatted shorter
```

## Entities Group
If you want to create multiple entities as fast as you can, you may use EntitiesGroup.
```csharp
// Length - amount of entities you want to create
// Allocator - Unity Allocator to store entities collection
// CopyMode - if set true some sort of optimizations will be used (in general this mean that you will create the same components onto all entities)
var group = world.AddEntities(100, Allocator.Temp, copyMode: true);
group.Set(new YourComponent()); // Set component data onto all entities
config.Apply(group); // Apply config onto all entities
```

[![](Footer.png)](/../../#glossary)

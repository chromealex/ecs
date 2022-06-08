# Creating Entities [![](Logo-Tiny.png)](/../../#glossary)
For creating entities you need to run world.AddEntity().
Names are used for Editor debug information only, so they are optional.
> If you use new Entity() instead of this.world.AddEntity() you **must** name and/or flags.
```csharp
var entity = this.world.AddEntity([name], [EntityFlag]);
var entity = new Entity([name], [EntityFlag]);
...
```

#### EntityFlag
In ME.ECS you have a few options to create Entity:

| Flag | Description |
| ------ | ----- |
| None | Default flag. Entity would never beed destroyed, you need to destroy it manually. |
| OneShot | Entity will be destroyed at the end of the tick. |
| DestroyWithoutComponents | Entity will be destroyed at the end of the tick if there are no components on it. |

[![](Footer.png)](/../../#glossary)

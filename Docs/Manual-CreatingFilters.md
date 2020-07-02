# Creating Filters [![](Logo-Tiny.png)](/../../#glossary)
In ME.ECS filters are storing entities with the certain components they have or not.
Filters must be created in constructors of systems or features and shouldn't been added at the runtime.
By default in **System with Filter** you have already defined filter and have a AdvanceTick method to implement logic working with certain entity.

But sometimes you need to create filters manually in constructors like this:
```csharp
Filter filter;

void ISystemBase.OnConstruct() {

    Filter.Create("YourFilterName").WithStructComponent<MyStructComponent>().WithoutComponent<MyComponent>().Push(ref this.filter);

}
```

You can use these methods to filter your entities:
> All methods are combined with **AND** operator

| Method | Description |
| ----- | ----- |
| WithStructComponent\<T\> | Filters all entities having T struct component |
| WithoutStructComponent\<T\> | Filters all entities that doesn't have T struct component |
| WithComponent\<T\> | Filters all entities having T class component |
| WithoutComponent\<T\> | Filters all entities that doesn't have T class component |

[![](Footer.png)](/../../#glossary)

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
| With\<T\> | Filters all entities having T struct component |
| Without\<T\> | Filters all entities that doesn't have T struct component |
| SetOnEntityAdd\<T\> | Call Execute method on callback instance when new entity added into filter |
| SetOnEntityRemove\<T\> | Call Execute method on callback instance when new entity removed into filter |
| OnVersionChangedOnly\<T\> | Filtered only entities if its version has changed. filter.ToArray() and filter.GetEnumerator() resets these versions. |

## Filter Actions
Define: ENTITY_ACTIONS
In ME.ECS you could use SetOnEntityAdd/SetOnEntityRemove for filters, but on high level you can use FilterAction class to automatically receive onAdd/onRemove callbacks as System.Action:

```csharp
void ISystemBase.OnConstruct() {

    FilterAction.Create(onAdd: (entity) => {
        // Here is your code
    }, onRemove: (entity) => {
        // Here is your code
    }, customName: "YourFilterActionName").WithStructComponent<MyStructComponent>().WithoutComponent<MyComponent>().Push();

}
```

As you see, all default With/Without filters can be used in FilterAction as in Filter.

[![](Footer.png)](/../../#glossary)

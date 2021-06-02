# Creating Filters [![](Logo-Tiny.png)](/../../#glossary)
In ME.ECS filters are storing entities with the certain components they have or not.
Filters must be created in OnConstruct methods and shouldn't be added at the runtime.
By default in **System with Filter** you have already defined filter and have an AdvanceTick method to implement logic working with certain entity.

But sometimes you need to create filters manually in constructors like this:
```csharp
Filter filter;

void ISystemBase.OnConstruct() {

    Filter.Create("YourFilterName").With<MyStructComponent>().Without<MyComponent>().Push(ref this.filter);

}
```

You can use these methods to filter your entities:
> All methods are combined with **AND** operator

| Method | Description |
| ----- | ----- |
| ```With<T>``` | Filters all entities having T component |
| ```Without<T>``` | Filters all entities that don't have T component |
| ```WithShared<T>``` | Filter works only if static shared T component exists |
| ```WithoutShared<T>``` | Filter works only if static shared T component doesn't exist |
| ```SetOnEntityAdd<T>``` | Call Execute method on callback instance when new entity added into filter |
| ```SetOnEntityRemove<T>``` | Call Execute method on callback instance when new entity removed into filter |
| ```OnVersionChangedOnly``` | Filtered only entities if its version has changed. filter.ToArray() and filter.GetEnumerator() resets these versions. |

## Multiple Filters
MultipleFilters - it is the solution which combines two filters inside and allows to filter Any<T0, T1> or WithoutAny<T0, T1> components:
```csharp
MultipleFilter filter;

void ISystemBase.OnConstruct() {

    MultipleFilter.Create("YourMultipleFilterName").Any<Component1, Component2>().Push(ref this.filter);

}
```

You can use these methods to filter your entities:
> All methods are combined with **AND** operator

| Method | Description |
| ----- | ----- |
| ```With<T>``` | Filters all entities having T component |
| ```Without<T>``` | Filters all entities that don't have T component |
| ```Any<T0, T1>``` | Filters all entities having T0 or T1 component |
| ```WithoutAny<T0, T1>``` | Filters all entities that don't have T0 and T1 component |
| ```OnVersionChangedOnly``` | Filtered only entities if its version has changed. filter.ToArray() and filter.GetEnumerator() resets these versions. |

## Filter Actions
Define: **ENTITY_ACTIONS**

In ME.ECS you could use SetOnEntityAdd/SetOnEntityRemove for filters, but on high level you can use FilterAction class to automatically receive onAdd/onRemove callbacks as System.Action:

```csharp
void ISystemBase.OnConstruct() {

    FilterAction.Create(onAdd: (entity) => {
        // Here is your code
    }, onRemove: (entity) => {
        // Here is your code
    }, customName: "YourFilterActionName").With<MyStructComponent>().Without<MyComponent>().Push();

}
```

As you see, all default With/Without filters can be used in FilterAction as in Filter.

[![](Footer.png)](/../../#glossary)

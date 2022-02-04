# Creating Filters (Archetypes) [![](Logo-Tiny.png)](/../../#glossary)

> Note! Turn on this feature with FILTERS_STORAGE_ARCHETYPES. Be default this storage type is inactive.

In ME.ECS filters are storing archetypes with the certain components they have or not.
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
| ```Any<T1, T2>``` | Filters all entities having T1 or T2 component |
| ```Any<T1, T2, T3>``` | Filters all entities having T1 or T2 or T3 component |
| ```Any<T1, T2, T3, T4>``` | Filters all entities having T1 or T2 or T3 or T4 component |
| ```WithLambda<TLambda, T>``` | Filters all entities having T component and TLambda::Execute(T) returns true |

[![](Footer.png)](/../../#glossary)

PocketContainer
===============

[![Build Status](https://ci.appveyor.com/api/projects/status/github/jonsequitur/PocketContainer?svg=true)](https://ci.appveyor.com/project/jonsequitur/pocketcontainer)

### An embeddable IoC container in a single C# file, with support for extensibility and conventions.

When you install the `PocketContainer` NuGet package, it will add the `PocketContainer` C# file to your project. `PocketContainer` is an internal class and it's quite small, around 250 lines of code including the comments. Embedding the class accomplishes a few goals: 

- Discourages usage as a service locator.
- If you want the capabilities of an IoC container within a library, it avoids forcing an additional assembly dependency on consumers of that library.
- Enables customizations that are very specific to your use case because you can tinker with the code or compose behaviors using partial classes.

#### How do you use it?

In the basics, it has the familiar-looking Register and Resolve methods, with generic and non-generic variants:

```csharp
    var container = new PocketContainer();

    container.Register<IDoThings>(c => c.Resolve<DoSpecificThings>());
    // or...
    container.Register(typeof(IDoThings), c => c.Resolve<DoSpecificThings>());
```

The `Register` methods all take delegates. Many of the constructs that most containers have for lifetime can be replicated using closures.

The `Resolve` methods will recursively satisfy all arguments to the longest constructor on a type. Concrete types don't require explicit registration. This is the default strategy for PocketContainer, but others can be added. This extensibility mechanism allows you to specify how to handle unregistered types the first time someone tries to resolve them.

Here's an example that resolves a concrete type if only one is found:

```csharp
    container.AddStrategy(type =>
    {
        if (type.IsInterface || type.IsAbstract)
        {
            var implementations = Discover.ConcreteTypes()
                                          .DerivedFrom(type)
                                          .ToArray();

            if (implementations.Count() == 1)
            {
                return c => c.Resolve(implementations.Single());
            }
        }
        return null;
    });
```


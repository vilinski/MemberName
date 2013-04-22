MemberName
------------

**MemberName** is a ReSharper plugin. 
Provides additional annotation attributes

The `[MemberName]` attribute allows to mark string parameter of any method as a property name. 
R#7 has already `[NotifyPropertyChangedInvocator]` attribute. 
It makes possible to interpret the string literals parameter of 
`INotifyPropertyChanged.NotifyPropertyChanged(string)` method as a member reference. 
It allows allows to use intellisense by writing such methods, 
highlight warnings by missing or misspelled property names, and shows the property in the find usages window.

HINT:
> The `[NotifyPropertyChangedInvocator]` attribute can be misused to mark string parameter of 
any methods as a member name. But it has a limitation to the members of current (or inherited) class.

The `MemberNameAttribute` just tries to remove this limitation and allows to define the type, 
where to look for members. The main intention was to use it in my Windows.Forms binding manager, 
where first method argument of a Bind... method is a data source instance, 
and second is the member of the data source, expressed by it's name in a string literal.

```csharp
public class UnderTest
{
  public void BindToUnderTest(
    UnderTest dataSrc,
    [MemberName(typeof (UnderTest))] string member1,
    [MemberName("dataSrc")] string member2,
    [TypeName("DataSourceTestProjekt.UnderTest")] string member1
    )
    {
      // some binding logic
    }

  public string SomeMember { get; set; }
}
```

Further development will be to add a `[TypeName]` attribute to use it in loosely coupled Builder's, 
where the type is provided by it's full name, because the type can not be accessed statically from the 
assembly where the builder is defined.

```csharp
  [Builder("Some.Name.Space.SomeBusinessObject")]
  public class SomeBuisinessObjectBuilder : BusinessObjectBuilder
  {
    // some builder logic
  }
```
and the corresponding `BuilderAttribute` with the `TypeNameAttribute` usages:
```csharp
  [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
  public sealed class BuilderAttribute : Attribute
  {
    private string m_businessClassName;
    
    public DataBuilderForBusinessClassAttribute([TypeName]string businessClassName)
    {
      BusinessClassName = businessClassName;
    }

    [TypeName]
    public string BusinessClassName { get; set;}
  }
```

Installation
------------
Visual Studio 2010 or Visual Studio 2012 is required.  
[ReSharper 7.1.x](http://www.jetbrains.com/resharper/) must be installed.
There are no installer yet. Just the binary version of MemberName from [here](https://dl.dropboxusercontent.com/u/10938305/MemberName.dll) into ReSharper's plugins directory.  
Note that the software is currently in pre-alpha phase. [Open an issue](https://github.com/vilinski/MemberName/issues) if it eats your wife, pets and children, or if you encounter any other problems.
Contributors are welcome.


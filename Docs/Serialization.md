# Serialization

Powerful serialization tool for workflow.

## SerializedType{T}

Serialize type of class implementing T and get new object from it at runtime.

```C#
SerializedType<ICustomInterface> myType;
ICustomInterface customInterface = myType.GetObject();
```

## SerializedObject{T}

More managable than Unity's `SerializeReference` attribute.

Serialize type and data of object implementing T and get template object from it at runtime.

You can edit all properties in the Insepector like `ScriptableObject` while finally serializing as a simple C# object.

```C#
SerializedObject<ICustomInterface> myType;
ICustomInterface customInterface = myType.GetObject();
```
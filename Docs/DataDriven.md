# Data Driven

Use Unreal-like DataTable workflow in Unity.

## Example

```C#
using System;
using Kurisu.Framework.DataDriven;
using Kurisu.Framework.Resource;
using Kurisu.Framework.Serialization;
using UnityEngine;

// Must add this attribute
[Serializable]
public class MyDataRow : IDataTableRow
{
    public int id;
    public SoftAssetReference reference;
    public string name;
}
```


## Editor

Support two kinds of editing mode.

Edit DataTable in Inspector.

![DataTable Inspector](./Images/datatable.png)

Edit DataTable in an EditorWindow (opened by double-left clicked).

![DataTable EditorWindow](./Images/datatable_editor_window.png)



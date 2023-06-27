using System;
using System.Collections.Generic;


[Serializable]
public class jsonFilter
{
    public List<filteredNamels> filteredNames;
}

[Serializable]
public class filteredNamels
{
    public string word;
}

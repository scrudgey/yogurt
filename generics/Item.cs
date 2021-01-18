using UnityEngine;
public class Item : Interactive {
    public string itemName;
    [TextArea(3, 10)]
    public string description;
    [TextArea(3, 10)]
    public string longDescription;
    public string referent;
    [TextArea(5, 10)]
    public string impDescription;
    public bool the;
}

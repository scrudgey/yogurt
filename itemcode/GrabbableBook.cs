using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// itemName = pickup.itemName;
//         description = pickup.description;
//         longDescription = pickup.longDescription;

public class GrabbableBook : Grabbable, ISaveable {
    private Book _book;
    public Book book {
        get {
            return _book;
        }
        set {
            _book = value;
            SetDesc();
        }
    }
    override public void Start() {
        if (book == null) {
            book = new Book("Book", "Joe Book");
        }
        itemPrefab = Resources.Load("prefabs/book") as GameObject;
        base.Start();
        itemName = "book";
        description = book.Describe();
    }
    override public GameObject Get(Inventory inventory) {
        GameObject item = base.Get(inventory);
        BookPickup pickup = item.GetComponent<BookPickup>();
        if (pickup != null) {
            pickup.book = book;
        }
        return item;
    }
    public void SetDesc() {
        itemName = "book";
        description = book.Describe();
    }
    public void SaveData(PersistentComponent data) {
        data.strings["title"] = book.title;
        data.strings["author"] = book.author;
        data.strings["comments"] = book.comments;
        data.strings["reading"] = book.reading;
    }
    public void LoadData(PersistentComponent data) {
        book = new Book(data.strings["title"], data.strings["author"], data.strings["comments"], data.strings["reading"]);
    }
}

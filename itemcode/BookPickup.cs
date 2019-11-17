using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookPickup : Pickup, ISaveable {
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
    public void SaveData(PersistentComponent data) {
        data.strings["title"] = book.title;
        data.strings["author"] = book.author;
    }
    public void LoadData(PersistentComponent data) {
        book = new Book(data.strings["title"], data.strings["author"]);
    }
    public void SetDesc() {
        itemName = "book";
        description = book.Describe();
    }
}

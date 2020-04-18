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
    MessageSpeech message;
    public SpriteRenderer spriteRenderer;
    public Sprite[] bookSprites;
    public int sprite;
    public void Awake() {
        Interaction read = new Interaction(this, "Read", "Read");
        read.otherOnSelfConsent = false;
        read.holdingOnOtherConsent = false;
        interactions.Add(read);

        sprite = Random.Range(0, bookSprites.Length);
        spriteRenderer.sprite = bookSprites[sprite];
    }
    public void SaveData(PersistentComponent data) {
        data.strings["title"] = book.title;
        data.strings["author"] = book.author;
        data.strings["comments"] = book.comments;
        data.strings["reading"] = book.reading;

        data.ints["sprite"] = sprite;
    }
    public void LoadData(PersistentComponent data) {
        book = new Book(data.strings["title"], data.strings["author"], data.strings["comments"], data.strings["reading"]);
        sprite = data.ints["sprite"];
        spriteRenderer.sprite = bookSprites[sprite];
    }
    public void SetDesc() {
        itemName = "book";
        description = book.Describe();
        message = new MessageSpeech(book.reading);
    }
    public void Read(Speech speech) {
        Toolbox.Instance.SendMessage(speech.gameObject, this, message);
    }
    public string Read_desc(Speech speech) {
        return "Read book";
    }
    public void LateUpdate() {
        if (book == null) {
            book = Library.nextBook();
        }
    }
}

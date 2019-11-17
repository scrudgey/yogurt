using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Book {
    public string author;
    public string title;
    public string comments;
    public Book(string title, string author, string comments) {
        this.author = author;
        this.title = title;
        this.comments = comments;
    }
    public Book(string title, string author) {
        this.title = title;
        this.author = author;
    }
    public Book(string title) {
        this.title = title;
    }
    public Book() {
        new Book("Blank book", "", "");
    }
    public string Describe() {
        string desc;
        if (author != "") {
            desc = title + ", by " + author + ".";
        } else { desc = title + "."; }
        if (comments != "") {
            desc += comments + ".";
        }
        return desc;
    }
}

public class Library : MonoBehaviour {

    public static List<Book> books = new List<Book>{
        new Book("The Life and Death of Julius Ceasar", "Gordon Farragut"),
        new Book("Physiology of Sneezing", ""),
        new Book("Beyond Fly Fishing", "Ceril B. Hayes"),
        new Book("The Heptameron"),
        new Book("Dune"),
        new Book("How to Tiptoe the Tippy-Toe Way"),
        new Book("A pamphlet on puppet craft"),
        new Book("A pamphlet on puppet craft"),
        new Book("Poetry of the Isle of Man"),
        new Book("Cave Art of Lascaux"),
        new Book("Design Principles for High-Security Prisons"),
        new Book("Fairies are Real and I Can Prove It", "Esmerelda Livingstone"),
        new Book("The Little Book of Vegetarian Recipes"),
        new Book("Moby Dick: Part II"),
        new Book("The Book of the Law"),
        // Various illustrated guides to castles, pirate ships, caves, other planets, the deep sea, and things like that.
        new Book("Physics of Stellar Interiors"),
        new Book("Shock and Awe: How I Made Millions Buying and Selling Pogs", "Shack Ripkin"),
        new Book("The Book of Lies"),
        new Book("The Origin of Clowns in Hyperdimensional Visition", "", "From Madame Tahuti Press"),
        new Book("Rock Gardens of New England", "Landstrom"),
        new Book("History of Atlantis", ""),
        new Book("The Pyramid Builders from Sirius", "Wilson R. Dick"),
        new Book("Oh, Sandcastles!"),
        new Book("Collected volumes of Anthropomorphic Tank High School Harem"),
        new Book("The Art of Cezanne"),
        new Book("Teas of the World"),
        new Book("A Photodiary of Deep Sea Diving"),
        new Book("Father Humility", "Joshua Quentin David", "Hailed by some as the new great American novel"),
        new Book("Journal of Dairy Science Volume 62"),
        new Book("Arrest me, arrest you! : zen-dada prose poems by Gillian Isaac"),
        new Book("Dolems, Henges, Megaliths: Large Rocks and Where To Find Them", "Sophia M."),
        new Book("Undoing Yourself with Energized Meditation and Other Devices", "C. S. Hyatt"),
        new Book("Hardy, Hardy, and Powers. Nuclear Reactor Engineering" , "", "3rd Ed"),
        new Book("Illustrated Circus Sideshows"),
        new Book("The Book of Urizen", "William Blake"),
        new Book("Faust", "Goethe"),
        new Book("A biography of Beethoven"),
        new Book("The Long and Bloody Trail West", "Atlas McTavish"),
        new Book("I Fought Bigfoot at Yosemite Falls", "", "15 pages, illustrated"),
        new Book("The Briefcase: A History"),
        new Book("A travel guide to Nova Scotia"),
        new Book("Paying the Ultimate Price: Fast Lives and Loose Change in the Vending Machine Underworld"),
        new Book("The Autocrat of the Breakfast Nook", "Judge Henry Adams Shipman Jr."),
        new Book("Coolidge", "P. Hoffman"),
        new Book("Encyclopedia of Sci-Fi Robots"),
    };
    public static Stack<Book> bookBag = new Stack<Book>();

    void Start() {
        List<GrabbableBook> grabbables = new List<GrabbableBook>(GameObject.FindObjectsOfType<GrabbableBook>());
        foreach (GrabbableBook grabbable in grabbables) {
            if (bookBag.Count == 0) {
                bookBag = new Stack<Book>(Toolbox.Shuffle(books));
            }
            grabbable.book = bookBag.Pop();
        }
    }
}

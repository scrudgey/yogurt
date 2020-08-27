using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Book {
    public string author = "";
    public string title = "";
    public string comments = "";
    public string reading = "";
    public Book(string title, string author, string comments, string reading) {
        this.author = author;
        this.title = title;
        this.comments = comments;
        this.reading = reading;
    }
    public Book(string title, string author, string reading) {
        this.title = title;
        this.author = author;
        this.reading = reading;
    }
    public Book(string title, string reading) {
        this.title = title;
        this.reading = reading;
    }
    public Book() {
        new Book("Blank book", "", "", "It's blank.");
    }
    public string Describe() {
        string desc;
        if (author != "") {
            desc = title + ", by " + author + ".";
        } else { desc = title + "."; }
        if (comments != "") {
            desc += " " + comments + ".";
        }
        return desc;
    }
}

public class Library : MonoBehaviour {
    public static List<Book> books = new List<Book>{
        new Book("The Life and Death of Julius Ceasar", "Gordon Farragut", "Never heard of the guy."),
        new Book("Physiology of Sneezing", "", "Quite a fascinating little thing, the uvula."),
        new Book("Beyond Fly Fishing", "Ceril B. Hayes", "Say what you will, but Ceril Hayes really did take fly fishing to a new level."),
        new Book("The Heptameron", "This book contains various instructions for summoning angels."),
        new Book("How to Tiptoe the Tippy-Toe Way", "Now I know!"),
        new Book("A pamphlet on puppet craft", "It has instructions for various different kinds of puppets."),
        new Book("Poetry of the Isle of Man", "Who am I to judge? But it's not really my thing."),
        new Book("Cave Art of Lascaux", "Pretty cool, wish I could have painted a cave wall 17,000 years ago."),
        new Book("Design Principles for High-Security Prisons", "This is dry stuff."),
        new Book("Fairies are Real and I Can Prove It", "Esmerelda Livingstone", "", "Not sure, but I don't think she really proved the existence of fairies here."),
        new Book("The Little Book of Vegetarian Recipes", @"""Number 36: falafel curry"""),
        new Book("Moby Dick: Part II", "I didn't think the first one needed a soft reboot by a young adult author in 2019."),
        new Book("The Book of the Law", "I won't discuss its contents."),
        // Various illustrated guides to castles, pirate ships, caves, other planets, the deep sea, and things like that.
        new Book("Physics of Stellar Interiors", "Stars are apparently just a bunch of hot gas."),
        new Book("Shock and Awe: How I Made Millions Buying and Selling Pogs", "Shack Ripkin", "", "This guy is so cool!"),
        new Book("The Book of Lies", "It seems a rather unsavory book."),
        new Book("The Origin of Clowns in Hyperdimensional Visition", "", "From Madame Tahuti Press", "This is pretty far-out stuff!"),
        new Book("Rock Gardens of New England", "Landstrom", "", @"""Thus, its climate makes New England the perfect locale for a charming rock garden."""),
        new Book("History of Atlantis", "Wow, I never knew all this stuff about Atlantis!"),
        new Book("The Pyramid Builders from Sirius", "Wilson R. Dick", "This guy is just putting me on!"),
        new Book("Oh, Sandcastles!", "The author displays an apparent love of sandcastles and their craft."),
        new Book("Collected volumes of Anthropomorphic Tank High School Harem", "I don't think I am the intended audience here."),
        new Book("The Art of Cezanne", "Pretty pictures!"),
        new Book("Teas of the World", @"""Number 20: Lu An Gua Pian green tea"""),
        new Book("A Photodiary of Deep Sea Diving", "Exotic, fascinating, dangerous, and a little scary."),
        new Book("Father Humility", "Joshua Quentin David", "Hailed by some as the new great American novel", "I'm exhausted by all these authors with their big books!"),
        new Book("Journal of Dairy Science Volume 62", "The section on yogurt production is dogeared and highlighted."),
        new Book("Arrest me, arrest you! : zen-dada prose poems by Gillian Isaac", @"""Cop, cop / everybody's a cop / doo wop doo wop"""),
        new Book("Dolems, Henges, Megaliths: Large Rocks and Where To Find Them", "Sophia M.", "", "I love a good henge!"),
        new Book("Undoing Yourself with Energized Meditation and Other Devices", "C. S. Hyatt", "", "I feel my robotic behavior dropping away already!"),
        new Book("Hardy, Hardy, and Powers. Nuclear Reactor Engineering" , "", "3rd Ed", "I feel inspired! I want to become a nuclear reactor engineer!"),
        new Book("Illustrated Circus Sideshows", "Elaborate re-creations, diagrams, background, and history of various circus stunts."),
        new Book("The Book of Urizen", "William Blake", @"What is this guy talking about?"),
        new Book("Faust", "Goethe", "If this Faust guy is so smart, why does he keep making all these dumb decisions?"),
        new Book("A biography of Beethoven", "Five chapters, quite illuminating."),
        new Book("The Long and Bloody Trail West", "Atlas McTavish", "Wow, how nihilistic and violent! This must be a Very Important Book!"),
        new Book("I Fought Bigfoot at Yosemite Falls", "Anon", "15 pages, illustrated", "What rubbish!"),
        new Book("The Briefcase: A History", "I never realized I had so many questions about briefcases!"),
        new Book("A travel guide to Nova Scotia", "That's where Moby was from, right? Oh wait, that was "),
        new Book("Paying the Ultimate Price: Fast Lives and Loose Change in the Vending Machine Underworld", "Written with unusual clarity and insight. Self-published."),
        new Book("The Autocrat of the Breakfast Nook", "Judge Henry Adams Jr.", "What a pompous windbag!"),
        new Book("Coolidge", "P. Hoffman", @"Ah, yes. The ""Oatmeal and Toast"" President."),
        new Book("Encyclopedia of Sci-Fi Robots", @"""Number 104: K9, the robotic dog."""),
        new Book("UFOs and The Military Industrial Establishment", "Roger Saltpepper", "", "Very thick and thoroughly documented."),
        new Book("I Loved Bigfoot in Acadia National Park", "Anon", "20 pages, illustrated", "I find this book troubling."),
        new Book("The Dummy's Guide to Amassing Wealth", "Matt Lictor", "Second edition", "The actual financial advice here, such as it is, is flimsy at best."),
        new Book("101 Photographs of Snails!", "Bob", "", "If I had to pick a favorite, it'd be #30."),
        new Book("I Hate Fish!", "Susan Wraithewhite", "", "She makes a strong argument."),
        new Book("Murder in the Sex House", "Bill Powers", "", "I can't wait to find out what happens next!"),
        new Book("Outta My Face, World! The Tammy Baxter Story", "Tammy Baxter", "", "I feel a renewed lust for life!"),
    };
    public static Stack<Book> bookBag = new Stack<Book>();

    void LateUpdate() {
        if (GameManager.Instance.data != null && GameManager.Instance.data.mayorLibraryShuffled)
            return;
        GameManager.Instance.data.mayorLibraryShuffled = true;
        List<GrabbableBook> grabbables = new List<GrabbableBook>(GameObject.FindObjectsOfType<GrabbableBook>());

        foreach (GrabbableBook grabbable in grabbables) {
            if (Random.Range(0, 1f) < 0.5f) {
                Destroy(grabbable.gameObject);
                continue;
            }
            grabbable.book = nextBook();
        }
        Destroy(gameObject);
    }

    public static Book nextBook() {
        if (bookBag.Count == 0) {
            Debug.Log("shuffling bookbag");
            bookBag = new Stack<Book>(Toolbox.Shuffle(books));
        }
        return bookBag.Pop();
    }
}

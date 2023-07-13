using Logic;
using Model;

namespace Tests;

public class DeckTests
{
    private const int NumberOfCards = 52;
    private Deck _deck = null!;

    [SetUp]
    public void SetUp() => _deck = new Deck();

    [Test]
    public void All_cards_drawn_from_deck_are_unique()
    {
        var cards = new HashSet<Card>();
        for (var i = 0; i < NumberOfCards; i++)
            cards.Add(_deck.Draw());

        Assert.That(cards, Has.Count.EqualTo(NumberOfCards));
        Assert.That(() => _deck.Draw(), Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void Draw_sequences_after_reset_are_not_equal()
    {
        var sequence1 = new HashSet<Card>();
        for (var i = 0; i < NumberOfCards; i++)
            sequence1.Add(_deck.Draw());

        _deck.Reset();

        var sequence2 = new HashSet<Card>();
        for (var i = 0; i < NumberOfCards; i++)
            sequence2.Add(_deck.Draw());

        Assert.Throws<AssertionException>(() => CollectionAssert.AreEqual(sequence1, sequence2));
    }
}
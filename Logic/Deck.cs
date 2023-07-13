using Model;

namespace Logic;

public class Deck
{
    private static readonly Random Random = new();

    private readonly List<Card> _cards;

    private int _position;

    public Deck()
    {
        _cards = new List<Card>
        {
            Cards.AceClubs,
            Cards.AceDiamonds,
            Cards.AceHearts,
            Cards.AceSpades,
            Cards.TwoClubs,
            Cards.TwoDiamonds,
            Cards.TwoHearts,
            Cards.TwoSpades,
            Cards.ThreeClubs,
            Cards.ThreeDiamonds,
            Cards.ThreeHearts,
            Cards.ThreeSpades,
            Cards.FourClubs,
            Cards.FourDiamonds,
            Cards.FourHearts,
            Cards.FourSpades,
            Cards.FiveClubs,
            Cards.FiveDiamonds,
            Cards.FiveHearts,
            Cards.FiveSpades,
            Cards.SixClubs,
            Cards.SixDiamonds,
            Cards.SixHearts,
            Cards.SixSpades,
            Cards.SevenClubs,
            Cards.SevenDiamonds,
            Cards.SevenHearts,
            Cards.SevenSpades,
            Cards.EightClubs,
            Cards.EightDiamonds,
            Cards.EightHearts,
            Cards.EightSpades,
            Cards.NineClubs,
            Cards.NineDiamonds,
            Cards.NineHearts,
            Cards.NineSpades,
            Cards.TenClubs,
            Cards.TenDiamonds,
            Cards.TenHearts,
            Cards.TenSpades,
            Cards.JackClubs,
            Cards.JackDiamonds,
            Cards.JackHearts,
            Cards.JackSpades,
            Cards.QueenClubs,
            Cards.QueenDiamonds,
            Cards.QueenHearts,
            Cards.QueenSpades,
            Cards.KingClubs,
            Cards.KingDiamonds,
            Cards.KingHearts,
            Cards.KingSpades
        };

        Reset();
    }

    public Card Draw()
    {
        if (_position == _cards.Count)
            throw new InvalidOperationException("Deck is empty");

        var card = _cards[_position];
        _position++;
        return card;
    }

    public void Reset()
    {
        _position = 0;
        _cards.Shuffle(Random);
    }
}
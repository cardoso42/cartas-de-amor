using CartasDeAmor.Domain.Cards;
using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Factories;

public static class CardFactory
{
    public static Card Create(CardType card)
    {
        return card switch
        {
            CardType.Spy => new Spy(),
            CardType.Guard => new Guard(),
            CardType.Priest => new Priest(),
            CardType.Baron => new Baron(),
            CardType.Servant => new Servant(),
            CardType.Prince => new Prince(),
            CardType.Chanceller => new Chanceller(),
            CardType.King => new King(),
            CardType.Countess => new Countess(),
            CardType.Princess => new Princess(),
            _ => throw new ArgumentException($"Invalid card type: {card}")
        };
    }

    public static ICollection<Card> CreateDeck()
    {
        // Card distribution according to Love Letter rules
        var deck = new List<Card>();

        // Add 2 Spies
        for (int i = 0; i < 2; i++)
            deck.Add(new Spy());

        // Add 5 Guards
        for (int i = 0; i < 5; i++)
            deck.Add(new Guard());
            
        // Add 2 Priests
        for (int i = 0; i < 2; i++)
            deck.Add(new Priest());
            
        // Add 2 Barons
        for (int i = 0; i < 2; i++)
            deck.Add(new Baron());
            
        // Add 2 Servants (Handmaids)
        for (int i = 0; i < 2; i++)
            deck.Add(new Servant());
            
        // Add 2 Princes
        for (int i = 0; i < 2; i++)
            deck.Add(new Prince());

        // Add 2 Chancellers
        for (int i = 0; i < 2; i++)
            deck.Add(new Chanceller());
            
        // Add unique cards
        deck.Add(new King());
        deck.Add(new Countess());
        deck.Add(new Princess());

        return deck;
    }

    public static ICollection<Card> CreateShuffledDeck()
    {
        var deck = CreateDeck().ToList();
        Random rng = new Random();
        
        int n = deck.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (deck[k], deck[n]) = (deck[n], deck[k]);
        }
        
        return deck;
    }
}

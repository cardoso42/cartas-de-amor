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
}

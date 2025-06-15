using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Cards;

public class Servant : Card
{
    public Servant()
    {
        Name = "Servant";
        Value = 4;
    }

    public override CardType CardType => CardType.Servant;

    public override CardActionResults Play(Game game, Player invokerPlayer, Player? targetPlayer, CardType? targetCardType)
    {
        // Player is protected from being targeted by other players' cards until their next turn.
        invokerPlayer.SetProtection(true);
        
        return CardActionResults.ProtectionGranted;
    }

    public override CardRequirements? GetCardActionRequirements()
    {
        return null;
    }
}
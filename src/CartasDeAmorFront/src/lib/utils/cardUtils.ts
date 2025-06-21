import { CardType, CardActionRequirements } from '$lib/types/game-types';

/**
 * Get the display name for a card type
 */
export function getCardName(cardType: CardType): string {
  const cardNames = {
    [CardType.Spy]: 'Spy',
    [CardType.Guard]: 'Guard',
    [CardType.Priest]: 'Priest', 
    [CardType.Baron]: 'Baron',
    [CardType.Handmaid]: 'Handmaid',
    [CardType.Prince]: 'Prince',
    [CardType.Chanceller]: 'Chanceller',
    [CardType.King]: 'King',
    [CardType.Countess]: 'Countess',
    [CardType.Princess]: 'Princess'
  };
  return cardNames[cardType] || 'Unknown';
}

/**
 * Get the display name for a card action requirement
 */
export function getRequirementName(requirement: CardActionRequirements): string {
  switch (requirement) {
    case CardActionRequirements.None: return 'None';
    case CardActionRequirements.SelectPlayer: return 'Select Player';
    case CardActionRequirements.SelectCardType: return 'Select Card Type';
    default: return 'Unknown';
  }
}

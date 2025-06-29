import { CardType, CardActionRequirements } from '$lib/types/game-types';
import { get } from 'svelte/store';
import { _ } from 'svelte-i18n';

/**
 * Get the display name for a card type
 */
export function getCardName(cardType: CardType): string {
  const cardNames = {
    [CardType.Spy]: get(_)('cards.spy'),
    [CardType.Guard]: get(_)('cards.guard'),
    [CardType.Priest]: get(_)('cards.priest'),
    [CardType.Baron]: get(_)('cards.baron'),
    [CardType.Handmaid]: get(_)('cards.handmaid'),
    [CardType.Prince]: get(_)('cards.prince'),
    [CardType.Chanceller]: get(_)('cards.chanceller'),
    [CardType.King]: get(_)('cards.king'),
    [CardType.Countess]: get(_)('cards.countess'),
    [CardType.Princess]: get(_)('cards.princess')
  };
  return cardNames[cardType] || get(_)('cards.unknown');
}

/**
 * Get the display name for a card action requirement
 */
export function getRequirementName(requirement: CardActionRequirements): string {
  switch (requirement) {
    case CardActionRequirements.None: return get(_)('game.none');
    case CardActionRequirements.SelectPlayer: return get(_)('game.selectPlayer');
    case CardActionRequirements.SelectCardType: return get(_)('game.selectCardType');
    default: return get(_)('cards.unknown');
  }
}

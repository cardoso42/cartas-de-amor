# Internationalization (i18n) Guide

This project uses `svelte-i18n` for internationalization support. The system is set up to support English (en), Spanish (es), and Portuguese (pt) with English as the default language.

## Structure

```
src/lib/i18n/
├── index.ts              # Main i18n configuration
├── locales/
│   ├── en.json          # English translations
│   ├── es.json          # Spanish translations
│   └── pt.json          # Portuguese translations
└── README.md            # This file
```

## Usage

### In Svelte Components

1. Import the translation function:
```typescript
import { _ } from 'svelte-i18n';
```

2. Use it in your template:
```svelte
<script>
  import { _ } from 'svelte-i18n';
</script>

<h1>{$_('auth.login')}</h1>
<p>{$_('common.loading')}</p>
```

### In TypeScript/JavaScript Files

```typescript
import { _ } from 'svelte-i18n';

export function getCardName(cardType: CardType): string {
  return $_('cards.spy'); // Returns translated card name
}
```

### Language Switching

The `LanguageSwitcher` component is available in the navbar and allows users to change the language. The selected language is stored in localStorage and persists across sessions.

## Adding New Translations

1. Add the new key to all language files (`en.json`, `es.json`, `pt.json`)
2. Use the key in your components with `$_('your.key')`

### Example

In `en.json`:
```json
{
  "newFeature": {
    "title": "New Feature",
    "description": "This is a new feature"
  }
}
```

In `es.json`:
```json
{
  "newFeature": {
    "title": "Nueva Función",
    "description": "Esta es una nueva función"
  }
}
```

In your component:
```svelte
<h2>{$_('newFeature.title')}</h2>
<p>{$_('newFeature.description')}</p>
```

## Translation Keys Organization

The translations are organized into logical groups:

- `common`: Common UI elements (loading, error, success, etc.)
- `auth`: Authentication-related text (login, register, etc.)
- `navigation`: Navigation menu items
- `rooms`: Game room management
- `game`: In-game text and messages
- `cards`: Card names and descriptions
- `errors`: Error messages
- `ui`: General UI elements

## Adding a New Language

1. Create a new JSON file in `locales/` (e.g., `fr.json`)
2. Add the language registration in `src/lib/i18n/index.ts`:
```typescript
register('fr', () => import('./locales/fr.json'));
```
3. Add the language to the `LanguageSwitcher` component:
```typescript
const languages = [
  { code: 'en', name: 'English', flag: '🇺🇸' },
  { code: 'es', name: 'Español', flag: '🇪🇸' },
  { code: 'pt', name: 'Português', flag: '🇧🇷' },
  { code: 'fr', name: 'Français', flag: '🇫🇷' } // New language
];
```

## Best Practices

1. **Use descriptive keys**: Instead of `'msg'`, use `'auth.loginFailed'`
2. **Group related translations**: Keep related translations under the same namespace
3. **Use interpolation for dynamic content**: `$_('game.playingCard', { cardName: 'Spy' })`
4. **Keep translations consistent**: Use the same terminology across the app
5. **Test all languages**: Make sure your UI works well with different text lengths

## Interpolation

For dynamic content, you can use interpolation:

```json
{
  "game": {
    "playerTurn": "It's {playerName}'s turn"
  }
}
```

```svelte
<p>{$_('game.playerTurn', { playerName: 'John' })}</p>
```

This will output: "It's John's turn" 
# CartasDeAmorFront

This is the frontend application for the Love Letter card game.

## Development Setup

Once you've created a project and installed dependencies with `npm install` (or `pnpm install` or `yarn`), start a development server:

```bash
npm run dev

# or start the server and open the app in a new browser tab
npm run dev -- --open
```

## API Configuration

The application is configured to communicate with the backend API. By default, it will proxy API requests to the server specified in your environment variables.

### Environment Variables

Create a `.env` file in the root of the project with the following variables:

```
API_URL=http://localhost:5000  # Change this to your backend API URL
```

This will be used by Vite's proxy configuration to route API requests appropriately.

### API Usage

The application uses a centralized configuration for API endpoints in `src/lib/config/api-config.ts`.
All API calls should use this configuration rather than hardcoding URLs.

Example usage:

```typescript
import API_CONFIG from '$lib/config/api-config';

// Making an API call
const response = await fetch(API_CONFIG.auth.login, {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(data)
});
```
```

## Building

To create a production version of your app:

```bash
npm run build
```

You can preview the production build with `npm run preview`.

> To deploy your app, you may need to install an [adapter](https://svelte.dev/docs/kit/adapters) for your target environment.

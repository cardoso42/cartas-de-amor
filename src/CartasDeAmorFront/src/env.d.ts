/// <reference types="vite/client" />

/**
 * SvelteKit environment variable type definitions
 * These are public variables that will be exposed to the browser
 */
interface ImportMetaEnv {
  readonly PUBLIC_API_BASE_URL: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}

// Make PUBLIC_API_BASE_URL available via $env/static/public import in SvelteKit
declare module '$env/static/public' {
  export const PUBLIC_API_BASE_URL: string;
}

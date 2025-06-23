import { svelteTesting } from '@testing-library/svelte/vite';
import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig } from 'vite';
import * as dotenv from 'dotenv';

// Load environment variables from .env file
dotenv.config();

export default defineConfig({
	plugins: [sveltekit()],
	server: {
		port: 3000, // Frontend server port
		proxy: {
			'/api': {
				target: process.env.VITE_API_URL || 'http://localhost:5149',
				changeOrigin: true,
				rewrite: (path) => path
			},
			'/gamehub': {
				target: process.env.VITE_API_URL || 'http://localhost:5149',
				changeOrigin: true,
				ws: true // Enable WebSocket proxying for SignalR
			}
		}
	},
	test: {
		projects: [
			{
				extends: './vite.config.ts',
				plugins: [svelteTesting()],
				test: {
					name: 'client',
					environment: 'jsdom',
					clearMocks: true,
					include: ['src/**/*.svelte.{test,spec}.{js,ts}'],
					exclude: ['src/lib/server/**'],
					setupFiles: ['./vitest-setup-client.ts']
				}
			},
			{
				extends: './vite.config.ts',
				test: {
					name: 'server',
					environment: 'node',
					include: ['src/**/*.{test,spec}.{js,ts}'],
					exclude: ['src/**/*.svelte.{test,spec}.{js,ts}']
				}
			}
		]
	}
});

// This is a mock implementation for the login API during development
// Replace this with your actual API integration

import type { RequestHandler } from '@sveltejs/kit';

export const POST = (async ({ request }) => {
  // Extract the login credentials from the request
  const data = await request.json();
  const { username, password } = data;

  // Mock authentication logic (replace with actual authentication)
  if (username === 'admin' && password === 'password') {
    // Successful login
    return new Response(JSON.stringify({
      token: 'mock-jwt-token',
      success: true
    }), {
      status: 200,
      headers: {
        'Content-Type': 'application/json'
      }
    });
  } else {
    // Failed login
    return new Response(JSON.stringify({
      success: false,
      message: 'Invalid username or password'
    }), {
      status: 401,
      headers: {
        'Content-Type': 'application/json'
      }
    });
  }
}) satisfies RequestHandler;

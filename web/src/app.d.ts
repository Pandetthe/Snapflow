/// <reference types="vite-plugin-pwa/info" />
/// <reference types="vite-plugin-pwa/client" />
/// <reference types="vite-plugin-pwa/svelte" />

import type { User } from '$lib/features/users/api/users';

declare module '*.svelte';

declare global {
  namespace App {
    interface Locals {
      session: string | null;
      user: User | null;
    }
  }
}

export { };

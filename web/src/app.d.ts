// See https://svelte.dev/docs/kit/types#app.d.ts
// for information about these interfaces
import type { User } from '$lib/services/users';

declare global {
  namespace App {
    interface Locals {
      session: string | null;
      user: User | null;
    }
  }
}

export { };

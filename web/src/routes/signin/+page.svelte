<script lang="ts">
  import { goto } from '$app/navigation';
  import { env } from '$env/dynamic/public';

  let email = '';
  let emailErrors: string[] = [];
  let password = '';
  let passwordErrors: string[] = [];
  let showPassword = false;

  async function handleSignIn() {
    emailErrors = [];
    passwordErrors = [];

    try {
      const res = await fetch('/api/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, password }),
      });

      if (res.ok) {
          goto('/', { invalidateAll: true });
      } else {
        const body = await res.json();

        if (body.title === 'Validation.General') {
          emailErrors = body.errors
            .filter(e => e.propertyName === 'Email')
            .map(e => e.description);
          passwordErrors = body.errors
            .filter(e => e.propertyName === 'Password')
            .map(e => e.description);
        }
      }
    } catch (error) {
      console.error("Sign-up failed", error);
    }
  }
</script>

<svelte:head>
  <title>Sign in</title>
</svelte:head>

<div class="flex bg-gray-700 h-full w-full justify-center items-center p-4">
  <form on:submit|preventDefault={handleSignIn} class="w-full max-w-md">
    <fieldset class="fieldset bg-gray-900 p-6 rounded-lg shadow-lg">
      <div class="text-2xl font-semibold mb-4 text-center">Sign in to Snapflow</div>

      <div class="mb-4">
        <label class="fieldset-label" for="email-input">Email</label>
        <input
          type="email"
          class="input w-full"
          id="email-input"
          placeholder="Enter your email"
          bind:value={email}
          aria-describedby="email-errors"
        />
        {#if emailErrors.length > 0}
          <ul id="email-errors" class="text-red-500 text-sm mt-1">
            {#each emailErrors as error}
                <li>{error}</li>
            {/each}
          </ul>
        {/if}
      </div>

      <div class="mb-4">
        <label class="fieldset-label" for="password-input">Password</label>
        <div class="flex items-center gap-2">
          <input
            type={showPassword ? "text" : "password"}
            class="input w-full"
            id="password-input"
            placeholder="Enter your password"
            bind:value={password}
            aria-describedby="password-errors"
          />
          <button
            type="button"
            class="btn btn-sm"
            on:click={() => (showPassword = !showPassword)}
            aria-label={showPassword ? "Hide password" : "Show password"}
          >
            {showPassword ? "🙈" : "👁️"}
          </button>
        </div>
        {#if passwordErrors.length > 0}
          <ul id="password-errors" class="text-red-500 text-sm mt-1">
            {#each passwordErrors as error}
              <li>{error}</li>
            {/each}
          </ul>
        {/if}
      </div>

      <button type="submit" class="btn btn-primary w-full mt-4">Sign in</button>

      <div class="divider my-4">Don't have an account?</div>

      <button type="button" class="btn btn-outline w-full" on:click={() => goto("/signup")}>
        Sign up
      </button>
    </fieldset>
  </form>
</div>
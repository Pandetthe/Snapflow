<script lang="ts">
  import { goto } from '$app/navigation';
  import { env } from '$env/dynamic/public';

  let email = '';
  let emailErrors: string[] = [];
  let userName = '';
  let userNameErrors: string[] = [];
  let password = '';
  let passwordErrors: string[] = [];
  let repeatedPassword = '';
  let repeatedPasswordErrors: string[] = [];
  let showPassword = false;
  let showRepeatedPassword = false;

  type Response = {
    type: string;
    title: string;
    status: number;
    detail: string;
    errors: { propertyName: string; code: string; description: string }[];
  };

  async function handleSignUp() {
    if (password !== repeatedPassword) {
      repeatedPasswordErrors = ["Passwords do not match."];
      return;
    }
    emailErrors = [];
    userNameErrors = [];
    passwordErrors = [];
    repeatedPasswordErrors = [];

    try {
      const res = await fetch('/api/auth/register', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, userName, password }),
      });

      if (res.ok) {
          goto('/signin');
      } else {
        const body = await res.json() as Response;

        if (body.title === 'Validation.General') {
          emailErrors = body.errors
            .filter(e => e.propertyName === 'Email')
            .map(e => e.description);
          userNameErrors = body.errors
            .filter(e => e.propertyName === 'UserName')
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
  <title>Sign up</title>
</svelte:head>

<div class="flex bg-gray-700 h-full w-full justify-center items-center p-4">
  <form on:submit|preventDefault={handleSignUp} class="w-full max-w-md">
    <fieldset class="fieldset bg-gray-900 p-6 rounded-lg shadow-lg">
      <div class="text-2xl font-semibold mb-4 text-center">Sign up to Snapflow</div>

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
        <label class="fieldset-label" for="username-input">UserName</label>
        <input
          type="text"
          class="input w-full"
          id="username-input"
          placeholder="Enter your username"
          bind:value={userName}
          aria-describedby="username-errors"
        />
        {#if userNameErrors.length > 0}
          <ul id="username-errors" class="text-red-500 text-sm mt-1">
            {#each userNameErrors as error}
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

      <div class="mb-4">
        <label class="fieldset-label" for="repeated-password-input">Repeat Password</label>
        <div class="flex items-center gap-2">
          <input
            type={showRepeatedPassword ? "text" : "password"}
            class="input w-full"
            id="repeated-password-input"
            placeholder="Repeat your password"
            bind:value={repeatedPassword}
            aria-describedby="repeated-password-errors"
          />
          <button
            type="button"
            class="btn btn-sm"
            on:click={() => (showRepeatedPassword = !showRepeatedPassword)}
            aria-label={showRepeatedPassword ? "Hide repeated password" : "Show repeated password"}
          >
            {showRepeatedPassword ? "🙈" : "👁️"}
          </button>
        </div>
        {#if repeatedPasswordErrors.length > 0}
          <ul id="repeated-password-errors" class="text-red-500 text-sm mt-1">
            {#each repeatedPasswordErrors as error}
              <li>{error}</li>
            {/each}
          </ul>
        {/if}
      </div>

      <button type="submit" class="btn btn-primary w-full mt-4">Sign up</button>

      <div class="divider my-4">Already have an account?</div>

      <button type="button" class="btn btn-outline w-full" on:click={() => goto("/signin")}>
        Sign in
      </button>
    </fieldset>
  </form>
</div>
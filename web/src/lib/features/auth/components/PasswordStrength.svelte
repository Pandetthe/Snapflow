<script lang="ts">
  import { authConfig } from '$lib/config/auth';
  import { slide } from 'svelte/transition';

  let { password = '' } = $props<{ password?: string }>();

  let passwordRequirements = $derived({
    length: password.length >= (authConfig.password.minLength || 1),
    lowercase: /[a-z]/.test(password),
    uppercase: /[A-Z]/.test(password),
    digit: /\d/.test(password),
    nonAlphanumeric: /[^a-zA-Z0-9]/.test(password)
  });

  let passwordStrength = $derived.by(() => {
    const metRequirements = Object.values(passwordRequirements).filter(Boolean).length;
    return (metRequirements / 5) * 100;
  });

  let passwordStrengthText = $derived.by(() => {
    if (passwordStrength < 40) return 'Weak';
    if (passwordStrength < 80) return 'Medium';
    return 'Strong';
  });
</script>

{#if password}
  <div transition:slide={{ axis: 'y', duration: 200 }} class="mt-2 animate-in fade-in duration-200">
    <div class="mb-1.5">
      <div class="mb-1 flex items-center justify-between">
        <span class="text-[10px] uppercase tracking-wider text-gray-500 dark:text-gray-400 font-bold">Strength</span>
        <span
          class="text-xs font-bold {passwordStrength < 40
            ? 'text-red-500'
            : passwordStrength < 80
              ? 'text-yellow-500'
              : 'text-green-500'}"
        >
          {passwordStrengthText}
        </span>
      </div>
      <div class="h-1.5 w-full rounded-full bg-gray-100 dark:bg-gray-800 overflow-hidden">
        <div
          class="h-full rounded-full transition-all duration-500 {passwordStrength < 40
            ? 'bg-red-500 shadow-[0_0_8px_rgba(239,68,68,0.4)]'
            : passwordStrength < 80
              ? 'bg-yellow-500 shadow-[0_0_8px_rgba(245,158,11,0.4)]'
              : 'bg-green-500 shadow-[0_0_8px_rgba(34,197,94,0.4)]'}"
          style="width: {passwordStrength}%"
        ></div>
      </div>
    </div>

    <div class="rounded-xl border border-gray-100 bg-gray-50/50 p-3 dark:border-gray-800 dark:bg-white/5">
      <ul class="grid grid-cols-1 gap-x-3 gap-y-1.5 sm:grid-cols-2">
        {#each [
          { met: passwordRequirements.length, text: `Min. ${authConfig.password.minLength} chars` },
          { met: passwordRequirements.lowercase, text: 'Lowercase', hide: !authConfig.password.requireLowercase },
          { met: passwordRequirements.uppercase, text: 'Uppercase', hide: !authConfig.password.requireUppercase },
          { met: passwordRequirements.digit, text: 'Number', hide: !authConfig.password.requireDigit },
          { met: passwordRequirements.nonAlphanumeric, text: 'Special', hide: !authConfig.password.requireNonAlphanumeric }
        ].filter(r => !r.hide) as req}
          <li class="flex items-center gap-1.5 text-[11px]">
            <div class="flex h-3.5 w-3.5 items-center justify-center rounded-full {req.met ? 'bg-green-500 text-white' : 'bg-gray-200 dark:bg-gray-700 text-gray-400'}">
              <svg class="h-2 w-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M5 13l4 4L19 7" />
              </svg>
            </div>
            <span class={req.met ? 'text-gray-700 dark:text-gray-300 font-medium' : 'text-gray-400 dark:text-gray-500'}>
              {req.text}
            </span>
          </li>
        {/each}
      </ul>
    </div>
  </div>
{/if}

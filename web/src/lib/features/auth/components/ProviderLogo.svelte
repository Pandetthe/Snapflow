<script lang="ts">
  import { Building2 } from 'lucide-svelte';
  import githubBlack from '$lib/assets/github-black.svg';
  import githubWhite from '$lib/assets/github-white.svg';
  import type { ExternalProviderType } from '../api/auth';

  let { type }: { type: ExternalProviderType | 'ldap' } = $props();
</script>

{#if type === 'google'}
  <svg viewBox="0 0 24 24" class="size-4.5 shrink-0" aria-hidden="true">
    <path
      fill="#4285F4"
      d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
    />
    <path
      fill="#34A853"
      d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
    />
    <path
      fill="#FBBC05"
      d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
    />
    <path
      fill="#EA4335"
      d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
    />
  </svg>
{:else if type === 'microsoft'}
  <svg viewBox="0 0 24 24" class="size-4.5 shrink-0" aria-hidden="true">
    <rect x="1" y="1" width="10" height="10" fill="#F25022" />
    <rect x="13" y="1" width="10" height="10" fill="#7FBA00" />
    <rect x="1" y="13" width="10" height="10" fill="#00A4EF" />
    <rect x="13" y="13" width="10" height="10" fill="#FFB900" />
  </svg>
{:else if type === 'github'}
  <img src={githubBlack} alt="" class="size-4.5 shrink-0 dark:hidden" />
  <img src={githubWhite} alt="" class="hidden size-4.5 shrink-0 dark:block" />
{:else if type === 'apple'}
  <svg viewBox="0 0 24 24" class="size-4.5 shrink-0 fill-black dark:fill-white" aria-hidden="true">
    <path
      d="M17.05 12.74c-.02-2.02 1.65-2.99 1.72-3.04-.94-1.37-2.4-1.56-2.92-1.58-1.24-.13-2.42.73-3.05.73-.63 0-1.6-.71-2.63-.69-1.35.02-2.6.79-3.29 2-1.4 2.44-.36 6.05 1.01 8.03.67.97 1.47 2.06 2.51 2.02 1.01-.04 1.39-.65 2.61-.65 1.22 0 1.56.65 2.62.63 1.08-.02 1.77-.99 2.43-1.96.77-1.12 1.08-2.21 1.1-2.27-.02-.01-2.11-.81-2.13-3.22M15.05 6.8c.56-.68.94-1.62.83-2.56-.81.03-1.79.54-2.36 1.21-.51.6-.96 1.56-.84 2.48.9.07 1.82-.46 2.37-1.13"
    />
  </svg>
{:else if type === 'facebook'}
  <svg viewBox="0 0 24 24" class="size-4.5 shrink-0" aria-hidden="true">
    <path
      fill="#1877F2"
      d="M24 12.07C24 5.41 18.63 0 12 0S0 5.4 0 12.07C0 18.1 4.39 23.1 10.13 24v-8.44H7.08v-3.49h3.04V9.41c0-3.02 1.8-4.7 4.54-4.7 1.31 0 2.68.24 2.68.24v2.97h-1.5c-1.5 0-1.96.93-1.96 1.89v2.26h3.32l-.53 3.5h-2.8V24C19.62 23.1 24 18.1 24 12.07"
    />
  </svg>
{:else}
  <Building2 size={18} class="shrink-0 text-gray-500 dark:text-gray-400" />
{/if}

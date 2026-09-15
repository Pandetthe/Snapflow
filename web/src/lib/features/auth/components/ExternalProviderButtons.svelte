<script lang="ts">
  import { Building2 } from 'lucide-svelte';
  import { Button } from '$lib/ui/components';
  import { cn } from '$lib/ui/utils';
  import githubBlack from '$lib/assets/github-black.svg';
  import githubWhite from '$lib/assets/github-white.svg';
  import { externalSignInUrl, type ExternalProvider, type ExternalProviderType } from '../api/auth';

  let {
    providers,
    rememberMe = false,
    action = 'Continue',
    showDivider = false
  }: {
    providers: ExternalProvider[];
    rememberMe?: boolean;
    action?: string;
    showDivider?: boolean;
  } = $props();

  const isOrganization = (type: ExternalProviderType) => type === 'oidc' || type === 'saml';
  const organizations = $derived(providers.filter((p) => isOrganization(p.type)));
  const brands = $derived(providers.filter((p) => !isOrganization(p.type)));
</script>

{#snippet logo(type: ExternalProviderType)}
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
  {:else if type === 'facebook'}
    <svg viewBox="0 0 24 24" class="size-4.5 shrink-0" aria-hidden="true">
      <path
        fill="#1877F2"
        d="M24 12.07C24 5.41 18.63 0 12 0S0 5.4 0 12.07C0 18.1 4.39 23.1 10.13 24v-8.44H7.08v-3.49h3.04V9.41c0-3.02 1.8-4.7 4.54-4.7 1.31 0 2.68.24 2.68.24v2.97h-1.5c-1.5 0-1.96.93-1.96 1.89v2.26h3.32l-.53 3.5h-2.8V24C19.62 23.1 24 18.1 24 12.07"
      />
    </svg>
  {/if}
{/snippet}

<div class="space-y-3">
  {#each organizations as provider (provider.scheme)}
    <Button
      variant="outline"
      size="md"
      class="w-full justify-center text-sm"
      href={externalSignInUrl(provider.scheme, rememberMe)}
      data-sveltekit-reload
    >
      <span class="flex items-center gap-2.5">
        <Building2 size={18} class="shrink-0 text-gray-500 dark:text-gray-400" />
        {action} with {provider.displayName}
      </span>
    </Button>
  {/each}

  {#if brands.length > 0}
    <div class="grid grid-cols-1 gap-3 sm:grid-cols-2">
      {#each brands as provider, index (provider.scheme)}
        <Button
          variant="outline"
          size="md"
          class={cn(
            'w-full justify-center text-sm',
            brands.length % 2 === 1 && index === brands.length - 1 && 'sm:col-span-2'
          )}
          href={externalSignInUrl(provider.scheme, rememberMe)}
          data-sveltekit-reload
        >
          <span class="flex items-center gap-2.5">
            {@render logo(provider.type)}
            {action} with {provider.displayName}
          </span>
        </Button>
      {/each}
    </div>
  {/if}
</div>

{#if showDivider}
  <div class="my-6 flex items-center gap-4 text-xs text-gray-400 dark:text-gray-500">
    <span class="h-px flex-1 bg-gray-200 dark:bg-gray-800"></span>
    Or
    <span class="h-px flex-1 bg-gray-200 dark:bg-gray-800"></span>
  </div>
{/if}

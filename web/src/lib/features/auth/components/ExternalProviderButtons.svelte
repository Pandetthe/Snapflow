<script lang="ts">
  import { Button } from '$lib/ui/components';
  import { cn } from '$lib/ui/utils';
  import ProviderLogo from './ProviderLogo.svelte';
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
        <ProviderLogo type={provider.type} />
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
            <ProviderLogo type={provider.type} />
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

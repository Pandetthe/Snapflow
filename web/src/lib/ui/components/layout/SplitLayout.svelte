<script lang="ts">
  import type { Snippet } from 'svelte';

  let {
    header,
    children,
    footer,
    rightContent
  }: {
    header?: Snippet;
    children: Snippet;
    footer?: Snippet;
    rightContent?: Snippet;
  } = $props();
</script>

<div class="relative z-1 bg-white dark:bg-gray-900">
  <div class="relative flex h-screen w-full flex-col overflow-hidden lg:flex-row dark:bg-gray-900">
    <div class="flex w-full flex-1 flex-col lg:w-1/2">
      <div class="mx-auto w-full max-w-md px-6 pt-5 sm:px-0 lg:pt-10">
        {#if header}
          {@render header()}
        {/if}
      </div>

      <div
        class="mx-auto flex w-full max-w-md flex-1 flex-col justify-center px-6 py-4 sm:px-0 lg:py-12"
      >
        {@render children()}

        {#if footer}
          <div class="mt-5">
            {@render footer()}
          </div>
        {/if}
      </div>
    </div>

    <div
      class="relative hidden min-h-screen w-full items-center justify-center overflow-hidden bg-brand-950 lg:flex lg:w-1/2 dark:bg-white/5"
    >
      {#if rightContent}
        {@render rightContent()}
      {:else}
        <div class="relative z-10 flex flex-col items-center justify-center p-8 text-center">
          <h2 class="text-7xl font-bold tracking-tight text-white">Snapflow</h2>
          <p class="mt-4 text-sm text-white/60">
            Free and Open-Source<br />Kanban-based Application
          </p>
        </div>

        <div 
          class="pointer-events-none absolute inset-0 opacity-20"
          style="mask-image: radial-gradient(ellipse at top right, black 10%, transparent 50%), radial-gradient(ellipse at bottom left, black 10%, transparent 50%); -webkit-mask-image: radial-gradient(ellipse at top right, black 10%, transparent 50%), radial-gradient(ellipse at bottom left, black 10%, transparent 50%);"
        >
          <svg class="h-full w-full" xmlns="http://www.w3.org/2000/svg">
            <defs>
              <pattern id="grid" width="64" height="64" x="50%" y="50%" patternUnits="userSpaceOnUse">
                <path d="M 64 0 L 0 0 0 64" fill="none" stroke="white" stroke-width="1" />
              </pattern>
            </defs>
            <rect width="100%" height="100%" fill="url(#grid)" />
          </svg>
        </div>
      {/if}
    </div>
  </div>
</div>

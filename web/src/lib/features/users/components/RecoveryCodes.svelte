<script lang="ts">
  import { Check, Copy } from 'lucide-svelte';
  import { Button } from '$lib/ui/components';

  let { codes }: { codes: string[] } = $props();

  let copied = $state(false);

  async function copy() {
    try {
      await navigator.clipboard.writeText(codes.join('\n'));
      copied = true;
      setTimeout(() => (copied = false), 2000);
    } catch {
      copied = false;
    }
  }
</script>

<div class="space-y-3">
  <ul
    class="grid grid-cols-2 gap-x-6 gap-y-2 rounded-lg border border-gray-200 bg-gray-50 px-5 py-4 font-mono text-sm text-gray-900 tabular-nums select-all dark:border-gray-800 dark:bg-gray-900 dark:text-white"
  >
    {#each codes as code (code)}
      <li>{code}</li>
    {/each}
  </ul>
  <Button
    variant="outline"
    size="sm"
    class="w-full justify-center"
    onclick={copy}
    startIcon={copied ? Check : Copy}
  >
    {copied ? 'Copied' : 'Copy codes'}
  </Button>
</div>

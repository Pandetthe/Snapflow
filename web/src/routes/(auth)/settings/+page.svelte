<script lang="ts">
  import { FullLayout, GoBackButton, SegmentedControl } from '$lib/ui/components';
  import SettingsSection from '$lib/features/users/components/SettingsSection.svelte';
  import { dragHandles, type DragHandleVisibility } from '$lib/features/boards/stores/dragHandles';
  import { triggerHaptic } from '$lib/ui/utils';
  import { Grip, GripVertical, LayoutGrid, MousePointer2, EyeOff } from 'lucide-svelte';
  import { afterNavigate } from '$app/navigation';

  let backHref = $state('/boards');
  afterNavigate(({ from }) => {
    backHref = from?.url.pathname ?? '/boards';
  });

  const descriptions: Record<DragHandleVisibility, string> = {
    always: 'The grip is always visible on cards, lists and swimlanes.',
    hover: 'The grip appears when you hover an item or focus its handle.',
    hidden: 'No grip at all — drag a card, list or swimlane by the item itself.'
  };

  function select(next: DragHandleVisibility) {
    dragHandles.set(next);
    triggerHaptic('selection');
  }
</script>

<svelte:head>
  <title>Snapflow | Settings</title>
</svelte:head>

<FullLayout>
  <div class="mx-auto w-full max-w-5xl space-y-6 pb-12 sm:space-y-8">
    <header class="flex flex-col gap-4">
      <GoBackButton href={backHref} />
      <div class="space-y-1">
        <h1 class="text-2xl font-bold tracking-tight text-gray-900 sm:text-4xl dark:text-white">
          Settings
        </h1>
        <p class="text-sm text-gray-600 sm:text-base dark:text-gray-400">
          Choose how Snapflow looks and behaves for you.
        </p>
      </div>
    </header>

    <div class="space-y-6">
      <SettingsSection
        icon={LayoutGrid}
        title="Boards"
        description="How boards behave while you work in them."
      >
        <div class="space-y-3">
          <div class="flex items-center gap-3">
            <div
              class="flex size-9 shrink-0 items-center justify-center rounded-lg border border-gray-200 text-gray-400 dark:border-gray-800 dark:text-gray-500"
            >
              <GripVertical size={16} />
            </div>
            <div class="min-w-0 flex-1">
              <p class="text-sm font-medium text-gray-900 dark:text-white">Drag handles</p>
              <p class="text-xs text-gray-400 dark:text-gray-500">
                {descriptions[$dragHandles]}
              </p>
            </div>
          </div>

          <SegmentedControl
            size="xs"
            options={[
              { value: 'always', label: 'Always', icon: Grip },
              { value: 'hover', label: 'On hover', icon: MousePointer2 },
              { value: 'hidden', label: 'Hidden', icon: EyeOff }
            ]}
            value={$dragHandles}
            onValueChange={select}
          />

          <p class="text-xs text-gray-400 dark:text-gray-500">Saved in this browser only.</p>
        </div>
      </SettingsSection>
    </div>
  </div>
</FullLayout>

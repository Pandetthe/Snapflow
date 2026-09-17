<script lang="ts">
  import {
    FullLayout,
    GoBackButton,
    SegmentedControl,
    SettingsSection,
    Switch
  } from '$lib/ui/components';
  import { dragHandles, type DragHandleVisibility } from '$lib/features/boards/stores/dragHandles';
  import { swimlaneFolding } from '$lib/features/boards/stores/swimlaneFolding';
  import { theme } from '$lib/ui/stores/theme';
  import { triggerHaptic } from '$lib/ui/utils';
  import {
    FoldVertical,
    Grip,
    GripVertical,
    LayoutGrid,
    MousePointer2,
    EyeOff,
    Monitor,
    Moon,
    Palette,
    Sun
  } from 'lucide-svelte';
  import { afterNavigate } from '$app/navigation';
  import type { Icon as IconType } from 'lucide-svelte';

  let backHref = $state('/');
  afterNavigate(({ from }) => {
    backHref = from?.url.pathname ?? '/';
  });

  const handleDescriptions: Record<DragHandleVisibility, string> = {
    always: 'The grip is always visible on cards, lists and swimlanes.',
    hover: 'The grip appears when you hover an item or focus its handle.',
    hidden: 'No grip at all — drag a card, list or swimlane by the item itself.'
  };

  type ThemeMode = 'light' | 'dark' | 'system';

  const themeDescriptions: Record<ThemeMode, string> = {
    light: 'Snapflow always uses the light theme.',
    dark: 'Snapflow always uses the dark theme.',
    system: 'Snapflow follows the light or dark setting of your device.'
  };

  const themeIcons: Record<ThemeMode, typeof IconType> = {
    light: Sun,
    dark: Moon,
    system: Monitor
  };

  function selectTheme(next: ThemeMode) {
    theme.set(next);
    triggerHaptic('selection');
  }

  function selectHandles(next: DragHandleVisibility) {
    dragHandles.set(next);
    triggerHaptic('selection');
  }

  function toggleFolding(next: boolean) {
    swimlaneFolding.set(next);
    triggerHaptic('selection');
  }
</script>

{#snippet settingIcon(Icon: typeof IconType)}
  <div
    class="flex size-9 shrink-0 items-center justify-center rounded-lg border border-gray-200 text-gray-400 dark:border-gray-800 dark:text-gray-500"
  >
    <Icon size={16} />
  </div>
{/snippet}

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
        icon={Palette}
        title="Appearance"
        description="How Snapflow looks on this device."
      >
        <div class="space-y-5">
          <div class="space-y-3">
            <div class="flex items-center gap-3">
              {@render settingIcon(themeIcons[$theme])}
              <div class="min-w-0 flex-1">
                <p class="text-sm font-medium text-gray-700 dark:text-gray-400">Theme</p>
                <p class="text-xs text-gray-500 dark:text-gray-400">
                  {themeDescriptions[$theme]}
                </p>
              </div>
            </div>

            <SegmentedControl
              size="xs"
              options={[
                { value: 'light', label: 'Light', icon: Sun },
                { value: 'dark', label: 'Dark', icon: Moon },
                { value: 'system', label: 'System', icon: Monitor }
              ]}
              value={$theme}
              onValueChange={selectTheme}
            />
          </div>

          <p class="text-xs text-gray-400 dark:text-gray-500">Saved in this browser only.</p>
        </div>
      </SettingsSection>

      <SettingsSection
        icon={LayoutGrid}
        title="Boards"
        description="How boards behave while you work in them."
      >
        <div class="space-y-5">
          <div class="space-y-3">
            <div class="flex items-center gap-3">
              {@render settingIcon(GripVertical)}
              <div class="min-w-0 flex-1">
                <p class="text-sm font-medium text-gray-700 dark:text-gray-400">Drag handles</p>
                <p class="text-xs text-gray-500 dark:text-gray-400">
                  {handleDescriptions[$dragHandles]}
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
              onValueChange={selectHandles}
            />
          </div>

          <div class="h-px bg-gray-100 dark:bg-gray-800"></div>

          <div class="flex items-start gap-3">
            {@render settingIcon(FoldVertical)}
            <div class="min-w-0 flex-1">
              <Switch
                label="Fold swimlanes while dragging"
                helperText="Every swimlane collapses to its header, so the board fits on a few screens while you carry one."
                checked={$swimlaneFolding}
                onCheckedChange={toggleFolding}
                class="w-full flex-row-reverse justify-between"
              />
            </div>
          </div>

          <p class="text-xs text-gray-400 dark:text-gray-500">Saved in this browser only.</p>
        </div>
      </SettingsSection>
    </div>
  </div>
</FullLayout>

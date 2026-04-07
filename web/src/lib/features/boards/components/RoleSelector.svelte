<script lang="ts">
  import { Select } from 'bits-ui';
  import { Check, ChevronDown, ArrowRightLeft } from 'lucide-svelte';
  import { cn } from '$lib/ui/utils';
  import type { MemberRole } from '$lib/features/boards/types/boards.api';

  interface Props {
    role: MemberRole | string;
    onRoleChange?: (role: MemberRole) => void;
    disabled?: boolean;
    showArrow?: boolean;
    allowTransferOwnership?: boolean;
    onTransferOwnership?: () => void;
    class?: string;
  }

  let {
    role,
    onRoleChange,
    disabled = false,
    showArrow = true,
    allowTransferOwnership = false,
    onTransferOwnership,
    class: className
  }: Props = $props();

  const variantClassMap: Record<string, string> = {
    owner: 'bg-brand-50 text-brand-700 dark:bg-brand-500/10 dark:text-brand-300',
    admin: 'bg-blue-50 text-blue-700 dark:bg-blue-500/10 dark:text-blue-300',
    member: 'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300',
    viewer: 'bg-amber-50 text-amber-700 dark:bg-amber-500/10 dark:text-amber-300'
  };

  const labelMap: Record<string, string> = {
    owner: 'Owner',
    admin: 'Admin',
    member: 'Member',
    viewer: 'Viewer'
  };

  import { untrack } from 'svelte';

  const options = $derived([
    ...(allowTransferOwnership ? [{ value: 'transfer_owner', label: 'Give ownership' }] : []),
    { value: 'admin', label: 'Admin' },
    { value: 'member', label: 'Member' },
    { value: 'viewer', label: 'Viewer' }
  ]);

  let open = $state(false);
  let internalValue = $state(untrack(() => role));

  $effect(() => {
    internalValue = role;
  });

  function handleValueChange(nextValue: string) {
    if (nextValue === 'transfer_owner') {
      onTransferOwnership?.();
      open = false;
      setTimeout(() => {
        internalValue = role;
      }, 0);
      return;
    }
    onRoleChange?.(nextValue as MemberRole);
    open = false;
  }
</script>

<Select.Root
  type="single"
  bind:value={internalValue}
  onValueChange={handleValueChange}
  bind:open
  {disabled}
>
  <Select.Trigger
    class={cn(
      'group inline-flex items-center gap-1.5 px-2.5 py-1 text-xs font-semibold whitespace-nowrap rounded-full transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-500 w-[88px]',
      showArrow ? 'justify-between' : 'justify-center',
      variantClassMap[role] || variantClassMap.member,
      disabled && 'opacity-50 cursor-not-allowed',
      !disabled && 'hover:opacity-80 active:scale-95 cursor-pointer',
      className
    )}
  >
    <span>{labelMap[role] || 'Unknown'}</span>
    {#if showArrow}
      <ChevronDown size={12} class="shrink-0 transition-transform duration-200 group-data-[state=open]:rotate-180 opacity-70" />
    {/if}
  </Select.Trigger>

  <Select.Portal>
    <Select.Content
      class="z-50 min-w-[130px] origin-top overflow-hidden rounded-xl border border-gray-200 bg-white p-1 shadow-lg dark:border-gray-800 dark:bg-gray-950"
      sideOffset={4}
    >
      <Select.Viewport class="p-1">
        {#each options as option}
          <Select.Item
            value={option.value}
            class={cn(
              "flex items-center justify-between gap-3 rounded-lg px-3 py-2 text-sm outline-none transition-colors cursor-pointer",
              option.value === 'transfer_owner' 
                ? "text-amber-700 hover:bg-amber-50 data-highlighted:bg-amber-50 dark:text-amber-400 dark:hover:bg-amber-500/10 dark:data-highlighted:bg-amber-500/10 font-medium mb-1 border-b border-gray-100 dark:border-gray-800 pb-2"
                : "text-gray-700 hover:bg-gray-100 data-highlighted:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-800 dark:data-highlighted:bg-gray-800"
            )}
          >
            {option.label}
            {#if option.value === 'transfer_owner'}
              <ArrowRightLeft size={14} class="text-amber-500 opacity-70" />
            {:else if role === option.value}
              <Check size={14} class="text-brand-500" />
            {/if}
          </Select.Item>
        {/each}
      </Select.Viewport>
    </Select.Content>
  </Select.Portal>
</Select.Root>

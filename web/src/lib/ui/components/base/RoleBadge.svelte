<script lang="ts">
  import { cn } from '$lib/ui/utils';

  type Role = 'owner' | 'admin' | 'member' | 'viewer';

  let {
    role,
    class: className
  }: {
    role: Role;
    class?: string;
  } = $props();

  const labelMap: Record<Role, string> = {
    owner: 'Owner',
    admin: 'Admin',
    member: 'Member',
    viewer: 'Viewer'
  };

  const variantClassMap: Record<Role, string> = {
    owner: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-500/10 dark:text-emerald-300',
    admin: 'bg-blue-100 text-blue-700 dark:bg-blue-500/10 dark:text-blue-300',
    member: 'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300',
    viewer: 'bg-amber-100 text-amber-700 dark:bg-amber-500/10 dark:text-amber-300'
  };
</script>

<span
  class={cn(
    'inline-flex items-center rounded-full px-2.5 py-1 text-xs font-semibold whitespace-nowrap',
    variantClassMap[role],
    className
  )}
>
  {labelMap[role]}
</span>
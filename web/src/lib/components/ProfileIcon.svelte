<script lang="ts">
	import type { User } from '$lib/types/api/users';
    import { idToColor } from '$lib/utils';
    export let user: User;
    export let size: 'small' | 'medium' | 'large' = 'medium';
    export let noTooltip = true;
    let dim, font, border;
    if (size === 'small') {
        dim = 'w-8 h-8';
        font = 'text-xs';
        border = 'border-1';
    } else if (size === 'medium') {
        dim = 'w-10 h-10';
        font = 'text-sm';
        border = 'border-2';
    } else {
        dim = 'w-12 h-12';
        font = 'text-xl';
        border = 'border-3';
    }
</script>
{#if noTooltip}
    <div
        class="{dim} rounded-full flex items-center justify-center {font} font-bold {border} border-white text-white"
        style="background-color: {idToColor(user.userName)}; display:flex;"
    >
        {user.userName.slice(0, 2)}
    </div>
{:else}
<div class="tooltip" data-tip={user.userName}>
    <div
        class="{dim} rounded-full flex items-center justify-center {font} font-bold {border} border-white text-white"
        style="background-color: {idToColor(user.userName)}"
    >
    {user.userName.slice(0,2)}
    </div>
</div>
{/if}
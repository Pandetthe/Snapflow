<script lang="ts">
  import UserAvatar from '../UserAvatar.svelte';

  // Two keyed lists, like two board lists: moving the item renders a new avatar for an image that has already loaded.
  let { src }: { src: string } = $props();

  let left = $state([{ id: 1 }]);
  let right = $state<{ id: number }[]>([]);

  function move() {
    right = [...left];
    left = [];
  }
</script>

<button type="button" onclick={move}>Move</button>

<div data-testid="left">
  {#each left as item (item.id)}
    <UserAvatar {src} name="Ann Lee" />
  {/each}
</div>

<div data-testid="right">
  {#each right as item (item.id)}
    <UserAvatar {src} name="Ann Lee" />
  {/each}
</div>

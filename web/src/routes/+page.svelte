<script lang="ts">
  import { FullLayout } from '$lib/ui/components';
  import BoardsOverview from '$lib/features/boards/components/BoardsOverview.svelte';
  import PublicBoards from '$lib/features/boards/components/PublicBoards.svelte';
  import LastRefreshed from '$lib/features/boards/components/LastRefreshed.svelte';
  import HomeIntro from '$lib/features/home/components/HomeIntro.svelte';

  let { data } = $props();
</script>

<svelte:head>
  <title>{data.user ? 'Snapflow | Boards' : 'Snapflow | Simple kanban for teams'}</title>
</svelte:head>

<FullLayout>
  {#if data.user && data.boards}
    <BoardsOverview
      boards={data.boards}
      publicBoards={data.publicBoards}
      refreshTime={data.refreshTime}
      user={data.user}
    />
  {:else}
    <HomeIntro />
    <PublicBoards boards={data.publicBoards} />
    <LastRefreshed refreshTime={data.refreshTime} />
  {/if}
</FullLayout>

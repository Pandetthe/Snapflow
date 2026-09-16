<script lang="ts">
  import { Button } from '$lib/ui/components';
  import { Bug, Github } from 'lucide-svelte';

  const version = __APP_VERSION__;
  const repositoryUrl = 'https://github.com/pandetthe/Snapflow';
  const issuesUrl = 'https://github.com/pandetthe/Snapflow/issues';
</script>

<section
  class="relative mb-8 overflow-hidden rounded-2xl bg-brand-950 shadow-sm sm:rounded-3xl lg:mb-10 dark:bg-white/5"
>
  <div
    class="pointer-events-none absolute inset-0 opacity-20"
    style="mask-image: radial-gradient(ellipse at top right, black 10%, transparent 50%), radial-gradient(ellipse at bottom left, black 10%, transparent 50%); -webkit-mask-image: radial-gradient(ellipse at top right, black 10%, transparent 50%), radial-gradient(ellipse at bottom left, black 10%, transparent 50%);"
    aria-hidden="true"
  >
    <svg class="h-full w-full" xmlns="http://www.w3.org/2000/svg">
      <defs>
        <pattern
          id="home-intro-grid"
          width="64"
          height="64"
          x="50%"
          y="50%"
          patternUnits="userSpaceOnUse"
        >
          <path d="M 64 0 L 0 0 0 64" fill="none" stroke="white" stroke-width="1" />
        </pattern>
      </defs>
      <rect width="100%" height="100%" fill="url(#home-intro-grid)" />
    </svg>
  </div>

  <div class="relative z-10 flex flex-col items-center px-5 py-12 text-center sm:px-10 sm:py-20">
    <h2
      class="mb-6 inline-flex items-center gap-2 rounded-full bg-warning-400/15 px-3.5 py-1.5 text-xs font-semibold text-warning-300 ring-1 ring-warning-400/40"
    >
      <span class="relative flex size-2">
        <span
          class="absolute inline-flex h-full w-full animate-ping rounded-full bg-warning-400 opacity-75 motion-reduce:animate-none"
        ></span>
        <span class="relative inline-flex size-2 rounded-full bg-warning-400"></span>
      </span>
      Under reconstruction
    </h2>

    <h1 class="max-w-3xl text-4xl leading-tight font-bold tracking-tight text-white sm:text-6xl">
      Plan work together and see every change live
    </h1>
    <p class="mt-5 max-w-2xl text-base text-white/60 sm:text-lg">
      Snapflow is a free and open-source kanban board for teams. Organize work in swimlanes, lists
      and cards, and watch your teammates move things around in real time.
    </p>
    <p class="mt-3 max-w-2xl text-sm text-warning-300/80">
      It is being rebuilt with new features and improvements, so some parts may change or not work
      yet.
    </p>

    <div class="mt-8 flex flex-col gap-3 sm:flex-row">
      <Button
        href={repositoryUrl}
        target="_blank"
        rel="noopener noreferrer"
        variant="primary"
        size="md"
        startIcon={Github}
        haptic="light"
      >
        Contribute on GitHub
      </Button>
      <Button
        href={issuesUrl}
        target="_blank"
        rel="noopener noreferrer"
        variant="outline"
        size="md"
        startIcon={Bug}
        haptic="light"
      >
        Report an issue
      </Button>
    </div>

    <p class="mt-8 font-mono text-xs text-white/40">v{version}</p>
  </div>
</section>

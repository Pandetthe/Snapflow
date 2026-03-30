<script lang="ts">
	import { Button } from 'bits-ui';
	import githubBlack from '$lib/assets/github-black.svg';
	import githubWhite from '$lib/assets/github-white.svg';

	interface Props {
		href?: string;
		class?: string;
	}

	let {
		href = 'https://github.com/pandetthe/Snapflow',
		class: className = ''
	}: Props = $props();
</script>

<Button.Root
	{href}
	target="_blank"
	rel="noopener noreferrer"
	class="inline-flex h-11 w-11 cursor-pointer items-center justify-center rounded-full border border-gray-200 bg-white text-gray-700 transition-all hover:bg-gray-50 active:scale-95 dark:border-gray-800 dark:bg-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 {className}"
	aria-label="GitHub Repository"
>
	<img src={githubBlack} alt="GitHub" class="h-6 w-6 dark:hidden" />
	<img src={githubWhite} alt="GitHub" class="hidden h-6 w-6 dark:block" />
</Button.Root>

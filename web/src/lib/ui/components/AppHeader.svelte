<script lang="ts">
	import { ExternalLink, Menu, X } from 'lucide-svelte';
	import ThemeToggle from '$lib/ui/components/ThemeToggle.svelte';
	import UserMenu from '$lib/ui/components/UserMenu.svelte';
	import GithubButton from '$lib/ui/components/GithubButton.svelte';
	import { Button as BitsButton } from 'bits-ui';
	import type { User } from '$lib/features/users/api/users';

	interface Props {
		onMenuToggle?: () => void;
		isSidebarOpen?: boolean;
		user: User | null;
	}

	let { onMenuToggle, isSidebarOpen = false, user = null }: Props = $props();
	let mobileMenuOpen = $state(false);

	function isMobileMenuOpen() {
		return onMenuToggle ? isSidebarOpen : mobileMenuOpen;
	}

	function toggleMobileMenu() {
		if (onMenuToggle) {
			onMenuToggle();
			return;
		}

		mobileMenuOpen = !mobileMenuOpen;
	}

	function closeMobileMenu() {
		if (onMenuToggle) {
			if (isSidebarOpen) {
				onMenuToggle();
			}
			return;
		}

		mobileMenuOpen = false;
	}
</script>

<header
	class="sticky top-0 z-40 flex w-full flex-col border-b border-gray-200 bg-white dark:border-gray-800 dark:bg-gray-900"
>
	<div class="flex w-full items-center justify-between gap-2 px-3 py-3 sm:gap-3 lg:px-6 lg:py-4">
		<a href="/" class="flex shrink-0 items-center gap-2">
			<span class="text-xl font-bold text-gray-900 dark:text-white">Snapflow</span>
		</a>

		<div class="hidden min-w-0 items-center justify-end gap-2 md:flex">
			<div class="flex shrink-0 items-center gap-2">
				<GithubButton />
				<ThemeToggle />
			</div>

			<div class="flex shrink-0 items-center border-l border-gray-200 pl-2 dark:border-gray-800 sm:pl-4">
				<UserMenu {user} />
			</div>
		</div>

		<BitsButton.Root
			onclick={toggleMobileMenu}
			class="inline-flex h-11 w-11 cursor-pointer items-center justify-center rounded-full border border-gray-200 bg-white text-gray-700 transition-all hover:bg-gray-50 active:scale-95 dark:border-gray-800 dark:bg-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 md:hidden"
			aria-label={isMobileMenuOpen() ? 'Close mobile menu' : 'Open mobile menu'}
			aria-expanded={isMobileMenuOpen()}
			aria-controls="mobile-header-menu"
		>
			{#if isMobileMenuOpen()}
				<X size={20} />
			{:else}
				<Menu size={20} />
			{/if}
		</BitsButton.Root>
	</div>

	{#if isMobileMenuOpen()}
		<div id="mobile-header-menu" class="border-t border-gray-200 px-3 pb-3 pt-3 dark:border-gray-800 md:hidden">
			<div
				class="flex items-center justify-between rounded-xl border border-gray-200 bg-gray-50 px-3 py-2 dark:border-gray-800 dark:bg-gray-800/40"
			>
				<span class="text-sm font-medium text-gray-700 dark:text-gray-200">Theme</span>
				<ThemeToggle />
			</div>

			<a
				href="https://github.com/pandetthe/Snapflow"
				target="_blank"
				rel="noopener noreferrer"
				onclick={closeMobileMenu}
				class="mt-2 flex items-center justify-between rounded-xl border border-gray-200 bg-gray-50 px-3 py-3 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-100 dark:border-gray-800 dark:bg-gray-800/40 dark:text-gray-300 dark:hover:bg-gray-800"
			>
				GitHub repository
				<ExternalLink size={16} class="text-gray-500 dark:text-gray-400" />
			</a>

			<div class="mt-3 rounded-xl border border-gray-200 bg-white p-3 dark:border-gray-800 dark:bg-gray-900">
				<UserMenu {user} mobile onAction={closeMobileMenu} />
			</div>
		</div>
	{/if}
</header>

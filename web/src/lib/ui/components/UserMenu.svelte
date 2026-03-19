<script lang="ts">
	import { ChevronDown, User, Settings, Info, LogOut } from 'lucide-svelte';
	import { DropdownMenu } from 'bits-ui';
	import { AuthService } from '$lib/features/auth/api/auth';
	import { apiClient } from '$lib/core/api.client';
	import { errorStore } from '$lib/ui/stores/error';
	import { Button } from '$lib/ui/components';

	interface Props {
		user: {
			userName: string;
			email: string;
			avatarUrl?: string;
		} | null;
		mobile?: boolean;
		onAction?: () => void;
	}

	let { user, mobile = false, onAction }: Props = $props();

	const authService = new AuthService(apiClient);

	async function handleSignOut() {
		try {
			const response = await authService.signOut();
			if (response.ok) {
				onAction?.();
				window.location.href = '/';
			} else {
				errorStore.addError(null, 'Problem with connection to the server');
			}
		} catch (err) {
			if (err instanceof Error) {
				if (err.message === 'Failed to fetch') {
					errorStore.addError('Web.ConnectionProblem', 'Problem with connection to the server');
				} else {
					errorStore.addError(err.name, err.message);
				}
			} else {
				errorStore.addError(null, 'Unknown error occurred during sign out');
			}
		}
	}

	const menuItems = [
		{ href: '/profile', icon: User, text: 'Edit profile' },
		{ href: '/settings', icon: Settings, text: 'Account settings' },
		{ href: '/support', icon: Info, text: 'Support' }
	];
</script>

<div class="relative">
	{#if mobile}
		{#if user}
			<div
				class="flex items-center gap-3 rounded-lg border border-gray-200 bg-gray-50 px-3 py-2 dark:border-gray-800 dark:bg-gray-800/40"
			>
				<div class="h-10 w-10 overflow-hidden rounded-full ring-2 ring-gray-100 dark:ring-gray-800">
					<img
						src={user.avatarUrl ||
							`https://ui-avatars.com/api/?name=${encodeURIComponent(user.userName || user.email || 'User')}&background=465fff&color=fff`}
						alt="User"
						class="h-full w-full object-cover"
					/>
				</div>

				<div class="min-w-0">
					<p class="truncate text-sm font-medium text-gray-900 dark:text-white">{user.userName}</p>
					<p class="truncate text-xs text-gray-500 dark:text-gray-400">{user.email}</p>
				</div>
			</div>

			<ul class="mt-3 flex flex-col gap-1 border-b border-gray-200 pb-3 dark:border-gray-800">
				{#each menuItems as item}
					<li>
						<a
							href={item.href}
							onclick={() => onAction?.()}
							class="group flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-white/5"
						>
							<item.icon
								size={18}
								class="text-gray-500 transition-colors group-hover:text-gray-700 dark:group-hover:text-gray-300"
							/>
							{item.text}
						</a>
					</li>
				{/each}
			</ul>

			<button
				type="button"
				onclick={handleSignOut}
				class="mt-3 flex w-full cursor-pointer items-center gap-3 rounded-lg px-3 py-2 text-left text-sm font-medium text-gray-700 transition-colors hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-white/5"
			>
				<LogOut size={18} class="text-gray-500" />
				Sign out
			</button>
		{:else}
			<div class="flex flex-col gap-2">
				<Button
					variant="outline"
					href="/sign-in"
					class="w-full justify-center"
					onclick={() => onAction?.()}
				>
					Sign in
				</Button>
				<Button
					variant="outline"
					href="/sign-up"
					class="w-full justify-center"
					onclick={() => onAction?.()}
				>
					Sign up
				</Button>
			</div>
		{/if}
	{:else if user}
		<DropdownMenu.Root>
			<DropdownMenu.Trigger
				class="group flex cursor-pointer items-center gap-3 rounded-lg p-1 transition-colors hover:bg-gray-100 dark:hover:bg-gray-800"
			>
				<div
					class="h-10 w-10 overflow-hidden rounded-full ring-2 ring-gray-100 dark:ring-gray-800"
				>
					<img
						src={user.avatarUrl ||
							`https://ui-avatars.com/api/?name=${encodeURIComponent(user.userName || user.email || 'User')}&background=465fff&color=fff`}
						alt="User"
						class="h-full w-full object-cover"
					/>
				</div>

				<div class="hidden text-left sm:block">
					<p class="flex items-center gap-1 text-sm font-medium text-gray-900 dark:text-white">
						{user.userName}
						<ChevronDown
							size={18}
							class="text-gray-400 transition-transform group-data-[state=open]:rotate-180"
						/>
					</p>
				</div>
			</DropdownMenu.Trigger>

			<DropdownMenu.Content
				class="z-50 mt-2 w-max min-w-60 rounded-2xl border border-gray-200 bg-white p-3 shadow-xl dark:border-gray-800 dark:bg-gray-900"
				align="end"
				sideOffset={8}
			>
				<div class="px-3 py-2">
					<span class="block text-sm font-medium text-gray-700 dark:text-gray-200">
						{user.userName}
					</span>
					<span class="mt-0.5 block text-xs text-gray-500 dark:text-gray-400">
						{user.email}
					</span>
				</div>

				<ul class="mt-3 flex flex-col gap-1 border-b border-gray-200 pb-3 dark:border-gray-800">
					{#each menuItems as item}
						<li>
							<DropdownMenu.Item
								class="group flex cursor-pointer items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-white/5 dark:hover:text-gray-300"
							>
								{#snippet child({ props })}
									<a href={item.href} {...props} class="flex w-full items-center gap-3 whitespace-nowrap">
										<item.icon
											size={20}
											class="text-gray-500 transition-colors group-hover:text-gray-700 dark:group-hover:text-gray-300"
										/>
										{item.text}
									</a>
								{/snippet}
							</DropdownMenu.Item>
						</li>
					{/each}
				</ul>

				<div class="mt-3">
					<DropdownMenu.Item
						onclick={handleSignOut}
						class="group flex cursor-pointer items-center gap-3 rounded-lg px-3 py-2 mt-3 font-medium text-gray-700 text-theme-sm transition-colors hover:bg-gray-100 hover:text-gray-700 dark:text-gray-400 dark:hover:bg-white/5 dark:hover:text-gray-300 whitespace-nowrap"
					>
						<LogOut
							size={20}
							class="text-gray-500 group-hover:text-gray-700 dark:group-hover:text-gray-300"
						/>
						Sign out
					</DropdownMenu.Item>
				</div>
			</DropdownMenu.Content>
		</DropdownMenu.Root>
	{:else}
		<div class="flex items-center gap-3">
			<Button variant="outline" href="/sign-in">Sign in</Button>
			<Button variant="outline" href="/sign-up">Sign up</Button>
		</div>
	{/if}
</div>

<style>
	:global(.text-theme-sm) {
		font-size: 0.875rem;
		line-height: 1.25rem;
	}
</style>

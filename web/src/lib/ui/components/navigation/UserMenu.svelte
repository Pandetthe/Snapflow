<script lang="ts">
	import { ChevronDown, User as UserIcon, LogOut } from 'lucide-svelte';
	import { DropdownMenu } from 'bits-ui';
	import { Button } from '$lib/ui/components';
	import type { User } from '$lib/features/users/api/users';

	interface Props {
		user: User | null;
		mobile?: boolean;
		handleSignOut?: () => void;
		onAction?: () => void;
	}

	let { user, mobile = false, handleSignOut, onAction }: Props = $props();

	const menuItems = [
		{ href: '/profile', icon: UserIcon, text: 'Edit profile' }
	];
</script>

	<div class="relative">
	{#if mobile}
		{#if user}
			<div
				class="mb-4 flex items-center gap-3 rounded-lg border border-gray-300 bg-gray-50/80 px-3 py-2.5 shadow-sm dark:border-gray-600 dark:bg-gray-800/50"
			>
				<div class="h-10 w-10 overflow-hidden rounded-full ring-2 ring-white shadow-sm dark:ring-gray-700">
					<img
						src={user.avatarUrl ||
							`https://ui-avatars.com/api/?name=${encodeURIComponent(user.userName || user.email || 'User')}&background=465fff&color=fff`}
						alt="User"
						class="h-full w-full object-cover"
					/>
				</div>

				<div class="min-w-0">
					<p class="truncate text-sm font-bold text-gray-900 dark:text-white">{user.userName}</p>
					<p class="truncate text-xs font-medium text-gray-500 dark:text-gray-400">{user.email}</p>
				</div>
			</div>

			<ul class="flex flex-col gap-2 border-b border-gray-200 pb-2 dark:border-gray-800">
				{#each menuItems as item}
					<li>
						<Button
							variant="outline"
							href={item.href}
							onclick={() => onAction?.()}
							class="w-full justify-start font-medium"
							startIcon={item.icon}
						>
							{item.text}
						</Button>
					</li>
				{/each}
			</ul>
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
			<DropdownMenu.Trigger>
				{#snippet child({ props: triggerProps })}
					<button
						{...triggerProps}
						class="group flex cursor-pointer items-center gap-3 rounded-lg p-1 transition-all hover:bg-gray-100 dark:hover:bg-gray-800 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white dark:focus-visible:ring-offset-gray-950 active:scale-95"
					>
						<div
							class="h-10 w-10 overflow-hidden rounded-full ring-2 ring-gray-100 transition-all dark:ring-gray-800 group-focus-visible:ring-brand-500"
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
					</button>
				{/snippet}
			</DropdownMenu.Trigger>

			<DropdownMenu.Content
				class="z-50 mt-1 min-w-56 origin-top overflow-hidden rounded-lg border border-gray-300 bg-white p-1 shadow-theme-lg will-change-[opacity,transform] data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=open]:zoom-in-95 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:zoom-out-95 dark:border-gray-700 dark:bg-gray-900"
				align="end"
				sideOffset={4}
			>
				<div class="px-3 py-2 mb-1 border-b border-gray-200 dark:border-gray-800">
					<span class="block text-sm font-semibold text-gray-900 dark:text-white">
						{user.userName}
					</span>
					<span class="truncate block text-xs text-gray-500 dark:text-gray-400">
						{user.email}
					</span>
				</div>

				<div class="space-y-0.5">
					{#each menuItems as item}
						<DropdownMenu.Item>
							{#snippet child({ props: itemProps })}
								<Button
									{...itemProps}
									variant="ghost"
									size="sm"
									href={item.href}
									class="w-full justify-start font-medium"
									startIcon={item.icon}
								>
									{item.text}
								</Button>
							{/snippet}
						</DropdownMenu.Item>
					{/each}
				</div>

				<div class="mt-1 pt-1 border-t border-gray-200 dark:border-gray-800">
					<DropdownMenu.Item>
						{#snippet child({ props: itemProps })}
							<Button
								{...itemProps}
								variant="ghost"
								size="sm"
								onclick={handleSignOut}
								class="w-full justify-start font-medium"
								startIcon={LogOut}
							>
								Sign out
							</Button>
						{/snippet}
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

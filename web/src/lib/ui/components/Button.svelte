<script lang="ts">
	import { Button as BitsButton } from 'bits-ui';
	import type { Icon as IconType } from 'lucide-svelte';
	import type { Snippet } from 'svelte';
	import type { HTMLButtonAttributes, HTMLAnchorAttributes } from 'svelte/elements';
	import { cn } from '$lib/ui/utils';

	type Variant = 'primary' | 'outline';
	type Size = 'xs' | 'sm' | 'md' | 'lg';

	interface Props {
		variant?: Variant;
		size?: Size;
		startIcon?: typeof IconType;
		endIcon?: typeof IconType;
		children?: Snippet;
		class?: string;
		href?: string;
		id?: string;
		type?: HTMLButtonAttributes['type'];
		disabled?: boolean;
		readonly?: boolean;
		onclick?: (e: MouseEvent & { currentTarget: HTMLButtonElement | HTMLAnchorElement }) => void;
		[key: string]: any;
	}

	let {
		class: className,
		variant = 'primary',
		size = 'md',
		startIcon: StartIcon,
		endIcon: EndIcon,
		id,
		disabled = false,
		readonly = false,
		children,
		type = 'button',
		href,
		onclick,
		...rest
	}: Props = $props();

	type ButtonClickEvent = MouseEvent & {
		currentTarget: HTMLButtonElement | HTMLAnchorElement;
	};

	const variantClasses: Record<Variant, string> = {
		primary: 'bg-brand-500 text-white shadow-sm hover:bg-brand-600 active:bg-brand-700 disabled:bg-brand-300',
		outline: 'bg-white text-gray-700 border border-gray-300 hover:bg-gray-50 active:bg-gray-100 focus-visible:border-brand-500 disabled:bg-gray-50/50 dark:bg-gray-800 dark:text-gray-400 dark:border-gray-700 dark:focus-visible:border-brand-500 dark:hover:bg-white/[0.03] dark:active:bg-white/[0.08] dark:hover:text-gray-300 dark:disabled:bg-gray-900/50'
	};

	const sizeClasses: Record<Size, string> = {
		xs: 'px-2 py-1 text-xs gap-1 h-7 min-w-7',
		sm: 'px-3 py-2 text-sm gap-2 h-9 min-w-9',
		md: 'px-4 py-2.5 text-md gap-2 h-11 min-w-11',
		lg: 'px-5 py-3 text-lg gap-2.5 h-12 min-w-12'
	};

	const iconSizes: Record<Size, number> = {
		xs: 14,
		sm: 16,
		md: 20,
		lg: 24
	};

	const isDisabled = $derived(Boolean(disabled || readonly));
	const resolvedVariantClass = $derived(variantClasses[variant]);

	function handleClick(event: ButtonClickEvent) {
		if (isDisabled) {
			event.preventDefault();
			event.stopPropagation();
			return;
		}

		onclick?.(event);
	}
</script>

{#if href}
	<a
		id={id}
		href={isDisabled ? undefined : href}
		class={cn(
			'inline-flex cursor-pointer items-center justify-center whitespace-nowrap rounded-lg font-medium transition focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white dark:focus-visible:ring-offset-gray-950',
			sizeClasses[size],
			resolvedVariantClass,
			!isDisabled && 'active:scale-95',
			isDisabled && 'cursor-not-allowed opacity-50 pointer-events-none',
			className
		)}
		aria-disabled={isDisabled}
		tabindex={isDisabled ? -1 : undefined}
		onclick={handleClick}
		{...rest}
	>
		{#if StartIcon}
			<StartIcon size={iconSizes[size]} />
		{/if}

		{#if children}
			{@render children()}
		{/if}

		{#if EndIcon}
			<EndIcon size={iconSizes[size]} />
		{/if}
	</a>
{:else}
	<BitsButton.Root
		{id}
		{type}
		disabled={isDisabled}
		class={cn(
			'inline-flex cursor-pointer items-center justify-center whitespace-nowrap rounded-lg font-medium transition focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white dark:focus-visible:ring-offset-gray-950 disabled:cursor-not-allowed disabled:opacity-50',
			sizeClasses[size],
			resolvedVariantClass,
			!isDisabled && 'active:scale-95',
			className
		)}
		onclick={handleClick}
		{...rest}
	>
		{#if StartIcon}
			<StartIcon size={iconSizes[size]} />
		{/if}

		{#if children}
			{@render children()}
		{/if}

		{#if EndIcon}
			<EndIcon size={iconSizes[size]} />
		{/if}
	</BitsButton.Root>
{/if}

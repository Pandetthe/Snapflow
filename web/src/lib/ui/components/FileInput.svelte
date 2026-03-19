<script lang="ts">
	import { Label } from 'bits-ui';
	import { FileText, Upload, X } from 'lucide-svelte';
	import { cn } from '$lib/ui/utils';

	interface Props {
		files?: File[];
		label?: string;
		helperText?: string;
		error?: string | boolean;
		accept?: string;
		multiple?: boolean;
		disabled?: boolean;
		readonly?: boolean;
		required?: boolean;
		name?: string;
		id?: string;
		class?: string;
		onFilesChange?: (files: File[]) => void;
	}

	const generatedId = $props.id();

	let {
		files = $bindable([] as File[]),
		label,
		helperText,
		error,
		accept,
		multiple = false,
		disabled = false,
		readonly = false,
		required = false,
		name,
		id = generatedId,
		class: className,
		onFilesChange
	}: Props = $props();

	let inputRef = $state<HTMLInputElement | null>(null);
	const isDisabled = $derived.by(() => Boolean(disabled));
	const isReadonly = $derived.by(() => Boolean(readonly));
	const isInteractive = $derived.by(() => !isDisabled && !isReadonly);
	const errorText = $derived.by(() => (typeof error === 'string' ? error : ''));
	const hasError = $derived.by(() => (typeof error === 'string' ? error.length > 0 : Boolean(error)));
	const helperTextId = $derived.by(() => (helperText ? `${id}-help` : undefined));
	const errorTextId = $derived.by(() => (hasError && errorText ? `${id}-error` : undefined));
	const describedBy = $derived.by(() => {
		if (hasError && errorTextId) {
			return errorTextId;
		}

		if (helperTextId) {
			return helperTextId;
		}

		return undefined;
	});

	function handleChange(event: Event) {
		if (!isInteractive) {
			return;
		}

		const target = event.currentTarget as HTMLInputElement;
		const selected = target.files ? Array.from(target.files) : [];
		files = multiple ? selected : selected.slice(0, 1);
		onFilesChange?.(files);
	}

	function openPicker() {
		if (!isInteractive) {
			return;
		}

		inputRef?.click();
	}

	function removeAt(index: number) {
		if (!isInteractive) {
			return;
		}

		files = files.filter((_, i) => i !== index);
		onFilesChange?.(files);

		if (files.length === 0 && inputRef) {
			inputRef.value = '';
		}
	}

	function clearFiles() {
		if (!isInteractive) {
			return;
		}

		files = [];
		onFilesChange?.(files);

		if (inputRef) {
			inputRef.value = '';
		}
	}

	function humanFileSize(size: number) {
		if (size < 1024) {
			return `${size} B`;
		}

		const kb = size / 1024;
		if (kb < 1024) {
			return `${kb.toFixed(1)} KB`;
		}

		return `${(kb / 1024).toFixed(1)} MB`;
	}
</script>

<div class="flex w-full flex-col gap-1.5">
	{#if label}
		<Label.Root for={id} class="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-400">
			{label}
		</Label.Root>
	{/if}

	<input
		bind:this={inputRef}
		{id}
		type="file"
		{name}
		{accept}
		{multiple}
		disabled={isDisabled || isReadonly}
		{required}
		aria-invalid={hasError}
		aria-describedby={describedBy}
		aria-errormessage={hasError && errorText ? errorTextId : undefined}
		class="sr-only"
		onchange={handleChange}
	/>

	<button
		type="button"
		onclick={openPicker}
		class={cn(
			'group flex h-11 w-full cursor-pointer items-center justify-between rounded-lg border bg-transparent px-4 py-2.5 text-left text-sm shadow-theme-xs transition-all duration-200',
			hasError
				? 'border-error-500 text-gray-800 hover:border-error-500 hover:bg-error-50/30 dark:border-error-500 dark:text-white/90 dark:hover:border-error-500 dark:hover:bg-error-500/10'
				: 'border-gray-300 text-gray-800 hover:border-brand-500 hover:bg-black/3 dark:border-gray-700 dark:text-white/90 dark:hover:border-brand-500 dark:hover:bg-white/6',
			hasError
				? 'focus-visible:border-error-500 focus-visible:ring-2 focus-visible:ring-error-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none dark:focus-visible:ring-offset-gray-950'
				: 'focus-visible:border-brand-500 focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none dark:focus-visible:ring-offset-gray-950',
			isReadonly && !isDisabled && (hasError
				? 'cursor-default hover:border-error-500 hover:bg-transparent dark:hover:border-error-500 dark:hover:bg-transparent'
				: 'cursor-default hover:border-gray-300 hover:bg-transparent dark:hover:border-gray-700 dark:hover:bg-transparent'),
			!isInteractive && 'cursor-not-allowed opacity-50',
			className
		)}
		disabled={!isInteractive}
		aria-describedby={describedBy}
	>
		<span class="inline-flex items-center gap-2">
			<Upload
				size={18}
				class={cn(
					'transition-colors',
					hasError
						? 'text-error-500 group-focus-visible:text-error-500 dark:text-error-400'
						: 'text-gray-500 group-focus-visible:text-brand-500'
				)}
			/>
			<span>{multiple ? 'Choose files' : 'Choose file'}</span>
		</span>
		<span class="text-xs text-gray-500 dark:text-gray-400">
			{files.length > 0 ? `${files.length} selected` : 'No file'}
		</span>
	</button>

	{#if helperText && !hasError}
		<span id={helperTextId} class="text-xs text-gray-500 dark:text-gray-400">{helperText}</span>
	{/if}

	{#if files.length > 0}
		<div class="mt-2 space-y-2">
			{#each files as file, index (file.name + index)}
				<div
					class="flex items-center justify-between gap-3 rounded-lg border border-gray-200 bg-gray-50 px-3 py-2 dark:border-gray-800 dark:bg-gray-900/60"
				>
					<div class="min-w-0 flex items-center gap-2">
						<FileText size={16} class="shrink-0 text-gray-500" />
						<div class="min-w-0">
							<p class="truncate text-sm text-gray-700 dark:text-gray-300">{file.name}</p>
							<p class="text-xs text-gray-500 dark:text-gray-400">{humanFileSize(file.size)}</p>
						</div>
					</div>

					<button
						type="button"
						onclick={() => removeAt(index)}
						class={cn(
							'inline-flex h-7 w-7 items-center justify-center rounded-md text-gray-500 transition-colors dark:text-gray-400',
							isInteractive && 'cursor-pointer hover:bg-gray-200 hover:text-gray-700 dark:hover:bg-gray-800 dark:hover:text-gray-300',
							!isInteractive && 'cursor-not-allowed opacity-50'
						)}
						aria-label={`Remove ${file.name}`}
						disabled={!isInteractive}
					>
						<X size={16} />
					</button>
				</div>
			{/each}

			{#if files.length > 1}
				<button
					type="button"
					onclick={clearFiles}
					class={cn(
						'text-xs font-medium transition-colors',
						isInteractive
							? 'cursor-pointer text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200'
							: 'cursor-not-allowed text-gray-400 dark:text-gray-600'
					)}
					disabled={!isInteractive}
				>
					Clear all
				</button>
			{/if}
		</div>
	{/if}

	{#if hasError && errorText}
		<span id={errorTextId} class="text-xs font-medium text-error-500">{errorText}</span>
	{/if}
</div>

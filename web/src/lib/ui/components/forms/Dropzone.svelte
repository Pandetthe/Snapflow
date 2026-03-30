<script lang="ts">
  import { Label } from 'bits-ui';
  import { FileText, CloudUpload, X } from 'lucide-svelte';
  import { cn } from '$lib/ui/utils';
  import { slide } from 'svelte/transition';

  interface Props {
    files?: File[];
    label?: string;
    title?: string;
    description?: string;
    helperText?: string;
    error?: string | boolean;
    accept?: string;
    multiple?: boolean;
    disabled?: boolean;
    readonly?: boolean;
    required?: boolean;
    name?: string;
    maxFiles?: number;
    id?: string;
    class?: string;
    onFilesChange?: (files: File[]) => void;
  }

  const generatedId = $props.id();

  let {
    files = $bindable([] as File[]),
    label,
    title = 'Drag and drop files here',
    description = 'or click to browse from your computer',
    helperText,
    error,
    accept,
    multiple = true,
    disabled = false,
    readonly = false,
    required = false,
    name,
    maxFiles,
    id = generatedId,
    class: className,
    onFilesChange
  }: Props = $props();

  let inputRef = $state<HTMLInputElement | null>(null);
  let isDragActive = $state(false);
  let dragDepth = $state(0);
  const isDisabled = $derived.by(() => Boolean(disabled));
  const isReadonly = $derived.by(() => Boolean(readonly));
  const isInteractive = $derived.by(() => !isDisabled && !isReadonly);
  const errorText = $derived.by(() => (typeof error === 'string' ? error : ''));
  const hasError = $derived.by(() =>
    typeof error === 'string' ? error.length > 0 : Boolean(error)
  );
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

  function applyFiles(newFiles: File[]) {
    if (!isInteractive) {
      return;
    }

    let nextFiles = multiple ? [...files, ...newFiles] : newFiles.slice(0, 1);

    if (!multiple && nextFiles.length > 1) {
      nextFiles = nextFiles.slice(0, 1);
    }

    if (maxFiles && maxFiles > 0) {
      nextFiles = nextFiles.slice(0, maxFiles);
    }

    files = nextFiles;
    onFilesChange?.(files);
  }

  function handleInputChange(event: Event) {
    const target = event.currentTarget as HTMLInputElement;
    const selected = target.files ? Array.from(target.files) : [];
    applyFiles(selected);
  }

  function openPicker() {
    if (!isInteractive) {
      return;
    }

    inputRef?.click();
  }

  function handleDragEnter(event: DragEvent) {
    event.preventDefault();
    if (!isInteractive) {
      return;
    }

    dragDepth += 1;
    isDragActive = true;
  }

  function handleDragOver(event: DragEvent) {
    event.preventDefault();
    if (!isInteractive) {
      return;
    }

    isDragActive = true;
  }

  function handleDragLeave(event: DragEvent) {
    event.preventDefault();
    if (!isInteractive) {
      return;
    }

    dragDepth = Math.max(0, dragDepth - 1);
    if (dragDepth === 0) {
      isDragActive = false;
    }
  }

  function handleDrop(event: DragEvent) {
    event.preventDefault();
    if (!isInteractive) {
      return;
    }

    dragDepth = 0;
    isDragActive = false;

    const dropped = event.dataTransfer?.files ? Array.from(event.dataTransfer.files) : [];
    if (dropped.length > 0) {
      applyFiles(dropped);
    }
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
    onchange={handleInputChange}
  />

  <div
    onclick={openPicker}
    onkeydown={(event) => {
      if (event.key === 'Enter' || event.key === ' ') {
        event.preventDefault();
        openPicker();
      }
    }}
    ondragenter={handleDragEnter}
    ondragover={handleDragOver}
    ondragleave={handleDragLeave}
    ondrop={handleDrop}
    class={cn(
      'cursor-pointer rounded-xl border-2 border-dashed px-4 py-8 text-center transition-all duration-200',
      hasError
        ? 'border-error-500 bg-transparent hover:border-error-500 hover:bg-error-50/30 dark:border-error-500 dark:hover:border-error-500 dark:hover:bg-error-500/10'
        : 'border-gray-300 bg-transparent hover:border-brand-500 hover:bg-black/3 dark:border-gray-700 dark:hover:border-brand-500 dark:hover:bg-white/6',
      hasError
        ? 'focus-visible:border-error-500 focus-visible:ring-2 focus-visible:ring-error-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none dark:focus-visible:ring-offset-gray-950'
        : 'focus-visible:border-brand-500 focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none dark:focus-visible:ring-offset-gray-950',
      isDragActive && 'border-brand-500 bg-brand-50/40 dark:bg-brand-500/10',
      hasError && isDragActive && 'border-error-500 bg-error-50/40 dark:bg-error-500/10',
      isReadonly && !isDisabled && 'cursor-default',
      !isInteractive && 'cursor-not-allowed opacity-50',
      className
    )}
    role="button"
    tabindex={isInteractive ? 0 : -1}
    aria-disabled={isDisabled || isReadonly}
    aria-describedby={describedBy}
  >
    <div class="mx-auto flex max-w-sm flex-col items-center gap-3">
      <div
        class={cn(
          'inline-flex h-12 w-12 items-center justify-center rounded-full border border-gray-200 bg-gray-50 text-gray-500 dark:border-gray-800 dark:bg-gray-900/80 dark:text-gray-400',
          isDragActive && 'border-brand-500 text-brand-500 dark:text-brand-400',
          hasError && 'border-error-500 text-error-500 dark:border-error-500 dark:text-error-400',
          hasError && isDragActive && 'border-error-500 text-error-500 dark:text-error-400'
        )}
      >
        <CloudUpload size={22} />
      </div>
      <p class="text-sm font-medium text-gray-700 dark:text-gray-300">{title}</p>
      <p class="text-xs text-gray-500 dark:text-gray-400">{description}</p>
      {#if maxFiles}
        <p class="text-xs text-gray-500 dark:text-gray-400">Maximum {maxFiles} files</p>
      {/if}
    </div>
  </div>

  {#if helperText && !hasError}
    <div transition:slide={{ axis: 'y', duration: 200 }}>
      <span id={helperTextId} class="text-xs text-gray-500 dark:text-gray-400">{helperText}</span>
    </div>
  {/if}

  {#if files.length > 0}
    <div class="mt-2 space-y-2">
      {#each files as file, index (file.name + index)}
        <div
          class="flex items-center justify-between gap-3 rounded-lg border border-gray-200 bg-gray-50 px-3 py-2 dark:border-gray-800 dark:bg-gray-900/60"
        >
          <div class="flex min-w-0 items-center gap-2">
            <FileText size={16} class="shrink-0 text-gray-500" />
            <div class="min-w-0">
              <p class="truncate text-sm text-gray-700 dark:text-gray-300">{file.name}</p>
              <p class="text-xs text-gray-500 dark:text-gray-400">{humanFileSize(file.size)}</p>
            </div>
          </div>

          <button
            type="button"
            onclick={(event) => {
              event.stopPropagation();
              removeAt(index);
            }}
            class={cn(
              'inline-flex h-7 w-7 items-center justify-center rounded-md text-gray-500 transition-colors dark:text-gray-400',
              isInteractive &&
                'cursor-pointer hover:bg-gray-200 hover:text-gray-700 dark:hover:bg-gray-800 dark:hover:text-gray-300',
              !isInteractive && 'cursor-not-allowed opacity-50'
            )}
            aria-label={`Remove ${file.name}`}
            disabled={!isInteractive}
          >
            <X size={16} />
          </button>
        </div>
      {/each}

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
    </div>
  {/if}

  {#if hasError && errorText}
    <div transition:slide={{ axis: 'y', duration: 200 }}>
      <span id={errorTextId} class="text-xs font-medium text-error-500">{errorText}</span>
    </div>
  {/if}
</div>

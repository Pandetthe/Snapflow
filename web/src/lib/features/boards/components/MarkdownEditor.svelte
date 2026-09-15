<script lang="ts">
  import { onMount, tick } from 'svelte';
  import { Editor } from '@tiptap/core';
  import type { EmojiItem } from '@tiptap/extension-emoji';
  import { isValidYoutubeUrl } from '@tiptap/extension-youtube';
  import { Placeholder } from '@tiptap/extensions';
  import type { SuggestionKeyDownProps, SuggestionProps } from '@tiptap/suggestion';
  import {
    Bold,
    Code,
    Heading,
    ImagePlus,
    Italic,
    Link,
    List,
    ListChecks,
    ListOrdered,
    Quote,
    Smile,
    SquareCode,
    Strikethrough,
    Table,
    Video
  } from 'lucide-svelte';
  import { Button } from '$lib/ui/components';
  import { cn } from '$lib/ui/utils';
  import { descriptionEmoji } from '$lib/features/boards/markdown/emoji';
  import {
    descriptionExtensions,
    normalizeLinkHref,
    normalizeMediaUrl
  } from '$lib/features/boards/markdown/extensions';
  import '$lib/features/boards/styles/markdown.css';

  /**
   * A rich text field that reads and writes markdown. `value` is read once, when the editor mounts
   * (remount it, e.g. with {#key}, to load another one), and written back on every edit.
   */
  let {
    value = $bindable(''),
    label,
    placeholder = '',
    maxLength,
    error,
    disabled = false
  }: {
    value?: string;
    label?: string;
    placeholder?: string;
    maxLength?: number;
    error?: string;
    disabled?: boolean;
  } = $props();

  const id = $props.id();

  let root: HTMLDivElement;
  let element: HTMLDivElement;
  let editor = $state.raw<Editor>();
  // Bumped on every transaction, so the toolbar reads again which marks and nodes are active.
  let revision = $state(0);

  const hasError = $derived(Boolean(error));

  /** Things added from an address typed into the row under the toolbar. */
  type Prompt = 'link' | 'image' | 'video';

  const PROMPTS: Record<Prompt, { label: string; placeholder: string }> = {
    link: { label: 'Link address', placeholder: 'https://' },
    image: { label: 'Image address', placeholder: 'https://example.com/image.png' },
    video: { label: 'YouTube link', placeholder: 'https://www.youtube.com/watch?v=…' }
  };

  let prompt = $state<Prompt | null>(null);
  let promptValue = $state('');
  let promptError = $state('');
  let promptInput = $state<HTMLInputElement>();

  type EmojiMenu = {
    items: EmojiItem[];
    index: number;
    top: number;
    left: number;
    select: (item: EmojiItem) => void;
  };

  const EMOJI_MENU_WIDTH = 224;
  let emojiMenu = $state<EmojiMenu | null>(null);

  onMount(() => {
    const instance = new Editor({
      element,
      extensions: [
        ...descriptionExtensions(),
        Placeholder.configure({ placeholder }),
        descriptionEmoji(() => ({
          onStart: showEmojiMenu,
          onUpdate: showEmojiMenu,
          onKeyDown: handleEmojiKeydown,
          onExit: () => {
            emojiMenu = null;
          }
        }))
      ],
      content: value,
      contentType: 'markdown',
      editable: !disabled,
      editorProps: {
        attributes: {
          class: 'markdown-content min-h-28 px-4 py-2.5 outline-none',
          role: 'textbox',
          'aria-multiline': 'true',
          'aria-describedby': `${id}-help`,
          ...(label ? { 'aria-labelledby': `${id}-label` } : {})
        },
        handleKeyDown: (_view, event) => {
          if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') {
            event.preventDefault();
            openPrompt('link');
            return true;
          }
          return false;
        }
      },
      // Only edits reach `value`. A description nobody touched keeps its text as stored, which the
      // serializer would otherwise rewrite (escaped characters, list markers).
      onUpdate: ({ editor }) => {
        value = editor.getMarkdown();
      },
      onTransaction: () => {
        revision++;
      }
    });
    editor = instance;
    return () => instance.destroy();
  });

  $effect(() => {
    editor?.setEditable(!disabled, false);
  });

  function isActive(name: string) {
    void revision;
    return editor?.isActive(name) ?? false;
  }

  const focused = () => editor!.chain().focus();

  type Tool = {
    label: string;
    icon: typeof Bold;
    /** The mark or node the tool applies, shown as pressed while the selection has it. */
    name?: string;
    prompt?: Prompt;
    run: () => void;
  };

  const toolGroups: Tool[][] = [
    [
      { label: 'Bold', icon: Bold, name: 'bold', run: () => focused().toggleBold().run() },
      { label: 'Italic', icon: Italic, name: 'italic', run: () => focused().toggleItalic().run() },
      {
        label: 'Strikethrough',
        icon: Strikethrough,
        name: 'strike',
        run: () => focused().toggleStrike().run()
      },
      { label: 'Inline code', icon: Code, name: 'code', run: () => focused().toggleCode().run() }
    ],
    [
      {
        label: 'Heading',
        icon: Heading,
        name: 'heading',
        run: () => focused().toggleHeading({ level: 3 }).run()
      },
      {
        label: 'Bulleted list',
        icon: List,
        name: 'bulletList',
        run: () => focused().toggleBulletList().run()
      },
      {
        label: 'Numbered list',
        icon: ListOrdered,
        name: 'orderedList',
        run: () => focused().toggleOrderedList().run()
      },
      {
        label: 'Checklist',
        icon: ListChecks,
        name: 'taskList',
        run: () => focused().toggleTaskList().run()
      },
      {
        label: 'Quote',
        icon: Quote,
        name: 'blockquote',
        run: () => focused().toggleBlockquote().run()
      },
      {
        label: 'Code block',
        icon: SquareCode,
        name: 'codeBlock',
        run: () => focused().toggleCodeBlock().run()
      }
    ],
    [
      {
        label: 'Link (Ctrl+K)',
        icon: Link,
        name: 'link',
        prompt: 'link',
        run: () => togglePrompt('link')
      },
      {
        label: 'Image from a link',
        icon: ImagePlus,
        name: 'image',
        prompt: 'image',
        run: () => togglePrompt('image')
      },
      {
        label: 'YouTube video',
        icon: Video,
        name: 'youtube',
        prompt: 'video',
        run: () => togglePrompt('video')
      },
      {
        label: 'Table',
        icon: Table,
        name: 'table',
        run: () => focused().insertTable({ rows: 3, cols: 3, withHeaderRow: true }).run()
      },
      { label: 'Emoji', icon: Smile, run: startEmoji }
    ]
  ];

  const tableActions: { label: string; danger?: boolean; run: () => void }[] = [
    { label: 'Add row', run: () => focused().addRowAfter().run() },
    { label: 'Add column', run: () => focused().addColumnAfter().run() },
    { label: 'Delete row', run: () => focused().deleteRow().run() },
    { label: 'Delete column', run: () => focused().deleteColumn().run() },
    { label: 'Delete table', danger: true, run: () => focused().deleteTable().run() }
  ];

  async function openPrompt(kind: Prompt) {
    if (!editor) return;
    prompt = kind;
    promptValue =
      kind === 'link' ? ((editor.getAttributes('link').href as string | undefined) ?? '') : '';
    promptError = '';
    await tick();
    promptInput?.focus();
    promptInput?.select();
  }

  function closePrompt() {
    prompt = null;
    editor?.commands.focus();
  }

  function togglePrompt(kind: Prompt) {
    if (prompt === kind) {
      closePrompt();
    } else {
      openPrompt(kind);
    }
  }

  function applyPrompt() {
    if (!editor || !prompt) return;
    const problem =
      prompt === 'link'
        ? applyLink(promptValue)
        : prompt === 'image'
          ? applyImage(promptValue)
          : applyVideo(promptValue);
    if (problem) {
      promptError = problem;
    } else {
      prompt = null;
    }
  }

  // Each returns why the address was not used, or null once it is in the description.

  function applyLink(input: string): string | null {
    if (!input.trim()) {
      focused().extendMarkRange('link').unsetLink().run();
      return null;
    }
    const href = normalizeLinkHref(input);
    if (!href) return 'Use a web or email address';

    if (editor!.state.selection.empty && !editor!.isActive('link')) {
      // Nothing selected to turn into a link, so the address itself becomes the text.
      focused()
        .insertContent({ type: 'text', text: href, marks: [{ type: 'link', attrs: { href } }] })
        .run();
    } else {
      focused().extendMarkRange('link').setLink({ href }).run();
    }
    return null;
  }

  function applyImage(input: string): string | null {
    const src = normalizeMediaUrl(input);
    if (!src) return 'Use the web address of an image';
    focused().setImage({ src }).run();
    return null;
  }

  function applyVideo(input: string): string | null {
    const src = normalizeMediaUrl(input);
    if (!src || !isValidYoutubeUrl(src)) return 'Paste a link to a YouTube video';
    focused().setYoutubeVideo({ src }).run();
    return null;
  }

  function handlePromptKeydown(e: KeyboardEvent) {
    if (e.key === 'Enter') {
      // Would submit the form the editor sits in.
      e.preventDefault();
      applyPrompt();
    } else if (e.key === 'Escape') {
      // Would close the dialog the editor sits in.
      e.preventDefault();
      e.stopPropagation();
      closePrompt();
    }
  }

  /** Types the ":" that opens emoji suggestions, after a space when it would follow a word. */
  function startEmoji() {
    if (!editor) return;
    const { from } = editor.state.selection;
    const before = editor.state.doc.textBetween(Math.max(0, from - 1), from, '\n', '\n');
    focused()
      .insertContent(before && !/\s/.test(before) ? ' :' : ':')
      .run();
  }

  function showEmojiMenu(props: SuggestionProps<EmojiItem>) {
    const rect = props.clientRect?.();
    if (!rect) {
      emojiMenu = null;
      return;
    }
    // Placed inside this component rather than on <body>: a click outside the dialog would close it.
    const box = root.getBoundingClientRect();
    emojiMenu = {
      items: props.items,
      index: 0,
      top: rect.bottom - box.top + 4,
      left: Math.max(0, Math.min(rect.left - box.left, box.width - EMOJI_MENU_WIDTH)),
      select: (item) => props.command(item)
    };
  }

  function handleEmojiKeydown({ event }: SuggestionKeyDownProps) {
    const menu = emojiMenu;
    if (!menu || menu.items.length === 0) return false;
    const count = menu.items.length;
    switch (event.key) {
      case 'ArrowDown':
        menu.index = (menu.index + 1) % count;
        return true;
      case 'ArrowUp':
        menu.index = (menu.index - 1 + count) % count;
        return true;
      case 'Enter':
      case 'Tab':
        menu.select(menu.items[menu.index]);
        return true;
      case 'Escape':
        emojiMenu = null;
        return true;
      default:
        return false;
    }
  }

  const toolButtonClass =
    'inline-flex h-7 w-7 shrink-0 cursor-pointer items-center justify-center rounded-md text-gray-500 transition-colors hover:bg-gray-100 hover:text-gray-900 focus-visible:outline-2 focus-visible:outline-brand-500 disabled:cursor-not-allowed disabled:opacity-40 dark:text-gray-400 dark:hover:bg-white/10 dark:hover:text-white';
  const toolActiveClass =
    'bg-brand-500/10 text-brand-600 hover:bg-brand-500/15 hover:text-brand-700 dark:bg-brand-500/20 dark:text-brand-300 dark:hover:bg-brand-500/25 dark:hover:text-brand-200';
</script>

<div bind:this={root} class="relative flex w-full flex-col gap-1.5">
  {#if label}
    <span id="{id}-label" class="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-400">
      {label}
    </span>
  {/if}

  <div
    class={cn(
      'overflow-hidden rounded-lg border shadow-theme-xs transition-colors duration-200 focus-within:border-transparent focus-within:outline-2 focus-within:outline-offset-2',
      hasError
        ? 'border-error-500 focus-within:outline-error-500 dark:border-error-500'
        : 'border-gray-300 focus-within:outline-brand-500 hover:border-brand-500 dark:border-gray-700 dark:hover:border-brand-500',
      disabled && 'opacity-80'
    )}
  >
    <div
      role="toolbar"
      aria-label="Formatting"
      class="flex items-center gap-0.5 overflow-x-auto border-b border-gray-200 bg-gray-50/80 px-1.5 py-1 dark:border-gray-800 dark:bg-white/3"
    >
      {#each toolGroups as group, groupIndex (groupIndex)}
        {#if groupIndex > 0}
          <span class="mx-1 h-4 w-px shrink-0 bg-gray-200 dark:bg-gray-700" aria-hidden="true"
          ></span>
        {/if}
        {#each group as tool (tool.label)}
          {@const active =
            (tool.prompt !== undefined && prompt === tool.prompt) ||
            (tool.name !== undefined && isActive(tool.name))}
          <button
            type="button"
            class={cn(toolButtonClass, active && toolActiveClass)}
            title={tool.label}
            aria-label={tool.label}
            aria-pressed={tool.name !== undefined ? active : undefined}
            aria-expanded={tool.prompt !== undefined ? prompt === tool.prompt : undefined}
            disabled={!editor || disabled}
            onmousedown={(e) => e.preventDefault()}
            onclick={tool.run}
          >
            <tool.icon class="h-3.5 w-3.5" />
          </button>
        {/each}
      {/each}
    </div>

    {#if isActive('table')}
      <div
        role="toolbar"
        aria-label="Table"
        class="flex items-center gap-0.5 overflow-x-auto border-b border-gray-200 px-1.5 py-1 dark:border-gray-800"
      >
        {#each tableActions as action (action.label)}
          <button
            type="button"
            class={cn(
              'h-6 shrink-0 cursor-pointer rounded-md px-2 text-xs font-medium transition-colors focus-visible:outline-2 focus-visible:outline-brand-500 disabled:cursor-not-allowed disabled:opacity-40',
              action.danger
                ? 'text-error-500 hover:bg-error-500/10 dark:text-error-400'
                : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900 dark:text-gray-400 dark:hover:bg-white/10 dark:hover:text-white'
            )}
            {disabled}
            onmousedown={(e) => e.preventDefault()}
            onclick={action.run}
          >
            {action.label}
          </button>
        {/each}
      </div>
    {/if}

    {#if prompt}
      <div
        class="flex flex-wrap items-center gap-2 border-b border-gray-200 px-2 py-1.5 dark:border-gray-800"
      >
        <input
          bind:this={promptInput}
          bind:value={promptValue}
          type="text"
          inputmode="url"
          placeholder={PROMPTS[prompt].placeholder}
          aria-label={PROMPTS[prompt].label}
          aria-invalid={Boolean(promptError)}
          class="h-7 min-w-0 flex-1 rounded-md border border-gray-300 bg-transparent px-2 text-xs text-gray-800 placeholder:text-gray-400 focus-visible:border-transparent focus-visible:outline-2 focus-visible:outline-brand-500 dark:border-gray-700 dark:text-white/90 dark:placeholder:text-white/30"
          oninput={() => (promptError = '')}
          onkeydown={handlePromptKeydown}
        />
        <div class="flex gap-1">
          <Button type="button" size="xs" variant="primary" onclick={applyPrompt}>
            {prompt === 'link' ? 'Apply' : 'Insert'}
          </Button>
          <Button type="button" size="xs" variant="ghost" onclick={closePrompt}>Cancel</Button>
        </div>
        {#if promptError}
          <span class="w-full text-xs font-medium text-error-500">{promptError}</span>
        {/if}
      </div>
    {/if}

    <div bind:this={element} class="max-h-[50vh] overflow-y-auto"></div>
  </div>

  <div id="{id}-help" class="flex items-start justify-between gap-3 text-xs">
    {#if hasError}
      <span class="font-medium text-error-500">{error}</span>
    {/if}
    {#if maxLength !== undefined}
      <span
        class="ml-auto shrink-0 tabular-nums {value.length > maxLength
          ? 'font-medium text-error-500'
          : 'text-gray-600 dark:text-gray-400'}"
      >
        {value.length}/{maxLength}
      </span>
    {/if}
  </div>

  {#if emojiMenu && emojiMenu.items.length > 0}
    <div
      class="absolute z-20 overflow-hidden rounded-lg border border-gray-200 bg-white p-1 shadow-lg dark:border-gray-700 dark:bg-gray-900"
      style:top="{emojiMenu.top}px"
      style:left="{emojiMenu.left}px"
      style:width="{EMOJI_MENU_WIDTH}px"
    >
      {#each emojiMenu.items as item, index (item.name)}
        <button
          type="button"
          class="flex w-full cursor-pointer items-center gap-2 rounded-md px-2 py-1 text-left {index ===
          emojiMenu.index
            ? 'bg-gray-100 dark:bg-white/10'
            : 'hover:bg-gray-50 dark:hover:bg-white/5'}"
          aria-current={index === emojiMenu.index}
          onmousedown={(e) => e.preventDefault()}
          onclick={() => emojiMenu?.select(item)}
        >
          <span class="text-base leading-none">{item.emoji}</span>
          <span class="truncate text-xs text-gray-600 dark:text-gray-300">
            :{item.shortcodes[0]}:
          </span>
        </button>
      {/each}
    </div>
  {/if}
</div>

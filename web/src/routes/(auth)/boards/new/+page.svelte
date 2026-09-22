<script lang="ts">
  import { BoardsService } from '$lib/features/boards/api/boards.api';
  import { apiClient } from '$lib/core/api.client';
  import { Button, FullLayout, InputTextField, GoBackButton, Textarea } from '$lib/ui/components';
  import BoardMembersEditor from '$lib/features/boards/components/BoardMembersEditor.svelte';
  import { createForm } from '$lib/ui/utils';
  import { Plus, Users } from 'lucide-svelte';
  import type { BoardVisibility } from '$lib/features/boards/types/boards.api';
  import type { OwnerMember, SelectedMember } from '$lib/features/boards/types/members';
  import VisibilitySelector from '$lib/features/boards/components/VisibilitySelector.svelte';

  const boardsService = new BoardsService(apiClient);

  let { data } = $props();

  const ownerMember = $derived.by<OwnerMember>(() => ({
    id: data.user.id,
    userName: data.user.userName,
    avatarUrl: data.user.avatarUrl,
    role: 'owner'
  }));

  let selectedMembers = $state<SelectedMember[]>([]);
  let visibility = $state<BoardVisibility>('private');

  const backHref = '/';
  const selectedMembersCount = $derived(selectedMembers.length + 1);

  const form = createForm({
    initialValues: {
      title: '',
      description: ''
    },
    validate: (values) => {
      const errors: Record<string, string> = {};

      if (!values.title.trim()) {
        errors.title = 'Title is required';
      } else if (values.title.trim().length < 3) {
        errors.title = 'Title must be at least 3 characters';
      } else if (values.title.trim().length > 100) {
        errors.title = 'Title must be less than 100 characters';
      }

      if (values.description.length > 500) {
        errors.description = 'Description must be less than 500 characters';
      }

      return errors;
    },
    onSubmit: async (values) => {
      const members = selectedMembers.map((member) => ({
        userId: member.id,
        role: member.role
      }));

      return await boardsService.createBoard({
        title: values.title.trim(),
        description: values.description.trim(),
        members: members.length > 0 ? members : undefined,
        visibility
      });
    },
    onSuccess: (response) => {
      window.location.href = `/boards/${response.id}`;
    }
  });
</script>

<svelte:head>
  <title>Snapflow | Create your new board!</title>
</svelte:head>

<FullLayout>
  <div class="mx-auto w-full max-w-5xl space-y-6 pb-12 sm:space-y-8">
    <header class="flex flex-col gap-4">
      <GoBackButton href={backHref} />

      <div class="flex flex-col gap-1 sm:flex-row sm:items-end sm:justify-between">
        <div class="space-y-1">
          <h1 class="text-2xl font-bold tracking-tight text-gray-900 sm:text-4xl dark:text-white">
            Create your new board!
          </h1>
          <p class="text-sm text-gray-600 sm:text-base dark:text-gray-400">
            Set up your project environment and invite your team.
          </p>
        </div>
      </div>
    </header>

    <div class="grid items-start gap-6 lg:grid-cols-[1fr_minmax(20rem,25rem)] lg:gap-8">
      <div class="space-y-6">
        <section
          class="group rounded-2xl border border-gray-200 bg-white p-5 shadow-sm transition-all duration-200 hover:border-brand-500/30 hover:shadow-md sm:rounded-3xl sm:p-6 dark:border-gray-800 dark:bg-gray-900/50 dark:hover:border-brand-500/20"
        >
          <form onsubmit={form.handleSubmit} novalidate class="space-y-6">
            <InputTextField
              id="title"
              name="title"
              label="Board title"
              placeholder="Development Roadmap"
              required={true}
              bind:value={form.values.title}
              error={form.errors.title}
              class="h-12 text-lg"
            />

            <Textarea
              id="description"
              name="description"
              label="Description"
              placeholder="Describe the goals and scope of this board..."
              maxlength={500}
              rows={4}
              bind:value={form.values.description}
              error={form.errors.description}
              helperText={`${form.values.description.length}/500`}
              class="resize-none"
            />

            <div class="space-y-2">
              <p class="text-sm font-medium text-gray-700 dark:text-gray-400">Visibility</p>
              <VisibilitySelector
                bind:value={visibility}
                allowedVisibilities={data.visibilityOptions.allowedVisibilities}
              />
            </div>

            <div
              class="flex flex-col-reverse gap-4 pt-2 sm:flex-row sm:items-center sm:justify-between"
            >
              <p class="text-xs text-gray-600 dark:text-gray-400">
                <span class="text-error-500">*</span> Required fields
              </p>
              <Button
                type="submit"
                variant="primary"
                size="lg"
                disabled={!form.values.title.trim() || form.isSubmitting}
                isLoading={form.isSubmitting}
                loadingText="Creating"
                startIcon={Plus}
                class="w-full px-8 sm:w-auto"
              >
                Create board
              </Button>
            </div>
          </form>
        </section>
      </div>

      <aside class="space-y-6">
        <section
          class="rounded-2xl border border-gray-200 bg-white p-5 shadow-sm transition-all duration-200 sm:rounded-3xl sm:p-6 dark:border-gray-800 dark:bg-gray-900/50"
        >
          <div class="mb-6 flex items-start justify-between">
            <div>
              <h2 class="text-lg font-bold text-gray-900 dark:text-white">Project team</h2>
              <p class="text-sm text-gray-600 dark:text-gray-400">
                {selectedMembersCount} member{selectedMembersCount === 1 ? '' : 's'} assigned
              </p>
            </div>
            <Users class="h-5 w-5 text-gray-500" />
          </div>

          <BoardMembersEditor bind:members={selectedMembers} owner={ownerMember} />
        </section>
      </aside>
    </div>
  </div>
</FullLayout>

<script lang="ts">
  import { InputDateField, InputTextField, InputTimeField } from '$lib/ui/components';
  import type { ComponentProps } from 'svelte';

  type FieldProps = ComponentProps<typeof InputTextField> &
    Partial<ComponentProps<typeof InputDateField>> &
    Partial<ComponentProps<typeof InputTimeField>>;

  let {
    value = $bindable(''),
    type = 'text',
    hourCycle = undefined,
    ...rest
  }: FieldProps & {
    value?: string;
    type?: string;
    hourCycle?: 12 | 24;
  } = $props();

  const forwardedProps = $derived(rest);
</script>

{#if type === 'date'}
  <InputDateField bind:value {...forwardedProps} />
{:else if type === 'time'}
  <InputTimeField bind:value {hourCycle} {...forwardedProps} />
{:else}
  <InputTextField bind:value {type} {...forwardedProps} />
{/if}

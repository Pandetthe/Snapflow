<script lang="ts">
	import InputDateField from './input/InputDateField.svelte';
	import InputTextField from './input/InputTextField.svelte';
	import InputTimeField from './input/InputTimeField.svelte';

	let {
		value = $bindable(''),
		type = 'text',
		hourCycle = undefined,
		...rest
	}: {
		value?: string;
		type?: string;
		hourCycle?: 12 | 24;
		[key: string]: any;
	} = $props();

	const forwardedProps = $derived.by(() => rest as Record<string, unknown>);
</script>

{#if type === 'date'}
	<InputDateField bind:value {...forwardedProps} />
{:else if type === 'time'}
	<InputTimeField bind:value {hourCycle} {...forwardedProps} />
{:else}
	<InputTextField bind:value {type} {...forwardedProps} />
{/if}

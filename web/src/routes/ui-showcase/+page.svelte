<script lang="ts">
  import {
    Button,
    Checkbox,
    Dropzone,
    FileInput,
    Input,
    Select,
    Switch,
    RadioButton,
    RadioGroup,
    Textarea,
    Progress
  } from '$lib/ui/components';
  import { CalendarDays, Clock3, CreditCard, Plus, Settings } from 'lucide-svelte';
  import { onMount } from 'svelte';

  let switchDefault = $state(false);
  let switchChecked = $state(true);
  let switchDisabled = $state(true);
  let inputDefault = $state('');
  let inputPlaceholder = $state('');
  let inputSearch = $state('');
  let inputPassword = $state('');
  let inputDate = $state('');
  let inputDateDmy = $state('');
  let inputDateMdy = $state('');
  let inputDateYmd = $state('');
  let inputDateDeselectable = $state('2026-03-18');
  let inputDateReadonly = $state('2026-03-18');
  let inputDateDisabled = $state('2026-03-18');
  let inputDateRequired = $state('');
  let inputDateError = $state('');
  let inputDateLeftIcon = $state('');
  let inputDateRightIcon = $state('');
  let inputDateLeftIconDecorated = $state('');
  let inputTime = $state('');
  let inputTime24h = $state('14:30');
  let inputTime12h = $state('02:30 PM');
  let inputTimeDeselectable = $state('10:00');
  let inputTimeReadonly = $state('09:00');
  let inputTimeDisabled = $state('08:00');
  let inputTimeRequired = $state('');
  let inputTimeError = $state('');
  let inputTimeLeftIconBasic = $state('');
  let inputTimeRightIcon = $state('');
  let inputTimeLeftIconDecorated = $state('');
  let inputPayment = $state('');
  let inputDisabled = $state('Readonly style preview');
  let inputFull = $state('');
  let inputReadonlyValue = $state('Readonly value');
  let inputErrorValue = $state('broken@example.com');
  let fileInputSingle = $state([] as File[]);
  let fileInputMultiple = $state([] as File[]);
  let fileInputReadonly = $state([] as File[]);
  let fileInputDisabled = $state([] as File[]);
  let fileInputError = $state([] as File[]);
  let dropzoneMultiple = $state([] as File[]);
  let dropzoneSingle = $state([] as File[]);
  let dropzoneReadonly = $state([] as File[]);
  let dropzoneDisabled = $state([] as File[]);
  let dropzoneError = $state([] as File[]);
  let checkboxFullDefault = $state(false);
  let checkboxFullReadonly = $state(true);
  let checkboxFullDisabled = $state(true);
  let checkboxFullError = $state(false);
  let switchFullDefault = $state(false);
  let switchFullReadonly = $state(true);
  let switchFullDisabled = $state(true);
  let switchFullError = $state(false);
  let selectFullRequired = $state('');
  let selectFullReadonly = $state('dark-green');
  let selectFullError = $state('');
  let selectFullMultiple = $state(['light-monochrome'] as string[]);

  const selectOptions = [
    { value: 'light-monochrome', label: 'Light Monochrome' },
    { value: 'dark-green', label: 'Dark Green' },
    { value: 'svelte-orange', label: 'Svelte Orange' },
    { value: 'punk-pink', label: 'Punk Pink' },
    { value: 'ocean-blue', label: 'Ocean Blue', disabled: true },
    { value: 'sunset-orange', label: 'Sunset Orange' },
    { value: 'sunset-red', label: 'Sunset Red' },
    { value: 'forest-green', label: 'Forest Green' },
    { value: 'lavender-purple', label: 'Lavender Purple', disabled: true },
    { value: 'mustard-yellow', label: 'Mustard Yellow' },
    { value: 'slate-gray', label: 'Slate Gray' },
    { value: 'neon-green', label: 'Neon Green' },
    { value: 'coral-reef', label: 'Coral Reef' },
    { value: 'midnight-blue', label: 'Midnight Blue' },
    { value: 'crimson-red', label: 'Crimson Red' },
    { value: 'mint-green', label: 'Mint Green' },
    { value: 'pastel-pink', label: 'Pastel Pink' },
    { value: 'golden-yellow', label: 'Golden Yellow' },
    { value: 'deep-purple', label: 'Deep Purple' },
    { value: 'turquoise-blue', label: 'Turquoise Blue' },
    { value: 'burnt-orange', label: 'Burnt Orange' }
  ];

  let progressValue = $state(13);
  onMount(() => {
    const interval = setInterval(() => {
      progressValue = (progressValue + 1) % 101;
    }, 150);
    return () => clearInterval(interval);
  });
</script>

<svelte:head>
  <title>Snapflow | UI Showcase</title>
</svelte:head>

<div class="container mx-auto space-y-12">
  <header class="border-b border-gray-200 pb-6 dark:border-gray-800">
    <h1 class="text-3xl font-bold text-gray-800 dark:text-white/90">UI Showcase</h1>
  </header>

  <div class="grid grid-cols-1 gap-12">
    <section class="space-y-8">
      <h2
        class="border-l-4 border-brand-500 pl-3 text-xl font-semibold text-gray-800 dark:text-white/90"
      >
        Buttons
      </h2>

      <div class="overflow-x-auto">
        <table class="w-full border-collapse">
          <thead>
            <tr
              class="border-b border-gray-200 text-left text-[10px] font-semibold tracking-wider text-gray-400 uppercase dark:border-gray-800 dark:text-gray-400"
            >
              <th class="bg-gray-50/50 p-4 dark:bg-gray-900/50">Size / Variant</th>
              <th class="p-4 text-center">Plain</th>
              <th class="p-4 text-center">Start Icon</th>
              <th class="p-4 text-center">End Icon</th>
              <th class="p-4 text-center">Both Icons</th>
              <th class="p-4 text-center">Icon Only</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 dark:divide-gray-800">
            {#each ['p-xs', 'o-xs', 'p-sm', 'o-sm', 'p-md', 'o-md', 'p-lg', 'o-lg'] as const as combination}
              {@const [vCode, sCode] = combination.split('-')}
              {@const variant = vCode === 'p' ? 'primary' : 'outline'}
              {@const size = sCode as 'xs' | 'sm' | 'md' | 'lg'}

              <tr>
                <td
                  class="bg-gray-50/50 p-4 font-medium whitespace-nowrap text-gray-700 dark:bg-gray-900/50 dark:text-gray-400"
                >
                  <div class="flex flex-col">
                    <span class="text-xs font-bold uppercase">{size}</span>
                    <span class="text-[10px] text-gray-500 capitalize dark:text-gray-400"
                      >{variant}</span
                    >
                  </div>
                </td>
                <td class="p-4 text-center">
                  <Button {size} {variant} haptic="success">Button</Button>
                </td>
                <td class="p-4 text-center">
                  <Button {size} {variant} startIcon={Plus} haptic="error">Start</Button>
                </td>
                <td class="p-4 text-center">
                  <Button {size} {variant} endIcon={Plus} haptic="warning">End</Button>
                </td>
                <td class="p-4 text-center">
                  <Button {size} {variant} startIcon={Plus} endIcon={Settings} haptic="success"
                    >Both</Button
                  >
                </td>
                <td class="p-4 text-center">
                  <Button {size} {variant} startIcon={Plus} haptic="light" />
                </td>
              </tr>
            {/each}
          </tbody>
        </table>
      </div>

      <div class="grid grid-cols-1 gap-6 md:grid-cols-2 xl:grid-cols-3">
        <Button id="button-primary" variant="primary">Primary</Button>

        <Button id="button-outline" variant="outline">Outline</Button>

        <Button id="button-primary-disabled" variant="primary" disabled>Primary disabled</Button>

        <Button id="button-outline-disabled" variant="outline" disabled>Outline disabled</Button>
      </div>
    </section>

    <section class="space-y-8">
      <h2
        class="border-l-4 border-brand-500 pl-3 text-xl font-semibold text-gray-800 dark:text-white/90"
      >
        Inputs
      </h2>

      <div class="grid grid-cols-1 gap-6 md:grid-cols-2 xl:grid-cols-3">
        <Input bind:value={inputDefault} label="Default" />

        <Input
          bind:value={inputPlaceholder}
          label="With placeholder"
          placeholder="info@gmail.com"
        />

        <Input bind:value={inputSearch} label="Search" type="search" placeholder="Search..." />

        <Input
          bind:value={inputPassword}
          label="Password"
          type="password"
          placeholder="Enter password"
          showPasswordToggle
        />

        <Input
          bind:value={inputDate}
          label="Date"
          type="date"
          placeholder="Select date"
          rightIcon={CalendarDays}
        />

        <Input
          bind:value={inputTime}
          label="Time"
          type="time"
          placeholder="Select time"
          rightIcon={Clock3}
        />

        <Input
          bind:value={inputPayment}
          label="Card number"
          placeholder="Card number"
          leftIcon={CreditCard}
          leftIconDecorated
        />

        <Input bind:value={inputDisabled} label="Disabled" disabled />

        <Input
          bind:value={inputFull}
          id="input-full"
          name="profileEmail"
          label="Required + helper"
          placeholder="name@domain.com"
          required
          helperText="Required field with helper text"
        />

        <Input
          bind:value={inputReadonlyValue}
          id="input-readonly-full"
          name="readonlyExample"
          label="Readonly"
          readonly
          helperText="Readonly blocks editing"
        />

        <Input
          bind:value={inputErrorValue}
          id="input-error-full"
          name="emailWithError"
          label="Validation error"
          error="Invalid email format"
          helperText="Helper text is hidden when error is shown"
        />
      </div>

      <div class="space-y-5">
        <h3 class="text-lg font-semibold text-gray-800 dark:text-white/90">Date</h3>
        <div class="grid grid-cols-1 gap-6 md:grid-cols-2 xl:grid-cols-3">
          <Input
            bind:value={inputDateDmy}
            id="date-dmy"
            label="DMY format"
            type="date"
            dateOrder="dmy"
          />

          <Input
            bind:value={inputDateMdy}
            id="date-mdy"
            label="MDY format"
            type="date"
            dateOrder="mdy"
          />

          <Input
            bind:value={inputDateYmd}
            id="date-ymd"
            label="YMD format"
            type="date"
            dateOrder="ymd"
          />

          <Input
            bind:value={inputDateDeselectable}
            id="date-deselectable"
            label="Deselectable"
            type="date"
            allowDeselect
          />

          <Input
            bind:value={inputDateReadonly}
            id="date-readonly"
            label="Readonly"
            type="date"
            readonly
          />

          <Input
            bind:value={inputDateDisabled}
            id="date-disabled"
            label="Disabled"
            type="date"
            disabled
          />

          <Input
            bind:value={inputDateRequired}
            id="date-required"
            label="Required"
            type="date"
            required
          />

          <Input
            bind:value={inputDateError}
            id="date-error"
            label="Error"
            type="date"
            error="Example error message"
          />

          <Input
            bind:value={inputDateLeftIcon}
            id="date-left-icon"
            label="Left icon"
            type="date"
            leftIcon={Clock3}
          />

          <Input
            bind:value={inputDateRightIcon}
            id="date-right-icon"
            label="Right icon override"
            type="date"
            rightIcon={Clock3}
          />

          <Input
            bind:value={inputDateLeftIconDecorated}
            id="date-left-icon-decorated"
            label="Left icon decorated"
            type="date"
            leftIcon={Clock3}
            leftIconDecorated
          />
        </div>
      </div>

      <div class="space-y-5">
        <h3 class="text-lg font-semibold text-gray-800 dark:text-white/90">Time</h3>
        <div class="grid grid-cols-1 gap-6 md:grid-cols-2 xl:grid-cols-3">
          <Input bind:value={inputTime24h} id="time-24h" label="24h" type="time" hourCycle={24} />

          <Input bind:value={inputTime12h} id="time-12h" label="12h" type="time" hourCycle={12} />

          <Input
            bind:value={inputTimeDeselectable}
            id="time-deselectable"
            label="Deselectable"
            type="time"
            allowDeselect
          />

          <Input
            bind:value={inputTimeReadonly}
            id="time-readonly"
            label="Readonly"
            type="time"
            readonly
          />

          <Input
            bind:value={inputTimeDisabled}
            id="time-disabled"
            label="Disabled"
            type="time"
            disabled
          />

          <Input
            bind:value={inputTimeRequired}
            id="time-required"
            label="Required"
            type="time"
            required
          />

          <Input
            bind:value={inputTimeError}
            id="time-error"
            label="Error"
            type="time"
            error="Invalid time selected"
          />

          <Input
            bind:value={inputTimeLeftIconBasic}
            id="time-left-icon-basic"
            label="Left icon"
            type="time"
            leftIcon={Clock3}
          />

          <Input
            bind:value={inputTimeRightIcon}
            id="time-right-icon"
            label="Right icon override"
            type="time"
            rightIcon={Clock3}
          />

          <Input
            bind:value={inputTimeLeftIconDecorated}
            id="time-left-icon-decorated"
            label="Left icon decorated"
            type="time"
            leftIcon={Clock3}
            leftIconDecorated
          />
        </div>
      </div>
    </section>

    <section class="space-y-8">
      <h2
        class="border-l-4 border-brand-500 pl-3 text-xl font-semibold text-gray-800 dark:text-white/90"
      >
        File Uploads
      </h2>

      <div class="space-y-5">
        <h3 class="text-lg font-semibold text-gray-800 dark:text-white/90">File input</h3>
        <div class="grid grid-cols-1 gap-6 md:grid-cols-2 xl:grid-cols-3">
          <FileInput
            bind:files={fileInputSingle}
            label="Single"
            helperText="Select one file (for example PDF or image)."
            accept=".pdf,.png,.jpg,.jpeg"
          />

          <FileInput
            bind:files={fileInputMultiple}
            label="Multiple"
            helperText="Select multiple files at once."
            accept=".pdf,.doc,.docx,.png,.jpg,.jpeg"
            multiple
          />

          <FileInput
            bind:files={fileInputReadonly}
            id="file-input-readonly"
            name="readonlyFile"
            label="Readonly"
            helperText="Readonly blocks picker and remove actions"
            accept=".pdf,.png,.jpg,.jpeg"
            readonly
          />

          <FileInput
            bind:files={fileInputDisabled}
            id="file-input-disabled"
            name="disabledFile"
            label="Disabled"
            helperText="Disabled blocks all interactions"
            accept=".pdf,.png,.jpg,.jpeg"
            disabled
          />

          <FileInput
            bind:files={fileInputError}
            id="file-input-error"
            name="errorFile"
            label="Error"
            error="Upload failed. Try another file."
            accept=".pdf,.png,.jpg,.jpeg"
            required
          />
        </div>
      </div>

      <div class="space-y-5">
        <h3 class="text-lg font-semibold text-gray-800 dark:text-white/90">Dropzone</h3>
        <div class="grid grid-cols-1 gap-6 md:grid-cols-2 xl:grid-cols-3">
          <Dropzone
            bind:files={dropzoneMultiple}
            label="Multiple"
            title="Drop files here"
            description="or click to select files"
            helperText="For example: images, PDFs, documents"
            maxFiles={5}
            accept=".pdf,.doc,.docx,.png,.jpg,.jpeg"
            multiple
          />

          <Dropzone
            bind:files={dropzoneSingle}
            label="Single"
            title="Add one file"
            description="click or drop a file"
            helperText="Single file variant"
            accept=".pdf,.png,.jpg,.jpeg"
            multiple={false}
          />

          <Dropzone
            bind:files={dropzoneReadonly}
            id="dropzone-readonly"
            name="readonlyDropzone"
            label="Readonly"
            title="Readonly dropzone"
            description="Dropzone is visible but interaction is locked"
            helperText="Readonly blocks pick, drag, remove and clear"
            accept=".pdf,.png,.jpg,.jpeg"
            readonly
          />

          <Dropzone
            bind:files={dropzoneDisabled}
            id="dropzone-disabled"
            name="disabledDropzone"
            label="Disabled"
            title="Disabled dropzone"
            description="All actions are disabled"
            helperText="Disabled blocks all interactions"
            accept=".pdf,.png,.jpg,.jpeg"
            disabled
          />

          <Dropzone
            bind:files={dropzoneError}
            id="dropzone-error"
            name="errorDropzone"
            label="Error"
            title="Drop files"
            description="Error state keeps hover and focus in error palette"
            error="At least one valid file is required."
            accept=".pdf,.png,.jpg,.jpeg"
            required
          />
        </div>
      </div>
    </section>

    <section class="space-y-8">
      <h2
        class="border-l-4 border-brand-500 pl-3 text-xl font-semibold text-gray-800 dark:text-white/90"
      >
        Textareas
      </h2>

      <div class="space-y-8 rounded-xl p-6">
        <div class="grid grid-cols-1 gap-8 sm:grid-cols-2">
          <Textarea label="Basic Textarea" placeholder="Enter some long text here..." />
          <Textarea label="With Rows" placeholder="This one has 6 rows..." rows={6} />
          <Textarea
            label="With Helper Text"
            placeholder="Description"
            helperText="Write a detailed description of the project."
          />
          <Textarea label="With Error" placeholder="Missing info" error="This field is required" />
          <Textarea label="Disabled" placeholder="Cannot type here" disabled />
          <Textarea label="Readonly" value="This content is readonly" readonly />
        </div>
      </div>
    </section>

    <section class="space-y-8">
      <h2
        class="border-l-4 border-brand-500 pl-3 text-xl font-semibold text-gray-800 dark:text-white/90"
      >
        Checkboxes
      </h2>

      <div class="space-y-8 rounded-xl p-6">
        <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4 2xl:grid-cols-8">
          <Checkbox checked={false} label="Unchecked" />
          <Checkbox checked={true} label="Checked" />
          <Checkbox checked={false} label="Disabled" disabled />
          <Checkbox checked={true} label="Disabled checked" disabled />
          <Checkbox checked={false} label="With error" error />
          <Checkbox checked={true} label="Checked with error" error />
          <Checkbox
            bind:checked={checkboxFullDefault}
            id="checkbox-full-required"
            name="acceptTerms"
            value="accepted"
            label="Required + helper"
            helperText="You must accept terms"
            required
          />
          <Checkbox
            bind:checked={checkboxFullReadonly}
            id="checkbox-full-readonly"
            name="readonlyCheckbox"
            label="Readonly"
            helperText="Readonly blocks toggling"
            readonly
          />
          <Checkbox
            bind:checked={checkboxFullDisabled}
            id="checkbox-full-disabled"
            name="disabledCheckbox"
            label="Disabled"
            helperText="Disabled state"
            disabled
          />
          <Checkbox
            bind:checked={checkboxFullError}
            id="checkbox-full-error"
            name="errorCheckbox"
            label="Error message"
            error="This consent is required"
          />
        </div>

        <div class="space-y-3">
          <p class="text-xs font-semibold tracking-wide text-gray-500 uppercase dark:text-gray-400">
            Without label
          </p>
          <div class="grid grid-cols-2 justify-items-start gap-4 sm:grid-cols-4 xl:grid-cols-8">
            <Checkbox checked={false} />
            <Checkbox checked={true} />
            <Checkbox checked={false} disabled />
            <Checkbox checked={true} disabled />
            <Checkbox checked={false} error />
            <Checkbox checked={true} error />
            <Checkbox checked={false} error disabled />
            <Checkbox checked={true} error disabled />
          </div>
          <div class="space-y-3">
            <p
              class="text-xs font-semibold tracking-wide text-gray-500 uppercase dark:text-gray-400"
            >
              Tri-state
            </p>
            <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
              <Checkbox tristate checked={false} label="Tri-state (X state)" />
              <Checkbox tristate checked={true} label="Tri-state (Check state)" />
              <Checkbox tristate checked={null} label="Tri-state (Empty state)" />
              <Checkbox checked={false} label="Standard (False = Empty)" />
            </div>
          </div>
        </div>
      </div>
    </section>

    <section class="space-y-8">
      <h2
        class="border-l-4 border-brand-500 pl-3 text-xl font-semibold text-gray-800 dark:text-white/90"
      >
        Radio Buttons
      </h2>

      <div class="space-y-8 rounded-xl p-6">
        <RadioGroup label="Horizontal Group" class="flex-row gap-8">
          <RadioButton value="apple" label="Apple" />
          <RadioButton value="banana" label="Banana" />
          <RadioButton value="cherry" label="Cherry" />
        </RadioGroup>

        <RadioGroup label="Vertical Group (Default)" value="option2">
          <RadioButton
            id="radio-1"
            value="option1"
            label="Option 1"
            helperText="Description for option 1"
          />
          <RadioButton
            id="radio-2"
            value="option2"
            label="Option 2"
            helperText="Description for option 2"
          />
          <RadioButton
            id="radio-3"
            value="option3"
            label="Option 3"
            disabled
            helperText="Disabled option"
          />
        </RadioGroup>

        <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
          <RadioGroup label="Required Group" required>
            <RadioButton value="r1" label="Required 1" />
            <RadioButton value="r2" label="Required 2" />
          </RadioGroup>

          <RadioGroup label="Readonly Group" value="ro1">
            <RadioButton value="ro1" label="Readonly 1" readonly />
            <RadioButton value="ro2" label="Readonly 2" readonly />
          </RadioGroup>
        </div>
      </div>
    </section>

    <section class="space-y-8">
      <h2
        class="border-l-4 border-brand-500 pl-3 text-xl font-semibold text-gray-800 dark:text-white/90"
      >
        Selects
      </h2>

      <div class="grid grid-cols-1 gap-6 md:grid-cols-2 xl:grid-cols-4">
        <Select label="Empty" placeholder="Select Option" options={selectOptions} />

        <Select
          value={'monochrome'}
          label="Prefilled"
          placeholder="Select Option"
          options={selectOptions}
        />

        <Select
          label="Deselectable"
          placeholder="Select Option"
          options={selectOptions}
          allowDeselect
        />
        <Select
          value={'monochrome'}
          label="Prefilled deselectable"
          placeholder="Select Option"
          options={selectOptions}
          allowDeselect
        />

        <Select label="Disabled" placeholder="Select Option" options={selectOptions} disabled />

        <Select
          value={'monochrome'}
          label="Prefilled disabled"
          placeholder="Select Option"
          options={selectOptions}
          disabled
        />

        <Select
          values={['light-monochrome', 'burnt-orange']}
          label="Multiple"
          placeholder="Select Options"
          options={selectOptions}
          multiple
        />

        <Select
          bind:value={selectFullRequired}
          id="select-full-required"
          name="themeSingle"
          label="Required + helper"
          placeholder="Select theme"
          helperText="Required field"
          required
          options={selectOptions}
        />

        <Select
          bind:value={selectFullReadonly}
          id="select-full-readonly"
          name="themeReadonly"
          label="Readonly"
          placeholder="Select theme"
          helperText="Readonly blocks opening and changing"
          readonly
          options={selectOptions}
        />

        <Select
          bind:value={selectFullError}
          id="select-full-error"
          name="themeError"
          label="Error message"
          placeholder="Select theme"
          error="Please select a valid option"
          options={selectOptions}
        />

        <Select
          bind:values={selectFullMultiple}
          id="select-full-multiple"
          name="themeMultiple"
          label="Multiple + helper"
          placeholder="Select multiple options"
          helperText="Multiple values are submitted as hidden inputs"
          options={selectOptions}
          multiple
        />

        <Select
          label="Empty with error"
          placeholder="Select Option"
          options={selectOptions}
          error="Example error message"
        />

        <Select
          value={'monochrome'}
          label="Prefilled with error"
          placeholder="Select Option"
          options={selectOptions}
          error="Example error message"
        />

        <Select
          label="Deselectable with error"
          placeholder="Select Option"
          options={selectOptions}
          allowDeselect
          error="Example error message"
        />
        <Select
          value={'monochrome'}
          label="Prefilled deselectable with error"
          placeholder="Select Option"
          options={selectOptions}
          allowDeselect
          error="Example error message"
        />

        <Select
          label="Disabled with error"
          placeholder="Select Option"
          options={selectOptions}
          disabled
          error="Example error message"
        />

        <Select
          value={'monochrome'}
          label="Prefilled disabled with error"
          placeholder="Select Option"
          options={selectOptions}
          disabled
          error="Example error message"
        />

        <Select
          values={['light-monochrome', 'burnt-orange']}
          label="Multiple with error"
          placeholder="Select Options"
          options={selectOptions}
          multiple
          error="Example error message"
        />
      </div>

      <p class="text-xs text-gray-500 dark:text-gray-400">
        Tip: in Deselectable mode, click the selected option again to clear selection.
      </p>
    </section>

    <section class="space-y-8">
      <h2
        class="border-l-4 border-brand-500 pl-3 text-xl font-semibold text-gray-800 dark:text-white/90"
      >
        Switches
      </h2>

      <div class="space-y-8 rounded-xl p-6">
        <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4 2xl:grid-cols-8">
          <Switch bind:checked={switchDefault} label="Default" />
          <Switch bind:checked={switchChecked} label="Checked" />
          <Switch bind:checked={switchDisabled} label="Disabled" disabled />
          <Switch
            bind:checked={switchFullDefault}
            id="switch-full-required"
            name="notifications"
            value="enabled"
            label="Required + helper"
            helperText="Switch value is submitted via hidden input"
            required
          />
          <Switch
            bind:checked={switchFullReadonly}
            id="switch-full-readonly"
            name="readonlySwitch"
            label="Readonly"
            helperText="Readonly blocks toggling"
            readonly
          />
          <Switch
            bind:checked={switchFullDisabled}
            id="switch-full-disabled"
            name="disabledSwitch"
            label="Disabled"
            helperText="Disabled state"
            disabled
          />
          <Switch
            bind:checked={switchFullError}
            id="switch-full-error"
            name="errorSwitch"
            label="Error message"
            error="This switch must be enabled"
          />
        </div>

        <div class="space-y-3">
          <p class="text-xs font-semibold tracking-wide text-gray-500 uppercase dark:text-gray-400">
            Without label
          </p>
          <div class="grid grid-cols-3 justify-items-start gap-4">
            <Switch checked={false} />
            <Switch checked={true} />
            <Switch checked={true} disabled />
          </div>
        </div>
      </div>
    </section>

    <section class="space-y-8">
      <h2
        class="border-l-4 border-brand-500 pl-3 text-xl font-semibold text-gray-800 dark:text-white/90"
      >
        Progress
      </h2>

      <div
        class="grid grid-cols-1 gap-12 rounded-xl border border-gray-100 p-8 md:grid-cols-2 dark:border-gray-800"
      >
        <div class="space-y-8">
          <div>
            <h3 class="mb-5 text-sm font-bold tracking-widest text-gray-400 uppercase">Variants</h3>
            <div class="space-y-6">
              <Progress value={progressValue} label="Primary (Default)" showValue />
              <Progress value={progressValue} variant="success" label="Success" showValue />
              <Progress value={progressValue} variant="warning" label="Warning" showValue />
              <Progress value={progressValue} variant="danger" label="Danger" showValue />
              <Progress value={progressValue} variant="info" label="Info" showValue />
            </div>
          </div>

          <div>
            <h3 class="mb-5 text-sm font-bold tracking-widest text-gray-400 uppercase">States</h3>
            <div class="space-y-6">
              <Progress indeterminate label="Indeterminate" variant="primary" />
              <Progress value={100} variant="success" label="Completed" showValue />
            </div>
          </div>
        </div>

        <div>
          <h3 class="mb-5 text-sm font-bold tracking-widest text-gray-400 uppercase">Sizes</h3>
          <div class="space-y-8">
            <Progress value={progressValue} size="xs" label="Extra Small (xs)" showValue />
            <Progress value={progressValue} size="sm" label="Small (sm)" showValue />
            <Progress value={progressValue} size="md" label="Medium (md)" showValue />
            <Progress value={progressValue} size="lg" label="Large (lg)" showValue />
            <Progress value={progressValue} size="xl" label="Extra Large (xl)" showValue />
          </div>
        </div>
      </div>
    </section>
  </div>
</div>

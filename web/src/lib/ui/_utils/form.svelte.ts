import { triggerHaptic } from './haptics';
import { errorStore } from '$lib/ui/stores/error.svelte';
import type { AppError, ProblemDetails, Result, ValidationError } from '$lib/core/types/app';

type ValidationErrors<TValues> = Partial<Record<keyof TValues, string>>;

export interface FormConfig<TValues extends Record<string, unknown>, TResponse = unknown> {
  initialValues: TValues;
  validate?: (values: TValues) => ValidationErrors<TValues>;
  onSubmit: (values: TValues) => Promise<Result<TResponse>>;
  onSuccess?: (response: TResponse) => void;
  onError?: (problem: ProblemDetails) => boolean | void;
  mapValidationError?: (err: ValidationError) => ValidationError;
}

export function createForm<TValues extends Record<string, unknown>, TResponse = unknown>(
  config: FormConfig<TValues, TResponse>
) {
  const formState = $state({
    values: { ...config.initialValues },
    serverErrors: {} as ValidationErrors<TValues>,
    isSubmitting: false,
    isSubmitted: false
  });

  const clientErrors = $derived(config.validate ? config.validate(formState.values) : {});
  const errors = $derived({
    ...(formState.isSubmitted ? clientErrors : {}),
    ...formState.serverErrors
  } as ValidationErrors<TValues>);

  let prevValues = { ...formState.values };

  $effect(() => {
    for (const key in formState.values) {
      if (formState.values[key] !== prevValues[key]) {
        delete formState.serverErrors[key as keyof TValues];
        prevValues[key as keyof TValues] = formState.values[key];
      }
    }
  });

  function resetErrors() {
    formState.serverErrors = {};
  }

  function reset(nextValues?: Partial<TValues>) {
    const values = {
      ...config.initialValues,
      ...(nextValues ?? {})
    } as TValues;

    formState.values = values;
    formState.serverErrors = {};
    formState.isSubmitted = false;
    formState.isSubmitting = false;
    prevValues = { ...values };
  }

  function setError(field: keyof TValues, message: string) {
    formState.serverErrors[field] = message;
  }

  function handleValidationErrors(validationErrors: ValidationError[]) {
    resetErrors();
    if (config.mapValidationError) {
      validationErrors = validationErrors.map(config.mapValidationError);
    }
    const generalErrors: AppError[] = [];

    validationErrors.forEach((err) => {
      if (err.propertyName) {
        const fieldKey = Object.keys(formState.values).find(
          (k) => k.toLowerCase() === err.propertyName!.toLowerCase()
        ) as keyof TValues | undefined;

        if (fieldKey) {
          if (!formState.serverErrors[fieldKey]) {
            formState.serverErrors[fieldKey] = err.description;
          } else {
            formState.serverErrors[fieldKey] =
              `${formState.serverErrors[fieldKey]}. ${err.description}`;
          }
        } else {
          generalErrors.push({ code: err.code, description: err.description });
        }
      } else {
        generalErrors.push({ code: err.code, description: err.description });
      }
    });

    if (generalErrors.length > 0) {
      errorStore.addErrors(generalErrors);
    }
  }

  async function handleSubmit(e?: Event) {
    if (e) e.preventDefault();
    if (formState.isSubmitting) return 'error';

    formState.isSubmitted = true;

    if (Object.keys(clientErrors).length > 0) {
      triggerHaptic('error');
      return 'error';
    }

    formState.isSubmitting = true;

    try {
      const response = await config.onSubmit(formState.values);

      if (!response?.ok) {
        triggerHaptic('error');

        const validationErrors = response?.validationProblem?.errors;
        if (validationErrors) {
          handleValidationErrors(validationErrors);
          return 'error';
        }

        const problem = response?.problem;
        if (!problem) {
          errorStore.addError(null, 'Problem with connection to the server');
          return 'error';
        }

        if (!config.onError?.(problem)) {
          errorStore.addError(problem.title ?? null, problem.detail ?? null);
        }
        return 'error';
      }

      triggerHaptic('success');
      config.onSuccess?.(response.value);
      return 'success';
    } catch (err) {
      triggerHaptic('error');
      if (err instanceof Error) {
        if (err.message === 'Failed to fetch') {
          errorStore.addError('Web.ConnectionProblem', 'Problem with connection to the server');
        } else {
          errorStore.addError(err.name, err.message);
        }
      } else {
        errorStore.addError(null, 'Unknown error occurred');
      }
      return 'error';
    } finally {
      formState.isSubmitting = false;
    }
  }

  return {
    get values() {
      return formState.values;
    },
    set values(v) {
      formState.values = v;
    },
    get errors() {
      return errors;
    },
    get isSubmitting() {
      return formState.isSubmitting;
    },
    handleSubmit,
    setError,
    resetErrors,
    reset
  };
}

import { triggerHaptic } from './haptics';
import { errorStore } from '$lib/ui/stores/error';
import type { ValidationProblemDetails, Response, ProblemDetails } from '$lib/core/types/api';

type ValidationErrors<TValues> = Partial<Record<keyof TValues, string>>;

export interface FormConfig<TValues extends Record<string, any>> {
  initialValues: TValues;
  validate?: (values: TValues) => ValidationErrors<TValues>;
  onSubmit: (values: TValues) => Promise<Response<any>>;
  onSuccess?: (response: any) => void;
  onError?: (problem: ProblemDetails) => boolean | void;
}

export function createForm<TValues extends Record<string, any>>(config: FormConfig<TValues>) {
  let formState = $state({
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

  function handleValidationErrors(validationErrors: { propertyName: string | null; code: string; description: string }[]) {
    resetErrors();
    const generalErrors: { code: string | null; description: string | null }[] = [];

    validationErrors.forEach((err) => {
      if (err.propertyName) {
        const fieldKey = Object.keys(formState.values).find(
          (k) => k.toLowerCase() === err.propertyName!.toLowerCase()
        ) as keyof TValues | undefined;

        if (fieldKey) {
          if (!formState.serverErrors[fieldKey]) {
            formState.serverErrors[fieldKey] = err.description;
          } else {
            formState.serverErrors[fieldKey] = `${formState.serverErrors[fieldKey]}. ${err.description}`;
          }
        } else {
          generalErrors.push({ code: err.code, description: err.description });
        }
      } else {
        generalErrors.push({ code: err.code, description: err.description });
      }
    });

    if (generalErrors.length > 0) {
      errorStore.addErrors(generalErrors as any[]);
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

      if (!response || !response.ok) {
        triggerHaptic('error');

        if (response && 'errors' in response && Array.isArray((response as ValidationProblemDetails).errors)) {
          handleValidationErrors((response as ValidationProblemDetails).errors);
        } else if (response && 'title' in response) {
          const problem = response as ProblemDetails;
          const handled = config.onError ? config.onError(problem) : false;

          if (!handled) {
            errorStore.addError(problem.title, problem.detail);
          }
        } else {
          errorStore.addError(null, 'Problem with connection to the server');
        }
        return 'error';
      }

      triggerHaptic('success');
      if (config.onSuccess) {
        config.onSuccess(response);
      }
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
    get values() { return formState.values; },
    set values(v) { formState.values = v; },
    get errors() { return errors; },
    get isSubmitting() { return formState.isSubmitting; },
    handleSubmit,
    setError,
    resetErrors,
    reset
  };
}

import { triggerHaptic } from './haptics';
import { errorStore } from '$lib/ui/stores/error';
import type {
  Response as ApiResponse,
  ProblemDetails as ApiProblemDetails
} from '$lib/core/types/api';
import type {
  Response as HubResponse,
  ValidationError,
  ProblemDetails as HubProblemDetails
} from '$lib/core/types/app';
import type { AppError } from '$lib/core/types/app';

type ValidationErrors<TValues> = Partial<Record<keyof TValues, string>>;
type FormResponse<T = unknown> = ApiResponse<T> | HubResponse<T>;
type ValidationErrorShape = Pick<ValidationError, 'propertyName' | 'code' | 'description'>;

export interface FormConfig<TValues extends Record<string, unknown>, TResponse = unknown> {
  initialValues: TValues;
  validate?: (values: TValues) => ValidationErrors<TValues>;
  onSubmit: (values: TValues) => Promise<FormResponse<TResponse>>;
  onSuccess?: (response: TResponse) => void;
  onError?: (problem: ApiProblemDetails) => boolean | void;
}

function getSuccessPayload<TResponse>(response: FormResponse<TResponse>): TResponse {
  if ('value' in response) {
    return response.value as TResponse;
  }

  const { ok: _ok, ...rest } = response;
  return rest as TResponse;
}

function isValidationErrorArray(value: unknown): value is ValidationErrorShape[] {
  return (
    Array.isArray(value) &&
    value.every(
      (item) =>
        typeof item === 'object' &&
        item !== null &&
        'propertyName' in item &&
        'code' in item &&
        'description' in item
    )
  );
}

function getHubValidationErrors(response: unknown): ValidationErrorShape[] | null {
  if (
    typeof response === 'object' &&
    response !== null &&
    'validationProblem' in response &&
    typeof response.validationProblem === 'object' &&
    response.validationProblem !== null &&
    'errors' in response.validationProblem &&
    isValidationErrorArray(response.validationProblem.errors)
  ) {
    return response.validationProblem.errors;
  }

  return null;
}

function getApiValidationErrors(response: unknown): ValidationErrorShape[] | null {
  if (
    typeof response === 'object' &&
    response !== null &&
    'errors' in response &&
    isValidationErrorArray(response.errors)
  ) {
    return response.errors;
  }

  return null;
}

function getProblemDetails(response: unknown): ApiProblemDetails | null {
  if (typeof response !== 'object' || response === null) {
    return null;
  }

  if ('problem' in response && typeof response.problem === 'object' && response.problem !== null) {
    const problem = response.problem as HubProblemDetails;
    return {
      type: problem.type ?? null,
      title: problem.title ?? null,
      status: problem.status ?? null,
      detail: problem.detail ?? null,
      instance: problem.instance ?? null,
      traceId: ''
    };
  }

  if ('title' in response || 'detail' in response || 'status' in response) {
    const apiProblem = response as Partial<ApiProblemDetails>;
    return {
      type: apiProblem.type ?? null,
      title: apiProblem.title ?? null,
      status: apiProblem.status ?? null,
      detail: apiProblem.detail ?? null,
      instance: apiProblem.instance ?? null,
      traceId: apiProblem.traceId ?? ''
    };
  }

  return null;
}

export function createForm<TValues extends Record<string, unknown>, TResponse = unknown>(
  config: FormConfig<TValues, TResponse>
) {
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

  function handleValidationErrors(validationErrors: ValidationErrorShape[]) {
    resetErrors();
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

      if (!response || !response.ok) {
        triggerHaptic('error');

        const hubValidationErrors = getHubValidationErrors(response);

        if (hubValidationErrors) {
          handleValidationErrors(hubValidationErrors);
        } else {
          const apiValidationErrors = getApiValidationErrors(response);

          if (apiValidationErrors) {
            handleValidationErrors(apiValidationErrors);
            return 'error';
          }

          const problem = getProblemDetails(response);

          if (problem) {
            const handled = config.onError ? config.onError(problem) : false;

            if (!handled) {
              errorStore.addError(problem.title ?? null, problem.detail ?? null);
            }
          } else {
            errorStore.addError(null, 'Problem with connection to the server');
          }
        }
        return 'error';
      }

      triggerHaptic('success');
      if (config.onSuccess) {
        config.onSuccess(getSuccessPayload(response));
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

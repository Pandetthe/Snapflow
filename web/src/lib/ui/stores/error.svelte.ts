import type { AppError } from '$lib/core/types/app';

class ErrorState {
  errors = $state<AppError[]>([]);

  addError(code: string | null, description: string | null) {
    this.errors = [...this.errors, { code, description }];
  }

  addErrors(newErrors: AppError[]) {
    this.errors = [...this.errors, ...newErrors];
  }

  reset() {
    this.errors = [];
  }
}

export const errorStore = new ErrorState();

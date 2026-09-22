import type { ApiClient, ApiEvent } from '$lib/core/types/api';
import logger from '$lib/logger';
import { apiRequestCounter, apiRequestDuration } from '$lib/metrics';

export interface ApiClientOptions {
  /** Read per request, so a base url that changes at runtime is picked up. */
  baseUrl(): string;
  /** Marks the log lines, keeping server requests apart from the browser's. */
  fromServer?: boolean;
  selectFetch?(event: ApiEvent | undefined): typeof fetch;
  prepareHeaders?(headers: Headers, event: ApiEvent | undefined): void;
  onResponse?(response: Response, event: ApiEvent | undefined): void;
}

export function createApiClient(options: ApiClientOptions): ApiClient {
  return {
    async fetch(path, init, event) {
      const start = Date.now();
      const cleanBase = options.baseUrl().replace(/\/+$/, '');
      const cleanPath = (path ?? '').replace(/^\/+/, '');
      const url = `${cleanBase}/${cleanPath}`;

      const headers = new Headers(init.headers ?? {});
      options.prepareHeaders?.(headers, event);

      const fetchFn = options.selectFetch?.(event) ?? fetch;
      const method = init.method ?? 'GET';

      const finalInit: RequestInit = {
        ...init,
        headers,
        credentials: init.credentials ?? 'include'
      };

      try {
        const response = await fetchFn(url, finalInit);
        const duration = Date.now() - start;

        apiRequestCounter.add(1, { method, url: path, status: response.status });
        apiRequestDuration.record(duration, { method, url: path, status: response.status });

        logger.trace(
          {
            method,
            url: path,
            status: response.status,
            duration: `${duration}ms`,
            ...(options.fromServer ? { fromServer: true } : {})
          },
          'API Client Request'
        );

        options.onResponse?.(response, event);

        return response;
      } catch (err) {
        const duration = Date.now() - start;
        logger.error(
          {
            method,
            url: path,
            err,
            duration: `${duration}ms`,
            ...(options.fromServer ? { fromServer: true } : {})
          },
          'API Client Request Failed'
        );
        throw err;
      }
    }
  };
}

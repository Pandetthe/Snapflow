import type { ApiClient } from '$lib/types/api';
import type { RequestEvent, ServerLoadEvent } from '@sveltejs/kit';
import { env } from '$env/dynamic/public';
import logger from '$lib/logger';
import { apiRequestCounter, apiRequestDuration } from '$lib/metrics';

class ClientApiClient implements ApiClient {
  async fetch(path: string | undefined, init: RequestInit, event?: RequestEvent | ServerLoadEvent): Promise<Response> {
    const start = Date.now();
    let base = env.PUBLIC_API_BASE_URL;

    const cleanBase = base.replace(/\/+$/, '');
    const cleanPath = (path ?? '').replace(/^\/+/, '');
    const url = `${cleanBase}/${cleanPath}`;

    const headers = new Headers(init.headers ?? {});

    const finalInit: RequestInit = {
      ...init,
      headers,
      credentials: init.credentials ?? 'include',
    };

    try {
      const response = await fetch(url, finalInit);
      const duration = Date.now() - start;

      apiRequestCounter.add(1, {
        method: init.method ?? 'GET',
        url: path,
        status: response.status
      });

      apiRequestDuration.record(duration, {
        method: init.method ?? 'GET',
        url: path,
        status: response.status
      });

      logger.trace(
        {
          method: init.method ?? 'GET',
          url: path,
          status: response.status,
          duration: `${duration}ms`,
        },
        'API Client Request'
      );

      return response;
    } catch (err) {
      const duration = Date.now() - start;
      logger.error(
        {
          method: init.method ?? 'GET',
          url: path,
          err,
          duration: `${duration}ms`,
        },
        'API Client Request Failed'
      );
      throw err;
    }
  }
}

export const apiClient = new ClientApiClient();

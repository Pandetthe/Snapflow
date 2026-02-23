import { parse } from 'set-cookie-parser';
import type { RequestEvent, ServerLoadEvent } from '@sveltejs/kit';
import { env } from '$env/dynamic/private';
import type { ApiClient } from '$lib/types/api';
import logger from '$lib/logger';
import { apiRequestCounter, apiRequestDuration } from '$lib/metrics';

class ServerApiClient implements ApiClient {
  async fetch(path: string | undefined, init: RequestInit, event?: RequestEvent | ServerLoadEvent): Promise<Response> {
    const start = Date.now();
    const base = env.API_BASE_URL || 'http://localhost:3001';
    const cleanBase = base.replace(/\/+$/, '');
    const cleanPath = (path ?? '').replace(/^\/+/, '');
    const url = `${cleanBase}/${cleanPath}`;

    const headers = new Headers(init.headers ?? {});
    let fetchFn: typeof fetch = fetch;

    if (event) {
      for (const [name, value] of event.request.headers) {
        if (name.toLowerCase() !== 'cookie' || headers.has(name)) continue;
        headers.set(name, value);
      }
      fetchFn = event.fetch;
    }

    const finalInit: RequestInit = {
      ...init,
      headers,
      credentials: init.credentials ?? 'include',
    };

    try {
      const response = await fetchFn(url, finalInit);
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
          fromServer: true,
        },
        'API Client Request'
      );

      if (event) {
        const setCookieHeader = response.headers.getSetCookie();

        if (setCookieHeader) {
          const cookies = parse(setCookieHeader);
          for (const cookie of cookies) {
            event.cookies.set(cookie.name, cookie.value, {
              domain: cookie.domain,
              expires: cookie.expires,
              httpOnly: cookie.httpOnly,
              maxAge: cookie.maxAge,
              path: cookie.path ?? '/',
              sameSite: cookie.sameSite as boolean | 'lax' | 'none' | 'strict' | undefined,
              secure: cookie.secure
            });
          }
        }
      }

      return response;
    } catch (err) {
      const duration = Date.now() - start;
      logger.error(
        {
          method: init.method ?? 'GET',
          url: path,
          err,
          duration: `${duration}ms`,
          fromServer: true,
        },
        'API Client Request Failed'
      );
      throw err;
    }
  }
}

export const apiClient = new ServerApiClient();

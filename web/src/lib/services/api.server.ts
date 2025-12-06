import { parse } from 'set-cookie-parser';
import type { RequestEvent, ServerLoadEvent } from '@sveltejs/kit';
import { env } from '$env/dynamic/private';
import type { ApiClient } from '$lib/types/api';

class ServerApiClient implements ApiClient {
  async fetch(path: string | undefined, init: RequestInit, event?: RequestEvent | ServerLoadEvent): Promise<Response> {
    const base = env.API_BASE_URL;
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

    const response = await fetchFn(url, finalInit);

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
  }
}

export const apiClient = new ServerApiClient();

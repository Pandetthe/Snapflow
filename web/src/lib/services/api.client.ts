import type { ApiClient } from '$lib/types/api';
import type { RequestEvent, ServerLoadEvent } from '@sveltejs/kit';
import { env } from '$env/dynamic/public';

class ClientApiClient implements ApiClient {
  async fetch(path: string | undefined, init: RequestInit, event?: RequestEvent | ServerLoadEvent): Promise<Response> {
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

    const response = await fetch(url, finalInit);

    return response;
  }
}

export const apiClient = new ClientApiClient();

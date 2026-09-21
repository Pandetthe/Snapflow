import type { RequestEvent, ServerLoadEvent } from '@sveltejs/kit';

export interface ApiClient {
  fetch(path: string | undefined, init: RequestInit, event?: ApiEvent): Promise<Response>;
}

export type ApiEvent = RequestEvent | ServerLoadEvent;

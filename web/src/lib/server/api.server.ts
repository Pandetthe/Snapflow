import { parse } from 'set-cookie-parser';
import { env } from '$env/dynamic/private';
import { createApiClient } from '$lib/core/createApiClient';

export const apiClient = createApiClient({
  baseUrl: () => env.API_BASE_URL || 'http://localhost:3001',
  fromServer: true,
  selectFetch: (event) => event?.fetch ?? fetch,
  prepareHeaders: (headers, event) => {
    if (!event) return;
    for (const [name, value] of event.request.headers) {
      if (name.toLowerCase() !== 'cookie' || headers.has(name)) continue;
      headers.set(name, value);
    }
  },
  onResponse: (response, event) => {
    if (!event) return;
    const setCookieHeader = response.headers.getSetCookie();
    if (!setCookieHeader) return;

    for (const cookie of parse(setCookieHeader)) {
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
});

import { env } from '$env/dynamic/public';
import { createApiClient } from '$lib/core/createApiClient';

export const apiClient = createApiClient({
  baseUrl: () => env.PUBLIC_API_BASE_URL || ''
});

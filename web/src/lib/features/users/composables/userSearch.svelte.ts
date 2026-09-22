import { apiClient } from '$lib/core/api.client';
import { UsersService, type SearchUserDto } from '$lib/features/users/api/users';

const DEBOUNCE_MS = 250;
const MIN_QUERY_LENGTH = 2;

export interface UserSearchOptions {
  /** Ids the server should leave out of the results. */
  excludedIds(): number[];
  /** Drops anyone already picked between the request going out and the answer coming back. */
  isExcluded(userId: number): boolean;
}

/**
 * Searches users as the query is typed. A request that returns after a newer one was sent is
 * discarded, so results never arrive out of order.
 */
export function createUserSearch(options: UserSearchOptions) {
  const usersService = new UsersService(apiClient);

  let query = $state('');
  let results = $state<SearchUserDto[]>([]);
  let error = $state('');
  let isSearching = $state(false);
  let requestId = 0;

  $effect(() => {
    const trimmed = query.trim();
    error = '';

    if (trimmed.length < MIN_QUERY_LENGTH) {
      results = [];
      isSearching = false;
      return;
    }

    isSearching = true;

    const timeoutId = setTimeout(async () => {
      const id = ++requestId;
      const result = await usersService.searchUsers(trimmed, options.excludedIds());

      if (id !== requestId) return;

      isSearching = false;

      if (result.ok) {
        results = result.value.filter((user) => !options.isExcluded(user.id));
        return;
      }

      results = [];
      error = result.problem?.detail ?? result.problem?.title ?? 'Failed to search users';
    }, DEBOUNCE_MS);

    return () => clearTimeout(timeoutId);
  });

  return {
    get query() {
      return query;
    },
    set query(value: string) {
      query = value;
    },
    get results() {
      return results;
    },
    get error() {
      return error;
    },
    get isSearching() {
      return isSearching;
    },
    clear() {
      query = '';
      results = [];
      error = '';
    }
  };
}

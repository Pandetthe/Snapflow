namespace Snapflow.Application.Users.Search;

public static class SearchUsersResponse
{
    public sealed record UserDto(int Id, string UserName, string AvatarUrl);
}

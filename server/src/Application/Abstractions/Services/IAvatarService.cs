namespace Snapflow.Application.Abstractions.Services;

public interface IAvatarService
{ 
    string GenerateAvatarUrl(int userId);

    string GetGravatarUrl(string email, int size = 200);

    string GenerateJdenticonSvg(int userId, int size = 200);
}

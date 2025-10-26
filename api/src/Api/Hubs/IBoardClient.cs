namespace Snapflow.Api.Hubs;

public interface IBoardClient
{
    Task BoardUpdated();

    Task BoardDeleted();

    Task SwimlaneCreated();

    Task SwimlaneUpdated();

    Task SwimlaneDeleted();

    Task ListCreated();

    Task ListUpdated();

    Task ListDeleted();

    Task TagCreated();

    Task TagUpdated();

    Task TagDeleted();

    Task CardLocked();

    Task CardUnlocked();

    Task CardCreated();

    Task CardUpdated();

    Task CardDeleted();

    Task CardMoved();
}
using Snapflow.Common;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Members;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Tags;
using Snapflow.Domain.Users;

namespace Snapflow.Domain.Boards;

public class Board : Entity<int, Board>, ISoftDeletable
{
    private readonly List<Member> _members = [];
    private readonly List<Swimlane> _swimlanes = [];
    private readonly List<List> _lists = [];
    private readonly List<Card> _cards = [];
    private readonly List<Tag> _tags = [];

    private Board() { }

    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = "";
    public BoardVisibility Visibility { get; private set; } = BoardVisibility.Private;

    public DateTimeOffset CreatedAt { get; private set; }
    public int CreatedById { get; private set; }
    public virtual IUser CreatedBy { get; private set; } = null!;

    public DateTimeOffset? UpdatedAt { get; private set; }
    public int? UpdatedById { get; private set; }
    public virtual IUser? UpdatedBy { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }
    public int? DeletedById { get; private set; }
    public virtual IUser? DeletedBy { get; private set; }
    public bool IsDeleted { get; private set; }

    public virtual IReadOnlyCollection<Member> Members => _members;
    public virtual IReadOnlyCollection<Swimlane> Swimlanes => _swimlanes;
    public virtual IReadOnlyCollection<List> Lists => _lists;
    public virtual IReadOnlyCollection<Card> Cards => _cards;
    public virtual IReadOnlyCollection<Tag> Tags => _tags;

    public static Board Create(string title, string description, BoardVisibility visibility, int createdById, DateTimeOffset createdAt, string? connectionId = null)
    {
        var board = new Board
        {
            Title = title,
            Description = description,
            Visibility = visibility,
            CreatedById = createdById,
            CreatedAt = createdAt
        };

        board._members.Add(Member.Create(board, createdById, MemberRole.Owner, connectionId));

        board.Raise(b => new BoardCreatedDomainEvent(b.Id, b.Title, b.CreatedById, connectionId));

        return board;
    }

    /// <summary>Changes the board. Returns false, leaving it untouched, when it already reads that way.</summary>
    public bool Update(string title, string description, int updatedById, DateTimeOffset updatedAt, string? connectionId = null)
    {
        if (Title == title && Description == description)
            return false;

        Title = title;
        Description = description;
        UpdatedById = updatedById;
        UpdatedAt = updatedAt;

        Raise(b => new BoardUpdatedDomainEvent(b.Id, b.Title, b.Description, connectionId));
        return true;
    }

    public void ChangeVisibility(BoardVisibility visibility, int updatedById, DateTimeOffset updatedAt, string? connectionId = null)
    {
        if (Visibility == visibility)
            return;

        var oldVisibility = Visibility;
        Visibility = visibility;
        UpdatedById = updatedById;
        UpdatedAt = updatedAt;

        Raise(b => new BoardVisibilityChangedDomainEvent(b.Id, oldVisibility, b.Visibility, connectionId));
    }

    public void SoftDelete(int deletedById, DateTimeOffset deletedAt, string? connectionId = null)
    {
        IsDeleted = true;
        DeletedById = deletedById;
        DeletedAt = deletedAt;

        var memberIds = _members.Select(m => m.UserId).ToList();
        Raise(b => new BoardDeletedDomainEvent(b.Id, memberIds, connectionId));
    }

    public Result<Tag> CreateTag(string title, TagColors color, int createdById, DateTimeOffset createdAt, string? connectionId = null)
    {
        if (IsTagTitleTaken(title, except: null))
            return TagErrors.TitleNotUnique(title);

        var tag = Tag.Create(this, title, color, createdById, createdAt, connectionId);
        _tags.Add(tag);
        return tag;
    }

    public Result<bool> UpdateTag(int tagId, string title, TagColors color, int updatedById, DateTimeOffset updatedAt, string? connectionId = null)
    {
        Tag? tag = FindTag(tagId);
        if (tag is null)
            return TagErrors.NotFound(tagId);
        if (IsTagTitleTaken(title, except: tag))
            return TagErrors.TitleNotUnique(title);

        return tag.Update(title, color, updatedById, updatedAt, connectionId);
    }

    public Result DeleteTag(int tagId, int deletedById, DateTimeOffset deletedAt, string? connectionId = null)
    {
        Tag? tag = FindTag(tagId);
        if (tag is null)
            return TagErrors.NotFound(tagId);

        tag.SoftDelete(deletedById, deletedAt, connectionId);
        return Result.Success();
    }

    private Tag? FindTag(int tagId) =>
        _tags.FirstOrDefault(t => t.Id == tagId && !t.IsDeleted);

    private bool IsTagTitleTaken(string title, Tag? except) =>
        _tags.Any(t => !t.IsDeleted && !ReferenceEquals(t, except) && t.Title == title);

    public Result AddMember(int userId, MemberRole role, string? connectionId = null)
    {
        if (role == MemberRole.Owner)
            return MemberErrors.CannotAssignOwner;
        if (_members.Any(m => m.UserId == userId))
            return MemberErrors.AlreadyMember(userId, Id);

        _members.Add(Member.Create(this, userId, role, connectionId));
        return Result.Success();
    }

    public Result ChangeMemberRole(int userId, MemberRole role, string? connectionId = null)
    {
        Member? member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member is null)
            return MemberErrors.NotFound(userId, Id);
        if (member.Role == MemberRole.Owner)
            return MemberErrors.CannotChangeOwnerRole;
        if (role == MemberRole.Owner)
            return MemberErrors.CannotAssignOwner;

        member.ChangeRole(role, connectionId);
        return Result.Success();
    }

    public Result RemoveMember(int userId, string? connectionId = null)
    {
        Member? member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member is null)
            return MemberErrors.NotFound(userId, Id);
        if (member.Role == MemberRole.Owner)
            return MemberErrors.CannotRemoveOwner;

        member.MarkRemoved(connectionId);
        _members.Remove(member);
        return Result.Success();
    }

    public bool IsOwnedBy(int userId) =>
        _members.Any(m => m.UserId == userId && m.Role == MemberRole.Owner);

    public Result HandOverOwnership(int successorUserId, string? connectionId = null)
    {
        Member owner = _members.Single(m => m.Role == MemberRole.Owner);
        if (owner.UserId == successorUserId)
            return MemberErrors.AlreadyOwner(successorUserId, Id);
        if (_members.All(m => m.UserId != successorUserId))
            return MemberErrors.NotFound(successorUserId, Id);

        owner.ChangeRole(MemberRole.Admin, connectionId);
        return Result.Success();
    }

    public Result TakeOwnership(int userId, string? connectionId = null)
    {
        if (_members.Any(m => m.Role == MemberRole.Owner))
            return MemberErrors.OwnerAlreadyExists(Id);

        Member? member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member is null)
            return MemberErrors.NotFound(userId, Id);

        member.ChangeRole(MemberRole.Owner, connectionId);
        return Result.Success();
    }

    public Result SyncMembers(IReadOnlyList<(int UserId, MemberRole Role)> requested, string? connectionId = null)
    {
        if (requested.Select(r => r.UserId).Distinct().Count() != requested.Count)
            return MemberErrors.DuplicateMember;

        var requestedIds = requested.Select(r => r.UserId).ToHashSet();

        foreach (Member member in _members.Where(m => m.Role != MemberRole.Owner && !requestedIds.Contains(m.UserId)).ToList())
        {
            member.MarkRemoved(connectionId);
            _members.Remove(member);
        }

        foreach ((int userId, MemberRole role) in requested.Where(r => r.Role != MemberRole.Owner))
        {
            Member? existing = _members.FirstOrDefault(m => m.UserId == userId);
            if (existing is null)
                _members.Add(Member.Create(this, userId, role, connectionId));
            else if (existing.Role != MemberRole.Owner)
                existing.ChangeRole(role, connectionId);
        }

        return Result.Success();
    }
}

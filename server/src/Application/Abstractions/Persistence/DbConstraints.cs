namespace Snapflow.Application.Abstractions.Persistence;

public static class DbConstraints
{
    public const string BoardMemberKey = "pk_board_members";

    public const string BoardSingleOwner = "ix_board_members_board_id_role";

    public const string CardTagKey = "pk_card_tag";

    public const string TagTitle = "ix_tags_board_id_title";
}

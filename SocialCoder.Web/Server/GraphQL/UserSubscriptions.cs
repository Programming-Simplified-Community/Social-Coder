using SocialCoder.Web.Shared.Models.Account;

namespace SocialCoder.Web.Server.GraphQL;

public class UserSubscriptions
{
    [Subscribe, Topic(nameof(UserUpdated))]
    public UserAccountItem UserUpdated([EventMessage] UserAccountItem user) => user;

    [Subscribe, Topic(nameof(UserDeleted))]
    public UserAccountItem UserDeleted([EventMessage] UserAccountItem user) => user;

    [Subscribe, Topic(nameof(UserBanned))]
    public UserAccountItem UserBanned([EventMessage] UserAccountItem user) => user;
}
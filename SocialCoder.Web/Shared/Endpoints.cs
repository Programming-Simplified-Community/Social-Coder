namespace SocialCoder.Web.Shared;

public static class Endpoints
{
    private const string API_BASE = "/api";

    public const string GraphQl = "/graphql";

    #region Auth Endpoints

    private const string AUTH_BASE = $"{API_BASE}/Auth";
    public const string AuthPostLogout = $"{AUTH_BASE}/Logout";

    /// <summary>
    /// String format.
    /// <list type="number">
    ///     <item>Scheme to use</item>
    /// </list>
    /// </summary>
    public const string AuthGetChallenge = $"{AUTH_BASE}/Challenge/{{0}}";

    public const string AuthGetUserInfo = $"{AUTH_BASE}/UserInfo";

    public const string AuthGetProviders = $"{AUTH_BASE}/Providers";

    #endregion

    #region Gamify Controller

    private const string GAMIFY_BASE = $"{API_BASE}/Gamify";

    /// <summary>
    /// String format.
    /// <list type="number">
    ///     <item>User Id</item>
    /// </list>
    /// </summary>
    public const string GamifyGetUserBadges = $"{GAMIFY_BASE}/{{0}}/badges";

    /// <summary>
    /// String format
    /// <list type="number">
    ///     <item>User Id</item>
    /// </list>
    /// </summary>
    public const string GamifyGetUserQuests = $"{GAMIFY_BASE}/{{0}}/quests";

    #endregion

    #region Code Jam

    private const string CODE_JAM_BASE = $"{API_BASE}/CodeJam";
    public const string CodeJamPostTopics = $"{CODE_JAM_BASE}/Topics";

    /// <summary>
    /// String format
    /// <list type="number">
    ///     <item>Topic Id</item>
    /// </list>
    /// </summary>
    public const string CodeJamPostGetTopic = $"{CODE_JAM_BASE}/{{0}}";

    /// <summary>
    /// String format
    /// <list type="number">
    ///     <item>Topic Id</item>
    /// </list>
    /// </summary>
    public const string CodejamDeleteTopic = $"{CODE_JAM_BASE}/admin/topics/{{0}}";

    /// <summary>
    /// String format
    /// <list type="number">
    ///     <item>Topic Id</item>
    /// </list>
    /// </summary>
    public const string CodeJamPostTopicRegister = $"{CodeJamPostTopics}/{{0}}/register";

    /// <summary>
    /// String format
    /// <list type="number">
    ///     <item>Topic Id</item>
    /// </list>
    /// </summary>
    public const string CodeJamPostTopicWithdraw = $"{CodeJamPostTopics}/{{0}}/withdraw";


    /// <summary>
    /// String format
    /// <list type="number">
    ///     <item>Topic Id</item>
    /// </list>
    /// </summary>
    public const string CodeJamPutTopic = $"{CODE_JAM_BASE}/admin/topics/{{0}}";

    /// <summary>
    /// String format
    /// <list type="number">
    ///     <item>Topic Id</item>
    /// </list>
    /// </summary>
    public const string CodeJamDeleteTopic = $"{CODE_JAM_BASE}/admin/topics/{{0}}";

    public const string CodeJamPostCreateTopic = $"{CODE_JAM_BASE}/admin/topics/create";

    #endregion
}
﻿namespace SocialCoder.Web.Shared;

public static class Endpoints
{
    private const string API_BASE = "/api";

    public const string GRAPH_QL = "/graphql";
    
    #region Auth Endpoints

    private const string AUTH_BASE = $"{API_BASE}/Auth";
    public const string AUTH_POST_LOGOUT = $"{AUTH_BASE}/Logout";
    
    /// <summary>
    /// String format.
    /// <list type="number">
    ///     <item>Scheme to use</item>
    /// </list>
    /// </summary>
    public const string AUTH_GET_CHALLENGE = $"{AUTH_BASE}/Challenge/{{0}}";

    public const string AUTH_GET_USER_INFO = $"{AUTH_BASE}/UserInfo";

    public const string AUTH_GET_PROVIDERS = $"{AUTH_BASE}/Providers";

    #endregion
    
    #region Gamify Controller

    private const string GAMIFY_BASE = $"{API_BASE}/Gamify";
    
    /// <summary>
    /// String format.
    /// <list type="number">
    ///     <item>User Id</item>
    /// </list>
    /// </summary>
    public const string GAMIFY_GET_USER_BADGES = $"{GAMIFY_BASE}/{{0}}/badges";

    /// <summary>
    /// String format
    /// <list type="number">
    ///     <item>User Id</item>
    /// </list>
    /// </summary>
    public const string GAMIFY_GET_USER_QUESTS = $"{GAMIFY_BASE}/{{0}}/quests";

    #endregion
    
    #region Code Jam

    private const string CODE_JAM_BASE = $"{API_BASE}/CodeJam";
    public const string CODE_JAM_POST_TOPICS = $"{CODE_JAM_BASE}/Topics";
    
    /// <summary>
    /// String format
    /// <list type="number">
    ///     <item>Topic Id</item>
    /// </list>
    /// </summary>
    public const string CODE_JAM_POST_GET_TOPIC = $"{CODE_JAM_BASE}/{{0}}";

    /// <summary>
    /// String format
    /// <list type="number">
    ///     <item>Topic Id</item>
    /// </list>
    /// </summary>
    public const string CODEJAM_DELETE_TOPIC = $"{CODE_JAM_BASE}/admin/topics/{{0}}";
    
    /// <summary>
    /// String format
    /// <list type="number">
    ///     <item>Topic Id</item>
    /// </list>
    /// </summary>
    public const string CODE_JAM_POST_TOPIC_REGISTER = $"{CODE_JAM_POST_TOPICS}/{{0}}/register";

    /// <summary>
    /// String format
    /// <list type="number">
    ///     <item>Topic Id</item>
    /// </list>
    /// </summary>
    public const string CODE_JAM_POST_TOPIC_WITHDRAW = $"{CODE_JAM_POST_TOPICS}/{{0}}/withdraw";


    /// <summary>
    /// String format
    /// <list type="number">
    ///     <item>Topic Id</item>
    /// </list>
    /// </summary>
    public const string CODE_JAM_PUT_TOPIC = $"{CODE_JAM_BASE}/admin/topics/{{0}}";
    
    /// <summary>
    /// String format
    /// <list type="number">
    ///     <item>Topic Id</item>
    /// </list>
    /// </summary>
    public const string CODE_JAM_DELETE_TOPIC = $"{CODE_JAM_BASE}/admin/topics/{{0}}";

    public const string CODE_JAM_POST_CREATE_TOPIC = $"{CODE_JAM_BASE}/admin/topics/create";

    #endregion
}
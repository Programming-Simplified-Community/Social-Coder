namespace SocialCoder.Web.Shared.Extensions;

public static class StringFormatExtensions
{
    /// <summary>
    /// Uses a standard for-loop, iterating over <see cref="variables"/> -- replacing each index from <paramref name="text"/>.
    ///
    /// <para>
    ///     Expects values to be in the format of  %INDEX%
    /// </para>
    /// </summary>
    /// <example>
    ///     <para>This is a %0% that should get %1% replaced</para>
    ///     <para>text.Replace(123, "something")</para>
    ///     <para>This is a 123 that should get something replaced</para>
    /// </example>
    /// <param name="text"></param>
    /// <param name="variables"></param>
    /// <returns></returns>
    public static string Format(this string text, params object[] variables)
    {
        for (var i = 0; i < variables.Length; i++)
        {
            text = text.Replace($"%{i}%", variables[i]?.ToString());
        }

        return text;
    }
}
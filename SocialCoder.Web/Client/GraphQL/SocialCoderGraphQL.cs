using System.Net.Http.Json;
using SocialCoder.Web.Shared;
using SocialCoder.Web.Shared.Extensions;
using SocialCoder.Web.Shared.Requests;

namespace SocialCoder.Web.Client.GraphQL;

public class SocialCoderGraphQL
{
    private readonly HttpClient _client;
    private readonly ILogger<SocialCoderGraphQL> _logger;

    public SocialCoderGraphQL(HttpClient client, ILogger<SocialCoderGraphQL> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<ResultOf<IPagedResponse<CodeJamTopic_UserView>>> FetchTopicCards(PaginationRequest request, string userId)
    {
        var requestItem = new {
            query = QUERIES.GET_CARD_TOPICS_USER_VIEW.Format( 
                request.PageSize,
                request.PageNumber * request.PageSize, 
                userId)
        };
        
        var response = await _client.PostAsJsonAsync(Endpoints.GRAPH_QL, requestItem);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Unable to process. {StatusCode} {Reason}", response.StatusCode, response.ReasonPhrase);
            return ResultOf<IPagedResponse<CodeJamTopic_UserView>>.Fail("Unable to do thing");
        }

        _logger.LogInformation(await response.Content.ReadAsStringAsync());
        return ResultOf<IPagedResponse<CodeJamTopic_UserView>>.Pass(null!);
    }
}
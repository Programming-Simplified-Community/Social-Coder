using SocialCoder.Web.Shared.Extensions;
using SocialCoder.Web.Shared.Requests;

namespace SocialCoder.Web.Client.GraphQL;

public static class QUERIES
{
  /// <summary>
  /// String format
  /// <list type="number">
  ///   <item>Number of items to take</item>
  ///   <item>Number of items to skip</item>
  ///   <item>User Id to search for -- isRegistered</item>
  /// </list>
  /// </summary>
  /// <remarks>
  ///   Because string formatting utilizes the curly brackets { }  -- we needed
  ///   an alternative for inserting variables.
  ///
  ///   So any variable that needs to be inserted (by index) will be
  ///
  ///   %indexNumber%
  ///
  ///   <see cref="StringFormatExtensions.Format"/>
  /// </remarks>
  public static string GET_CARD_TOPICS_USER_VIEW = 
@"query {  
  topics(take: %0%, skip: %1%) {
    
    pageInfo {
      hasNextPage,
      hasPreviousPage
    }
     
    items {        
       isRegistered(userId: ""%2%""),
       topicId,
       teamApplicants,
       soloApplicants,
       totalApplicants,
       title,
       description,
       jamStartDate,
       jamEndDate,
       registrationStartDate,
       backgroundImageUrl
    }
  }
}";
}
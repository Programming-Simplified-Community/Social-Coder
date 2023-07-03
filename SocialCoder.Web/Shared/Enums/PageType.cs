using SocialCoder.Web.Shared.Attributes;

namespace SocialCoder.Web.Shared.Enums;

public enum PageType
{
    [Image("../img/banner/events-icon.png")]
    Events,
    
    [Image("../img/banner/forums-icon.png")]
    Forums,
    
    [Image("../img/banner/groups-icon.png")]
    Groups,
    
    [Image("../img/banner/marketplace-icon.png")]
    Marketplace,
    
    [Image("../img/banner/members-icon.png")]
    Members,
    
    [Image("../img/banner/newsfeed-icon.png")]
    Newsfeed,
    
    [Image("../img/banner/overview-icon.png")]
    Overview,
    
    [Image("../img/banner/quests-icon.png")]
    Quests,
    
    [Image("../img/banner/streams-icon.png")]
    Streams,
    
    [Image("../img/banner/accounthub-icon.png")]
    AccountHub
}
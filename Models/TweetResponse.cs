using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DraftAdmin.Models
{
    public class TweetResponse
    {
        [JsonProperty(PropertyName = "coordinates")]
        public string coordinates { get; set; }
        [JsonProperty(PropertyName = "created_at")]
        public string createdAt { get; set; }
        [JsonProperty(PropertyName = "id_str")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "text")]
        public string tweetText { get; set; }
        public User user { get; set; }
        [JsonProperty(PropertyName = "favorited")]
        public string favorited { get; set; }
        [JsonProperty(PropertyName = "filter_level")]
        public string filterLevel { get; set; }
        [JsonProperty(PropertyName = "in_reply_to_screen_name")]
        public string inReplyToScreenName { get; set; }
        [JsonProperty(PropertyName = "in_reply_to_status_id_str")]
        public string inReplyToStatusIdStr { get; set; }
        [JsonProperty(PropertyName = "in_reply_to_user_id")]
        public string inReplyToUserId { get; set; }
        [JsonProperty(PropertyName = "place")]
        public string place { get; set; }
        [JsonProperty(PropertyName = "possible_sensitive")]
        public bool possibleSensitive { get; set; }
        [JsonProperty(PropertyName = "retweet_count")]
        public int retweetCount { get; set; }
        [JsonProperty(PropertyName = "retweeted")]
        public bool retweeted { get; set; }
        [JsonProperty(PropertyName = "source")]
        public string source { get; set; }
        [JsonProperty(PropertyName = "lang")]
        public string lang { get; set; }
        [JsonProperty(PropertyName = "truncated")]
        public string truncated { get; set; }
        [JsonProperty(PropertyName = "withheld_copywrite")]
        public string withheldCopywrite { get; set; }
        [JsonProperty(PropertyName = "withheld_in_countries")]
        public string withheldInCountries { get; set; }
        [JsonProperty(PropertyName = "withheld_scope")]
        public string withheldScope { get; set; }
        public Entities entities { get; set; }

    }

    public class User
    {
        [JsonProperty(PropertyName = "profile_sidebar_fill_color")]
        public string profileSidebarFillColor { get; set; }
        [JsonProperty(PropertyName = "profile_background_tile")]
        public string profileBackgroundTile { get; set; }
        [JsonProperty(PropertyName = "profile_sidebar_border_color")]
        public string profileSidebarBorderColor { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "profile_image_url")]
        public string profileImageUrl { get; set; }
        [JsonProperty(PropertyName = "location")]
        public string Location { get; set; }
        [JsonProperty(PropertyName = "created_at")]
        public string createdAt { get; set; }
        [JsonProperty(PropertyName = "follow_request_sent")]
        public string followRequestSent { get; set; }
        [JsonProperty(PropertyName = "is_translator")]
        public string isTranslator { get; set; }
        [JsonProperty(PropertyName = "id_str")]
        public string idStr { get; set; }
        [JsonProperty(PropertyName = "profile_link_color")]
        public string profileLinkColor { get; set; }
        [JsonProperty(PropertyName = "screen_name")]
        public string screenName { get; set; }
        public UserEntities entities { get; set; }
    }

    public class UserEntities
    {
        public List<Urls> urls { get; set; }
        //[JsonProperty(PropertyName = "description")]
        //public string description { get; set; }
    }

    public class Entities
    {
        public List<Urls> urls { get; set; }
        public List<Hashtags> hashtags { get; set; }
        public UserMentions userMentions { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string description { get; set; }
        public List<Media> media { get; set; }
    }

    public class Urls
    {
        [JsonProperty(PropertyName = "url")]
        public string url { get; set; }
        [JsonProperty(PropertyName = "expanded_url")]
        public string expandedUrl { get; set; }
        [JsonProperty(PropertyName = "display_url")]
        public string displayUrl { get; set; }
        [JsonProperty(PropertyName = "indices")]
        public JArray indices { get; set; }
    }

    public class Hashtags
    {
        [JsonProperty(PropertyName = "text")]
        public string text { get; set; }
        [JsonProperty(PropertyName = "indices")]
        public JArray indices { get; set; }
    }

    public class UserMentions
    {
        [JsonProperty(PropertyName = "screen_name")]
        public string screenName { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "id_str")]
        public string idStr { get; set; }
        [JsonProperty(PropertyName = "indices")]
        public JArray indices { get; set; }
    }

    public class Media
    {
        [JsonProperty(PropertyName = "id_str")]
        public string IdStr { get; set; }
        [JsonProperty(PropertyName = "indices")]
        public JArray indices { get; set; }
        [JsonProperty(PropertyName = "media_url")]
        public string mediaUrl { get; set; }
        [JsonProperty(PropertyName = "media_url_https")]
        public string mediaUrlHttps { get; set; }
        [JsonProperty(PropertyName = "url")]
        public string url { get; set; }
        [JsonProperty(PropertyName = "display_url")]
        public string displayUrl { get; set; }
        [JsonProperty(PropertyName = "expanded_url")]
        public string expandedUrl { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string type { get; set; }
        //[JsonProperty(PropertyName = "sizes")]
        public Sizes sizes { get; set; }
    }

    public class Sizes
    {
        public Thumb thumb { get; set; }
        public Small small { get; set; }
        public Medium medium { get; set; }
        public Large large { get; set; }
    }

    public class Thumb
    {
        [JsonProperty(PropertyName = "w")]
        public string width { get; set; }
        [JsonProperty(PropertyName = "h")]
        public string height { get; set; }
        [JsonProperty(PropertyName = "resize")]
        public string resize { get; set; }
    }

    public class Small
    {
        [JsonProperty(PropertyName = "w")]
        public string width { get; set; }
        [JsonProperty(PropertyName = "h")]
        public string height { get; set; }
        [JsonProperty(PropertyName = "resize")]
        public string resize { get; set; }
    }

    public class Medium
    {
        [JsonProperty(PropertyName = "w")]
        public string width { get; set; }
        [JsonProperty(PropertyName = "h")]
        public string height { get; set; }
        [JsonProperty(PropertyName = "resize")]
        public string resize { get; set; }
    }

    public class Large
    {
        [JsonProperty(PropertyName = "w")]
        public string width { get; set; }
        [JsonProperty(PropertyName = "h")]
        public string height { get; set; }
        [JsonProperty(PropertyName = "resize")]
        public string resize { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DraftAdmin.Models
{
    public class Tweet : ModelBase
    {
        //public Tweet();

        public string CreatedAt { get; set; }
        public string ScreenName { get; set; }
        public string Text { get; set; }
        public string TweetID { get; set; }
        public string TweetType { get; set; }
        public string User { get; set; }
        public string UserImage { get; set; }
    }
}

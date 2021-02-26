using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SleepyTimeSoaps.Models
{
    public class BlogsModel
    {
        public List<Blog> _BlogPosts = new List<Blog>();
        public List<Blog> BlogPosts { get { return _BlogPosts; } }
    }
}
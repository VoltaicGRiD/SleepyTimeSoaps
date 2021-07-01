using System.Collections.Generic;

namespace SleepyTimeSoaps.CustomAuthentication
{
    public class CustomSerializeModel
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> RoleName
        {
            get; set;
        }
    }
}
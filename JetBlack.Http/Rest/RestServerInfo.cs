using System.Collections.Generic;

namespace JetBlack.Http.Rest
{
    public class RestServerInfo
    {
        public Dictionary<string, object?> Data { get; }

        public RestServerInfo()
        {
            Data = new Dictionary<string, object?>();
        }
    }
}
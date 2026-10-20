using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Cyber
{
    /// <summary>
    /// 不使用Result返回方式
    /// </summary>
    public class NoResultModelAttribute : Attribute
    { 
        
    }
    public class CyberErrorResponse
    {
        public Cyber.ResultCode Code { get; set; }
        public string? Error { get; set; }
        public long? Timestamp { get; set; }
    }

    public class CyberErrorsResponse
    {
        public Cyber.ResultCode Code { get; set; }
        public string[]? Error { get; set; }
        public long? Timestamp { get; set; }
    }

}

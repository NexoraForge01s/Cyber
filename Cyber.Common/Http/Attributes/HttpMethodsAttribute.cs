using System;

namespace Cyber.Http.Attributes
{
    /// <summary>
    /// HTTP GET 方法特性
    /// </summary>
    public class HttpGetAttribute : HttpMethodAttribute
    {
        public override string Method => "GET";

        public HttpGetAttribute(string path = "") : base(path)
        {
        }
    }

    /// <summary>
    /// HTTP POST 方法特性
    /// </summary>
    public class HttpPostAttribute : HttpMethodAttribute
    {
        public override string Method => "POST";

        public HttpPostAttribute(string path = "") : base(path)
        {
        }
    }

    /// <summary>
    /// HTTP PUT 方法特性
    /// </summary>
    public class HttpPutAttribute : HttpMethodAttribute
    {
        public override string Method => "PUT";

        public HttpPutAttribute(string path = "") : base(path)
        {
        }
    }

    /// <summary>
    /// HTTP DELETE 方法特性
    /// </summary>
    public class HttpDeleteAttribute : HttpMethodAttribute
    {
        public override string Method => "DELETE";

        public HttpDeleteAttribute(string path = "") : base(path)
        {
        }
    }

    /// <summary>
    /// HTTP PATCH 方法特性
    /// </summary>
    public class HttpPatchAttribute : HttpMethodAttribute
    {
        public override string Method => "PATCH";

        public HttpPatchAttribute(string path = "") : base(path)
        {
        }
    }

    /// <summary>
    /// HTTP HEAD 方法特性
    /// </summary>
    public class HttpHeadAttribute : HttpMethodAttribute
    {
        public override string Method => "HEAD";

        public HttpHeadAttribute(string path = "") : base(path)
        {
        }
    }

    /// <summary>
    /// HTTP OPTIONS 方法特性
    /// </summary>
    public class HttpOptionsAttribute : HttpMethodAttribute
    {
        public override string Method => "OPTIONS";

        public HttpOptionsAttribute(string path = "") : base(path)
        {
        }
    }
}
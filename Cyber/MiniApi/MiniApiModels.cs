using System.Collections.Generic;

namespace Cyber.AOT.MiniApi
{
    /// <summary>
    /// 实体属性信息（用于从查询字符串组装实体对象）
    /// </summary>
    internal class PropertyInfo
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public bool IsSimpleType { get; set; }
        /// <summary>
        /// 是否为数组类型（List、IEnumerable、数组等）
        /// </summary>
        public bool IsArrayType { get; set; }
        /// <summary>
        /// 数组元素类型（仅当 IsArrayType 为 true 时有效）
        /// </summary>
        public string ElementType { get; set; } = "";
    }
    /// <summary>
    /// MiniAPI 类信息
    /// </summary>
    internal class MiniApiClassInfo
    {
        public string ClassName { get; set; } = "";
        public string FullClassName { get; set; } = "";
        public string Namespace { get; set; } = "";
        public string RoutePrefix { get; set; } = "";
        public string Tag { get; set; } = "";
        public List<FilterInfo> ClassFilters { get; set; } = new();
        public List<MiniApiMethodInfo> Methods { get; set; } = new();
        public List<AutowiredMember> ConstructorParameters { get; set; } = new();

        /// <summary>
        /// 该控制器所在程序集中所有发现的 Validator 类型
        /// Key: 参数类型完整名称，Value: Validator 类型完整名称
        /// </summary>
        public Dictionary<string, string> ValidatorTypes { get; set; } = new();

        /// <summary>
        /// 类是否标记了 IgnoreRepeatedRequest 特性
        /// </summary>
        public bool IgnoreRepeatedRequest { get; set; }
    }

    /// <summary>
    /// MiniAPI 方法信息
    /// </summary>
    internal class MiniApiMethodInfo
    {
        public string MethodName { get; set; } = "";
        public string HttpMethod { get; set; } = "POST";
        public string RouteTemplate { get; set; } = "";
        public string ReturnType { get; set; } = "";
        public string ActualReturnType { get; set; } = "";
        public bool IsAsync { get; set; }
        public bool IsVoid { get; set; }
        public bool NoResultModel { get; set; }
        public List<MiniApiParameterInfo> Parameters { get; set; } = new();
        public List<FilterInfo> MethodFilters { get; set; } = new();

        /// <summary>
        /// 方法是否标记了 IgnoreRepeatedRequest 特性
        /// </summary>
        public bool IgnoreRepeatedRequest { get; set; }

        /// <summary>
        /// 方法是否标记了 AllowAnonymous 特性（JWT 鉴权忽略）
        /// </summary>
        public bool AllowAnonymous { get; set; }
    }

    /// <summary>
    /// MiniAPI 参数信息
    /// </summary>
    internal class MiniApiParameterInfo
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";

        public bool IsSimpleType { get; set; }
        public bool IsFileType { get; set; }
        public bool HasFromRoute { get; set; }
        public bool HasFromQuery { get; set; }
        public bool HasFromBody { get; set; }
        public bool HasFromForm { get; set; }
        public bool IsOptional { get; set; }
        public bool IsParams { get; set; }

        /// <summary>
        /// 是否存在对应的 FluentValidation Validator 类
        /// </summary>
        public bool HasValidator { get; set; }

        /// <summary>
        /// Validator 类的完整类型名称（含命名空间）
        /// </summary>
        public string ValidatorTypeName { get; set; } = "";

        /// <summary>
        /// 实体类型的属性列表（用于从查询字符串组装实体对象）
        /// </summary>
        public List<PropertyInfo> Properties { get; set; } = new();
    }

    /// <summary>
    /// 过滤器信息
    /// </summary>
    internal class FilterInfo
    {
        public string FilterTypeName { get; set; } = "";
    }

    /// <summary>
    /// 自动装配成员信息
    /// </summary>
    internal class AutowiredMember
    {
        public string Name { get; set; } = "";
        public string TypeName { get; set; } = "";
        public bool Required { get; set; } = true;
    }

    /// <summary>
    /// 响应类型信息
    /// </summary>
    internal class ResponseTypeInfo
    {
        public string ClassName { get; set; } = "";
        public string MethodName { get; set; } = "";
        public string DataType { get; set; } = "";
        public string ResponseTypeName { get; set; } = "";
        public bool IsVoid { get; set; }
        public string Namespace { get; set; } = "";
        public MiniApiMethodInfo? Method { get; set; } // 用于生成签名
    }
}
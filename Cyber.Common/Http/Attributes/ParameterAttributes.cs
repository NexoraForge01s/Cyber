using System;

namespace Cyber.Http.Attributes
{
    /// <summary>
    /// 标记参数使用 JSON 格式作为请求体
    /// </summary>
    public class JsonContentAttribute : Attribute
    {
    }

    /// <summary>
    /// 标记参数使用 Form 格式（x-www-form-urlencoded）作为请求体
    /// </summary>
    public class FormContentAttribute : Attribute
    {
    }

    /// <summary>
    /// 标记字段使用 Form 格式（x-www-form-urlencoded）
    /// </summary>
    public class FormFieldAttribute : Attribute
    {
        /// <summary>
        /// 字段名称别名
        /// </summary>
        public string? Name { get; }

        public FormFieldAttribute(string? name = null)
        {
            Name = name;
        }
    }

    /// <summary>
    /// 标记参数使用 Multipart/Form-Data 格式作为请求体
    /// </summary>
    public class FormDataContentAttribute : Attribute
    {
    }

    /// <summary>
    /// 标记参数为上传文件（Multipart/Form-Data）
    /// </summary>
    public class FormDataFileAttribute : Attribute
    {
        /// <summary>
        /// 文件字段名称
        /// </summary>
        public string? Name { get; }

        public FormDataFileAttribute(string? name = null)
        {
            Name = name;
        }
    }

    /// <summary>
    /// 参数别名特性
    /// </summary>
    public class AliasAsAttribute : Attribute
    {
        /// <summary>
        /// 别名
        /// </summary>
        public string Name { get; }

        public AliasAsAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// 时间格式特性
    /// </summary>
    public class DateTimeFormatAttribute : Attribute
    {
        /// <summary>
        /// 格式字符串
        /// </summary>
        public string Format { get; }

        public DateTimeFormatAttribute(string format)
        {
            Format = format;
        }
    }

    /// <summary>
    /// 标记参数为完整 URL（动态 URL）
    /// </summary>
    public class UriAttribute : Attribute
    {
    }

    /// <summary>
    /// 标记方法启用模型验证
    /// </summary>
    public class ValidationAttribute : Attribute
    {
    }

    /// <summary>
    /// 标记参数从请求头获取
    /// </summary>
    public class HeaderAttribute : Attribute
    {
        /// <summary>
        /// Header 名称
        /// </summary>
        public string Name { get; }

        public HeaderAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// 标记接口或方法使用特定过滤器
    /// </summary>
    public class ApiFilterAttribute : Attribute
    {
    }
}
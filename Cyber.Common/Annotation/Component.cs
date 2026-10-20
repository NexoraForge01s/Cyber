using System;
using System.Collections.Generic;
using System.Text;

namespace Cyber
{
    /// <summary>
    /// 标记一个类为可被依赖注入容器管理的组件。
    /// 源生成器会在编译时为打了此标签的类生成 DI 注册代码。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class Component : System.Attribute
    {
        #region Constructor
        /// <summary>
        /// 构造函数
        /// </summary>
        public Component()
        {

        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_service"></param>
        public Component(Type _service)
        {
            Service = _service;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_service"></param>
        /// <param name="key"></param>
        public Component(Type _service, string key) : this(_service)
        {
            this.Key = key;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="key"></param>
        public Component(string key)
        {
            this.Key = key;
        }

        #endregion

        #region Services
        /// <summary>
        /// 注册的类型
        /// 如果为null则注册为本身类型
        /// </summary>
        public Type[]? Services { get; set; }

        /// <summary>
        /// 注册单个的类型
        /// </summary>
        public Type? Service { get; set; }

        /// <summary>
        /// 注册单个的key
        /// </summary>
        public string? Key { get; set; }


        /// <summary>
        /// 自定义注册顺序 越小越先注册 但是注意 相同的类型谁最后注册就会拿resolve谁
        /// 因为autofac是允许重复注册的  以最后一次注册为准
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        ///  注册key 在同一个类型注册多个的时候就需要用到key来做区分
        /// </summary>
        public string[]? Keys { get; set; }


        #endregion

        /// <summary>
        /// A Boolean indicating if the component should auto-activate.
        /// </summary>
        public bool AutoActivate { get; set; }

        /// <summary>
        /// A Boolean indicating whether property (setter) injection for the component should be enabled.
        /// </summary>
        public bool InjectProperties { get; set; } = true;

        /// <summary>
        /// 属性自动装配的类型
        /// </summary>
        public InjectPropertyType InjectPropertyType { get; set; } = InjectPropertyType.Autowired;

        /// <summary>
        /// 作用域
        /// </summary>
        public AutofacScope AutofacScope { get; set; } = AutofacScope.InstancePerDependency;

        /// <summary>
        /// 被创建后执行的方法（支持同步或异步方法）
        /// </summary>
        public string? InitMethod { get; set; }


    }
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Cyber.AOT.Annotation
{
    /// <summary>
    /// 自动注入基类描述（用于批量注册继承类）
    /// </summary>
    internal sealed class InjectBaseClassDescriptor
    {
        /// <summary>
        /// 基类名称（不含命名空间，支持泛型如 "AbstractValidator"）
        /// </summary>
        public string BaseClassName { get; }

        /// <summary>
        /// 基类完整名称（含命名空间，如 "FluentValidation.AbstractValidator"）
        /// </summary>
        public string? BaseClassFullName { get; }

        /// <summary>
        /// 生命周期：Singleton、Scoped、Transient
        /// </summary>
        public string Lifetime { get; }

        /// <summary>
        /// 是否注册为基类类型（如 AbstractValidator&lt;T&gt;）
        /// true: services.AddScoped&lt;AbstractValidator&lt;UserDto&gt;, UserValidator&gt;()
        /// false: services.AddScoped&lt;UserValidator&gt;()
        /// </summary>
        public bool RegisterAsBaseType { get; }

        public InjectBaseClassDescriptor(string baseClassName, string lifetime, bool registerAsBaseType = true, string? baseClassFullName = null)
        {
            BaseClassName = baseClassName;
            Lifetime = lifetime;
            RegisterAsBaseType = registerAsBaseType;
            BaseClassFullName = baseClassFullName;
        }
    }

    [Generator]
    public partial class AnnotationGenerator : IIncrementalGenerator
    {
        /// <summary>
        /// 预定义的自动注入基类列表
        /// 所有继承这些基类的类都会自动注册到 DI 容器
        /// 示例：
        /// new InjectBaseClassDescriptor(
        ///     "AbstractValidator",     // 基类名称（支持泛型）
        ///     "Scoped",                 // 生命周期
        ///     true,                     // 注册为基类类型
        ///     "FluentValidation.AbstractValidator" // 可选：完整名称
        /// ),
        /// </summary>
        private static readonly InjectBaseClassDescriptor[] _injectBaseClasses = new InjectBaseClassDescriptor[]
        {
            // 在此处添加需要自动注入的基类
            // 示例：所有继承 AbstractValidator<T> 的类自动注册为 Scoped
             new InjectBaseClassDescriptor("AbstractValidator", "Scoped", true, "FluentValidation.AbstractValidator")
        };
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // 收集带有 [Component] 特性的类
            var componentClasses = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => IsClassWithComponentAttribute(s),
                    transform: static (ctx, _) => GetClassWithComponentInfo(ctx))
                .Where(static m => m is not null);

            // 收集实现 IMiniApiController 接口的类（转换为 ComponentClassInfo）
            var miniApiClasses = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => IsClassImplementingMiniApiController(s),
                    transform: static (ctx, _) => GetMiniApiControllerAsComponentInfo(ctx))
                .Where(static m => m is not null);

            // 收集继承预定义基类的类（自动注入）
            var injectedBaseClasses = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => IsClassWithInjectBaseClass(s),
                    transform: static (ctx, _) => GetClassWithInjectBaseClassInfo(ctx))
                .Where(static m => m is not null);

            // 合并三类，统一处理
            context.RegisterImplementationSourceOutput(
                componentClasses.Collect().Combine(miniApiClasses.Collect()).Combine(injectedBaseClasses.Collect()),
                static (spc, combined) =>
                {
                    var allClasses = combined.Left.Left.AddRange(combined.Left.Right).AddRange(combined.Right);
                    // 即使没有组件也要生成方法，以便其他项目可以调用
                    var source = GenerateRegistrationExtension(allClasses);
                    spc.AddSource("ServiceCollectionExtensions.g.cs", SourceText.From(source, Encoding.UTF8, SourceHashAlgorithm.Sha256));
                });
        }

        private static bool IsClassImplementingMiniApiController(SyntaxNode node)
        {
            return node is ClassDeclarationSyntax classDecl
                && classDecl.BaseList != null
                && classDecl.BaseList.Types.Any(t =>
                    t.Type.ToString().Contains("IMiniApiController"));
        }

        private static ComponentClassInfo? GetMiniApiControllerAsComponentInfo(GeneratorSyntaxContext context)
        {
            var classDecl = (ClassDeclarationSyntax)context.Node;
            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl);

            if (classSymbol == null) return null;

            if (classSymbol.IsAbstract || classSymbol.DeclaredAccessibility != Accessibility.Public)
                return null;

            // 检查类是否实现了 IMiniApiController 接口
            bool implementsMiniApi = false;
            foreach (var iface in classSymbol.AllInterfaces)
            {
                if (iface.ToDisplayString() == "Cyber.IMiniApiController" ||
                    iface.Name == "IMiniApiController")
                {
                    implementsMiniApi = true;
                    break;
                }
            }

            if (!implementsMiniApi) return null;

            // 转换为 ComponentClassInfo，默认 Scoped
            return new ComponentClassInfo
            {
                ClassName = classSymbol.Name,
                Namespace = classSymbol.ContainingNamespace.ToDisplayString(),
                FullClassName = classSymbol.ToDisplayString(),
                IsAbstract = classSymbol.IsAbstract,
                Scope = "Scoped",  // MiniAPI 控制器默认 Scoped
                ServiceTypes = new List<string> { classSymbol.ToDisplayString() },  // 默认注册为自身类型
                Key = ExtractKeyFromMiniApi(classSymbol),
                Keys = ExtractKeysFromMiniApi(classSymbol),
                OrderIndex = 0,
                AutoActivate = false,
                InjectProperties = true,
                InjectPropertyType = "Autowired",
                InitMethod = null,
                IsInitMethodAsync = false,
                ConstructorParameters = ExtractConstructorParameters(classSymbol),
                AutowiredProperties = ExtractAutowiredProperties(classSymbol)
            };
        }

        private static string? ExtractKeyFromMiniApi(INamedTypeSymbol classSymbol)
        {
            // 检查是否有 MiniApi 特性中的 Key 参数
            foreach (var attr in classSymbol.GetAttributes())
            {
                var attrName = attr.AttributeClass?.ToDisplayString();
                if (attrName == "Cyber.MiniApi" || attrName == "MiniApi")
                {
                    foreach (var namedArg in attr.NamedArguments)
                    {
                        if (namedArg.Key == "Key" && namedArg.Value.Value is string key)
                        {
                            return key;
                        }
                    }
                }
            }
            return null;
        }

        private static List<string> ExtractKeysFromMiniApi(INamedTypeSymbol classSymbol)
        {
            var keys = new List<string>();
            // 检查是否有 MiniApi 特性中的 Keys 参数
            foreach (var attr in classSymbol.GetAttributes())
            {
                var attrName = attr.AttributeClass?.ToDisplayString();
                if (attrName == "Cyber.MiniApi" || attrName == "MiniApi")
                {
                    foreach (var namedArg in attr.NamedArguments)
                    {
                        if (namedArg.Key == "Keys")
                        {
                            var values = namedArg.Value.Values;
                            foreach (var v in values)
                            {
                                if (v.Value is string key)
                                {
                                    keys.Add(key);
                                }
                            }
                        }
                    }
                }
            }
            return keys;
        }

        private static bool IsClassWithComponentAttribute(SyntaxNode node)
        {
            return node is ClassDeclarationSyntax classDecl
                && classDecl.AttributeLists.Count > 0
                && classDecl.AttributeLists.Any(al => al.Attributes.Any(a =>
                    a.Name.ToString() == "Component" ||
                    a.Name.ToString() == "Cyber.Component" ||
                    a.Name.ToString().EndsWith("Component")));
        }

        private static ComponentClassInfo? GetClassWithComponentInfo(GeneratorSyntaxContext context)
        {
            var classDecl = (ClassDeclarationSyntax)context.Node;
            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl);

            if (classSymbol == null) return null;

            // 查找 Component 特性
            AttributeData? componentAttr = null;
            foreach (var attr in classSymbol.GetAttributes())
            {
                var attrName = attr.AttributeClass?.ToDisplayString();
                if (attrName == "Cyber.Component" || attrName == "Component")
                {
                    componentAttr = attr;
                    break;
                }
            }

            if (componentAttr == null) return null;

            // 解析特性参数
            var initMethod = ExtractInitMethod(componentAttr);

            var info = new ComponentClassInfo
            {
                ClassName = classSymbol.Name,
                Namespace = classSymbol.ContainingNamespace.ToDisplayString(),
                FullClassName = classSymbol.ToDisplayString(),
                IsAbstract = classSymbol.IsAbstract,
                Scope = ExtractScope(componentAttr),
                ServiceTypes = ExtractServiceTypes(componentAttr, classSymbol),
                Key = ExtractKey(componentAttr),
                Keys = ExtractKeys(componentAttr),
                OrderIndex = ExtractOrderIndex(componentAttr),
                AutoActivate = ExtractAutoActivate(componentAttr),
                InjectProperties = ExtractInjectProperties(componentAttr),
                InjectPropertyType = ExtractInjectPropertyType(componentAttr),
                InitMethod = initMethod,
                IsInitMethodAsync = initMethod != null ? IsMethodAsync(classSymbol, initMethod) : false,
                ConstructorParameters = ExtractConstructorParameters(classSymbol),
                AutowiredProperties = ExtractAutowiredProperties(classSymbol)
            };

            return info;
        }

        private static string ExtractScope(AttributeData attr)
        {
            // 查找 AutofacScope 属性
            foreach (var namedArg in attr.NamedArguments)
            {
                if (namedArg.Key == "AutofacScope")
                {
                    var scopeValue = (int)namedArg.Value.Value!;
                    return scopeValue switch
                    {
                        0 => "Transient",  // InstancePerDependency
                        1 => "Singleton",  // SingleInstance
                        2 => "Scoped",     // InstancePerLifetimeScope
                        _ => "Transient"
                    };
                }
            }

            return "Transient"; // 默认值，对应 InstancePerDependency
        }

        private static List<string> ExtractServiceTypes(AttributeData attr, INamedTypeSymbol classSymbol)
        {
            var serviceTypes = new List<string>();

            // 检查构造函数参数中的服务类型
            if (attr.ConstructorArguments.Length > 0)
            {
                var firstArg = attr.ConstructorArguments[0];
                if (firstArg.Value is INamedTypeSymbol serviceType)
                {
                    serviceTypes.Add(serviceType.ToDisplayString());
                }
            }

            // 检查 Service 属性
            foreach (var namedArg in attr.NamedArguments)
            {
                if (namedArg.Key == "Service" && namedArg.Value.Value is INamedTypeSymbol serviceType)
                {
                    serviceTypes.Add(serviceType.ToDisplayString());
                }
                else if (namedArg.Key == "Services")
                {
                    var values = namedArg.Value.Values;
                    foreach (var v in values)
                    {
                        if (v.Value is INamedTypeSymbol s)
                        {
                            serviceTypes.Add(s.ToDisplayString());
                        }
                    }
                }
            }

            // 如果没有指定服务类型，则自动推断
            if (serviceTypes.Count == 0)
            {
                // 获取类实现的所有接口，排除系统接口
                var interfaces = classSymbol.AllInterfaces
                    .Where(i => i.ContainingNamespace?.ToDisplayString() != "System" &&
                                !i.ContainingNamespace?.ToDisplayString().StartsWith("System.") == true)
                    .ToList();

                if (interfaces.Count > 0)
                {
                    // 如果类实现了接口，使用接口作为服务类型
                    foreach (var iface in interfaces)
                    {
                        serviceTypes.Add(iface.ToDisplayString());
                    }
                }
                else
                {
                    // 如果没有实现接口，使用类本身
                    serviceTypes.Add(classSymbol.ToDisplayString());
                }
            }

            return serviceTypes;
        }

        private static string? ExtractKey(AttributeData attr)
        {
            // 检查构造函数参数中的 key
            if (attr.ConstructorArguments.Length >= 2)
            {
                if (attr.ConstructorArguments[1].Value is string key)
                {
                    return key;
                }
            }
            if (attr.ConstructorArguments.Length == 1)
            {
                if (attr.ConstructorArguments[0].Value is string key)
                {
                    return key;
                }
            }

            // 检查 Key 属性
            foreach (var namedArg in attr.NamedArguments)
            {
                if (namedArg.Key == "Key" && namedArg.Value.Value is string keyValue)
                {
                    return keyValue;
                }
            }

            return null;
        }

        private static List<string> ExtractKeys(AttributeData attr)
        {
            var keys = new List<string>();
            foreach (var namedArg in attr.NamedArguments)
            {
                if (namedArg.Key == "Keys")
                {
                    var values = namedArg.Value.Values;
                    foreach (var v in values)
                    {
                        if (v.Value is string key)
                        {
                            keys.Add(key);
                        }
                    }
                }
            }
            return keys;
        }

        private static int ExtractOrderIndex(AttributeData attr)
        {
            foreach (var namedArg in attr.NamedArguments)
            {
                if (namedArg.Key == "OrderIndex" && namedArg.Value.Value is int orderIndex)
                {
                    return orderIndex;
                }
            }
            return 0;
        }

        private static bool ExtractAutoActivate(AttributeData attr)
        {
            foreach (var namedArg in attr.NamedArguments)
            {
                if (namedArg.Key == "AutoActivate" && namedArg.Value.Value is bool autoActivate)
                {
                    return autoActivate;
                }
            }
            return false;
        }

        private static bool ExtractInjectProperties(AttributeData attr)
        {
            foreach (var namedArg in attr.NamedArguments)
            {
                if (namedArg.Key == "InjectProperties" && namedArg.Value.Value is bool injectProperties)
                {
                    return injectProperties;
                }
            }
            return true;
        }

        private static string ExtractInjectPropertyType(AttributeData attr)
        {
            foreach (var namedArg in attr.NamedArguments)
            {
                if (namedArg.Key == "InjectPropertyType")
                {
                    var value = (int)namedArg.Value.Value!;
                    return value switch
                    {
                        0 => "Autowired",  // InjectPropertyType.Autowired
                        1 => "All",        // InjectPropertyType.All
                        _ => "Autowired"
                    };
                }
            }
            return "Autowired";
        }

        private static string? ExtractInitMethod(AttributeData attr)
        {
            foreach (var namedArg in attr.NamedArguments)
            {
                if (namedArg.Key == "InitMethod" && namedArg.Value.Value is string initMethod)
                {
                    return initMethod;
                }
            }
            return null;
        }

        private static bool IsMethodAsync(INamedTypeSymbol classSymbol, string methodName)
        {
            var method = classSymbol.GetMembers(methodName)
                .OfType<IMethodSymbol>()
                .FirstOrDefault(m => m.DeclaredAccessibility == Accessibility.Public);

            if (method == null) return false;

            // 检查返回类型是否是 Task 或 Task<T>
            var returnType = method.ReturnType;
            return returnType?.Name == "Task" ||
                   returnType?.ToDisplayString().StartsWith("System.Threading.Tasks.Task") == true;
        }

        private static List<AutowiredMember> ExtractConstructorParameters(INamedTypeSymbol classSymbol)
        {
            var parameters = new List<AutowiredMember>();

            // 获取第一个公共构造函数
            var constructor = classSymbol.Constructors.FirstOrDefault(c => c.DeclaredAccessibility == Accessibility.Public);
            if (constructor == null) return parameters;

            foreach (var param in constructor.Parameters)
            {
                var autowiredParam = new AutowiredMember
                {
                    Name = param.Name,
                    TypeName = param.Type.ToDisplayString(),
                    Required = true
                };

                // 检查参数是否有 Autowired 特性（可选）
                var autowiredAttr = param.GetAttributes().FirstOrDefault(a =>
                    a.AttributeClass?.ToDisplayString() == "Cyber.AutowiredAttribute" ||
                    a.AttributeClass?.ToDisplayString() == "Autowired");

                if (autowiredAttr != null)
                {
                    // 提取 Autowired 特性的属性
                    foreach (var namedArg in autowiredAttr.NamedArguments)
                    {
                        if (namedArg.Key == "Key" && namedArg.Value.Value is string key)
                        {
                            autowiredParam.Key = key;
                        }
                        else if (namedArg.Key == "Service" && namedArg.Value.Value is INamedTypeSymbol serviceType)
                        {
                            autowiredParam.ServiceType = serviceType.ToDisplayString();
                        }
                        else if (namedArg.Key == "Required" && namedArg.Value.Value is bool required)
                        {
                            autowiredParam.Required = required;
                        }
                    }
                }

                parameters.Add(autowiredParam);
            }

            return parameters;
        }

        private static List<AutowiredMember> ExtractAutowiredProperties(INamedTypeSymbol classSymbol)
        {
            var properties = new List<AutowiredMember>();

            foreach (var member in classSymbol.GetMembers())
            {
                if (member is IPropertySymbol property && property.SetMethod != null)
                {
                    var autowiredAttr = property.GetAttributes().FirstOrDefault(a =>
                        a.AttributeClass?.ToDisplayString() == "Cyber.AutowiredAttribute" ||
                        a.AttributeClass?.ToDisplayString() == "Autowired");

                    if (autowiredAttr != null)
                    {
                        var autowiredProp = new AutowiredMember
                        {
                            Name = property.Name,
                            TypeName = property.Type.ToDisplayString()
                        };

                        // 提取 Autowired 特性的属性
                        foreach (var namedArg in autowiredAttr.NamedArguments)
                        {
                            if (namedArg.Key == "Key" && namedArg.Value.Value is string key)
                            {
                                autowiredProp.Key = key;
                            }
                            else if (namedArg.Key == "Service" && namedArg.Value.Value is INamedTypeSymbol serviceType)
                            {
                                autowiredProp.ServiceType = serviceType.ToDisplayString();
                            }
                            else if (namedArg.Key == "Required" && namedArg.Value.Value is bool required)
                            {
                                autowiredProp.Required = required;
                            }
                        }

                        properties.Add(autowiredProp);
                    }
                }
            }

            return properties;
        }

        private static string GenerateRegistrationExtension(ImmutableArray<ComponentClassInfo?> classes)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            sb.AppendLine("using Microsoft.Extensions.DependencyInjection.Extensions;");
            sb.AppendLine();
            sb.AppendLine("namespace Cyber");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 自动生成的依赖注入扩展方法");
            sb.AppendLine("    /// 此代码由Cyber.AOT项目自动生成");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static partial class ServiceCollectionExtensions");
            sb.AppendLine("    {");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// 自动注册所有标记了 [Component] 特性的服务");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static IServiceCollection AddCyberComponents(this IServiceCollection services)");
            sb.AppendLine("        {");

            // 按 OrderIndex 排序，处理空数组的情况
            var orderedClasses = classes.IsDefaultOrEmpty
                ? new List<ComponentClassInfo?>()
                : classes
                    .Where(c => c != null && !c.IsAbstract)
                    .OrderBy(c => c!.OrderIndex)
                    .ToList();

            foreach (var classInfo in orderedClasses)
            {
                if (classInfo == null) continue;

                var implementationType = classInfo.FullClassName;

                // 判断是否有 Keyed 注册
                bool hasKeyedRegistration = classInfo.Keys.Count > 0 || !string.IsNullOrEmpty(classInfo.Key);

                // 如果有 Keyed 注册，则只生成 Keyed 注册；否则生成普通注册
                if (hasKeyedRegistration)
                {
                    // 处理带 Key 的注册
                    if (classInfo.Keys.Count > 0)
                    {
                        foreach (var key in classInfo.Keys)
                        {
                            foreach (var serviceType in classInfo.ServiceTypes)
                            {
                                GenerateKeyedServiceRegistration(sb, serviceType, implementationType, classInfo, key);
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(classInfo.Key))
                    {
                        // 单个 Key
                        foreach (var serviceType in classInfo.ServiceTypes)
                        {
                            GenerateKeyedServiceRegistration(sb, serviceType, implementationType, classInfo, classInfo.Key!);
                        }
                    }
                }
                else
                {
                    // 处理主要的 ServiceTypes（不带 Key）
                    foreach (var serviceType in classInfo.ServiceTypes)
                    {
                        GenerateServiceRegistration(sb, serviceType, implementationType, classInfo, null);
                    }
                }
            }

            // 处理 AutoActivate
            var autoActivateClasses = orderedClasses.Where(c => c != null && c.AutoActivate).ToList();
            if (autoActivateClasses.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("            // Auto-activate services");
                foreach (var classInfo in autoActivateClasses)
                {
                    if (classInfo == null) continue;
                    sb.AppendLine($"            services.GetRequiredService<{classInfo.FullClassName}>();");
                }
            }

            sb.AppendLine("            return services;");
            sb.AppendLine("        }");

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static void GenerateServiceRegistration(StringBuilder sb, string serviceType,
            string implementationType, ComponentClassInfo classInfo, string? key)
        {
            var addMethod = classInfo.Scope switch
            {
                "Singleton" => "AddSingleton",
                "Scoped" => "AddScoped",
                _ => "AddTransient"
            };

            // 检查是否需要工厂方法（有 Autowired 参数、属性注入或 InitMethod）
            bool needsFactory = classInfo.ConstructorParameters.Count > 0 ||
                                classInfo.AutowiredProperties.Count > 0 ||
                                !string.IsNullOrEmpty(classInfo.InitMethod);

            if (needsFactory)
            {
                GenerateFactoryBasedRegistration(sb, serviceType, implementationType, classInfo, addMethod, key);
            }
            else
            {
                // 简单注册
                if (key != null)
                {
                    sb.AppendLine($"            services.AddKeyed{addMethod.Substring(3)}<{serviceType}, {implementationType}>(\"{key}\");");
                }
                else
                {
                    sb.AppendLine($"            services.{addMethod}<{serviceType}, {implementationType}>();");
                }
            }
        }

        private static void GenerateKeyedServiceRegistration(StringBuilder sb, string serviceType,
            string implementationType, ComponentClassInfo classInfo, string key)
        {
            var addMethod = classInfo.Scope switch
            {
                "Singleton" => "Singleton",
                "Scoped" => "Scoped",
                _ => "Transient"
            };

            bool needsFactory = classInfo.ConstructorParameters.Count > 0 ||
                                classInfo.AutowiredProperties.Count > 0 ||
                                !string.IsNullOrEmpty(classInfo.InitMethod);

            if (needsFactory)
            {
                GenerateKeyedFactoryBasedRegistration(sb, serviceType, implementationType, classInfo, addMethod, key);
            }
            else
            {
                sb.AppendLine($"            services.AddKeyed{addMethod}<{serviceType}, {implementationType}>(\"{key}\");");
            }
        }

        private static void GenerateFactoryBasedRegistration(StringBuilder sb, string serviceType,
            string implementationType, ComponentClassInfo classInfo, string addMethod, string? key)
        {
            sb.AppendLine($"            services.{addMethod}<{serviceType}>(sp =>");
            sb.AppendLine("            {");

            // 生成构造函数参数解析
            sb.Append($"                var instance = new {implementationType}(");
            if (classInfo.ConstructorParameters.Count > 0)
            {
                var paramList = new List<string>();
                foreach (var param in classInfo.ConstructorParameters)
                {
                    var resolveType = param.ServiceType ?? param.TypeName;
                    if (!string.IsNullOrEmpty(param.Key))
                    {
                        paramList.Add($"sp.GetRequiredKeyedService<{resolveType}>(\"{param.Key}\")");
                    }
                    else
                    {
                        if (param.Required)
                        {
                            paramList.Add($"sp.GetRequiredService<{resolveType}>()");
                        }
                        else
                        {
                            paramList.Add($"sp.GetService<{resolveType}>()");
                        }
                    }
                }
                sb.Append(string.Join(", ", paramList));
            }
            sb.AppendLine(");");

            // 生成属性注入
            if (classInfo.InjectProperties && classInfo.AutowiredProperties.Count > 0)
            {
                foreach (var prop in classInfo.AutowiredProperties)
                {
                    var resolveType = prop.ServiceType ?? prop.TypeName;
                    if (!string.IsNullOrEmpty(prop.Key))
                    {
                        sb.AppendLine($"                instance.{prop.Name} = sp.GetRequiredKeyedService<{resolveType}>(\"{prop.Key}\");");
                    }
                    else
                    {
                        if (prop.Required)
                        {
                            sb.AppendLine($"                instance.{prop.Name} = sp.GetRequiredService<{resolveType}>();");
                        }
                        else
                        {
                            sb.AppendLine($"                instance.{prop.Name} = sp.GetService<{resolveType}>();");
                        }
                    }
                }
            }

            // 调用 InitMethod（支持异步）
            if (!string.IsNullOrEmpty(classInfo.InitMethod))
            {
                if (classInfo.IsInitMethodAsync)
                {
                    sb.AppendLine($"                instance.{classInfo.InitMethod}().GetAwaiter().GetResult();");
                }
                else
                {
                    sb.AppendLine($"                instance.{classInfo.InitMethod}();");
                }
            }

            sb.AppendLine($"                return instance;");
            sb.AppendLine("            });");
        }

        private static void GenerateKeyedFactoryBasedRegistration(StringBuilder sb, string serviceType,
            string implementationType, ComponentClassInfo classInfo, string addMethod, string key)
        {
            sb.AppendLine($"            services.AddKeyed{addMethod}<{serviceType}>(\"{key}\", (sp, _) =>");
            sb.AppendLine("            {");

            // 生成构造函数参数解析
            sb.Append($"                var instance = new {implementationType}(");
            if (classInfo.ConstructorParameters.Count > 0)
            {
                var paramList = new List<string>();
                foreach (var param in classInfo.ConstructorParameters)
                {
                    var resolveType = param.ServiceType ?? param.TypeName;
                    if (!string.IsNullOrEmpty(param.Key))
                    {
                        paramList.Add($"sp.GetRequiredKeyedService<{resolveType}>(\"{param.Key}\")");
                    }
                    else
                    {
                        if (param.Required)
                        {
                            paramList.Add($"sp.GetRequiredService<{resolveType}>()");
                        }
                        else
                        {
                            paramList.Add($"sp.GetService<{resolveType}>()");
                        }
                    }
                }
                sb.Append(string.Join(", ", paramList));
            }
            sb.AppendLine(");");

            // 生成属性注入
            if (classInfo.InjectProperties && classInfo.AutowiredProperties.Count > 0)
            {
                foreach (var prop in classInfo.AutowiredProperties)
                {
                    var resolveType = prop.ServiceType ?? prop.TypeName;
                    if (!string.IsNullOrEmpty(prop.Key))
                    {
                        sb.AppendLine($"                instance.{prop.Name} = sp.GetRequiredKeyedService<{resolveType}>(\"{prop.Key}\");");
                    }
                    else
                    {
                        if (prop.Required)
                        {
                            sb.AppendLine($"                instance.{prop.Name} = sp.GetRequiredService<{resolveType}>();");
                        }
                        else
                        {
                            sb.AppendLine($"                instance.{prop.Name} = sp.GetService<{resolveType}>();");
                        }
                    }
                }
            }

            // 调用 InitMethod（支持异步）
            if (!string.IsNullOrEmpty(classInfo.InitMethod))
            {
                if (classInfo.IsInitMethodAsync)
                {
                    sb.AppendLine($"                instance.{classInfo.InitMethod}().GetAwaiter().GetResult();");
                }
                else
                {
                    sb.AppendLine($"                instance.{classInfo.InitMethod}();");
                }
            }

            sb.AppendLine($"                return instance;");
            sb.AppendLine("            });");
        }

        /// <summary>
        /// 判断类是否继承自预定义的注入基类
        /// </summary>
        private static bool IsClassWithInjectBaseClass(SyntaxNode node)
        {
            if (_injectBaseClasses.Length == 0)
                return false;

            if (node is not ClassDeclarationSyntax classDecl)
                return false;

            if (classDecl.BaseList == null)
                return false;

            // 检查是否有任何基类类型名称匹配预定义的基类名
            foreach (var baseType in classDecl.BaseList.Types)
            {
                var typeName = baseType.Type.ToString();
                // 提取类型名称（去掉泛型参数）
                var baseName = typeName.Split('<', '>')[0];
                foreach (var injectBase in _injectBaseClasses)
                {
                    if (baseName == injectBase.BaseClassName)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取继承预定义基类的类的注册信息
        /// </summary>
        private static ComponentClassInfo? GetClassWithInjectBaseClassInfo(GeneratorSyntaxContext context)
        {
            var classDecl = (ClassDeclarationSyntax)context.Node;
            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl);

            if (classSymbol == null) return null;
            if (classSymbol.IsAbstract) return null;
            if (classSymbol.DeclaredAccessibility != Accessibility.Public) return null;

            // 遍历所有基类，查找匹配的预定义基类
            InjectBaseClassDescriptor? matchedDescriptor = null;
            INamedTypeSymbol? matchedBaseType = null;

            // 检查直接基类
            var baseType = classSymbol.BaseType;
            while (baseType != null)
            {
                var baseName = baseType.Name;
                var baseFullName = baseType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    .Replace("global::", "");

                foreach (var descriptor in _injectBaseClasses)
                {
                    if (baseName == descriptor.BaseClassName ||
                        (!string.IsNullOrEmpty(descriptor.BaseClassFullName) &&
                         baseFullName.StartsWith(descriptor.BaseClassFullName, StringComparison.OrdinalIgnoreCase)))
                    {
                        matchedDescriptor = descriptor;
                        matchedBaseType = baseType;
                        break;
                    }
                }

                if (matchedDescriptor != null)
                    break;

                baseType = baseType.BaseType;
            }

            // 检查接口
            if (matchedDescriptor == null)
            {
                foreach (var iface in classSymbol.AllInterfaces)
                {
                    var ifaceName = iface.Name;
                    var ifaceFullName = iface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                        .Replace("global::", "");

                    foreach (var descriptor in _injectBaseClasses)
                    {
                        if (ifaceName == descriptor.BaseClassName ||
                            (!string.IsNullOrEmpty(descriptor.BaseClassFullName) &&
                             ifaceFullName.StartsWith(descriptor.BaseClassFullName, StringComparison.OrdinalIgnoreCase)))
                        {
                            matchedDescriptor = descriptor;
                            matchedBaseType = iface;
                            break;
                        }
                    }

                    if (matchedDescriptor != null)
                        break;
                }
            }

            if (matchedDescriptor == null || matchedBaseType == null)
                return null;

            // 检查是否已有 [Component] 特性（避免重复注册）
            if (classSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "Component" ||
                                                     a.AttributeClass?.ToDisplayString() == "Cyber.ComponentAttribute"))
                return null;

            var fullClassName = classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            // 构造函数参数
            var constructorParams = new List<AutowiredMember>();
            var constructor = classSymbol.InstanceConstructors
                .OrderByDescending(c => c.Parameters.Length)
                .FirstOrDefault();

            if (constructor != null)
            {
                foreach (var param in constructor.Parameters)
                {
                    constructorParams.Add(new AutowiredMember
                    {
                        Name = param.Name,
                        TypeName = param.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        Required = !param.HasExplicitDefaultValue
                    });
                }
            }

            // 属性注入
            var autowiredProps = new List<AutowiredMember>();
            foreach (var member in classSymbol.GetMembers())
            {
                if (member is IPropertySymbol prop && prop.SetMethod != null)
                {
                    var autowiredAttr = prop.GetAttributes().FirstOrDefault(a =>
                        a.AttributeClass?.Name == "Autowired" ||
                        a.AttributeClass?.ToDisplayString() == "Cyber.AutowiredAttribute");

                    if (autowiredAttr != null)
                    {
                        string? key = null;
                        string? serviceType = null;
                        bool required = true;

                        foreach (var namedArg in autowiredAttr.NamedArguments)
                        {
                            if (namedArg.Key == "Key" && namedArg.Value.Value is string k)
                                key = k;
                            if (namedArg.Key == "ServiceType" && namedArg.Value.Value != null)
                                serviceType = namedArg.Value.Value.ToString();
                            if (namedArg.Key == "Required" && namedArg.Value.Value is bool r)
                                required = r;
                        }

                        autowiredProps.Add(new AutowiredMember
                        {
                            Name = prop.Name,
                            TypeName = prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                            Key = key,
                            ServiceType = serviceType,
                            Required = required
                        });
                    }
                }
            }

            // 构建服务类型列表
            var serviceTypes = new List<string>();
            if (matchedDescriptor.RegisterAsBaseType)
            {
                // 注册为基类类型
                serviceTypes.Add(matchedBaseType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            }

            return new ComponentClassInfo
            {
                ClassName = classSymbol.Name,
                Namespace = classSymbol.ContainingNamespace?.ToDisplayString() ?? "",
                FullClassName = fullClassName,
                IsAbstract = classSymbol.IsAbstract,
                Scope = matchedDescriptor.Lifetime,
                ServiceTypes = serviceTypes,
                Key = null,
                Keys = new List<string>(),
                OrderIndex = 0,
                AutoActivate = false,
                InjectProperties = autowiredProps.Count > 0,
                InjectPropertyType = "Autowired",
                InitMethod = null,
                IsInitMethodAsync = false,
                ConstructorParameters = constructorParams,
                AutowiredProperties = autowiredProps
            };
        }
    }

    internal class ComponentClassInfo
    {
        public string ClassName { get; set; } = "";
        public string Namespace { get; set; } = "";
        public string FullClassName { get; set; } = "";
        public bool IsAbstract { get; set; }
        public string Scope { get; set; } = "Transient";
        public List<string> ServiceTypes { get; set; } = new();
        public string? Key { get; set; }
        public List<string> Keys { get; set; } = new();
        public int OrderIndex { get; set; } = 0;
        public bool AutoActivate { get; set; }
        public bool InjectProperties { get; set; } = true;
        public string InjectPropertyType { get; set; } = "Autowired";
        public string? InitMethod { get; set; }
        public bool IsInitMethodAsync { get; set; }
        public List<AutowiredMember> ConstructorParameters { get; set; } = new();
        public List<AutowiredMember> AutowiredProperties { get; set; } = new();
    }

    internal class AutowiredMember
    {
        public string Name { get; set; } = "";
        public string TypeName { get; set; } = "";
        public string? Key { get; set; }
        public string? ServiceType { get; set; }
        public bool Required { get; set; } = true;
    }
}

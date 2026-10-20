using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Cyber.AOT.MiniApi;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Cyber.AOT.Json
{
    /// <summary>
    /// 描述一个额外需要注册到 ConverterRegistry 的类型-Converter 对。
    /// </summary>
    internal sealed class AdditionalConverter
    {
        /// <summary>
        /// 用于在 TypeInfoResolver 中比较的类型名字符串。
        /// 格式为 C# typeof 表达式，如 "typeof(System.Collections.Generic.Dictionary&lt;string, object&gt;)"
        /// </summary>
        public string TargetTypeExpression { get; }
        /// <summary>
        /// Converter 类型的完整名称（含命名空间，不含程序集限定符）。
        /// 例如: JWT.Serializers.Converters.DictionaryStringObjectJsonConverterCustomWrite
        /// </summary>
        public string ConverterTypeFullName { get; }
        /// <summary>
        /// 需要在生成代码中添加的 using 命名空间。
        /// </summary>
        public string UsingNamespace { get; }

        public AdditionalConverter(
            string targetTypeExpression,
            string converterTypeFullName,
            string usingNamespace)
        {
            TargetTypeExpression = targetTypeExpression;
            ConverterTypeFullName = converterTypeFullName;
            UsingNamespace = usingNamespace;
        }
    }

    [Generator]
    public class JsonContextGenerator : IIncrementalGenerator
    {
        /// <summary>
        /// 额外的 Converter 类型，用于注册到 TypeInfoResolver 和 ConverterRegistry。
        /// 格式：(TargetTypeFullName, ConverterTypeFullName, ConverterAssemblyName)
        /// </summary>
        private static readonly AdditionalConverter[] _additionalConverters = new AdditionalConverter[]
        {
        };

        /// <summary>
        /// 需要忽略的基类名称列表（不含命名空间，匹配泛型基类时只匹配基础名称）
        /// 例如：["AbstractValidator"] 将忽略所有继承自 AbstractValidator&lt;T&gt; 的类
        /// </summary>
        private static readonly HashSet<string> _ignoredBaseClassNames = new HashSet<string>
        {
          
        };

        /// <summary>
        /// 需要忽略的基类完整名称列表（含命名空间）
        /// 例如：["FluentValidation.AbstractValidator"] 将精确匹配
        /// </summary>
        private static readonly HashSet<string> _ignoredBaseClassFullNames = new HashSet<string>
        {
            "FluentValidation.AbstractValidator",
            "FluentValidation.IValidator"
        };

        /// <summary>
        /// 需要忽略的特性完整名称列表（含命名空间）
        /// 例如：["FluentValidation.ValidatorAttribute"] 将忽略带有此特性的类
        /// </summary>
        private static readonly HashSet<string> _ignoredAttributeFullNames = new HashSet<string>
        {
            // 在此处添加需要忽略的特性
        };

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // 自动发现所有需要 JSON 序列化的实体类
            var serializableTypes = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => IsPotentialSerializableType(s),
                    transform: static (ctx, _) => GetSerializableTypeInfo(ctx))
                .Where(static m => m is not null);

            var miniApiClasses = context.SyntaxProvider
               .CreateSyntaxProvider(
                   predicate: static (s, _) => MiniApiGenerationStatic.IsClassImplementingMiniApiController(s),
                   transform: static (ctx, _) => MiniApiGenerationStatic.GetMiniApiClassInfo(ctx))
               .Where(static m => m is not null);

            var allSerializableTypes = serializableTypes.Collect();
            var allMiniApiClassInfos = miniApiClasses.Collect();

            var combinedData = allSerializableTypes.Combine(allMiniApiClassInfos);

            context.RegisterImplementationSourceOutput(
                combinedData,
                static (spc, types) =>
                {
                    // 收集 MiniAPI 响应类型和请求参数类型
                    var responseTypes = new List<TypeInfo>();
                    var requestParamTypes = new List<TypeInfo>();
                    
                    foreach (var classInfo in types.Right.Where(c => c != null))
                    {
                        if (classInfo == null) continue;
                        foreach (var method in classInfo.Methods)
                        {
                            // 收集响应类型
                            if (!method.NoResultModel)
                            {
                                var responseTypeName = MiniApiGenerationStatic.GetResponseTypeName(classInfo.ClassName, method.MethodName, method.HttpMethod, method);
                                responseTypes.Add(new TypeInfo
                                {
                                    TypeName = $"{classInfo.Namespace}.{responseTypeName}",
                                    Namespace = classInfo.Namespace,
                                    SimpleName = responseTypeName,
                                    // 为 MiniAPI 响应类型构造属性列表
                                    Properties = BuildResponseProperties(method)
                                });
                            }

                            // 收集请求参数类型（FromBody 参数）
                            foreach (var param in method.Parameters)
                            {
                                if (param.HasFromBody && !IsSimpleOrPrimitiveType(param.Type))
                                {
                                    // 需要为复杂类型生成 TypeInfo
                                    requestParamTypes.Add(BuildRequestParamTypeInfo(param.Type, classInfo.Namespace));
                                }
                            }
                        }
                    }

                  
                    // 为每个自动发现的实体类型添加 IEnumerable<实体> 的 converter
                    var enumerableTypes = new List<TypeInfo>();
                    foreach (var type in types.Left.ToList())
                    {
                        // 只为非集合、非字典、非枚举的实体类型生成 IEnumerable converter
                        if (type!=null &&!IsCollectionType(type.TypeName) && !IsDictionaryType(type.TypeName) && !IsEnumType(type.TypeName))
                        {
                            var enumerableTypeName = $"global::System.Collections.Generic.IEnumerable<{type.TypeName}>";
                            var enumerableSimpleName = $"IEnumerable_{type.SimpleName}";
                            // 检查是否已存在
                            if (!types.Left.Any(t => t!=null && t.TypeName == enumerableTypeName))
                            {
                                enumerableTypes.Add(new TypeInfo
                                {
                                    TypeName = enumerableTypeName,
                                    Namespace = type.Namespace,
                                    SimpleName = enumerableSimpleName,
                                    Properties = new List<PropertyInfo>() // IEnumerable 没有属性
                                });
                            }
                        }
                    }

                    var allTypesForConverter = types.Left
                      .Where(t => t != null)
                      .Select(t => t!)
                      .Concat(responseTypes)
                      .Concat(requestParamTypes)
                      .GroupBy(t => t.TypeName)  // 按 TypeName 分组
                      .Select(g => g.First())    // 每组只取第一个，避免重复
                      .Where(t => !IsListType(t.TypeName))  // 过滤掉 List<实体> 类型，使用 IEnumerable<实体> 代替
                      .ToList();


                    allTypesForConverter.AddRange(enumerableTypes);

                    // 1. 生成 JsonConverters 文件（只包含 converter 类）
                    var converterSource = GenerateJsonConverters(allTypesForConverter);
                    spc.AddSource("CyberJsonConverters.g.cs", SourceText.From(converterSource, Encoding.UTF8));

                    // 2. 生成 JsonTypeInfos 文件（静态类，包含 Options + converter 单例 + JsonTypeInfo 缓存）
                    var typeInfosSource = GenerateJsonTypeInfosSource(allTypesForConverter);
                    spc.AddSource("CyberJsonTypeInfos.g.cs", SourceText.From(typeInfosSource, Encoding.UTF8));
                });


        }
        /// <summary>
        /// 为 MiniAPI 响应类型构建属性列表
        /// </summary>
        private static List<PropertyInfo> BuildResponseProperties(MiniApiMethodInfo method)
        {
            var properties = new List<PropertyInfo>();

            // Code 属性
            properties.Add(new PropertyInfo
            {
                Name = "Code",
                Type = "global::Cyber.ResultCode",
                CanWrite = true,
                IsInitOnly = false,
                IsNullable = false
            });

            // Data 属性（如果不是 void 方法）
            if (!method.IsVoid)
            {
                properties.Add(new PropertyInfo
                {
                    Name = "Data",
                    Type = method.ActualReturnType,
                    CanWrite = true,
                    IsInitOnly = false,
                    IsNullable = true
                });
            }

            // Error 属性
            properties.Add(new PropertyInfo
            {
                Name = "Error",
                Type = "string",
                CanWrite = true,
                IsInitOnly = false,
                IsNullable = true
            });

            // Timestamp 属性
            properties.Add(new PropertyInfo
            {
                Name = "Timestamp",
                Type = "long?",
                CanWrite = true,
                IsInitOnly = false,
                IsNullable = true
            });

            return properties;
        }

        /// <summary>
        /// 判断类型是否为简单类型或原始类型（不需要生成 converter）
        /// </summary>
        private static bool IsSimpleOrPrimitiveType(string type)
        {
            var simpleTypes = new[]
            {
                "string", "global::System.String",
                "int", "global::System.Int32",
                "long", "global::System.Int64",
                "double", "global::System.Double",
                "float", "global::System.Single",
                "decimal", "global::System.Decimal",
                "bool", "global::System.Boolean",
                "DateTime", "global::System.DateTime",
                "DateTimeOffset", "global::System.DateTimeOffset",
                "Guid", "global::System.Guid",
                "byte", "global::System.Byte",
                "short", "global::System.Int16",
                "ushort", "global::System.UInt16",
                "uint", "global::System.UInt32",
                "ulong", "global::System.UInt64",
                "char", "global::System.Char",
                "sbyte", "global::System.SByte"
            };

            // 检查是否是简单类型
            if (simpleTypes.Contains(type))
                return true;

            // 检查是否是可空简单类型
            if (type.EndsWith("?"))
            {
                var baseType = type.Substring(0, type.Length - 1);
                return simpleTypes.Contains(baseType);
            }

            return false;
        }

        /// <summary>
        /// 为请求参数类型构建 TypeInfo（简化版本，只有类型信息，没有属性信息）
        /// </summary>
        private static TypeInfo BuildRequestParamTypeInfo(string type, string namespaceName)
        {
            // 使用与 GetJsonTypeInfoPropertyName 相同的逻辑提取 SimpleName
            var simpleName = GetJsonTypeInfoPropertyNameInternal(type);

            return new TypeInfo
            {
                TypeName = type,
                SimpleName = simpleName,
                Namespace = namespaceName,
                Properties = new List<PropertyInfo>() // 空属性列表，因为这是参数类型
            };
        }

        /// <summary>
        /// 内部方法：获取类型在 JsonTypeInfos 中的属性名称
        /// </summary>
        private static string GetJsonTypeInfoPropertyNameInternal(string type)
        {
            // 处理 global:: 前缀
            var typeName = type.Replace("global::", "");

            // 处理泛型类型
            if (typeName.Contains("<"))
            {
                // 提取基础类型名（去掉 <...> 部分）
                var genericIndex = typeName.IndexOf('<');
                var baseName = typeName.Substring(0, genericIndex);
                
                // 提取基础类型的 SimpleName（去掉命名空间）
                var lastDot = baseName.LastIndexOf('.');
                if (lastDot >= 0)
                    baseName = baseName.Substring(lastDot + 1);

                // 提取泛型参数
                var genericParamsStr = typeName.Substring(genericIndex + 1, typeName.Length - genericIndex - 2);
                var genericParams = genericParamsStr.Split(',').Select(p => p.Trim()).ToList();

                // 对每个泛型参数递归提取 SimpleName
                var simplifiedParams = genericParams.Select(p => GetJsonTypeInfoPropertyNameInternal(p)).ToList();

                // 组合：BaseName_Param1_Param2...
                return $"{baseName}_{string.Join("_", simplifiedParams)}";
            }

            // 非泛型：去掉命名空间，只保留类型名
            if (typeName.Contains("."))
            {
                typeName = typeName.Substring(typeName.LastIndexOf('.') + 1);
            }

            return typeName;
        }

        private static bool IsPotentialSerializableType(SyntaxNode node)
        {
            // 只考虑类和记录类型
            return node is ClassDeclarationSyntax or RecordDeclarationSyntax;
        }

        private static TypeInfo? GetSerializableTypeInfo(GeneratorSyntaxContext context)
        {
            var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node) as INamedTypeSymbol;
            if (symbol == null)
                return null;

            // 排除抽象类
            if (symbol.IsAbstract)
                return null;

            // 排除静态类
            if (symbol.IsStatic)
                return null;

            // 排除枚举
            if (symbol.TypeKind == TypeKind.Enum)
                return null;

            // 排除委托
            if (symbol.TypeKind == TypeKind.Delegate)
                return null;

            // 排除接口
            if (symbol.TypeKind == TypeKind.Interface)
                return null;

            // 排除 JsonSerializerContext 本身及其子类
            if (IsJsonSerializerContext(symbol))
                return null;

            // 排除 JsonConverter 本身及其子类
            if (IsJsonConverter(symbol))
                return null;

            // 排除继承自忽略基类列表的类型
            if (InheritsFromIgnoredBaseClass(symbol))
                return null;

            // 排除带有忽略特性列表的类型
            if (HasIgnoredAttribute(symbol))
                return null;

            // 排除系统命名空间
            var namespaceName = symbol.ContainingNamespace?.ToDisplayString() ?? "";
            if (IsSystemNamespace(namespaceName))
                return null;

            // 排除 Generated 命名空间
            if (namespaceName.Contains("Generated"))
                return null;

            // 排除匿名类型
            if (symbol.IsAnonymousType)
                return null;

            // 排除接口类型
            if (symbol.TypeKind == TypeKind.Interface)
                return null;

            // 只包含 public 类型
            if (symbol.DeclaredAccessibility != Accessibility.Public)
                return null;

            // 判断是否是 record 类型
            var isRecord = symbol.IsRecord;

            // 如果不是 record 类型，且继承了自定义接口，则忽略（通常是 DI 注入的服务类）
            // record 类型默认会继承 IEquatable<T> 等系统接口，不应被过滤
            if (!isRecord && HasCustomInterfaces(symbol))
                return null;

            // 获取所有可序列化的属性（包括基类的属性）
            var properties = GetSerializablePropertiesIncludingBase(symbol);

            // 如果实体类及其基类都没有任何属性，则忽略
            if (properties.Count == 0)
                return null;

            // 对于 record 类型，获取主构造函数的参数
            var primaryConstructorParameters = new List<ConstructorParameterInfo>();
            if (isRecord)
            {
                var primaryConstructor = symbol.Constructors
                    .FirstOrDefault(c => c.Parameters.Length > 0 && c.DeclaredAccessibility == Accessibility.Public);

                if (primaryConstructor != null)
                {
                    foreach (var param in primaryConstructor.Parameters)
                    {
                        primaryConstructorParameters.Add(new ConstructorParameterInfo
                        {
                            Name = param.Name,
                            Type = param.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                            IsNullable = param.Type.NullableAnnotation == NullableAnnotation.Annotated ||
                                        param.Type.IsReferenceType
                        });
                    }
                }
            }

            return new TypeInfo
            {
                TypeName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                SimpleName = symbol.Name,
                Namespace = namespaceName,
                Properties = properties,
                IsRecord = isRecord,
                PrimaryConstructorParameters = primaryConstructorParameters
            };
        }

        /// <summary>
        /// 递归获取实体类及其基类的所有可序列化属性
        /// </summary>
        private static List<PropertyInfo> GetSerializablePropertiesIncludingBase(INamedTypeSymbol symbol)
        {
            var allProperties = new List<PropertyInfo>();
            
            // 递归获取基类的属性
            CollectPropertiesRecursive(symbol, allProperties);
            
            return allProperties;
        }

        /// <summary>
        /// 递归收集属性
        /// </summary>
        private static void CollectPropertiesRecursive(INamedTypeSymbol symbol, List<PropertyInfo> properties)
        {
            // 获取当前类的属性
            var currentProperties = GetSerializableProperties(symbol);
            properties.AddRange(currentProperties);

            // 递归获取基类的属性
            if (symbol.BaseType != null && 
                symbol.BaseType.SpecialType != SpecialType.System_Object &&
                !IsJsonSerializerContext(symbol.BaseType) &&
                !IsJsonConverter(symbol.BaseType))
            {
                CollectPropertiesRecursive(symbol.BaseType, properties);
            }
        }

        private static List<PropertyInfo> GetSerializableProperties(INamedTypeSymbol symbol)
        {
            var properties = new List<PropertyInfo>();

            foreach (var member in symbol.GetMembers())
            {
                if (member is IPropertySymbol property)
                {
                    // 排除静态属性
                    if (property.IsStatic)
                        continue;

                    // 排除只写属性
                    if (property.IsWriteOnly)
                        continue;

                    // 排除索引器
                    if (property.IsIndexer)
                        continue;

                    // 排除 protected 属性（如 record 的 EqualityContract）
                    if (property.DeclaredAccessibility == Accessibility.Protected ||
                        property.DeclaredAccessibility == Accessibility.ProtectedAndInternal ||
                        property.DeclaredAccessibility == Accessibility.ProtectedOrInternal)
                        continue;

                    // 排除带有 [JsonIgnore] 特性的属性
                    if (property.GetAttributes().Any(a =>
                        a.AttributeClass?.ToDisplayString() == "System.Text.Json.Serialization.JsonIgnoreAttribute"))
                        continue;

                    // 排除接口类型的属性（通常是 DI 注入的属性）
                    if (property.Type.TypeKind == TypeKind.Interface)
                        continue;

                    // 确保属性有 getter
                    if (property.GetMethod == null)
                        continue;

                    properties.Add(new PropertyInfo
                    {
                        Name = property.Name,
                        Type = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        CanWrite = property.SetMethod != null,
                        IsInitOnly = property.SetMethod?.IsInitOnly == true,
                        IsNullable = property.Type.NullableAnnotation == NullableAnnotation.Annotated ||
                                    property.Type.IsReferenceType
                    });
                }
            }

            return properties;
        }

        private static bool IsJsonSerializerContext(INamedTypeSymbol symbol)
        {
            var baseType = symbol.BaseType;
            while (baseType != null)
            {
                if (baseType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    == "System.Text.Json.Serialization.JsonSerializerContext")
                    return true;

                baseType = baseType.BaseType;
            }
            return false;
        }

        private static bool IsJsonConverter(INamedTypeSymbol symbol)
        {
            var baseType = symbol.BaseType;
            while (baseType != null)
            {
                if (baseType.ToDisplayString().StartsWith("System.Text.Json.Serialization.JsonConverter"))
                    return true;

                baseType = baseType.BaseType;
            }
            return false;
        }

        /// <summary>
        /// 检查类型是否继承自忽略基类列表中的基类
        /// </summary>
        private static bool InheritsFromIgnoredBaseClass(INamedTypeSymbol symbol)
        {
            // 检查直接基类
            var baseType = symbol.BaseType;
            while (baseType != null)
            {
                // 获取基类的完整名称（含命名空间）
                var baseTypeFullName = baseType.OriginalDefinition.ToDisplayString();

                // 获取基类的简单名称（不含命名空间和泛型参数）
                var baseTypeName = baseType.OriginalDefinition.Name;

                // 检查完整名称匹配
                if (_ignoredBaseClassFullNames.Any(ignored =>
                    baseTypeFullName.StartsWith(ignored) ||
                    baseTypeFullName == ignored))
                {
                    return true;
                }

                // 检查简单名称匹配（用于泛型基类）
                if (_ignoredBaseClassNames.Contains(baseTypeName))
                {
                    return true;
                }

                baseType = baseType.BaseType;
            }

            // 检查接口
            foreach (var iface in symbol.Interfaces)
            {
                var ifaceFullName = iface.OriginalDefinition.ToDisplayString();
                var ifaceName = iface.OriginalDefinition.Name;

                if (_ignoredBaseClassFullNames.Any(ignored =>
                    ifaceFullName.StartsWith(ignored) ||
                    ifaceFullName == ignored))
                {
                    return true;
                }

                if (_ignoredBaseClassNames.Contains(ifaceName))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 检查类型是否带有忽略特性列表中的特性
        /// </summary>
        private static bool HasIgnoredAttribute(INamedTypeSymbol symbol)
        {
            return symbol.GetAttributes().Any(attr =>
            {
                var attrFullName = attr.AttributeClass?.ToDisplayString() ?? "";
                return _ignoredAttributeFullNames.Contains(attrFullName);
            });
        }

        private static bool ShouldExcludeComponentClass(INamedTypeSymbol symbol)
        {
            // 检查类是否有 [Component] 特性，如果有就排除（DI 容器管理的类不应该被序列化）
            return symbol.GetAttributes().Any(a =>
                a.AttributeClass?.Name == "ComponentAttribute" ||
                a.AttributeClass?.ToDisplayString().Contains("ComponentAttribute") == true);
        }

        /// <summary>
        /// 判断是否继承了自定义接口（排除系统接口）
        /// 系统接口如 IEquatable<T>, IComparable<T>, IEnumerable<T>, IDisposable 等不应被过滤
        /// </summary>
        private static bool HasCustomInterfaces(INamedTypeSymbol symbol)
        {
            foreach (var iface in symbol.Interfaces)
            {
                var interfaceNamespace = iface.ContainingNamespace?.ToDisplayString() ?? "";
                
                // 如果接口不在 System/Microsoft 命名空间，则认为是自定义接口
                if (!IsSystemNamespace(interfaceNamespace))
                    return true;
            }
            
            return false;
        }

        private static bool IsSystemNamespace(string namespaceName)
        {
            if (string.IsNullOrEmpty(namespaceName))
                return false;

            // 排除所有 System.* 命名空间
            return namespaceName.StartsWith("System") ||
                   namespaceName.StartsWith("Microsoft") ||
                   namespaceName.StartsWith("Newtonsoft");
        }

        private static string GenerateJsonConverters(IEnumerable<TypeInfo> types)
        {
            var allTypes = types.Distinct().ToList();

            if (allTypes.Count == 0)
            {
                return "// <auto-generated/>\n// No serializable types found.";
            }

            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("// This file is auto-generated by Cyber.AOT JsonContextGenerator");
            sb.AppendLine("// Custom JsonConverters with optimized serialization logic");
            sb.AppendLine("// Configuration: PropertyNamingPolicy=CamelCase, WriteIndented=false, IgnoreNullValues=true");
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Text.Json;");
            sb.AppendLine("using System.Text.Json.Serialization;");
            sb.AppendLine("using System.Text.Json.Serialization.Metadata;");
            sb.AppendLine("using System.Collections.Generic;");
            // 引入额外 Converter 的命名空间
            foreach (var ns in _additionalConverters.Select(c => c.UsingNamespace).Distinct())
            {
                sb.AppendLine($"using {ns};");
            }
            sb.AppendLine();

            // 为每个类型生成 JsonConverter
            foreach (var type in allTypes)
            {
                GenerateJsonConverter(sb, type);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 生成独立的 JsonTypeInfos 文件
        /// </summary>
        private static string GenerateJsonTypeInfosSource(List<TypeInfo> types)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("// This file is auto-generated by Cyber.AOT JsonContextGenerator");
            sb.AppendLine("// Static JsonTypeInfos class with cached JsonTypeInfo for AOT-safe serialization");
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Text.Json;");
            sb.AppendLine("using System.Text.Json.Serialization;");
            sb.AppendLine("using System.Text.Json.Serialization.Metadata;");
            sb.AppendLine("using System.Collections.Generic;");
            // 引入额外 Converter 的命名空间
            foreach (var ns in _additionalConverters.Select(c => c.UsingNamespace).Distinct())
            {
                sb.AppendLine($"using {ns};");
            }
            sb.AppendLine();

            GenerateJsonTypeInfos(sb, types);

            return sb.ToString();
        }

        private static void GenerateJsonConverter(StringBuilder sb, TypeInfo type)
        {
            var converterName = $"{type.SimpleName}JsonConverter";
            var indent = string.IsNullOrEmpty(type.Namespace) ? "" : "    ";

            // 如果类型在全局命名空间，不生成 namespace 声明
            if (!string.IsNullOrEmpty(type.Namespace))
            {
                sb.AppendLine($"namespace {type.Namespace}");
                sb.AppendLine("{");
            }

            sb.AppendLine($"{indent}public class {converterName} : JsonConverter<{type.TypeName}>");
            sb.AppendLine($"{indent}{{");

            // 生成 Write 方法
            GenerateWriteMethod(sb, type, indent + "    ");

            sb.AppendLine();

            // 生成 Read 方法
            GenerateReadMethod(sb, type, indent + "    ");

            sb.AppendLine($"{indent}}}");

            // 如果类型在全局命名空间，不生成 namespace 结束标记
            if (!string.IsNullOrEmpty(type.Namespace))
            {
                sb.AppendLine("}");
            }

            sb.AppendLine();
        }

        private static void GenerateWriteMethod(StringBuilder sb, TypeInfo type, string indent)
        {
            sb.AppendLine($"{indent}public override void Write(Utf8JsonWriter writer, {type.TypeName} value, JsonSerializerOptions options)");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    if (value == null)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        writer.WriteNullValue();");
            sb.AppendLine($"{indent}        return;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();

            // 检查是否为集合类型
            if (IsCollectionType(type.TypeName))
            {
                // 集合类型：生成数组写入逻辑
                GenerateWriteValueForType(sb, type.TypeName, "value", indent + "    ");
            }
            else if (IsDictionaryType(type.TypeName))
            {
                // 字典类型：生成对象写入逻辑（键值对）
                GenerateWriteValueForType(sb, type.TypeName, "value", indent + "    ");
            }
            else
            {
                // 普通类型：生成对象写入逻辑
                sb.AppendLine($"{indent}    writer.WriteStartObject();");
                sb.AppendLine();

                // 对于每个属性，生成写入逻辑
                foreach (var prop in type.Properties)
                {
                    // 将属性名转换为 camelCase
                    var jsonName = ConvertToCamelCase(prop.Name);

                    sb.AppendLine($"{indent}    // Property: {prop.Name}");
                    sb.AppendLine($"{indent}    var {prop.Name}Value = value.{prop.Name};");

                    // 如果配置了忽略 null 值
                    if (prop.IsNullable)
                    {
                        sb.AppendLine($"{indent}    if ({prop.Name}Value != null)");
                        sb.AppendLine($"{indent}    {{");
                        sb.AppendLine($"{indent}        writer.WritePropertyName(\"{jsonName}\");");
                        GenerateWriteValue(sb, prop, $"{prop.Name}Value", indent + "        ");
                        sb.AppendLine($"{indent}    }}");
                    }
                    else
                    {
                        sb.AppendLine($"{indent}    writer.WritePropertyName(\"{jsonName}\");");
                        GenerateWriteValue(sb, prop, $"{prop.Name}Value", indent + "    ");
                    }

                    sb.AppendLine();
                }

                sb.AppendLine($"{indent}    writer.WriteEndObject();");
            }

            sb.AppendLine($"{indent}}}");
        }

        private static void GenerateWriteValue(StringBuilder sb, PropertyInfo prop, string valueExpression, string indent)
        {
            var type = prop.Type;

            // 处理可空类型 (例如: int?, long?, DateTime? 等)
            if (type.EndsWith("?"))
            {
                var baseType = type.Substring(0, type.Length - 1);
                // 对于可空类型，由于已经在外层检查了 != null，所以这里直接写入基础类型值
                GenerateWriteValueForType(sb, baseType, valueExpression + ".Value", indent);
                return;
            }

            GenerateWriteValueForType(sb, type, valueExpression, indent);
        }

        private static void GenerateWriteValueForType(StringBuilder sb, string type, string valueExpression, string indent)
        {
            // 根据类型生成不同的写入逻辑
            if (type == "string" || type == "global::System.String")
            {
                sb.AppendLine($"{indent}writer.WriteStringValue({valueExpression});");
            }
            else if (type == "int" || type == "global::System.Int32")
            {
                sb.AppendLine($"{indent}writer.WriteNumberValue({valueExpression});");
            }
            else if (type == "long" || type == "global::System.Int64")
            {
                sb.AppendLine($"{indent}writer.WriteNumberValue({valueExpression});");
            }
            else if (type == "double" || type == "global::System.Double")
            {
                sb.AppendLine($"{indent}writer.WriteNumberValue({valueExpression});");
            }
            else if (type == "float" || type == "global::System.Single")
            {
                sb.AppendLine($"{indent}writer.WriteNumberValue({valueExpression});");
            }
            else if (type == "decimal" || type == "global::System.Decimal")
            {
                sb.AppendLine($"{indent}writer.WriteNumberValue({valueExpression});");
            }
            else if (type == "bool" || type == "global::System.Boolean")
            {
                sb.AppendLine($"{indent}writer.WriteBooleanValue({valueExpression});");
            }
            else if (type == "DateTime" || type == "global::System.DateTime")
            {
                sb.AppendLine($"{indent}writer.WriteStringValue({valueExpression}.ToString(Cyber.AppGlobalSettings.CyberSetting.JsonDateTimeFormat));");
            }
            else if (type == "DateTimeOffset" || type == "global::System.DateTimeOffset")
            {
                sb.AppendLine($"{indent}writer.WriteStringValue({valueExpression}.ToString(Cyber.AppGlobalSettings.CyberSetting.JsonDateTimeFormat));");
            }
            else if (type == "Guid" || type == "global::System.Guid")
            {
                sb.AppendLine($"{indent}writer.WriteStringValue({valueExpression});");
            }
            else if (type == "byte" || type == "global::System.Byte")
            {
                sb.AppendLine($"{indent}writer.WriteNumberValue({valueExpression});");
            }
            else if (type == "short" || type == "global::System.Int16")
            {
                sb.AppendLine($"{indent}writer.WriteNumberValue({valueExpression});");
            }
            else if (type == "ushort" || type == "global::System.UInt16")
            {
                sb.AppendLine($"{indent}writer.WriteNumberValue({valueExpression});");
            }
            else if (type == "uint" || type == "global::System.UInt32")
            {
                sb.AppendLine($"{indent}writer.WriteNumberValue({valueExpression});");
            }
            else if (type == "ulong" || type == "global::System.UInt64")
            {
                sb.AppendLine($"{indent}writer.WriteNumberValue({valueExpression});");
            }
            else if (type == "char" || type == "global::System.Char")
            {
                sb.AppendLine($"{indent}writer.WriteStringValue({valueExpression}.ToString());");
            }
            else if (IsCollectionType(type))
            {
                // 集合类型：手动展开数组，使用 converter 序列化每个元素（最快速度）
                var elementType = ExtractElementType(type);
                if (elementType != null)
                {
                    sb.AppendLine($"{indent}writer.WriteStartArray();");
                    sb.AppendLine($"{indent}if ({valueExpression} != null)");
                    sb.AppendLine($"{indent}{{");
                    sb.AppendLine($"{indent}    foreach (var item in {valueExpression})");
                    sb.AppendLine($"{indent}    {{");

                    // 根据元素类型决定如何序列化每个元素
                    if (IsPrimitiveOrEnumType(elementType))
                    {
                        // 基本类型或枚举：直接写入值
                        GenerateWriteValueForType(sb, elementType, "item", indent + "        ");
                    }
                    else
                    {
                        // 复杂类型：直接使用 converter（最快速度）
                        var converterName = GetConverterName(elementType);
                        sb.AppendLine($"{indent}        var converter = new {converterName}();");
                        sb.AppendLine($"{indent}        converter.Write(writer, item, options);");
                        sb.AppendLine($"{indent}        converter = null;");
                    }

                    sb.AppendLine($"{indent}    }}");
                    sb.AppendLine($"{indent}}}");
                    sb.AppendLine($"{indent}writer.WriteEndArray();");
                }
                else
                {
                    // 无法提取元素类型，使用默认序列化
                    sb.AppendLine($"{indent}JsonSerializer.Serialize(writer, {valueExpression}, options);");
                }
            }
            else if (IsDictionaryType(type))
            {
                // 字典类型：手动展开，使用 converter 序列化值（最快速度）
                var (keyType, valueType) = ExtractDictionaryTypes(type);
                if (keyType != null && valueType != null)
                {
                    sb.AppendLine($"{indent}writer.WriteStartObject();");
                    sb.AppendLine($"{indent}if ({valueExpression} != null)");
                    sb.AppendLine($"{indent}{{");
                    sb.AppendLine($"{indent}    foreach (var kvp in {valueExpression})");
                    sb.AppendLine($"{indent}    {{");
                    sb.AppendLine($"{indent}        writer.WritePropertyName(kvp.Key.ToString());");

                    // 根据值类型决定如何序列化
                    if (IsPrimitiveOrEnumType(valueType))
                    {
                        GenerateWriteValueForType(sb, valueType, "kvp.Value", indent + "        ");
                    }
                    else
                    {
                        var converterName = GetConverterName(valueType);
                        sb.AppendLine($"{indent}        var converter = new {converterName}();");
                        sb.AppendLine($"{indent}        converter.Write(writer, kvp.Value, options);");
                        sb.AppendLine($"{indent}        converter = null;");
                    }

                    sb.AppendLine($"{indent}    }}");
                    sb.AppendLine($"{indent}}}");
                    sb.AppendLine($"{indent}writer.WriteEndObject();");
                }
                else
                {
                    sb.AppendLine($"{indent}JsonSerializer.Serialize(writer, {valueExpression}, options);");
                }
            }
            else if (IsEnumType(type))
            {
                // 枚举类型：转换为 int 写入
                sb.AppendLine($"{indent}writer.WriteNumberValue((int){valueExpression});");
            }
            else
            {
                // 对于复杂类型，直接调用 converter 的 Write 方法，避免 Reflection 和 IL 警告
                var converterName = GetConverterName(type);
                sb.AppendLine($"{indent}new {converterName}().Write(writer, {valueExpression}, options);");
            }
        }

        /// <summary>
        /// 判断类型是否为集合类型
        /// </summary>
        private static bool IsCollectionType(string type)
        {
            // 检查是否是数组
            if (type.EndsWith("[]"))
                return true;

            // 检查是否是泛型集合类型
            var collectionPrefixes = new[]
            {
                "List<",
                "global::System.Collections.Generic.List<",
                "IList<",
                "global::System.Collections.Generic.IList<",
                "IEnumerable<",
                "global::System.Collections.Generic.IEnumerable<",
                "ICollection<",
                "global::System.Collections.Generic.ICollection<",
                "IReadOnlyList<",
                "global::System.Collections.Generic.IReadOnlyList<",
                "IReadOnlyCollection<",
                "global::System.Collections.Generic.IReadOnlyCollection<",
                "HashSet<",
                "global::System.Collections.Generic.HashSet<"
            };

            return collectionPrefixes.Any(prefix => type.StartsWith(prefix) || type.Contains(prefix));
        }

        /// <summary>
        /// 判断类型是否为字典类型
        /// </summary>
        private static bool IsDictionaryType(string type)
        {
            var dictionaryPrefixes = new[]
            {
                "Dictionary<",
                "global::System.Collections.Generic.Dictionary<",
                "IDictionary<",
                "global::System.Collections.Generic.IDictionary<",
                "IReadOnlyDictionary<",
                "global::System.Collections.Generic.IReadOnlyDictionary<"
            };

            return dictionaryPrefixes.Any(prefix => type.StartsWith(prefix) || type.Contains(prefix));
        }

        /// <summary>
        /// 判断类型是否为 List/IList 类型（需要过滤掉，使用 IEnumerable<实体> 代替）
        /// 注意：IEnumerable<实体> 不会被过滤，因为这是统一的集合转换器
        /// </summary>
        private static bool IsListType(string type)
        {
            return type.StartsWith("List<") ||
                   type.StartsWith("global::System.Collections.Generic.List<") ||
                   type.StartsWith("IList<") ||
                   type.StartsWith("global::System.Collections.Generic.IList<");
        }

        /// <summary>
        /// 从集合类型中提取元素类型
        /// 例如：List<Test.SMSTemplate> -> Test.SMSTemplate
        /// </summary>
        private static string? ExtractElementType(string collectionType)
        {
            // 处理数组类型
            if (collectionType.EndsWith("[]"))
            {
                return collectionType.Substring(0, collectionType.Length - 2);
            }

            // 处理泛型集合类型
            var match = System.Text.RegularExpressions.Regex.Match(collectionType, @"<(.+)>");
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            return null;
        }

        /// <summary>
        /// 从字典类型中提取键和值的类型
        /// 例如：Dictionary<string, Test.User> -> (string, Test.User)
        /// </summary>
        private static (string? KeyType, string? ValueType) ExtractDictionaryTypes(string dictionaryType)
        {
            var match = System.Text.RegularExpressions.Regex.Match(dictionaryType, @"<(.+),\s*(.+)>");
            if (match.Success)
            {
                return (match.Groups[1].Value.Trim(), match.Groups[2].Value.Trim());
            }

            return (null, null);
        }

        /// <summary>
        /// 获取类型在 JsonTypeInfos 中的属性名称
        /// </summary>
        private static string GetJsonTypeInfoPropertyName(string type)
        {
            // 处理 global:: 前缀
            var typeName = type.Replace("global::", "");

            // 如果有命名空间，取最后一部分作为属性名
            if (typeName.Contains(".") && !typeName.Contains("<"))
            {
                typeName = typeName.Substring(typeName.LastIndexOf('.') + 1);
            }

            // 处理泛型：替换 < > , 为 _
            if (typeName.Contains("<"))
            {
                typeName = typeName.Replace("<", "_").Replace(">", "").Replace(", ", "_").Replace(",", "_");
            }

            return typeName;
        }

        /// <summary>
        /// 判断类型是否为枚举类型
        /// </summary>
        private static bool IsEnumType(string type)
        {
            // 已知的枚举类型
            var knownEnums = new[]
            {
                "Cyber.ResultCode",
                "global::Cyber.ResultCode"
            };

            if (knownEnums.Contains(type))
                return true;

            // 简单启发式：如果类型名以 Code, Status, Type, Kind, State 等结尾，可能是枚举
            // 但这不准确，所以主要依赖已知枚举列表
            return false;
        }

        /// <summary>
        /// 判断类型是否为基本类型或枚举类型（可以直接写入值，不需要 JsonTypeInfo）
        /// </summary>
        private static bool IsPrimitiveOrEnumType(string type)
        {
            var primitiveTypes = new[]
            {
                "string", "global::System.String",
                "int", "global::System.Int32",
                "long", "global::System.Int64",
                "double", "global::System.Double",
                "float", "global::System.Single",
                "decimal", "global::System.Decimal",
                "bool", "global::System.Boolean",
                "DateTime", "global::System.DateTime",
                "DateTimeOffset", "global::System.DateTimeOffset",
                "Guid", "global::System.Guid",
                "byte", "global::System.Byte",
                "short", "global::System.Int16",
                "ushort", "global::System.UInt16",
                "uint", "global::System.UInt32",
                "ulong", "global::System.UInt64",
                "char", "global::System.Char"
            };

            if (primitiveTypes.Contains(type))
                return true;

            if (IsEnumType(type))
                return true;

            return false;
        }

        private static string GetConverterName(string typeName)
        {
            // 从类型名称提取 converter 名称
            // 例如:
            //   "Test.TestDI66" -> "Test.TestDI66JsonConverter"
            //   "global::Test.TestDI66" -> "Test.TestDI66JsonConverter"
            //   "TestDI66" -> "TestDI66JsonConverter"

            // 移除 global:: 前缀
            var normalizedType = typeName.StartsWith("global::") ? typeName.Substring(8) : typeName;

            // 在类型名称后添加 JsonConverter
            return normalizedType + "JsonConverter";
        }

        private static void GenerateReadMethod(StringBuilder sb, TypeInfo type, string indent)
        {
            sb.AppendLine($"{indent}public override {type.TypeName} Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    if (reader.TokenType == JsonTokenType.Null)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        return default({type.TypeName})!;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();

            // 检查是否为集合类型
            if (IsCollectionType(type.TypeName))
            {
                // 集合类型：生成数组读取逻辑
                sb.AppendLine($"{indent}    if (reader.TokenType != JsonTokenType.StartArray)");
                sb.AppendLine($"{indent}    {{");
                sb.AppendLine($"{indent}        throw new JsonException(\"Expected StartArray token\");");
                sb.AppendLine($"{indent}    }}");
                sb.AppendLine();
                
                GenerateReadValueForType(sb, type.TypeName, "list", indent + "    ");
                sb.AppendLine($"{indent}    return list;");
            }
            else if (IsDictionaryType(type.TypeName))
            {
                // 字典类型：生成对象读取逻辑（键值对）
                sb.AppendLine($"{indent}    if (reader.TokenType != JsonTokenType.StartObject)");
                sb.AppendLine($"{indent}    {{");
                sb.AppendLine($"{indent}        throw new JsonException(\"Expected StartObject token\");");
                sb.AppendLine($"{indent}    }}");
                sb.AppendLine();
                
                GenerateReadValueForType(sb, type.TypeName, "dict", indent + "    ");
                sb.AppendLine($"{indent}    return dict;");
            }
            else
            {
                // 普通类型：生成对象读取逻辑
                sb.AppendLine($"{indent}    if (reader.TokenType != JsonTokenType.StartObject)");
                sb.AppendLine($"{indent}    {{");
                sb.AppendLine($"{indent}        throw new JsonException(\"Expected StartObject token\");");
                sb.AppendLine($"{indent}    }}");
                sb.AppendLine();

                // 创建临时变量存储属性值
                if (type.IsRecord && type.PrimaryConstructorParameters.Count > 0)
                {
                    // 对于 record 类型，创建构造函数参数的临时变量（不生成 Found 变量以避免警告）
                    foreach (var param in type.PrimaryConstructorParameters)
                    {
                        sb.AppendLine($"{indent}    var {param.Name}Value = default({param.Type});");
                    }

                    // 对于其他属性（非构造函数参数），也创建临时变量
                    var nonConstructorProps = type.Properties
                        .Where(p => !type.PrimaryConstructorParameters.Any(cp => cp.Name == p.Name))
                        .ToList();

                    foreach (var prop in nonConstructorProps)
                    {
                        sb.AppendLine($"{indent}    var {prop.Name}Value = default({prop.Type});");
                        // 只对可写属性生成 Found 变量
                        if (prop.CanWrite && !prop.IsInitOnly)
                        {
                            sb.AppendLine($"{indent}    var {prop.Name}Found = false;");
                        }
                    }
                }
                else
                {
                    // 对于普通类，先创建实例，然后创建属性临时变量
                    sb.AppendLine($"{indent}    var result = new {type.TypeName}();");
                    sb.AppendLine();

                    foreach (var prop in type.Properties)
                    {
                        sb.AppendLine($"{indent}    var {prop.Name}Value = default({prop.Type});");
                        // 只对可写属性生成 Found 变量
                        if (prop.CanWrite && !prop.IsInitOnly)
                        {
                            sb.AppendLine($"{indent}    var {prop.Name}Found = false;");
                        }
                    }
                }

                sb.AppendLine();
                sb.AppendLine($"{indent}    while (reader.Read())");
                sb.AppendLine($"{indent}    {{");
                sb.AppendLine($"{indent}        if (reader.TokenType == JsonTokenType.EndObject)");
                sb.AppendLine($"{indent}        {{");
                sb.AppendLine($"{indent}            break;");
                sb.AppendLine($"{indent}        }}");
                sb.AppendLine();
                sb.AppendLine($"{indent}        if (reader.TokenType != JsonTokenType.PropertyName)");
                sb.AppendLine($"{indent}        {{");
                sb.AppendLine($"{indent}            throw new JsonException(\"Expected PropertyName token\");");
                sb.AppendLine($"{indent}        }}");
                sb.AppendLine();
                sb.AppendLine($"{indent}        var propertyName = reader.GetString();");
                sb.AppendLine($"{indent}        reader.Read();");
                sb.AppendLine();

                // 为每个属性生成匹配逻辑
                foreach (var prop in type.Properties)
                {
                    var jsonName = ConvertToCamelCase(prop.Name);
                    sb.AppendLine($"{indent}        if (propertyName?.ToLower() == \"{jsonName.ToLower()}\")");
                    sb.AppendLine($"{indent}        {{");

                    // 对于 record 类型的构造函数参数，不设置 Found 标志
                    if (type.IsRecord && type.PrimaryConstructorParameters.Count > 0 &&
                        type.PrimaryConstructorParameters.Any(cp => cp.Name == prop.Name))
                    {
                        GenerateReadValue(sb, prop, indent + "            ");
                    }
                    else if (prop.CanWrite && !prop.IsInitOnly)
                    {
                        // 只对可写属性设置 Found 标志
                        sb.AppendLine($"{indent}            {prop.Name}Found = true;");
                        GenerateReadValue(sb, prop, indent + "            ");
                    }
                    else
                    {
                        // 只读属性只读取值，不设置 Found 标志
                        GenerateReadValue(sb, prop, indent + "            ");
                    }

                    sb.AppendLine($"{indent}            continue;");
                    sb.AppendLine($"{indent}        }}");
                }

                sb.AppendLine();
                sb.AppendLine($"{indent}        // Skip unknown properties");
                sb.AppendLine($"{indent}        reader.Skip();");
                sb.AppendLine($"{indent}    }}");
                sb.AppendLine();

                // 创建结果对象
                if (type.IsRecord && type.PrimaryConstructorParameters.Count > 0)
                {
                    // 对于 record 类型，使用主构造函数创建实例
                    sb.AppendLine($"{indent}    // Create record instance using primary constructor");

                    // 使用构造函数参数创建实例，对参数添加 null-forgiving 操作符
                    var constructorArgs = type.PrimaryConstructorParameters
                        .Select(p => $"{p.Name}Value!")
                        .JoinWith(", ");

                    sb.AppendLine($"{indent}    var result = new {type.TypeName}({constructorArgs});");
                    sb.AppendLine();

                    // 对于其他属性，使用对象初始值设定项
                    var nonConstructorProps = type.Properties
                        .Where(p => !type.PrimaryConstructorParameters.Any(cp => cp.Name == p.Name) && p.CanWrite)
                        .ToList();

                    if (nonConstructorProps.Count > 0)
                    {
                        sb.AppendLine($"{indent}    // Set additional properties");
                        foreach (var prop in nonConstructorProps)
                        {
                            if (!prop.IsInitOnly)
                            {
                                sb.AppendLine($"{indent}    if ({prop.Name}Found)");
                                sb.AppendLine($"{indent}    {{");
                                sb.AppendLine($"{indent}        result.{prop.Name} = {prop.Name}Value!;");
                                sb.AppendLine($"{indent}    }}");
                            }
                            else
                            {
                                // init-only 属性不能在创建后设置，跳过
                                sb.AppendLine($"{indent}    // Note: {prop.Name} is init-only and cannot be set after construction");
                            }
                        }
                    }
                }
                else
                {
                    // 对于普通类，将读取的值赋给结果对象
                    foreach (var prop in type.Properties)
                    {
                        if (prop.CanWrite && !prop.IsInitOnly)
                        {
                            sb.AppendLine($"{indent}    if ({prop.Name}Found)");
                            sb.AppendLine($"{indent}    {{");
                            sb.AppendLine($"{indent}        result.{prop.Name} = {prop.Name}Value!;");
                            sb.AppendLine($"{indent}    }}");
                        }
                    }
                }

                sb.AppendLine();
                sb.AppendLine($"{indent}    return result;");
            }

            sb.AppendLine($"{indent}}}");
        }

        private static void GenerateReadValue(StringBuilder sb, PropertyInfo prop, string indent)
        {
            var type = prop.Type;

            // 处理可空类型 (例如: int?, long?, DateTime? 等)
            if (type.EndsWith("?"))
            {
                var baseType = type.Substring(0, type.Length - 1);
                sb.AppendLine($"{indent}if (reader.TokenType == JsonTokenType.Null)");
                sb.AppendLine($"{indent}{{");
                sb.AppendLine($"{indent}    {prop.Name}Value = null;");
                sb.AppendLine($"{indent}}}");
                sb.AppendLine($"{indent}else");
                sb.AppendLine($"{indent}{{");
                GenerateReadValueForType(sb, baseType, prop.Name + "Value", indent + "    ");
                sb.AppendLine($"{indent}}}");
                return;
            }

            GenerateReadValueForType(sb, type, prop.Name + "Value", indent);
        }

        private static void GenerateReadValueForType(StringBuilder sb, string type, string valueName, string indent)
        {
            // 根据类型生成不同的读取逻辑
            if (type == "string" || type == "global::System.String")
            {
                // String 可能返回 null，使用 null-forgiving 操作符或添加检查
                sb.AppendLine($"{indent}{valueName} = reader.GetString() ?? string.Empty;");
            }
            else if (type == "int" || type == "global::System.Int32")
            {
                sb.AppendLine($"{indent}{valueName} = reader.GetInt32();");
            }
            else if (type == "long" || type == "global::System.Int64")
            {
                sb.AppendLine($"{indent}{valueName} = reader.GetInt64();");
            }
            else if (type == "double" || type == "global::System.Double")
            {
                sb.AppendLine($"{indent}{valueName} = reader.GetDouble();");
            }
            else if (type == "float" || type == "global::System.Single")
            {
                sb.AppendLine($"{indent}{valueName} = reader.GetSingle();");
            }
            else if (type == "decimal" || type == "global::System.Decimal")
            {
                sb.AppendLine($"{indent}{valueName} = reader.GetDecimal();");
            }
            else if (type == "bool" || type == "global::System.Boolean")
            {
                sb.AppendLine($"{indent}{valueName} = reader.GetBoolean();");
            }
            else if (type == "DateTime" || type == "global::System.DateTime")
            {
                sb.AppendLine($"{indent}{valueName} = reader.GetDateTime();");
            }
            else if (type == "DateTimeOffset" || type == "global::System.DateTimeOffset")
            {
                sb.AppendLine($"{indent}{valueName} = reader.GetDateTimeOffset();");
            }
            else if (type == "Guid" || type == "global::System.Guid")
            {
                sb.AppendLine($"{indent}{valueName} = reader.GetGuid();");
            }
            else if (type == "byte" || type == "global::System.Byte")
            {
                sb.AppendLine($"{indent}{valueName} = reader.GetByte();");
            }
            else if (type == "short" || type == "global::System.Int16")
            {
                sb.AppendLine($"{indent}{valueName} = reader.GetInt16();");
            }
            else if (type == "ushort" || type == "global::System.UInt16")
            {
                sb.AppendLine($"{indent}{valueName} = reader.GetUInt16();");
            }
            else if (type == "uint" || type == "global::System.UInt32")
            {
                sb.AppendLine($"{indent}{valueName} = reader.GetUInt32();");
            }
            else if (type == "ulong" || type == "global::System.UInt64")
            {
                sb.AppendLine($"{indent}{valueName} = reader.GetUInt64();");
            }
            else if (type == "char" || type == "global::System.Char")
            {
                sb.AppendLine($"{indent}var charStr = reader.GetString();");
                sb.AppendLine($"{indent}{valueName} = charStr != null && charStr.Length > 0 ? charStr[0] : default;");
            }
            else if (IsCollectionType(type))
            {
                // 集合类型：手动读取数组，使用 converter 反序列化每个元素（最快速度）
                var elementType = ExtractElementType(type);
                if (elementType != null)
                {
                    var listTypeName = $"global::System.Collections.Generic.List<{elementType}>";

                    sb.AppendLine($"{indent}var list = new {listTypeName}();");
                    sb.AppendLine($"{indent}while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)");
                    sb.AppendLine($"{indent}{{");

                    // 根据元素类型决定如何读取
                    if (IsPrimitiveOrEnumType(elementType))
                    {
                        // 基本类型或枚举：直接读取值
                        GenerateReadValueForType(sb, elementType, "element", indent + "    ");
                        sb.AppendLine($"{indent}    list.Add(element);");
                    }
                    else
                    {
                        // 复杂类型：直接使用 converter（最快速度）
                        var converterName = GetConverterName(elementType);
                        var typeNameForTypeof = elementType.StartsWith("global::") ? elementType.Substring(8) : elementType;
                        sb.AppendLine($"{indent}    var converter = new {converterName}();");
                        sb.AppendLine($"{indent}    var element = converter.Read(ref reader, typeof({typeNameForTypeof}), options)!;");
                        sb.AppendLine($"{indent}    list.Add(element);");
                        sb.AppendLine($"{indent}    converter = null;");
                    }

                    sb.AppendLine($"{indent}}}");

                    // 根据目标类型转换
                    if (valueName!="list" && (type.StartsWith("List<") || type.StartsWith("global::System.Collections.Generic.List<")))
                    {
                        sb.AppendLine($"{indent}{valueName} = list;");
                    }
                    else if (
                        valueName != "list" &&
                        (type.StartsWith("IList<") || type.StartsWith("global::System.Collections.Generic.IList<") ||
                             type.StartsWith("IEnumerable<") || type.StartsWith("global::System.Collections.Generic.IEnumerable<") ||
                             type.StartsWith("ICollection<") || type.StartsWith("global::System.Collections.Generic.ICollection<")))
                    {
                        sb.AppendLine($"{indent}{valueName} = list;");
                    }
                    else if (
                        valueName != "list" &&
                        (type.StartsWith("IReadOnlyList<") || type.StartsWith("global::System.Collections.Generic.IReadOnlyList<") ||
                             type.StartsWith("IReadOnlyCollection<") || type.StartsWith("global::System.Collections.Generic.IReadOnlyCollection<")))
                    {
                        sb.AppendLine($"{indent}{valueName} = list;");
                    }
                    else if (type.EndsWith("[]"))
                    {
                        sb.AppendLine($"{indent}{valueName} = list.ToArray();");
                    }
                    else if(valueName != "list")
                    {
                        sb.AppendLine($"{indent}{valueName} = list;");
                    }
                }
                else
                {
                    sb.AppendLine($"{indent}{valueName} = JsonSerializer.Deserialize<{type}>(ref reader, options)!;");
                }
            }
            else if (IsDictionaryType(type))
            {
                // 字典类型：手动读取，使用 converter 反序列化值（最快速度）
                var (keyType, valueType) = ExtractDictionaryTypes(type);
                if (keyType != null && valueType != null)
                {
                    var dictTypeName = $"global::System.Collections.Generic.Dictionary<{keyType}, {valueType}>";

                    sb.AppendLine($"{indent}var dict = new {dictTypeName}();");
                    sb.AppendLine($"{indent}while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)");
                    sb.AppendLine($"{indent}{{");
                    sb.AppendLine($"{indent}    if (reader.TokenType != JsonTokenType.PropertyName)");
                    sb.AppendLine($"{indent}    {{");
                    sb.AppendLine($"{indent}        throw new JsonException(\"Expected PropertyName token\");");
                    sb.AppendLine($"{indent}    }}");
                    sb.AppendLine($"{indent}    var key = reader.GetString();");
                    sb.AppendLine($"{indent}    reader.Read();");

                    // 根据值类型决定如何读取
                    if (IsPrimitiveOrEnumType(valueType))
                    {
                        GenerateReadValueForType(sb, valueType, "value", indent + "    ");
                    }
                    else
                    {
                        var converterName = GetConverterName(valueType);
                        var typeNameForTypeof = valueType.StartsWith("global::") ? valueType.Substring(8) : valueType;
                        sb.AppendLine($"{indent}    var converter = new {converterName}();");
                        sb.AppendLine($"{indent}    var value = converter.Read(ref reader, typeof({typeNameForTypeof}), options)!;");
                        sb.AppendLine($"{indent}    converter = null;");
                    }

                    // 根据键类型转换
                    if (keyType == "string" || keyType == "global::System.String")
                    {
                        sb.AppendLine($"{indent}    dict[key] = value;");
                    }
                    else if (keyType == "int" || keyType == "global::System.Int32")
                    {
                        sb.AppendLine($"{indent}    dict[int.Parse(key)] = value;");
                    }
                    else
                    {
                        sb.AppendLine($"{indent}    dict[({keyType})Convert.ChangeType(key, typeof({keyType}))] = value;");
                    }

                    sb.AppendLine($"{indent}}}");

                    // 根据目标类型转换
                    if (
                        valueName!="dict"&&
                        (type.StartsWith("Dictionary<") || type.StartsWith("global::System.Collections.Generic.Dictionary<") ||
                        type.StartsWith("IDictionary<") || type.StartsWith("global::System.Collections.Generic.IDictionary<")))
                    {
                        sb.AppendLine($"{indent}{valueName} = dict;");
                    }
                    else if(valueName != "dict")
                    {
                        sb.AppendLine($"{indent}{valueName} = dict;");
                    }
                }
                else
                {
                    sb.AppendLine($"{indent}{valueName} = JsonSerializer.Deserialize<{type}>(ref reader, options)!;");
                }
            }
            else if (IsEnumType(type))
            {
                // 枚举类型：读取 int 值然后转换为枚举
                var enumTypeName = type.StartsWith("global::") ? type.Substring(8) : type;
                sb.AppendLine($"{indent}{valueName} = ({enumTypeName})reader.GetInt32();");
            }
            else
            {
                // 对于复杂类型，直接调用 converter 的 Read 方法，避免 Reflection 和 IL 警告
                var converterName = GetConverterName(type);
                // 提取类型名称用于 typeof
                var typeNameForTypeof = type.StartsWith("global::") ? type.Substring(8) : type;
                sb.AppendLine($"{indent}var converter = new {converterName}();");
                sb.AppendLine($"{indent}{valueName} = converter.Read(ref reader, typeof({typeNameForTypeof}), options)!;");
                sb.AppendLine($"{indent}converter = null;");
            }
        }

        private static void GenerateJsonTypeInfos(StringBuilder sb, List<TypeInfo> types)
        {
            sb.AppendLine("namespace Cyber");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 静态 JsonTypeInfo 缓存，为每个已知类型提供预计算的序列化元数据。");
            sb.AppendLine("    /// 避免运行时反射，提升 AOT 兼容性和序列化性能。");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static class JsonTypeInfos");
            sb.AppendLine("    {");
           

            // 2. Converter 单例
            sb.AppendLine("        // =========================");
            sb.AppendLine("        // Converter 单例");
            sb.AppendLine("        // =========================");
            foreach (var type in types)
            {
                var converterReference = string.IsNullOrEmpty(type.Namespace)
                    ? type.SimpleName + "JsonConverter"
                    : type.Namespace + "." + type.SimpleName + "JsonConverter";
                sb.AppendLine($"        private static readonly {converterReference} {type.SimpleName}Converter = new();");
            }
            // 额外的 Converter
            foreach (var conv in _additionalConverters)
            {
                var typeNameOnly = conv.ConverterTypeFullName.Contains(".")
                    ? conv.ConverterTypeFullName.Substring(conv.ConverterTypeFullName.LastIndexOf('.') + 1).Split('`')[0]
                    : conv.ConverterTypeFullName;
                var fieldName = typeNameOnly.Replace("JsonConverter", "Converter").Replace("`", "_");
                sb.AppendLine($"        private static readonly {typeNameOnly} {fieldName} = new();");
            }
            sb.AppendLine();

            // 3. JsonTypeInfo 缓存
            sb.AppendLine("        // =========================");
            sb.AppendLine("        // JsonTypeInfo 缓存");
            sb.AppendLine("        // =========================");
            foreach (var type in types)
            {
                sb.AppendLine($"        private static JsonTypeInfo<{type.TypeName}>? _{CamelCase(type.SimpleName)};");
            }
            foreach (var conv in _additionalConverters)
            {
                var actualTypeName = conv.TargetTypeExpression;
                var simpleName = actualTypeName.Contains("<")
                    ? actualTypeName.Replace("<", "_").Replace(">", "").Replace(", ", "_").Replace(" ", "")
                    : (actualTypeName.Contains(".") 
                        ? actualTypeName.Substring(actualTypeName.LastIndexOf('.') + 1)
                        : actualTypeName);
                var cacheFieldName = CamelCase(simpleName);
                sb.AppendLine($"        private static JsonTypeInfo<{actualTypeName}>? _{cacheFieldName};");
            }
            sb.AppendLine();

            // 4. 公开属性（延迟初始化）
            sb.AppendLine("        // =========================");
            sb.AppendLine("        // 4. 公开属性（延迟初始化）");
            sb.AppendLine("        // =========================");
            foreach (var type in types)
            {
                var converterReference = string.IsNullOrEmpty(type.Namespace)
                    ? type.TypeName + "JsonConverter"
                    : type.Namespace + "." + type.TypeName + "JsonConverter";
                sb.AppendLine($"        public static JsonTypeInfo<{type.TypeName}> {type.SimpleName}");
                sb.AppendLine($"            => _{CamelCase(type.SimpleName)} ??= JsonMetadataServices.CreateValueInfo<{type.TypeName}>(AppGlobalSettings.Option, {type.SimpleName}Converter);");
                sb.AppendLine();
            }
            foreach (var conv in _additionalConverters)
            {
                var typeNameOnly = conv.ConverterTypeFullName.Contains(".")
                    ? conv.ConverterTypeFullName.Substring(conv.ConverterTypeFullName.LastIndexOf('.') + 1).Split('`')[0]
                    : conv.ConverterTypeFullName;
                var fieldName = typeNameOnly.Replace("JsonConverter", "Converter").Replace("`", "_");

                var actualTypeName = conv.TargetTypeExpression;
                var simpleName = actualTypeName.Contains("<")
                    ? actualTypeName.Replace("<", "_").Replace(">", "").Replace(", ", "_").Replace(" ", "")
                    : (actualTypeName.Contains(".") 
                        ? actualTypeName.Substring(actualTypeName.LastIndexOf('.') + 1)
                        : actualTypeName);
                
                var cacheFieldName = CamelCase(simpleName);
                
                sb.AppendLine($"        public static JsonTypeInfo<{actualTypeName}> {simpleName}");
                sb.AppendLine($"            => _{cacheFieldName} ??= JsonMetadataServices.CreateValueInfo<{actualTypeName}>(AppGlobalSettings.Option, {fieldName});");
                sb.AppendLine();
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        /// <summary>
        /// 首字母小写，用于生成字段名
        /// </summary>
        private static string CamelCase(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;
            return char.ToLowerInvariant(name[0]) + name.Substring(1);
        }

        private static string ConvertToCamelCase(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return propertyName;

            // PascalCase -> camelCase
            /*
            var firstChar = propertyName[0];
            if (char.IsUpper(firstChar))
            {
                return char.ToLower(firstChar) + propertyName.Substring(1);
            }
            */

            return propertyName;
        }

        private class TypeInfo
        {
            public string TypeName { get; set; } = "";
            public string SimpleName { get; set; } = "";
            public string Namespace { get; set; } = "";
            public List<PropertyInfo> Properties { get; set; } = new List<PropertyInfo>();
            public bool IsRecord { get; set; }
            public List<ConstructorParameterInfo> PrimaryConstructorParameters { get; set; } = new List<ConstructorParameterInfo>();
        }

        private class PropertyInfo
        {
            public string Name { get; set; } = "";
            public string Type { get; set; } = "";
            public bool CanWrite { get; set; }
            public bool IsInitOnly { get; set; }
            public bool IsNullable { get; set; }
        }

        private class ConstructorParameterInfo
        {
            public string Name { get; set; } = "";
            public string Type { get; set; } = "";
            public bool IsNullable { get; set; }
        }
    }

    // Helper extension for string joining
    internal static class StringExtensions
    {
        public static string JoinWith(this IEnumerable<string> strings, string separator)
        {
            return string.Join(separator, strings);
        }
    }
}

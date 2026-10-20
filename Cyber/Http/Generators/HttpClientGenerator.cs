using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cyber.AOT.MiniApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cyber.Http.Generators
{
    /// <summary>
    /// HTTP 客户端 Source Generator
    /// 扫描所有带有 HTTP 方法特性的接口，生成实现类和 CyberSetting.AddHttpClient 扩展方法
    /// 用户通过调用 option.AddHttpClient&lt;TInterface&gt;() 来注册需要的接口
    /// </summary>
    [Generator]
    public class HttpClientGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // 扫描所有带有 HTTP 方法特性的接口
            var interfaceDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => IsCandidateInterface(s),
                    transform: static (ctx, _) => GetInterfaceDeclaration(ctx))
                .Where(static m => m != null);

            // 合合编译信息
            var compilationAndInterfaces = context.CompilationProvider
                .Combine(interfaceDeclarations.Collect());

            // 注册生成源代码
            context.RegisterSourceOutput(compilationAndInterfaces,
                static (spc, source) => Execute(spc, source.Left, source.Right!));
        }

        private static bool IsCandidateInterface(SyntaxNode node)
        {
            if (node is InterfaceDeclarationSyntax interfaceDecl)
            {
                // 检查是否有 HTTP 方法特性
                foreach (var method in interfaceDecl.Members.OfType<MethodDeclarationSyntax>())
                {
                    foreach (var attrList in method.AttributeLists)
                    {
                        foreach (var attr in attrList.Attributes)
                        {
                            var attrName = attr.Name.ToString();
                            // 支持完整命名空间特性名，如 Cyber.Http.Attributes.HttpGet
                            if (attrName.EndsWith("HttpGet") || attrName.EndsWith("HttpGetAttribute") ||
                                attrName.EndsWith("HttpPost") || attrName.EndsWith("HttpPostAttribute") ||
                                attrName.EndsWith("HttpPut") || attrName.EndsWith("HttpPutAttribute") ||
                                attrName.EndsWith("HttpDelete") || attrName.EndsWith("HttpDeleteAttribute") ||
                                attrName.EndsWith("HttpPatch") || attrName.EndsWith("HttpPatchAttribute") ||
                                attrName.EndsWith("HttpHead") || attrName.EndsWith("HttpHeadAttribute") ||
                                attrName.EndsWith("HttpOptions") || attrName.EndsWith("HttpOptionsAttribute") ||
                                attrName == "HttpGet" || attrName == "HttpPost" ||
                                attrName == "HttpPut" || attrName == "HttpDelete" ||
                                attrName == "HttpPatch" || attrName == "HttpHead" ||
                                attrName == "HttpOptions")
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static InterfaceDeclarationSyntax? GetInterfaceDeclaration(GeneratorSyntaxContext context)
        {
            var interfaceDecl = (InterfaceDeclarationSyntax)context.Node;

            // 再次确认有 HTTP 方法特性
            foreach (var method in interfaceDecl.Members.OfType<MethodDeclarationSyntax>())
            {
                foreach (var attrList in method.AttributeLists)
                {
                    foreach (var attr in attrList.Attributes)
                    {
                        var attrName = attr.Name.ToString();
                        // 支持完整命名空间特性名，如 Cyber.Http.Attributes.HttpGet
                        if (attrName.EndsWith("HttpGet") || attrName.EndsWith("HttpGetAttribute") ||
                            attrName.EndsWith("HttpPost") || attrName.EndsWith("HttpPostAttribute") ||
                            attrName.EndsWith("HttpPut") || attrName.EndsWith("HttpPutAttribute") ||
                            attrName.EndsWith("HttpDelete") || attrName.EndsWith("HttpDeleteAttribute") ||
                            attrName.EndsWith("HttpPatch") || attrName.EndsWith("HttpPatchAttribute") ||
                            attrName.EndsWith("HttpHead") || attrName.EndsWith("HttpHeadAttribute") ||
                            attrName.EndsWith("HttpOptions") || attrName.EndsWith("HttpOptionsAttribute") ||
                            attrName == "HttpGet" || attrName == "HttpPost" ||
                            attrName == "HttpPut" || attrName == "HttpDelete" ||
                            attrName == "HttpPatch" || attrName == "HttpHead" ||
                            attrName == "HttpOptions")
                        {
                            return interfaceDecl;
                        }
                    }
                }
            }

            return null;
        }

        private static void Execute(SourceProductionContext context, Compilation compilation,
            IReadOnlyList<InterfaceDeclarationSyntax> interfaces)
        {
            if (interfaces.Count == 0) return;

            var httpInterfaces = ExtractHttpInterfaces(compilation, interfaces);

            if (httpInterfaces.Count == 0) return;

            // 生成实现类
            foreach (var httpInterface in httpInterfaces)
            {
                var sourceCode = GenerateImplementationClass(httpInterface, compilation);
                var fileName = $"{httpInterface.Namespace}.{httpInterface.InterfaceName}Impl.g.cs";
                context.AddSource(fileName, sourceCode);
            }

            // 生成 CyberSetting 扩展方法
            var registerExtension = GenerateCyberSettingExtension(httpInterfaces);
            context.AddSource("CyberSettingHttpClientExtensions.g.cs", registerExtension);
        }

        private static List<HttpInterfaceInfo> ExtractHttpInterfaces(Compilation compilation,
            IReadOnlyList<InterfaceDeclarationSyntax> interfaces)
        {
            var result = new List<HttpInterfaceInfo>();

            foreach (var interfaceDecl in interfaces)
            {
                var semanticModel = compilation.GetSemanticModel(interfaceDecl.SyntaxTree);
                var interfaceSymbol = semanticModel.GetDeclaredSymbol(interfaceDecl);
                
                if (interfaceSymbol == null) continue;

                var httpInterface = ExtractHttpInterfaceInfo(interfaceSymbol);
                if (httpInterface != null && httpInterface.Methods.Count > 0)
                {
                    result.Add(httpInterface);
                }
            }

            return result;
        }

        private static HttpInterfaceInfo? ExtractHttpInterfaceInfo(ITypeSymbol interfaceSymbol)
        {
            var httpInterface = new HttpInterfaceInfo
            {
                InterfaceName = interfaceSymbol.Name,
                InterfaceFullName = interfaceSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                Namespace = interfaceSymbol.ContainingNamespace.ToDisplayString()
            };

            // 检查接口级别的过滤器和验证
            foreach (var attr in interfaceSymbol.GetAttributes())
            {
                var attrName = attr.AttributeClass?.Name ?? "";
                if (attrName == "ApiFilterAttribute" || attrName == "ApiFilterAttributeBase")
                {
                    httpInterface.FilterType = attr.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                }
                if (attrName == "ValidationAttribute")
                {
                    httpInterface.HasValidation = true;
                }
            }

            // 提取方法
            foreach (var member in interfaceSymbol.GetMembers())
            {
                if (member is IMethodSymbol methodSymbol)
                {
                    var httpMethod = ExtractHttpMethodInfo(methodSymbol);
                    if (httpMethod != null)
                    {
                        httpInterface.Methods.Add(httpMethod);
                    }
                }
            }

            return httpInterface.Methods.Count > 0 ? httpInterface : null;
        }

        private static HttpMethodInfo? ExtractHttpMethodInfo(IMethodSymbol methodSymbol)
        {
            HttpMethodInfo? httpMethod = null;

            // 查找 HTTP 方法特性
            foreach (var attr in methodSymbol.GetAttributes())
            {
                var attrClass = attr.AttributeClass;
                if (attrClass == null) continue;

                var attrName = attrClass.Name;

                if (attrName.StartsWith("Http"))
                {
                    var httpMethodName = attrName switch
                    {
                        "HttpGetAttribute" => "GET",
                        "HttpPostAttribute" => "POST",
                        "HttpPutAttribute" => "PUT",
                        "HttpDeleteAttribute" => "DELETE",
                        "HttpPatchAttribute" => "PATCH",
                        "HttpHeadAttribute" => "HEAD",
                        "HttpOptionsAttribute" => "OPTIONS",
                        _ => ""
                    };

                    if (string.IsNullOrEmpty(httpMethodName)) continue;

                    var path = attr.ConstructorArguments.Length > 0
                        ? attr.ConstructorArguments[0].Value?.ToString() ?? ""
                        : "";

                    httpMethod = new HttpMethodInfo
                    {
                        MethodName = methodSymbol.Name,
                        HttpMethod = httpMethodName,
                        PathTemplate = path,
                        ReturnType = methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    };

                    break;
                }
            }

            if (httpMethod == null) return null;

            // 检查方法级别的验证和过滤器
            foreach (var attr in methodSymbol.GetAttributes())
            {
                var attrName = attr.AttributeClass?.Name ?? "";
                if (attrName == "ValidationAttribute")
                {
                    httpMethod.HasValidation = true;
                }
                if (attrName == "ApiFilterAttribute" || attrName == "ApiFilterAttributeBase")
                {
                    httpMethod.FilterType = attr.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                }
            }

            // 提取参数
            foreach (var param in methodSymbol.Parameters)
            {
                var paramInfo = ExtractParameterInfo(param, httpMethod.PathTemplate);
                httpMethod.Parameters.Add(paramInfo);
            }

            return httpMethod;
        }

        private static HttpParameterInfo ExtractParameterInfo(IParameterSymbol param, string pathTemplate)
        {
            var paramInfo = new HttpParameterInfo
            {
                Name = param.Name,
                Type = param.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                IsSimpleType = IsSimpleType(param.Type),
                IsOptional = param.IsOptional,
                DefaultValue = param.HasExplicitDefaultValue ? param.ExplicitDefaultValue?.ToString() : null
            };

            // 检查是否在路径模板中
            if (pathTemplate.Contains($"{{{param.Name}}}") || pathTemplate.Contains($"{{{param.Name}:"))
            {
                paramInfo.SourceType = ParameterSourceType.Path;
                paramInfo.IsInPath = true;
            }

            // 检查参数特性
            foreach (var attr in param.GetAttributes())
            {
                var attrName = attr.AttributeClass?.Name ?? "";

                switch (attrName)
                {
                    case "JsonContentAttribute":
                        paramInfo.SourceType = ParameterSourceType.JsonBody;
                        break;
                    case "FormContentAttribute":
                        paramInfo.SourceType = ParameterSourceType.FormBody;
                        break;
                    case "FormFieldAttribute":
                        paramInfo.SourceType = ParameterSourceType.FormField;
                        if (attr.ConstructorArguments.Length > 0)
                        {
                            paramInfo.Alias = attr.ConstructorArguments[0].Value?.ToString();
                        }
                        break;
                    case "FormDataContentAttribute":
                        paramInfo.SourceType = ParameterSourceType.FormDataBody;
                        break;
                    case "FormDataFileAttribute":
                        paramInfo.SourceType = ParameterSourceType.FormDataFile;
                        if (attr.ConstructorArguments.Length > 0)
                        {
                            paramInfo.Alias = attr.ConstructorArguments[0].Value?.ToString();
                        }
                        break;
                    case "HeaderAttribute":
                        paramInfo.SourceType = ParameterSourceType.Header;
                        if (attr.ConstructorArguments.Length > 0)
                        {
                            paramInfo.Alias = attr.ConstructorArguments[0].Value?.ToString();
                        }
                        break;
                    case "UriAttribute":
                        paramInfo.SourceType = ParameterSourceType.FullUri;
                        break;
                    case "AliasAsAttribute":
                        paramInfo.Alias = attr.ConstructorArguments.Length > 0
                            ? attr.ConstructorArguments[0].Value?.ToString()
                            : null;
                        break;
                    case "DateTimeFormatAttribute":
                        paramInfo.DateTimeFormat = attr.ConstructorArguments.Length > 0
                            ? attr.ConstructorArguments[0].Value?.ToString()
                            : null;
                        break;
                }
            }

            // CancellationToken 特殊处理
            if (paramInfo.Type == "System.Threading.CancellationToken" ||
                paramInfo.Type == "global::System.Threading.CancellationToken")
            {
                paramInfo.SourceType = ParameterSourceType.CancellationToken;
            }

            return paramInfo;
        }

        private static bool IsSimpleType(ITypeSymbol type)
        {
            var fullName = type.ToDisplayString();
            return fullName == "int" || fullName == "global::System.Int32" ||
                   fullName == "long" || fullName == "global::System.Int64" ||
                   fullName == "float" || fullName == "global::System.Single" ||
                   fullName == "double" || fullName == "global::System.Double" ||
                   fullName == "decimal" || fullName == "global::System.Decimal" ||
                   fullName == "bool" || fullName == "global::System.Boolean" ||
                   fullName == "string" || fullName == "global::System.String" ||
                   fullName == "Guid" || fullName == "global::System.Guid" ||
                   fullName == "DateTime" || fullName == "global::System.DateTime" ||
                   fullName == "DateTimeOffset" || fullName == "global::System.DateTimeOffset" ||
                   fullName == "int?" || fullName == "global::System.Int32?" ||
                   fullName == "long?" || fullName == "global::System.Int64?" ||
                   fullName == "double?" || fullName == "global::System.Double?" ||
                   fullName == "bool?" || fullName == "global::System.Boolean?" ||
                   fullName == "Guid?" || fullName == "global::System.Guid?" ||
                   fullName == "DateTime?" || fullName == "global::System.DateTime?";
        }

        private static string GenerateImplementationClass(HttpInterfaceInfo httpInterface, Compilation compilation)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("#nullable enable");
            sb.AppendLine();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Net.Http;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("using System.Threading;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using Cyber;");
            sb.AppendLine("using Cyber.Http.Models;");
            sb.AppendLine("using Cyber.Http.Extensions;");
            sb.AppendLine($"using {httpInterface.Namespace};");
            sb.AppendLine();
            sb.AppendLine($"namespace {httpInterface.Namespace}.Generated");
            sb.AppendLine("{");
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// {httpInterface.InterfaceName} 实现类（自动生成）");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    public class {httpInterface.InterfaceName}Impl : {httpInterface.InterfaceName}");
            sb.AppendLine("    {");
            sb.AppendLine($"        private readonly HttpClient _httpClient;");
            sb.AppendLine($"        private readonly HttpClientOptions _options;");
            sb.AppendLine();
            sb.AppendLine($"        public {httpInterface.InterfaceName}Impl(HttpClient httpClient, HttpClientOptions options)");
            sb.AppendLine("        {");
            sb.AppendLine($"            _httpClient = httpClient;");
            sb.AppendLine($"            _options = options;");
            sb.AppendLine("        }");
            sb.AppendLine();

            foreach (var method in httpInterface.Methods)
            {
                GenerateMethod(sb, method, httpInterface);
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static void GenerateMethod(StringBuilder sb, HttpMethodInfo method, HttpInterfaceInfo httpInterface)
        {
            var returnType = method.ReturnType;
            var isTask = returnType.StartsWith("System.Threading.Tasks.Task") ||
                         returnType.StartsWith("global::System.Threading.Tasks.Task");

            var actualReturnType = returnType;
            if (isTask && returnType.Contains("<"))
            {
                var start = returnType.IndexOf("<") + 1;
                var end = returnType.LastIndexOf(">");
                actualReturnType = returnType.Substring(start, end - start);
            }

            var paramList = method.Parameters
                .Where(p => p.SourceType != ParameterSourceType.CancellationToken)
                .Select(p => $"{p.Type} {p.Name}")
                .ToList();

            var hasCancellationToken = method.Parameters.Any(p => p.SourceType == ParameterSourceType.CancellationToken);
            if (hasCancellationToken)
            {
                var ctParam = method.Parameters.First(p => p.SourceType == ParameterSourceType.CancellationToken);
                paramList.Add($"{ctParam.Type} {ctParam.Name} = default");
            }

            sb.AppendLine($"        public async {returnType} {method.MethodName}({string.Join(", ", paramList)})");
            sb.AppendLine("        {");

            sb.AppendLine("            var __urlBuilder = new StringBuilder();");
            sb.AppendLine("            var __baseUrl = _options.HttpHost?.ToString() ?? \"\";");

            var uriParam = method.Parameters.FirstOrDefault(p => p.SourceType == ParameterSourceType.FullUri);
            if (uriParam != null)
            {
                sb.AppendLine($"            __urlBuilder.Append({uriParam.Name});");
            }
            else
            {
                sb.AppendLine("            __urlBuilder.Append(__baseUrl);");
                sb.AppendLine($"            __urlBuilder.Append(\"{method.PathTemplate}\");");
            }

            foreach (var pathParam in method.Parameters.Where(p => p.SourceType == ParameterSourceType.Path))
            {
                sb.AppendLine($"            __urlBuilder.Replace(\"{{{pathParam.Name}}}\", {pathParam.Name}.ToString());");
            }

            sb.AppendLine("            var __url = __urlBuilder.ToString();");

            var queryParams = method.Parameters.Where(p => p.SourceType == ParameterSourceType.Query).ToList();
            if (queryParams.Count > 0)
            {
                sb.AppendLine("            var __queryParams = new List<string>();");

                foreach (var queryParam in queryParams)
                {
                    var paramName = queryParam.Alias ?? queryParam.Name;
                    if (queryParam.IsSimpleType)
                    {
                        if (queryParam.IsOptional)
                        {
                            sb.AppendLine($"            if ({queryParam.Name} != null)");
                            sb.AppendLine("            {");
                            sb.AppendLine($"                __queryParams.Add(\"{paramName}=\" + Uri.EscapeDataString({queryParam.Name}.ToString()));");
                            sb.AppendLine("            }");
                        }
                        else
                        {
                            sb.AppendLine($"            __queryParams.Add(\"{paramName}=\" + Uri.EscapeDataString({queryParam.Name}.ToString()));");
                        }
                    }
                }

                sb.AppendLine("            if (__queryParams.Count > 0)");
                sb.AppendLine("            {");
                sb.AppendLine("                __url += \"?\" + string.Join(\"&\", __queryParams);");
                sb.AppendLine("            }");
            }

            sb.AppendLine($"            var __request = new HttpRequestMessage(new HttpMethod(\"{method.HttpMethod}\"), __url);");

            foreach (var headerParam in method.Parameters.Where(p => p.SourceType == ParameterSourceType.Header))
            {
                var headerName = headerParam.Alias ?? headerParam.Name;
                sb.AppendLine($"            __request.Headers.Add(\"{headerName}\", {headerParam.Name}.ToString());");
            }

            var jsonBodyParam = method.Parameters.FirstOrDefault(p => p.SourceType == ParameterSourceType.JsonBody);
            var formBodyParam = method.Parameters.FirstOrDefault(p => p.SourceType == ParameterSourceType.FormBody);
            var formFieldParams = method.Parameters.Where(p => p.SourceType == ParameterSourceType.FormField).ToList();

            if (jsonBodyParam != null)
            {
                sb.AppendLine($"            var __jsonContent = System.Text.Json.JsonSerializer.Serialize({jsonBodyParam.Name}, JsonTypeInfos.{MiniApiGenerationStatic.GetJsonTypeInfoPropertyName(jsonBodyParam.Type)});");
                sb.AppendLine("            __request.Content = new StringContent(__jsonContent, Encoding.UTF8, \"application/json\");");
            }
            else if (formBodyParam != null || formFieldParams.Count > 0)
            {
                sb.AppendLine("            var __formContent = new List<string>();");
                foreach (var fieldParam in formFieldParams)
                {
                    var fieldName = fieldParam.Alias ?? fieldParam.Name;
                    sb.AppendLine($"            __formContent.Add(\"{fieldName}=\" + Uri.EscapeDataString({fieldParam.Name}.ToString()));");
                }
                sb.AppendLine("            __request.Content = new StringContent(string.Join(\"&\", __formContent), Encoding.UTF8, \"application/x-www-form-urlencoded\");");
            }
            else if (method.HttpMethod == "POST" || method.HttpMethod == "PUT" || method.HttpMethod == "PATCH")
            {
                var simpleParams = method.Parameters
                    .Where(p => p.IsSimpleType && p.SourceType == ParameterSourceType.Query)
                    .ToList();

                if (simpleParams.Count > 0)
                {
                    sb.AppendLine("            var __formContent = new List<string>();");
                    foreach (var simpleParam in simpleParams)
                    {
                        var paramName = simpleParam.Alias ?? simpleParam.Name;
                        sb.AppendLine($"            __formContent.Add(\"{paramName}=\" + Uri.EscapeDataString({simpleParam.Name}.ToString()));");
                    }
                    sb.AppendLine("            __request.Content = new StringContent(string.Join(\"&\", __formContent), Encoding.UTF8, \"application/x-www-form-urlencoded\");");
                }
            }

            var cancellationTokenName = hasCancellationToken
                ? method.Parameters.First(p => p.SourceType == ParameterSourceType.CancellationToken).Name
                : "CancellationToken.None";

            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine($"                var __response = await _httpClient.SendAsync(__request, {cancellationTokenName});");
            sb.AppendLine("                __response.EnsureSuccessStatusCode();");

            if (actualReturnType == "string" || actualReturnType == "global::System.String")
            {
                sb.AppendLine("                return await __response.Content.ReadAsStringAsync();");
            }
            else if (actualReturnType == "HttpResponseMessage" || actualReturnType == "global::System.Net.Http.HttpResponseMessage")
            {
                sb.AppendLine("                return __response;");
            }
            else if (actualReturnType == "void" || actualReturnType == "System.Void" || actualReturnType == "global::System.Void")
            {
                // 无返回值
            }
            else
            {
                sb.AppendLine($"                var __responseContent = await __response.Content.ReadAsStringAsync();");
                sb.AppendLine($"                return System.Text.Json.JsonSerializer.Deserialize<{actualReturnType}>(__responseContent, JsonTypeInfos.{MiniApiGenerationStatic.GetJsonTypeInfoPropertyName(actualReturnType)});");
            }

            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception ex)");
            sb.AppendLine("            {");
            sb.AppendLine($"                throw new HttpRequestException(\"HTTP request failed: \" + \"{method.HttpMethod}\" + \" \" + __url, ex);");
            sb.AppendLine("            }");

            sb.AppendLine("        }");
            sb.AppendLine();
        }

      

        private static string GenerateCyberSettingExtension(List<HttpInterfaceInfo> httpInterfaces)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("#nullable enable");
            sb.AppendLine();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            sb.AppendLine("using System.Net.Http;");
            sb.AppendLine("using Cyber.Http.Models;");
            
            // 添加所有接口的命名空间 using 指令
            var namespaces = httpInterfaces.Select(i => i.Namespace).Distinct();
            foreach (var ns in namespaces)
            {
                sb.AppendLine($"using {ns};");
            }
            
            sb.AppendLine();
            sb.AppendLine("namespace Cyber");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// CyberSetting HTTP 客户端注册扩展方法（自动生成）");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static class CyberSettingHttpClientExtensions");
            sb.AppendLine("    {");
            
            // 生成接口类型映射字典
            sb.AppendLine("        private static readonly global::System.Collections.Generic.Dictionary<global::System.Type, global::System.Type> _httpInterfaceImplementations = new()");
            sb.AppendLine("        {");
            foreach (var httpInterface in httpInterfaces)
            {
                var interfaceType = httpInterface.InterfaceFullName.Replace("global::", "");
                sb.AppendLine($"            [typeof({interfaceType})] = typeof({httpInterface.Namespace}.Generated.{httpInterface.InterfaceName}Impl),");
            }
            sb.AppendLine("        };");
            sb.AppendLine();
            
            // 生成通用的 AddHttpClient<T> 方法
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// 注册 HTTP 客户端（支持所有带有 HTTP 方法特性的接口）");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <typeparam name=\"T\">HTTP 客户端接口类型</typeparam>");
            sb.AppendLine("        /// <param name=\"option\">CyberSetting 配置</param>");
            sb.AppendLine("        /// <param name=\"configureOptions\">HTTP 客户端配置选项</param>");
            sb.AppendLine("        /// <returns>CyberSetting 配置</returns>");
            sb.AppendLine("        public static CyberSetting AddHttpClient<T>(this CyberSetting option, Action<HttpClientOptions> configureOptions) where T : class");
            sb.AppendLine("        {");
            sb.AppendLine("            var interfaceType = typeof(T);");
            sb.AppendLine("            if (!_httpInterfaceImplementations.TryGetValue(interfaceType, out var implementationType))");
            sb.AppendLine("            {");
            sb.AppendLine("                throw new global::System.InvalidOperationException($\"Interface {interfaceType.Name} is not registered as an HTTP client interface. Only interfaces with HTTP method attributes (HttpGet, HttpPost, etc.) can be registered.\");");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            var httpClientOptions = new HttpClientOptions();");
            sb.AppendLine("            configureOptions(httpClientOptions);");
            sb.AppendLine();
            sb.AppendLine("            option.ConfigureServices(services =>");
            sb.AppendLine("            {");
            sb.AppendLine("                services.AddSingleton(httpClientOptions);");
            sb.AppendLine("                services.AddHttpClient(interfaceType.Name)");
            sb.AppendLine("                    .ConfigureHttpClient(client =>");
            sb.AppendLine("                    {");
            sb.AppendLine("                        if (httpClientOptions.HttpHost != null)");
            sb.AppendLine("                        {");
            sb.AppendLine("                            client.BaseAddress = httpClientOptions.HttpHost;");
            sb.AppendLine("                        }");
            sb.AppendLine("                        client.Timeout = TimeSpan.FromSeconds(httpClientOptions.TimeoutSeconds);");
            sb.AppendLine("                        foreach (var header in httpClientOptions.DefaultHeaders)");
            sb.AppendLine("                        {");
            sb.AppendLine("                            client.DefaultRequestHeaders.Add(header.Key, header.Value);");
            sb.AppendLine("                        }");
            sb.AppendLine("                    });");
            sb.AppendLine();
            sb.AppendLine("                services.AddScoped(interfaceType, implementationType);");
            sb.AppendLine("            });");
            sb.AppendLine();
            sb.AppendLine("            return option;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }

    /// <summary>
    /// HTTP 接口信息
    /// </summary>
    public class HttpInterfaceInfo
    {
        public string InterfaceName { get; set; } = "";
        public string InterfaceFullName { get; set; } = "";
        public string Namespace { get; set; } = "";
        public string? FilterType { get; set; }
        public bool HasValidation { get; set; }
        public List<HttpMethodInfo> Methods { get; set; } = new();
    }

    /// <summary>
    /// HTTP 方法信息
    /// </summary>
    public class HttpMethodInfo
    {
        public string MethodName { get; set; } = "";
        public string HttpMethod { get; set; } = "";
        public string PathTemplate { get; set; } = "";
        public string ReturnType { get; set; } = "";
        public string? FilterType { get; set; }
        public bool HasValidation { get; set; }
        public List<HttpParameterInfo> Parameters { get; set; } = new();
    }

    /// <summary>
    /// HTTP 参数信息
    /// </summary>
    public class HttpParameterInfo
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public ParameterSourceType SourceType { get; set; } = ParameterSourceType.Query;
        public string? Alias { get; set; }
        public string? DateTimeFormat { get; set; }
        public bool IsSimpleType { get; set; }
        public bool IsOptional { get; set; }
        public bool IsInPath { get; set; }
        public string? DefaultValue { get; set; }
    }

    /// <summary>
    /// 参数来源类型
    /// </summary>
    public enum ParameterSourceType
    {
        Query,
        Path,
        JsonBody,
        FormBody,
        FormField,
        FormDataBody,
        FormDataFile,
        Header,
        FullUri,
        CancellationToken
    }
}
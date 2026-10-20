
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Cyber
{
    
    public class OutPutException : Exception
    {
      
        public string[]  _ArrayData { get; set; }


        /// <summary>
        /// 根据前端协议 用此方法返回的错误才会显示到页面上
        /// 其他错误 都会显示 系统错误，请稍后再试
        /// 因为此方法应该适用于验证错误，重复错误等非程序报错，所以此方法的报错不会存储日志
        /// </summary>
        public OutPutException(string? message) : base(message)
        {
        }

      
        /// <summary>
        /// 根据前端协议 用此方法返回的错误才会显示到页面上
        /// 其他错误 都会显示 系统错误，请稍后再试
        /// 因为此方法应该适用于验证错误，重复错误等非程序报错，所以此方法的报错不会存储日志
        ///  </summary>
        public OutPutException(params string[] message) 
        {
            _ArrayData = message;
        }

       
        /// <summary>
        /// 根据前端协议 用此方法返回的错误才会显示到页面上
        /// 其他错误 都会显示 系统错误，请稍后再试
        /// 因为此方法应该适用于验证错误，重复错误等非程序报错，所以此方法的报错不会存储日志
        /// </summary>
        public OutPutException(Action<ParamsSelectLanguage> SelectLanguage)
        {
            ParamsSelectLanguage paramsSelectLanguage = new ParamsSelectLanguage();
            SelectLanguage(paramsSelectLanguage);
            var returnMessage = paramsSelectLanguage.ReturnMessage();
            string ValidationLanguage = "zh-cn";
            HttpContext HttpContext = GetServiceProvider.GetCurrentScope().HttpContext;
            if (HttpContext != null)
            {
                var headerName = AppGlobalSettings.CyberSetting?.ValidationLanguageHeaderName ?? "ValidationLanguage";
                ValidationLanguage = HttpContext.Request.Headers[headerName].ToString();
                if (ValidationLanguage.IsNullOrEmpty())
                {
                    ValidationLanguage = CultureInfo.CurrentUICulture.ToString().ToLower();
                }

                if (ValidationLanguage == "en")
                    _ArrayData = returnMessage.Item1;
                else if (ValidationLanguage == "zh-cn")
                    _ArrayData = returnMessage.Item2;
                else if (ValidationLanguage == "zh-tw")
                    _ArrayData = returnMessage.Item3;
                else
                    _ArrayData = returnMessage.Item1.IsNullOrEmpty() ? returnMessage.Item2.IsNullOrEmpty() ? returnMessage.Item3: returnMessage.Item2 : returnMessage.Item1;

            }

        }
       
        /// <summary>
        /// 根据前端协议 用此方法返回的错误才会显示到页面上
        /// 其他错误 都会显示 系统错误，请稍后再试
        /// 因为此方法应该适用于验证错误，重复错误等非程序报错，所以此方法的报错不会存储日志
        /// </summary>
        public OutPutException(Action<SelectLanguage> SelectLanguage)
        {
            SelectLanguage selectLanguage = new SelectLanguage();
            SelectLanguage(selectLanguage);
            string ValidationLanguage = "zh-cn";
            IHttpContextAccessor httpContextAccessor = GetServiceProvider.GetService<IHttpContextAccessor>();
            if (httpContextAccessor != null)
            {
                var headerName = AppGlobalSettings.CyberSetting?.ValidationLanguageHeaderName ?? "ValidationLanguage";
                ValidationLanguage = httpContextAccessor.HttpContext.Request.Headers[headerName].ToString();
                if (ValidationLanguage == "en")
                    _ArrayData = selectLanguage.English.Split(",");
                else if (ValidationLanguage == "zh-cn")
                    _ArrayData = selectLanguage.ChineseSimplified.Split(",");
                else if (ValidationLanguage == "zh-tw")
                    _ArrayData = selectLanguage.ChineseTraditional.Split(",");
                else
                    _ArrayData = selectLanguage.English.IsNullOrEmpty() ? selectLanguage.ChineseSimplified.IsNullOrEmpty() ? selectLanguage.ChineseTraditional.Split(",") : selectLanguage.ChineseSimplified.Split(",") : selectLanguage.English.Split(",");
            }
        }
      
    }
   
}

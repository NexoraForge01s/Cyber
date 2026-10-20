#region License
// Copyright (c) .NET Foundation and contributors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// The latest version of this file can be found at https://github.com/FluentValidation/FluentValidation
#endregion

namespace FluentValidation.Resources {
    using Microsoft.AspNetCore.Http;
    using Cyber;
    using System;
    using System.Globalization;
    using Validators;

	
	public class ActionStringSource : IStringSource {
		readonly Action<SelectLanguage> _SelectLanguage;

		public string _message { get; set; }

		internal string String => _message;

		/// <summary>
		/// Creates a new StringErrorMessageSource using the specified error message as the error template.
		/// </summary>
		/// <param name="message">The error message template.</param>
		public ActionStringSource(Action<SelectLanguage> SelectLanguage) {
			_SelectLanguage = SelectLanguage;
		}

		/// <summary>
		/// Construct the error message template
		/// </summary>
		/// <returns>Error message template</returns>
		public string GetString(ICommonContext context) {
            HttpContext HttpContext = GetServiceProvider.GetCurrentScope().HttpContext;
			if (HttpContext != null)
			{
				var headerName = AppGlobalSettings.CyberSetting?.ValidationLanguageHeaderName ?? "ValidationLanguage";
				var ValidationLanguage = HttpContext.Request.Headers[headerName];
				if (ValidationLanguage.IsNullOrEmpty())
				{
					ValidationLanguage = CultureInfo.CurrentUICulture.ToString().ToLower();

				}
				SelectLanguage language = new SelectLanguage();
				if (_SelectLanguage != null)
				{
					_SelectLanguage(language);
					if (!ValidationLanguage.IsNullOrEmpty() && ValidationLanguage == "en")
						_message = language.English;
					else if (!ValidationLanguage.IsNullOrEmpty() && ValidationLanguage == "zh-cn")
						_message = language.ChineseSimplified;
					else if (!ValidationLanguage.IsNullOrEmpty() && ValidationLanguage == "zh-cht")
						_message = language.ChineseTraditional;
					else
						_message = language.English.IsNullOrEmpty() ? language.ChineseSimplified.IsNullOrEmpty() ? language.ChineseTraditional : language.ChineseSimplified : language.English;
				}
				return _message;
			}
			else
			{
				return string.Empty;
			}
		}
	}
}

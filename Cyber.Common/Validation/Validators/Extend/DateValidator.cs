#region License
// Copyright (c) Jeremy Skinner (http://www.jeremyskinner.co.uk)
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
// The latest version of this file can be found at https://github.com/jeremyskinner/FluentValidation
#endregion

namespace FluentValidation.Validators {
	using System;
	using System.Text.RegularExpressions;
    using Cyber;
    using Resources;


	//Number regex matches the one used in the DataAnnotations NumberAddressAttribute for consistency/parity with DataAnnotations. This is not a fully comprehensive solution, but is "good enough" for most cases.
	public class DateValidator : PropertyValidator, IDateValidator
	{
		public bool _IsIgnore { get; set; } = false;
		public string _FormatInfo { get; set; } = "";
		public DateValidator(string FormatInfo="",bool IsIgnore = false) : base(new LanguageStringSource(nameof(DateValidator))) {
			_IsIgnore = IsIgnore;
			_FormatInfo = FormatInfo;
		}


		protected override bool IsValid(PropertyValidatorContext context) {
			if (context.PropertyValue == null && _IsIgnore) return true;

			if (!_FormatInfo.IsNullOrEmpty())
			{
				DateTime reval = DateTime.MinValue;
				return DateTime.TryParseExact((string)context.PropertyValue,
							_FormatInfo,
							System.Globalization.CultureInfo.InvariantCulture,
							System.Globalization.DateTimeStyles.None,
							out reval);
			}
			else
			{
				try
				{
					DateTime.Parse((string)context.PropertyValue);
					return true;
				}
				catch
				{
					return false;
				}
			}

		}

		
	}

	

	//TODO: Remove IRegularExpresionValidator from the inheritance chain for FV9.
	public interface IDateValidator : IPropertyValidator
	{
		public bool _IsIgnore { get; set; }
		public string _FormatInfo { get; set; }
	}
}

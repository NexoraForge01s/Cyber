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


	//Tel regex matches the one used in the DataAnnotations TelAddressAttribute for consistency/parity with DataAnnotations. This is not a fully comprehensive solution, but is "good enough" for most cases.
	public class TelValidator : PropertyValidator,  ITelValidator {
		public bool _IsIgnore { get; set; } = false;
		public TelValidator(bool IsIgnore = false) : base(new LanguageStringSource(nameof(TelValidator))) {
			_IsIgnore = IsIgnore;
		}


		protected override bool IsValid(PropertyValidatorContext context) {
			if (context.PropertyValue == null && _IsIgnore) return true;

			if (!RegexTool.Tel((string)context.PropertyValue, _IsIgnore))
			{
				return false;
			}

			return true;
		}

	
	}

	

	//TODO: Remove IRegularExpresionValidator from the inheritance chain for FV9.
	public interface ITelValidator : IPropertyValidator
	{
		public bool _IsIgnore { get; set; }
	}
}

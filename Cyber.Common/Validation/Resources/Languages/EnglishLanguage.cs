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

#pragma warning disable 618

namespace FluentValidation.Resources {
	using Validators;

	internal class EnglishLanguage {
		public const string Culture = "en";
		public const string AmericanCulture = "en-US";
		public const string BritishCulture = "en-GB";

		public static string GetTranslation(string key) => key switch {
			nameof(EmailValidator) => "'{PropertyName}' is not a valid email address.",
			nameof(GreaterThanOrEqualValidator) => "'{PropertyName}' must be greater than or equal to '{ComparisonValue}'.",
			nameof(GreaterThanValidator) => "'{PropertyName}' must be greater than '{ComparisonValue}'.",
			nameof(LengthValidator) => "'{PropertyName}' must be between {MinLength} and {MaxLength} characters. You entered {TotalLength} characters.",
			nameof(MinimumLengthValidator) => "The length of '{PropertyName}' must be at least {MinLength} characters. You entered {TotalLength} characters.",
			nameof(MaximumLengthValidator) => "The length of '{PropertyName}' must be {MaxLength} characters or fewer. You entered {TotalLength} characters.",
			nameof(LessThanOrEqualValidator) => "'{PropertyName}' must be less than or equal to '{ComparisonValue}'.",
			nameof(LessThanValidator) => "'{PropertyName}' must be less than '{ComparisonValue}'.",
			nameof(NotEmptyValidator) => "'{PropertyName}' must not be empty.",
			nameof(NotEqualValidator) => "'{PropertyName}' must not be equal to '{ComparisonValue}'.",
			nameof(NotNullValidator) => "'{PropertyName}' must not be empty.",
			nameof(PredicateValidator) => "The specified condition was not met for '{PropertyName}'.",
			nameof(AsyncPredicateValidator) => "The specified condition was not met for '{PropertyName}'.",
			nameof(RegularExpressionValidator) => "'{PropertyName}' is not in the correct format.",
			nameof(EqualValidator) => "'{PropertyName}' must be equal to '{ComparisonValue}'.",
			nameof(ExactLengthValidator) => "'{PropertyName}' must be {MaxLength} characters in length. You entered {TotalLength} characters.",
			nameof(InclusiveBetweenValidator) => "'{PropertyName}' must be between {From} and {To}. You entered {Value}.",
			nameof(ExclusiveBetweenValidator) => "'{PropertyName}' must be between {From} and {To} (exclusive). You entered {Value}.",
			nameof(CreditCardValidator) => "'{PropertyName}' is not a valid credit card number.",
			nameof(ScalePrecisionValidator) => "'{PropertyName}' must not be more than {ExpectedPrecision} digits in total, with allowance for {ExpectedScale} decimals. {Digits} digits and {ActualScale} decimals were found.",
			nameof(EmptyValidator) => "'{PropertyName}' must be empty.",
			nameof(NullValidator) => "'{PropertyName}' must be empty.",
			nameof(EnumValidator) => "'{PropertyName}' has a range of values which does not include '{PropertyValue}'.",

			nameof(PhoneValidator) => "'{PropertyName}' is not a valid phone.",
			nameof(PhoneAllValidator) => "'{PropertyName}' is not a valid phone.",
			nameof(ZipCodeValidator) => "'{PropertyName}' is not a valid zip code.",
			nameof(UserNameValidator) => "'{PropertyName}' must start with a letter from 6 to 20 digits and can contain (letters, numbers, underscores, minus signs).",
			nameof(PassWordValidator) => "'{PropertyName}' must be between 6 and 20 digits, must contain numbers and letters",
			nameof(TelValidator) => "'{PropertyName}' is not a valid tel.",
			nameof(CompanyValidator) => "'{PropertyName}' is not a valid company name and may contain (Chinese, English, numbers, spaces, brackets, horizontal lines, underscores).",
			nameof(ChineseValidator) => "'{PropertyName}'  must be Chinese.",
			nameof(EnglishValidator) => "'{PropertyName}' must be English.",
			nameof(NumberValidator) => "'{PropertyName}' must be number.",
			nameof(NumberOrLetterValidator) => "'{PropertyName}' must be number or letter.",
			nameof(NumberOrLetterContainSpecialCharactersValidator) => "'{PropertyName}' must be number or letter. Can contain special characters",
			nameof(ChineseIdentificationValidator) => "'{PropertyName}' is not a valid ID number.",
			nameof(DateValidator) => "'{PropertyName}' Must be date type.",


			// Additional fallback messages used by clientside validation integration.
			"Length_Simple" => "'{PropertyName}' must be between {MinLength} and {MaxLength} characters.",
			"MinimumLength_Simple" => "The length of '{PropertyName}' must be at least {MinLength} characters.",
			"MaximumLength_Simple" => "The length of '{PropertyName}' must be {MaxLength} characters or fewer.",
			"ExactLength_Simple" => "'{PropertyName}' must be {MaxLength} characters in length.",
			"InclusiveBetween_Simple" => "'{PropertyName}' must be between {From} and {To}.",
			_ => null,
		};
	}
}

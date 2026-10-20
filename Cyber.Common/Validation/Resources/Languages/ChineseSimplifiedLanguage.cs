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

	internal class ChineseSimplifiedLanguage {
		public const string Culture = "zh-CN";

		public static string GetTranslation(string key) => key switch {
			nameof(EmailValidator) => "'{PropertyName}' 不是有效的电子邮件地址。",
			nameof(GreaterThanOrEqualValidator) => "'{PropertyName}' 必须大于或等于 '{ComparisonValue}'。",
			nameof(GreaterThanValidator) => "'{PropertyName}' 必须大于 '{ComparisonValue}'。",
			nameof(LengthValidator) => "'{PropertyName}' 的长度必须在 {MinLength} 到 {MaxLength} 字符，您输入了 {TotalLength} 字符。",
			nameof(MinimumLengthValidator) => "'{PropertyName}' 必须大于或等于{MinLength}个字符。您输入了{TotalLength}个字符。",
			nameof(MaximumLengthValidator) => "'{PropertyName}' 必须小于或等于{MaxLength}个字符。您输入了{TotalLength}个字符。",
			nameof(LessThanOrEqualValidator) => "'{PropertyName}' 必须小于或等于 '{ComparisonValue}'。",
			nameof(LessThanValidator) => "'{PropertyName}' 必须小于 '{ComparisonValue}'。",
			nameof(NotEmptyValidator) => "'{PropertyName}' 不能为空。",
			nameof(NotEqualValidator) => "'{PropertyName}' 不能和 '{ComparisonValue}' 相等。",
			nameof(NotNullValidator) => "'{PropertyName}' 不能为Null。",
			nameof(PredicateValidator) => "'{PropertyName}' 不符合指定的条件。",
			nameof(AsyncPredicateValidator) => "'{PropertyName}' 不符合指定的条件。",
			nameof(RegularExpressionValidator) => "'{PropertyName}' 的格式不正确。",
			nameof(EqualValidator) => "'{PropertyName}' 应该和 '{ComparisonValue}' 相等。",
			nameof(ExactLengthValidator) => "'{PropertyName}' 必须是 {MaxLength} 个字符，您输入了 {TotalLength} 字符。",
			nameof(InclusiveBetweenValidator) => "'{PropertyName}' 必须在 {From} (包含)和 {To} (包含)之间， 您输入了 {Value}。",
			nameof(ExclusiveBetweenValidator) => "'{PropertyName}' 必须在 {From} (不包含)和 {To} (不包含)之间， 您输入了 {Value}。",
			nameof(CreditCardValidator) => "'{PropertyName}' 不是有效的信用卡号。",
			nameof(ScalePrecisionValidator) => "'{PropertyName}' 总位数不能超过 {ExpectedPrecision} 位，其中小数部分 {ExpectedScale} 位。您共计输入了 {Digits} 位数字，其中小数部分{ActualScale} 位。",
			nameof(EmptyValidator) => "'{PropertyName}' 必须为空。",
			nameof(NullValidator) => "'{PropertyName}' 必须为Null。",
			nameof(EnumValidator) => "'{PropertyName}' 的值范围不包含 '{PropertyValue}'。",

			nameof(PhoneValidator)=>"'{PropertyName}' 不是有效的手机号码。",
            nameof(PhoneAllValidator)=>"'{PropertyName}' 不是有效的手机号码。",
			nameof(ZipCodeValidator)=>"'{PropertyName}' 不是有效的邮编。",
            nameof(UserNameValidator)=>"'{PropertyName}' 必须以6到20位以字母开头，可包含（字母，数字，下划线，减号。",
            nameof(PassWordValidator)=>"'{PropertyName}' 必须以6到20位 必须包含数字、字母",
            nameof(TelValidator)=>"'{PropertyName}' 不是有效的座机号。",
            nameof(CompanyValidator)=>"'{PropertyName}' 不是有效的公司名称，可包含（中文，英文，数字，空格，括号，横线，下划线）。",
            nameof(ChineseValidator)=>"'{PropertyName}' 必须为汉字。",
            nameof(EnglishValidator)=>"'{PropertyName}' 必须为英文。",
            nameof(NumberValidator)=>"'{PropertyName}' 必须为数字。",
			nameof(NumberOrLetterValidator) => "'{PropertyName}' 必须为数字或字母。",
			nameof(NumberOrLetterContainSpecialCharactersValidator) => "'{PropertyName}' 必须为数字或字母，可包含特殊字符。",
			nameof(ChineseIdentificationValidator)=>"'{PropertyName}' 不是有效的身份证号。",
			nameof(DateValidator)=>"'{PropertyName}' 不是有效的日期类型。",


			// Additional fallback messages used by clientside validation integration.
			"Length_Simple" => "'{PropertyName}' 的长度必须在 {MinLength} 到 {MaxLength} 字符。",
			"MinimumLength_Simple" => "'{PropertyName}' 必须大于或等于{MinLength}个字符。",
			"MaximumLength_Simple" => "'{PropertyName}' 必须小于或等于{MaxLength}个字符。",
			"ExactLength_Simple" => "'{PropertyName}' 必须是 {MaxLength} 个字符。",
			"InclusiveBetween_Simple" => "'{PropertyName}' 必须在 {From} (包含)和 {To} (包含)之间。",
			_ => null,
		};
	}
}

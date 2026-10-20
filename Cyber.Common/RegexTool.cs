
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Cyber
{
    /// <summary>
    /// 正则类库
    /// </summary>
    public class RegexTool
    {
        #region 正则表达式
        private static bool Regex(string RegexString, string var, bool IsNull = false)
        {
            bool state = false;
            try
            {
                if (IsNull&& string.IsNullOrEmpty(var))
                {
                    state = true;
                }
                else if (new Regex(RegexString).IsMatch(var))
                {
                    state = true;
                }
            }
            catch (Exception)
            {

            }
            return state;
        }
        #endregion

        /// <summary>
        /// 手机号；11位，13\14\15\17\18开头，半角数字
        /// </summary>
        public const string PhoneRegex = @"^((13[0-9])|(14[0-9])|(15([0-9]))|(17[0-9])|(18[0-9])|(16[0-9])|(19[0-9]))[0-9]{8}$";
        /// <summary>
        /// 邮箱
        /// </summary>
        public const string EmailRegex = @"([a-zA-Z0-9]+[_.\-]*[a-zA-Z0-9]+)+\@(([a-zA-Z0-9_\-\.])+)+(\.[a-zA-Z]{2,6})(\.[a-zA-Z]{2})?$";
        /// <summary>
        /// 邮编；6位半角数字，可以以0开头
        /// </summary>
        public const string ZipCodeRegex = @"^[0-9]{6}$";
        /// <summary>
        /// 用户名；6到20位以字母开头，可包含（字母，数字，下划线，减号
        /// </summary>
        public const string UserNameRegex= @"^[a-zA-Z]{1}([a-zA-Z0-9]|[_-]){5,19}$";
        /// <summary>
        /// 密码；6到20位，必须包含数字、字母
        /// </summary>
        public const string PassWordRegex = @"^(?![0-9]+$)(?![a-zA-Z]+$)[0-9A-Za-z]{6,19}$";
        /// <summary>
        /// 座机号；使用半角数字，支持以下两种方式：010-82312233/01082312267
        /// </summary>
        public const string TelRegex = @"^(([0\+]\d{2,3}-?)?(0\d{2,3})-?)?(\d{7,8})(-(\d{3,}))?$";
        /// <summary>
        /// 公司名称；只能输入中文、英文、数字、空格、括号、横线、下划线
        /// </summary>
        public const string CompanyRegex= @"^([A-Za-z]|[\u4e00-\u9fa5]|[0-9 （）()_-])$";
        /// <summary>
        /// 汉字
        /// </summary>
        public const string ChineseRegex = @"^[\u4E00-\u9FA5]{0,}$";
        /// <summary>
        /// 英文
        /// </summary>
        public const string EnglishRegex = @"^[a-zA-Z]{0,}$";
        /// <summary>
        /// 数字
        /// </summary>
        public const string NumberRegex = @"^[0-9]*$";

        /// <summary>
        /// 数字或字母
        /// </summary>
        public const string NumberOrLetterRegex = @"^[a-zA-Z0-9]+$";

        /// <summary>
        /// 数字或字母必填 可填符号包括（!;:@#$%?&amp;*\\/_ -）
        /// </summary>
        public const string NumberOrLetterContainSpecialCharactersRegex = @"^((?=.*[a-z])|(?=.*[A-Z])|(?=.*[0-9]))([a-zA-Z0-9!;:@#$%?&*\\/()_-])+$";


        /// <summary>
        /// 手机，座机，400，800号；座机号支持以下两种方式：010-82312233/01082312267
        /// </summary>
        public const string PhoneAllRegex = @"(^((13[0-9])|(14[0-9])|(15([0-9]))|(17[0-9])|(18[0-9])|(16[0-9])|(19[0-9]))[0-9]{8}$)|(^(([0\+]\d{2,3}-?)?(0\d{2,3})-?)?(\d{7,8})(-(\d{3,}))?$)|(^400[0-9]{7})|(^800[0-9]{7})";

        /// <summary>
        /// 手机号 支持多个验证逗号隔开；11位，13\14\15\17\18开头，半角数字
        /// </summary>
        /// <param name="var">字符串</param>
        /// <param name="IsNull">为空是否通过</param>
        /// <returns></returns>
        public static bool Phone(string var,bool IsNull=false)
        {
            if (var.IsNullOrEmpty() && !IsNull)
                return false;
            bool status = true;
            if (var.Contains(","))
            {
                var PhoneSplit = var.Split(',');
                for (int i = 0; i < PhoneSplit.Length; i++)
                {
                    if (!Regex(PhoneRegex, PhoneSplit[i].Trim(), IsNull))
                    {
                        status = false;
                        break;
                    }
                }
            }
            else
            {
                status = Regex(PhoneRegex, var, IsNull);
            }
            return status;
        }
        /// <summary>
        /// 邮箱 支持多个验证逗号/分号隔开；支持国际邮箱格式（xxx.xxx@xxx.xx 或 xxx@xxx.xx）
        /// </summary>
        /// <param name="var">字符串</param>
        /// <param name="IsNull">为空是否通过</param>
        /// <returns></returns>
        public static bool Email(string var, bool IsNull = false)
        {
            if (var.IsNullOrEmpty() && !IsNull)
                return false;
            bool status = true;
            if (var.Contains(";"))
            {
                var addressSplit = var.Split(';');
                for (int i = 0; i < addressSplit.Length; i++)
                {
                    if (!Regex(EmailRegex, addressSplit[i].Trim(), IsNull))
                    {
                        status = false;
                        break;
                    }
                }
            }
            else if (var.Contains(","))
            {
                var addressSplit = var.Split(',');
                for (int i = 0; i < addressSplit.Length; i++)
                {
                    if (!Regex(EmailRegex, addressSplit[i].Trim(), IsNull))
                    {
                        status = false;
                        break;
                    }
                }
            }
            else
            {
                status= Regex(EmailRegex, var, IsNull);
            }
            return status;
        }
        /// <summary>
        /// 邮编；6位半角数字，可以以0开头
        /// </summary>
        /// <param name="var">字符串</param>
        /// <param name="IsNull">为空是否通过</param>
        /// <returns></returns>
        public static bool ZipCode(string var, bool IsNull = false)
        {
            if (var.IsNullOrEmpty() && !IsNull)
                return false;
            return Regex(ZipCodeRegex, var, IsNull);
        }
        /// <summary>
        /// 用户名；6到20位以字母开头，可包含（字母，数字，下划线，减号
        /// </summary>
        /// <param name="var">字符串</param>
        /// <param name="IsNull">为空是否通过</param>
        /// <returns></returns>
        public static bool UserName(string var, bool IsNull = false)
        {
            if (var.IsNullOrEmpty() && !IsNull)
                return false;
            return Regex(UserNameRegex, var, IsNull);
        }
        /// <summary>
        /// 密码；6到10位，必须包含数字、字母
        /// </summary>
        /// <param name="var">字符串</param>
        /// <param name="IsNull">为空是否通过</param>
        /// <returns></returns>
        public static bool PassWord(string var, bool IsNull = false)
        {
            if (var.IsNullOrEmpty() && !IsNull)
                return false;
            return Regex(PassWordRegex, var, IsNull);
        }

        /// <summary>
        /// 座机号；必填情况下使用半角数字，支持以下两种方式：010-82312233/01082312267
        /// </summary>
        /// <param name="var">字符串</param>
        /// <param name="IsNull">为空是否通过</param>
        /// <returns></returns>
        public static bool Tel(string var, bool IsNull = false)
        {
            if (var.IsNullOrEmpty() && !IsNull)
                return false;
            return Regex(TelRegex, var, IsNull);
        }

        /// <summary>
        /// 手机，座机，400，800号；座机号支持以下两种方式：010-82312233/01082312267
        /// </summary>
        /// <param name="var">字符串</param>
        /// <param name="IsNull">为空是否通过</param>
        /// <returns></returns>
        public static bool PhoneAll(string var, bool IsNull = false)
        {
            if (var.IsNullOrEmpty() && !IsNull)
                return false;
            return Regex(PhoneAllRegex, var, IsNull);
        }
        /// <summary>
        /// 公司名称；只能输入中文、英文、数字、空格、括号、横线、下划线
        /// </summary>
        /// <param name="var">字符串</param>
        /// <param name="IsNull">为空是否通过</param>
        /// <returns></returns>
        public static bool Company(string var, bool IsNull = false)
        {
            if (var.IsNullOrEmpty() && !IsNull)
                return false;
            return Regex(CompanyRegex, var, IsNull);
        }
        /// <summary>
        /// 中文姓名；只能中文
        /// </summary>
        /// <param name="var">字符串</param>
        /// <param name="IsNull">为空是否通过</param>
        /// <returns></returns>
        public static bool ChineseName(string var, bool IsNull = false)
        {
            if (var.IsNullOrEmpty() && !IsNull)
                return false;
            return Regex(ChineseRegex, var, IsNull);
        }

        /// <summary>
        /// 验证汉字
        /// </summary>
        /// <param name="var">字符串</param>
        /// <param name="IsNull">为空是否通过</param>
        /// <returns></returns>
        public static bool Chinese(string var, bool IsNull = false)
        {
            if (var.IsNullOrEmpty() && !IsNull)
                return false;
            return Regex(ChineseRegex, var, IsNull);
        }

        /// <summary>
        /// 验证英文
        /// </summary>
        /// <param name="var">字符串</param>
        /// <param name="IsNull">为空是否通过</param>
        /// <returns></returns>
        public static bool English(string var, bool IsNull = false)
        {
            if (var.IsNullOrEmpty() && !IsNull)
                return false;
            return Regex(EnglishRegex, var, IsNull);
        }
        /// <summary>
        /// 验证数字
        /// </summary>
        /// <param name="var">字符串</param>
        /// <param name="IsNull">为空是否通过</param>
        /// <returns></returns>
        public static bool Number(string var, bool IsNull = false)
        {
            if (var.IsNullOrEmpty() && !IsNull)
                return false;
            return Regex(NumberRegex, var, IsNull);
        }


        /// <summary>
        /// 验证数字
        /// </summary>
        /// <param name="var">字符串</param>
        /// <param name="IsNull">为空是否通过</param>
        /// <returns></returns>
        public static bool NumberOrLetter(string var, bool IsNull = false)
        {
            if (var.IsNullOrEmpty() && !IsNull)
                return false;
            return Regex(NumberOrLetterRegex, var, IsNull);
        }


        /// <summary>
        /// 数字或字母必填 可填符号包括（!;:@#$%?&amp;*\\/_ -）
        /// </summary>
        /// <param name="var">字符串</param>
        /// <param name="IsNull">为空是否通过</param>
        /// <returns></returns>
        public static bool NumberOrLetterContainSpecialCharacters(string var, bool IsNull = false)
        {
            if (var.IsNullOrEmpty() && !IsNull)
                return false;
            return Regex(NumberOrLetterContainSpecialCharactersRegex, var, IsNull);
        }
        /// <summary>
        /// 中国身份证验证
        /// </summary>
        /// <param name="var">字符串</param>
        /// <param name="IsNull">为空是否通过</param>
        /// <returns></returns>
        public static bool Identification(string var,bool IsNull = false)
        {
            if (var.IsNullOrEmpty() && !IsNull)
                return false;
            string i="";
            return Identification(var,ref i);
        }
        /// <summary>
        /// 中国身份证验证
        /// </summary>
        /// <param name="var">字符串</param>
        /// <param name="area">返回该身份证省份</param>
        /// <param name="IsNull">为空是否通过</param>
        /// <returns></returns>
        public static bool Identification(string var, ref string area ,bool IsNull = false)
        {
            bool state = false;
            try
            {
                if (IsNull && string.IsNullOrEmpty(var))
                {
                    state = true;
                }
                else if (var.IsNullOrEmpty())
                {
                    state = false;
                }
                else
                {
                    Dictionary<int, string> Area = new Dictionary<int, string>();
                    #region 省份字典
                    Area.Add(11, "北京");
                    Area.Add(12, "天津");
                    Area.Add(13, "河北");
                    Area.Add(14, "山西");
                    Area.Add(15, "内蒙古");
                    Area.Add(21, "辽宁");
                    Area.Add(22, "吉林");
                    Area.Add(23, "黑龙江");
                    Area.Add(31, "上海");
                    Area.Add(32, "江苏");
                    Area.Add(33, "浙江");
                    Area.Add(34, "安徽");
                    Area.Add(35, "福建");
                    Area.Add(36, "江西");
                    Area.Add(37, "山东");
                    Area.Add(41, "河南");
                    Area.Add(42, "湖北");
                    Area.Add(43, "湖南");
                    Area.Add(44, "广东");
                    Area.Add(45, "广西");
                    Area.Add(46, "海南");
                    Area.Add(50, "重庆");
                    Area.Add(51, "四川");
                    Area.Add(52, "贵州");
                    Area.Add(53, "云南");
                    Area.Add(54, "西藏");
                    Area.Add(61, "陕西");
                    Area.Add(62, "甘肃");
                    Area.Add(63, "青海");
                    Area.Add(64, "宁夏");
                    Area.Add(65, "新疆");
                    Area.Add(71, "台湾");
                    Area.Add(81, "香港");
                    Area.Add(82, "澳门");
                    Area.Add(91, "国外");
                    #endregion
                    int Y, S;
                    string M, JYM;
                    string RegexString;
                    char[] idcard_array = var.ToCharArray();
                    area = Area[Convert.ToInt32(var.Substring(0, 2))];
                    if (area != null)
                    {
                        switch (var.Count())
                        {
                            case 15:
                                if ((Convert.ToInt32(var.Substring(6, 2)) + 1900) % 4 == 0 || ((Convert.ToInt32(var.Substring(6, 2)) + 1900) % 100 == 0 && (Convert.ToInt32(var.Substring(6, 2)) + 1900) % 4 == 0))
                                {
                                    RegexString = @"^[1-9][0-9]{5}[0-9]{2}((01|03|05|07|08|10|12)(0[1-9]|[1-2][0-9]|3[0-1])|(04|06|09|11)(0[1-9]|[1-2][0-9]|30)|02(0[1-9]|[1-2][0-9]))[0-9]{3}$";
                                }
                                else
                                {
                                    RegexString = @"^[1-9][0-9]{5}[0-9]{2}((01|03|05|07|08|10|12)(0[1-9]|[1-2][0-9]|3[0-1])|(04|06|09|11)(0[1-9]|[1-2][0-9]|30)|02(0[1-9]|1[0-9]|2[0-8]))[0-9]{3}$";
                                }
                                if (new Regex(RegexString).IsMatch(var))
                                {
                                    state = true;
                                }
                                break;
                            case 18:
                                // 18位身份号码检测
                                // 出生日期的合法性检查
                                // 闰年月日:((01|03|05|07|08|10|12)(0[1-9]|[1-2][0-9]|3[0-1])|(04|06|09|11)(0[1-9]|[1-2][0-9]|30)|02(0[1-9]|[1-2][0-9]))
                                // 平年月日:((01|03|05|07|08|10|12)(0[1-9]|[1-2][0-9]|3[0-1])|(04|06|09|11)(0[1-9]|[1-2][0-9]|30)|02(0[1-9]|1[0-9]|2[0-8]))
                                if (Convert.ToInt32(var.Substring(6, 4)) % 4 == 0 || (Convert.ToInt32(var.Substring(6, 4)) % 100 == 0 && Convert.ToInt32(var.Substring(6, 4)) % 4 == 0))
                                {
                                    RegexString = @"^[1-9][0-9]{5}(19|20)[0-9]{2}((01|03|05|07|08|10|12)(0[1-9]|[1-2][0-9]|3[0-1])|(04|06|09|11)(0[1-9]|[1-2][0-9]|30)|02(0[1-9]|[1-2][0-9]))[0-9]{3}[0-9Xx]$";
                                }
                                else
                                {
                                    RegexString = @"^[1-9][0-9]{5}(19|20)[0-9]{2}((01|03|05|07|08|10|12)(0[1-9]|[1-2][0-9]|3[0-1])|(04|06|09|11)(0[1-9]|[1-2][0-9]|30)|02(0[1-9]|1[0-9]|2[0-8]))[0-9]{3}[0-9Xx]$";
                                }
                                if (new Regex(RegexString).IsMatch(var))
                                {
                                    S = (Convert.ToInt32(idcard_array[0].ToString()) + Convert.ToInt32(idcard_array[10].ToString())) * 7
                                         + (Convert.ToInt32(idcard_array[1].ToString()) + Convert.ToInt32(idcard_array[11].ToString()))
                                         * 9
                                         + (Convert.ToInt32(idcard_array[2].ToString()) + Convert.ToInt32(idcard_array[12].ToString()))
                                         * 10
                                         + (Convert.ToInt32(idcard_array[3].ToString()) + Convert.ToInt32(idcard_array[13].ToString()))
                                         * 5
                                         + (Convert.ToInt32(idcard_array[4].ToString()) + Convert.ToInt32(idcard_array[14].ToString()))
                                         * 8
                                         + (Convert.ToInt32(idcard_array[5].ToString()) + Convert.ToInt32(idcard_array[15].ToString()))
                                         * 4
                                         + (Convert.ToInt32(idcard_array[6].ToString()) + Convert.ToInt32(idcard_array[16].ToString()))
                                         * 2 + Convert.ToInt32(idcard_array[7].ToString()) * 1
                                         + Convert.ToInt32(idcard_array[8].ToString()) * 6 + Convert.ToInt32(idcard_array[9].ToString())
                                         * 3;
                                    Y = S % 11;
                                    JYM = "10X98765432";
                                    M = JYM.Substring(Y, 1);// 判断校验位
                                    if (M == idcard_array[17].ToString())
                                    {
                                        state = true;
                                    }
                                }

                                break;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return state;
        }


        /// <summary>
        /// 是否是日期类型
        /// </summary>
        /// <param name="var">字符串</param>
        /// <returns></returns>
        public static bool IsDate(string var)
        {
            try
            {
                DateTime.Parse(var); 
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 是否是日期（特殊日期）
        /// </summary>
        /// <param name="var">字符串</param>
        /// <param name="FormatInfo">日期格式</param>
        /// <returns></returns>
        public static bool IsDate(string var,string FormatInfo)
        {
            DateTime dt;
            try
            {
                if (DateTime.TryParseExact(var,
                           FormatInfo,
                           System.Globalization.CultureInfo.InvariantCulture,
                           System.Globalization.DateTimeStyles.None,
                           out dt))
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 判断是否是数字类型
        /// </summary>
        /// <param name="str">要判断的字符串</param>
        /// <returns></returns>
        public static bool IsNumeric(string str)
        {
            if (str == null || str.Length == 0)
                return false;
            ASCIIEncoding ascii = new ASCIIEncoding();
            byte[] bytestr = ascii.GetBytes(str);

            foreach (byte c in bytestr)
            {
                if (c < 48 || c > 57)
                {
                    return false;
                }
            }
            return true;
        }


    }
}

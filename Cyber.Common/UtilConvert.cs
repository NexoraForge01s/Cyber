using Cyber;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using System.Web;

namespace Cyber
{
    /// <summary>
    /// 
    /// </summary>
    public static class UtilConvert
    {

        /// <summary>
        /// 转为INT
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static int ObjToInt(this object thisValue)
        {
            int reval = 0;
            if (thisValue == null) return 0;
            if (thisValue is Enum)
            {
                return (int)thisValue;
            }
            if (thisValue != null && thisValue != DBNull.Value && int.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return reval;
        }
        /// <summary>
        /// 转为INT
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static int? ObjToIntNull(this object thisValue)
        {
            int reval = 0;
            if (thisValue == null) return null;
            if (thisValue is Enum)
            {
                return (int)thisValue;
            }
            if (thisValue != null && thisValue != DBNull.Value && int.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return null;
        }


        /// <summary>
        /// 转为INT
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="errorValue">如果转换错误返回的值</param>
        /// <returns></returns>
        public static int ObjToInt(this object thisValue, int errorValue)
        {
            int reval = 0;
            if (thisValue is Enum)
            {
                return (int)thisValue;
            }
            if (thisValue != null && thisValue != DBNull.Value && int.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return errorValue;
        }
        /// <summary>
        /// 转为INT
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="errorValue">如果转换错误返回的值</param>
        /// <returns></returns>
        public static int? ObjToIntNull(this object thisValue, int? errorValue)
        {
            int reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && int.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return errorValue;
        }
        /// <summary>
        /// 转换double
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static double ObjToDouble(this object thisValue)
        {
            double reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && double.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return 0;
        }
        /// <summary>
        /// 转换double
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static double? ObjToDoubleNull(this object thisValue)
        {
            double reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && double.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return null;
        }
        /// <summary>
        /// 转换double
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="errorValue">如果转换错误返回的值</param>
        /// <returns></returns>
        public static double ObjToDouble(this object thisValue, double errorValue)
        {
            double reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && double.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return errorValue;
        }
        /// <summary>
        /// 转换double
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="errorValue">如果转换错误返回的值</param>
        /// <returns></returns>
        public static double? ObjToDouble(this object thisValue, double? errorValue)
        {
            double reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && double.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return errorValue;
        }


        /// <summary>
        /// 转换double
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static long ObjToLong(this object thisValue)
        {
            long reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && long.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return 0;
        }
        /// <summary>
        /// 转换double
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static long? ObjToLongNull(this object thisValue)
        {
            long reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && long.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return null;
        }
        /// <summary>
        /// 转换double
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="errorValue">如果转换错误返回的值</param>
        /// <returns></returns>
        public static long ObjToLong(this object thisValue, long errorValue)
        {
            long reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && long.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return errorValue;
        }
        /// <summary>
        /// 转换double
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="errorValue">如果转换错误返回的值</param>
        /// <returns></returns>
        public static long? ObjToLongNull(this object thisValue, long? errorValue)
        {
            long reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && long.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return errorValue;
        }

        /// <summary>
        /// 转换字符串 会去掉头部和尾部的空格
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static string ObjToString(this object thisValue)
        {
            if (thisValue != null) return thisValue.ToString().Trim();
            return "";
        }
        /// <summary>
        /// 是否为空
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this object thisValue)
        {
            if (thisValue is Array)
            {
                if (thisValue == null)
                {
                    return true;
                }
                Type[] genericArgTypes = thisValue.GetType().GetGenericArguments();
                if (genericArgTypes == null || genericArgTypes.Count() == 0) return true;
            }
            if (thisValue is System.Collections.IEnumerable
                // && Reflection.GetEnumerableTypes(thisValue.GetType()).Any(x => x.IsEnum) //IEnumerable<enum> 专属
                )
            {
                var _thisValue = thisValue as System.Collections.IEnumerable;
                foreach (var _enum in _thisValue)
                {
                    return false;
                }
                return true;
            }
            return string.IsNullOrEmpty(ObjToString(thisValue));
        }
        /// <summary>
        /// 是否为空meiyou 
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this IEnumerable<object> thisValue)
        {
            if (thisValue == null || thisValue.Count() == 0) return true;
            return false;
        }

        /// <summary>
        /// 是否为空
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this IEnumerable<bool> thisValue)
        {
            if (thisValue == null || thisValue.Count() == 0) return true;
            return false;
        }
        /// <summary>
        /// 是否为空
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this IEnumerable<byte> thisValue)
        {
            if (thisValue == null || thisValue.Count() == 0) return true;
            return false;
        }
        /// <summary>
        /// 是否为空
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this IEnumerable<char> thisValue)
        {
            if (thisValue == null || thisValue.Count() == 0) return true;
            return false;
        }
        /// <summary>
        /// 是否为空
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this IEnumerable<decimal> thisValue)
        {
            if (thisValue == null || thisValue.Count() == 0) return true;
            return false;
        }
        /// <summary>
        /// 是否为空
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this IEnumerable<double> thisValue)
        {
            if (thisValue == null || thisValue.Count() == 0) return true;
            return false;
        }
        /// <summary>
        /// 是否为空
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this IEnumerable<float> thisValue)
        {
            if (thisValue == null || thisValue.Count() == 0) return true;
            return false;
        }
        /// <summary>
        /// 是否为空
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this IEnumerable<int> thisValue)
        {
            if (thisValue == null || thisValue.Count() == 0) return true;
            return false;
        }
        /// <summary>
        /// 是否为空
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this IEnumerable<long> thisValue)
        {
            if (thisValue == null || thisValue.Count() == 0) return true;
            return false;
        }
        /// <summary>
        /// 是否为空
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this IEnumerable<sbyte> thisValue)
        {
            if (thisValue == null || thisValue.Count() == 0) return true;
            return false;
        }
        /// <summary>
        /// 是否为空
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this IEnumerable<short> thisValue)
        {
            if (thisValue == null || thisValue.Count() == 0) return true;
            return false;
        }
        /// <summary>
        /// 是否为空
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this IEnumerable<uint> thisValue)
        {
            if (thisValue == null || thisValue.Count() == 0) return true;
            return false;
        }
        /// <summary>
        /// 是否为空
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this IEnumerable<ulong> thisValue)
        {
            if (thisValue == null || thisValue.Count() == 0) return true;
            return false;
        }
        /// <summary>
        /// 是否为空
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this IEnumerable<ushort> thisValue)
        {
            if (thisValue == null || thisValue.Count() == 0) return true;
            return false;
        }

        /// <summary>
        /// 是否包含
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="thisValue"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool IsIn<T>(this T thisValue, params T[] values)
        {
            return values.Contains(thisValue);
        }
        /// <summary>
        /// 是否包含
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="inValues"></param>
        /// <returns></returns>
        public static bool IsContainsIn(this string thisValue, params string[] inValues)
        {
            return inValues.Any(it => thisValue.Contains(it));
        }
        /// <summary>
        /// 是否在范围之内
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static bool IsInRange(this int thisValue, int begin, int end)
        {
            return thisValue >= begin && thisValue <= end;
        }
        /// <summary>
        /// 是否在范围之内
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static bool IsInRange(this DateTime thisValue, DateTime begin, DateTime end)
        {
            return thisValue >= begin && thisValue <= end;
        }
        /// <summary>
        /// 转换字符串 会去掉头部和尾部的空格
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="errorValue">如果转换错误返回的值</param>
        /// <returns></returns>
        public static string ObjToString(this object thisValue, string errorValue)
        {
            if (thisValue != null) return thisValue.ToString().Trim();
            return errorValue;
        }
        /// <summary>
        /// 转换金额
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static Decimal ObjToDecimal(this object thisValue)
        {
            Decimal reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && decimal.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return 0;
        }
        /// <summary>
        /// 转换金额
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static Decimal? ObjToDecimalNull(this object thisValue)
        {
            Decimal reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && decimal.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return null;
        }
        /// <summary>
        /// 转换金额
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="errorValue">如果转换错误返回的值</param>
        /// <returns></returns>
        public static Decimal ObjToDecimal(this object thisValue, decimal errorValue)
        {
            Decimal reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && decimal.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return errorValue;
        }
        /// <summary>
        /// 转换金额
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="errorValue">如果转换错误返回的值</param>
        /// <returns></returns>
        public static Decimal? ObjToDecimalNull(this object thisValue, decimal? errorValue)
        {
            Decimal reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && decimal.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return errorValue;
        }
        /// <summary>
        /// 转为时间类型
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static DateTime ObjToDate(this object thisValue)
        {
            DateTime reval = DateTime.MinValue;
            if (thisValue != null && thisValue != DBNull.Value && DateTime.TryParse(thisValue.ToString(), out reval))
            {
                reval = Convert.ToDateTime(thisValue);
            }
            return reval;
        }
        public static DateTime ObjToDateException(this object thisValue)
        {
            DateTime reval = DateTime.MinValue;
            if (thisValue != null && thisValue != DBNull.Value && DateTime.TryParse(thisValue.ToString(), out reval))
            {
                reval = Convert.ToDateTime(thisValue);
                return reval;
            }
            throw new Exception("日期类型格式错误");
        }


        /// <summary>
        /// 转为时间类型
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="errorValue">如果转换错误返回的值</param>
        /// <returns></returns>
        public static DateTime ObjToDate(this object thisValue, DateTime errorValue)
        {
            DateTime reval = DateTime.MinValue;
            if (thisValue != null && thisValue != DBNull.Value && DateTime.TryParse(thisValue.ToString(), out reval))
            {
                reval = Convert.ToDateTime(thisValue);
            }
            return errorValue;
        }


        /// <summary>
        /// 转为时间类型
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static DateTime? ObjToDateNull(this object thisValue)
        {
            if (thisValue != null && thisValue != DBNull.Value)
            {
                try
                {
                    return Convert.ToDateTime(thisValue);
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }
        /// <summary>
        /// 转为时间类型
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="FormatInfo">格式 yyyy/MM/dd</param>
        /// <returns></returns>
        public static DateTime? ObjToDateNull(this object thisValue, string FormatInfo)
        {
            DateTime reval = DateTime.MinValue;
            try
            {
                if (DateTime.TryParseExact(thisValue.ToString(),
                            FormatInfo,
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None,
                            out reval))
                    return reval;
                else
                {
                    return null;
                }

            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 转为时间类型
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="errorValue">如果转换错误返回的值</param>
        /// <returns></returns>
        public static DateTime? ObjToDateNull(this object thisValue, DateTime? errorValue)
        {
            if (thisValue != null && thisValue != DBNull.Value)
            {
                try
                {
                    return Convert.ToDateTime(thisValue);
                }
                catch
                {
                    return errorValue;
                }
            }
            return errorValue;
        }
        /// <summary>
        /// 转换特殊格式日期
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="FormatInfo">yyyyMMdd</param>
        /// <returns></returns>
        public static DateTime ObjToDate(this object thisValue, string FormatInfo)
        {
            DateTime reval = DateTime.MinValue;
            try
            {
                DateTime.TryParseExact(Convert.ToString(thisValue),
                                FormatInfo,
                                System.Globalization.CultureInfo.InvariantCulture,
                                System.Globalization.DateTimeStyles.None,
                                out reval);
            }
            catch
            {
                return reval;
            }
            return reval;
        }
        public static DateTime ObjToDateException(this object thisValue, string FormatInfo)
        {
            DateTime reval = DateTime.MinValue;
            try
            {
                DateTime.TryParseExact(Convert.ToString(thisValue),
                                FormatInfo,
                                System.Globalization.CultureInfo.InvariantCulture,
                                System.Globalization.DateTimeStyles.None,
                                out reval);
            }
            catch
            {
                throw new Exception("日期类型格式错误");
            }
            return reval;
        }

        /// <summary>
        /// 转换特殊格式日期
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="FormatInfo">yyyyMMdd</param>
        /// <returns></returns>
        public static DateTime ObjToDate(this object thisValue, string FormatInfo, DateTime errorValue)
        {
            DateTime reval = DateTime.MinValue;
            try
            {
                DateTime.TryParseExact(Convert.ToString(thisValue),
                                FormatInfo,
                                System.Globalization.CultureInfo.InvariantCulture,
                                System.Globalization.DateTimeStyles.None,
                                out reval);
            }
            catch
            {
                return errorValue;
            }
            return errorValue;
        }
        /// <summary>
        /// 转换布尔类型
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static bool ObjToBool(this object thisValue)
        {
            bool reval = false;
            if (thisValue != null && thisValue != DBNull.Value && bool.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return reval;
        }
        /// <summary>
        /// 转换布尔类型
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static bool? ObjToBoolNull(this object thisValue)
        {
            bool reval = false;
            if (thisValue != null && thisValue != DBNull.Value && bool.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return null;
        }

        /// <summary>
        /// 字符串根据逗号转换List_int
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static List<int> ObjToListInt(this object thisValue)
        {
            return ObjToListInt(thisValue, ',');
        }
        /// <summary>
        /// 字符串根据转separator换List_int
        /// </summary>
        /// <param name="separator">分隔符</param>
        /// <returns></returns>
        public static List<int> ObjToListInt(this object thisValue, char separator)
        {
            try
            {
                string[] ar = Convert.ToString(thisValue).Split(separator);
                List<int> ints = new List<int>();
                foreach (var item in ar)
                {
                    int v;
                    if (int.TryParse(item, out v))
                        ints.Add(v);
                }
                return ints;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 字符串根据逗号转换list_string
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static List<string> ObjToListString(this object thisValue)
        {
            return ObjToListString(thisValue, ',');
        }
        /// <summary>
        /// 字符串根据转separator换list
        /// </summary>
        /// <param name="separator">分隔符</param>
        /// <returns></returns>
        public static List<string> ObjToListString(this object thisValue, char separator)
        {
            try
            {
                string[] ar = Convert.ToString(thisValue).Split(separator);
                List<string> strings = new List<string>();
                foreach (var item in ar)
                {
                    strings.Add(item.ObjToString());
                }
                return strings;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// List转换为字符串(逗号)
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static string ListIntToString(this IEnumerable<int> thisValue)
        {
            try
            {
                return string.Join(",", thisValue.ToArray());

            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// List转换为字符串
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="separator">分隔符</param>
        /// <returns></returns>
        public static string ListIntToString(this IEnumerable<int> thisValue, string separator)
        {
            try
            {
                return string.Join(separator, thisValue.ToArray());

            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// List转换为字符串(逗号)
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static string ListStringToString(this IEnumerable<string> thisValue)
        {
            try
            {
                return string.Join(",", thisValue.ToArray());

            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// List转换为字符串
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="separator">分隔符</param>
        /// <returns></returns>
        public static string ListStringToString(this IEnumerable<string> thisValue, string separator)
        {
            try
            {
                return string.Join(separator, thisValue.ToArray());

            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 去掉所有HTML标签
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static string ObjToNoHtml(this object thisValue)
        {
            try
            {
                string _thisValue = thisValue.ObjToString();
                _thisValue = Regex.Replace(_thisValue, @"<script[^>]*?>.*?</script>", "",
                RegexOptions.IgnoreCase);
                //删除HTML
                _thisValue = Regex.Replace(_thisValue, @"<(.[^>]*)>", "",
                RegexOptions.IgnoreCase);
                _thisValue = Regex.Replace(_thisValue, @"([\r\n])[\s]+", "",
                RegexOptions.IgnoreCase);
                _thisValue = Regex.Replace(_thisValue, @"–>", "", RegexOptions.IgnoreCase);
                _thisValue = Regex.Replace(_thisValue, @"<!–.*", "", RegexOptions.IgnoreCase);
                _thisValue = Regex.Replace(_thisValue, @"&(quot|#34);", "\"",
                RegexOptions.IgnoreCase);
                _thisValue = Regex.Replace(_thisValue, @"&(amp|#38);", "&",
                RegexOptions.IgnoreCase);
                _thisValue = Regex.Replace(_thisValue, @"&(lt|#60);", "<",
                RegexOptions.IgnoreCase);
                _thisValue = Regex.Replace(_thisValue, @"&(gt|#62);", ">",
                RegexOptions.IgnoreCase);
                _thisValue = Regex.Replace(_thisValue, @"&(nbsp|#160);", " ",
                RegexOptions.IgnoreCase);
                _thisValue = Regex.Replace(_thisValue, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
                _thisValue = Regex.Replace(_thisValue, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
                _thisValue = Regex.Replace(_thisValue, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
                _thisValue = Regex.Replace(_thisValue, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
                _thisValue = Regex.Replace(_thisValue, @"&#(\d+);", "", RegexOptions.IgnoreCase);
                _thisValue.Replace("<", "");
                _thisValue.Replace(">", "");
                _thisValue.Replace("\r\n", "");

                _thisValue = HttpUtility.HtmlEncode(_thisValue).Trim();
                return _thisValue;

            }
            catch { return null; }
        }


        /// <summary>
        /// 去掉所有HTML标签 保留A标签
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static string ObjToNoHtmlKeep_A(this object thisValue)
        {
            try
            {
                string _thisValue = thisValue.ObjToString();
                //删除内含的 样式表代码
                Regex CutStyle = new Regex(@"<style([^>])*>(\w|\W)*?</style([^>])*>", RegexOptions.IgnoreCase);
                String TempStr = CutStyle.Replace(_thisValue, "");

                //<([^>]+)> 不过滤 img标签
                TempStr = TempStr.Replace("</a>", "[/a]");
                TempStr = TempStr.Replace("</a>", "[/a]");
                TempStr = TempStr.Replace("<a>", "[a]");
                TempStr = TempStr.Replace("<a>", "[a]");

                Regex SpanHtml1 = new Regex("<a", RegexOptions.IgnoreCase);
                TempStr = SpanHtml1.Replace(TempStr, "[a");
                Regex SpanHtml2 = new Regex("</a>", RegexOptions.IgnoreCase);
                TempStr = SpanHtml2.Replace(TempStr, "[/a]");
                Regex CutHtml = new Regex("<([^>]+)>", RegexOptions.IgnoreCase);
                TempStr = CutHtml.Replace(TempStr, "");


                TempStr = TempStr.Replace("[a", "<a");
                TempStr = TempStr.Replace("[/a]", "</a>");
                return TempStr;

            }
            catch { return null; }
        }

        /// <summary>
        /// 将字符串格式化为HTML格式
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static string ObjToHtml(this object thisValue)
        {
            return thisValue.ObjToString()
                .Replace("\r\n", "<br />")
                .Replace("\n", "<br />")
                .Replace("&quot;", "\"")
                .Replace("&amp;", "&")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&nbsp;", " ");
        }

        /// <summary>
        /// 获得Html内容的简介部分（截取）
        /// </summary>
        /// <param name="thisValue">内容</param>
        /// <param name="showLength">要显示的长度</param>
        /// <returns></returns>
        public static string ObjToIntroduction(this object thisValue, int showLength)
        {
            string _thisValue = thisValue.ObjToNoHtml();
            string introduction = _thisValue.Length > showLength ? _thisValue.Substring(0, showLength) + "..." : _thisValue;
            return introduction;
        }
        /// <summary>
        /// 去除字符串最后一个','号
        /// </summary>
        /// <param name="thisValue">:要做处理的字符串</param>
        /// <returns>返回已处理的字符串</returns>
        public static string ObjToLost(this object thisValue)
        {
            string _obj = thisValue.ObjToString();
            try
            {
                return _obj.Remove(_obj.LastIndexOf(","));
            }
            catch
            {
                return _obj;
            }

        }
      


        public static byte[] StreamToBytes(Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            int totalRead = 0;
            int bytesToRead = bytes.Length;

            while (bytesToRead > 0)
            {
                int bytesRead = stream.Read(bytes, totalRead, bytesToRead);
                if (bytesRead == 0)
                {
                    throw new EndOfStreamException("Stream ended before reading all bytes.");
                }
                totalRead += bytesRead;
                bytesToRead -= bytesRead;
            }

            // 设置当前流的位置为流的开始
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }




    }


   
}

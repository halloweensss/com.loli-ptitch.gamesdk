using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace GameSDK.RemoteConfigs
{
    public static class RemoteConfigTypeConverter
    {
        private static readonly Regex BooleanTruePattern = new Regex("^(1|true|t|yes|y|on)$", RegexOptions.IgnoreCase);
        private static readonly Regex BooleanFalsePattern = new Regex("^(0|false|f|no|n|off|)$", RegexOptions.IgnoreCase);

        public static bool TryConvertToType<T>(ref RemoteConfigValue remoteValue, out T value)
        {
            value = default;
            var typeCode = Type.GetTypeCode(typeof(T));

            try
            {
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                        {
                            var str = remoteValue.StringValue;
                            if (BooleanTruePattern.IsMatch(str))
                            {
                                value = (T)(object)true;
                                return true;
                            }
                            if (BooleanFalsePattern.IsMatch(str))
                            {
                                value = (T)(object)false;
                                return true;
                            }
                            return false;
                        }
                    case TypeCode.Byte:
                        value = (T)(object)remoteValue.Data[0];
                        return true;
                    case TypeCode.Char:
                        {
                            var str = remoteValue.StringValue;
                            if (!string.IsNullOrEmpty(str))
                                value = (T)(object)str[0];
                            else
                                value = (T)(object)(char)remoteValue.Data[0];
                            return true;
                        }
                    case TypeCode.DateTime:
                        value = (T)(object)DateTime.Parse(remoteValue.StringValue, CultureInfo.InvariantCulture);
                        return true;
                    case TypeCode.Decimal:
                        value = (T)(object)(decimal)BitConverter.ToDouble(remoteValue.Data, 0);
                        return true;
                    case TypeCode.Double:
                        value = (T)(object)BitConverter.ToDouble(remoteValue.Data, 0);
                        return true;
                    case TypeCode.Int16:
                        value = (T)(object)BitConverter.ToInt16(remoteValue.Data, 0);
                        return true;
                    case TypeCode.Int32:
                        value = (T)(object)BitConverter.ToInt32(remoteValue.Data, 0);
                        return true;
                    case TypeCode.Int64:
                        value = (T)(object)BitConverter.ToInt64(remoteValue.Data, 0);
                        return true;
                    case TypeCode.SByte:
                        value = (T)(object)(sbyte)remoteValue.Data[0];
                        return true;
                    case TypeCode.Single:
                        value = (T)(object)BitConverter.ToSingle(remoteValue.Data, 0);
                        return true;
                    case TypeCode.String:
                        value = (T)(object)remoteValue.StringValue;
                        return true;
                    case TypeCode.UInt16:
                        value = (T)(object)BitConverter.ToUInt16(remoteValue.Data, 0);
                        return true;
                    case TypeCode.UInt32:
                        value = (T)(object)BitConverter.ToUInt32(remoteValue.Data, 0);
                        return true;
                    case TypeCode.UInt64:
                        value = (T)(object)BitConverter.ToUInt64(remoteValue.Data, 0);
                        return true;
                    case TypeCode.Object:
                        {
                            var str = remoteValue.StringValue;
                            if (!string.IsNullOrEmpty(str))
                            {
                                value = JsonUtility.FromJson<T>(str);
                                return true;
                            }
                            return false;
                        }
                    default:
                        return false;
                }
            }
            catch
            {
                value = default;
                return false;
            }
        }
        
        public static bool TryConvertToType(Type type, ref RemoteConfigValue remoteValue, out object value)
        {
            var typeCode = Type.GetTypeCode(type);

            try
            {
                var stringValue = remoteValue.StringValue;
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                        if (BooleanTruePattern.IsMatch(stringValue))
                        {
                            value = true;
                            return true;
                        }

                        if (BooleanFalsePattern.IsMatch(stringValue))
                        {
                            value = false;
                            return true;
                        }

                        value = null;
                        return false;
                    case TypeCode.Byte:
                        value = Convert.ToByte(stringValue);
                        return true;
                    case TypeCode.Char:
                        if (string.IsNullOrEmpty(stringValue) == false && stringValue.Length > 0)
                            value = stringValue[0];
                        else
                            value = remoteValue.Data[0];
                        return true;
                    case TypeCode.DateTime:
                        value = DateTime.Parse(stringValue, CultureInfo.InvariantCulture);
                        return true;
                    case TypeCode.DBNull:
                        value = null;
                        return false;
                    case TypeCode.Decimal:
                        value = Convert.ToDouble(stringValue);
                        return true;
                    case TypeCode.Double:
                        value = Convert.ToDouble(stringValue);
                        return true;
                    case TypeCode.Empty:
                        value = null;
                        return false;
                    case TypeCode.Int16:
                        value = Convert.ToInt16(stringValue);
                        return true;
                    case TypeCode.Int32:
                        value = Convert.ToInt32(stringValue);
                        return true;
                    case TypeCode.Int64:
                        value = Convert.ToInt64(stringValue);
                        return true;
                    case TypeCode.SByte:
                        value = Convert.ToSByte(stringValue);
                        return true;
                    case TypeCode.Single:
                        value = Convert.ToSingle(stringValue);
                        return true;
                    case TypeCode.String:
                        value = remoteValue.StringValue;
                        return true;
                    case TypeCode.UInt16:
                        value = Convert.ToUInt16(stringValue);
                        return true;
                    case TypeCode.UInt32:
                        value = Convert.ToUInt32(stringValue);
                        return true;
                    case TypeCode.UInt64:
                        value = Convert.ToUInt64(stringValue);
                        return true;
                    case TypeCode.Object:
                        if (string.IsNullOrEmpty(stringValue) == false)
                        {
                            value = JsonUtility.FromJson(stringValue, type);
                            return true;
                        }

                        value = null;
                        return false;
                    default:
                        value = null;
                        return false;
                }
            }
            catch
            {
                value = null;
                return false;
            }
        }
    }
}
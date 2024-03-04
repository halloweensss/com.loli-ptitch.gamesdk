using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace GameSDK.RemoteConfigs
{
    public struct RemoteConfigValue
    {
        internal static Regex booleanTruePattern = new Regex("^(1|true|t|yes|y|on)$", RegexOptions.IgnoreCase);
        internal static Regex booleanFalsePattern = new Regex("^(0|false|f|no|n|off|)$", RegexOptions.IgnoreCase);

        internal RemoteConfigValue(byte[] data, ConfigValueSource source)
            : this()
        {
            this.Data = data;
            this.Source = source;
        }

        public bool BooleanValue
        {
            get
            {
                string stringValue = this.StringValue;
                if (booleanTruePattern.IsMatch(stringValue))
                    return true;
                if (booleanFalsePattern.IsMatch(stringValue))
                    return false;
                throw new FormatException(string.Format("RemoteConfigValue '{0}' is not a boolean value", (object) stringValue));
            }
        }

        public IEnumerable<byte> ByteArrayValue => (IEnumerable<byte>) this.Data;

        public double DoubleValue
        {
            get => Convert.ToDouble(this.StringValue, (IFormatProvider) CultureInfo.InvariantCulture);
        }

        public long LongValue
        {
            get => Convert.ToInt64(this.StringValue, (IFormatProvider) CultureInfo.InvariantCulture);
        }

        public string StringValue => Encoding.UTF8.GetString(this.Data);

        internal byte[] Data { get; set; }

        public ConfigValueSource Source { get; internal set; }   
        
        public override string ToString()
        {
            return this.StringValue;
        }
    }
}
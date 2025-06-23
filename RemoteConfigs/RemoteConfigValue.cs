using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace GameSDK.RemoteConfigs
{
    public struct RemoteConfigValue
    {
        private static readonly Regex BooleanTruePattern = new Regex("^(1|true|t|yes|y|on)$", RegexOptions.IgnoreCase);
        private static readonly Regex BooleanFalsePattern = new Regex("^(0|false|f|no|n|off|)$", RegexOptions.IgnoreCase);
        
        public RemoteConfigValue(byte[] data, ConfigValueSource source)
            : this()
        {
            this.Data = data;
            this.Source = source;
        }

        public bool BooleanValue
        {
            get
            {
                var stringValue = StringValue;
                if (BooleanTruePattern.IsMatch(stringValue))
                    return true;
                if (BooleanFalsePattern.IsMatch(stringValue))
                    return false;
                throw new FormatException($"RemoteConfigValue '{(object)stringValue}' is not a boolean value");
            }
        }

        public IEnumerable<byte> ByteArrayValue => (IEnumerable<byte>) this.Data;

        public double DoubleValue => Convert.ToDouble(this.StringValue, (IFormatProvider) CultureInfo.InvariantCulture);

        public long LongValue => Convert.ToInt64(this.StringValue, (IFormatProvider) CultureInfo.InvariantCulture);

        public string StringValue => Encoding.UTF8.GetString(this.Data);

        internal byte[] Data { get; set; }

        public ConfigValueSource Source { get; internal set; }
        
        public bool TryGetValue<T>(out T value) => RemoteConfigTypeConverter.TryConvertToType(ref this, out value);

        public bool TryGetValue(Type type, out object value) => RemoteConfigTypeConverter.TryConvertToType(type, ref this, out value);

        public override string ToString()
        {
            return this.StringValue;
        }
    }
}
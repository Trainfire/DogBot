using Google.Apis.Sheets.v4.Data;

namespace Extensions.GoogleSpreadsheets
{
    static class ExtendedValueEx
    {
        public static object GetValue(this ExtendedValue value)
        {
            if (value.BoolValue.HasValue)
                return value.BoolValue.Value;

            if (value.NumberValue.HasValue)
                return value.NumberValue.Value;

            if (!string.IsNullOrEmpty(value.StringValue))
                return value.StringValue;

            if (value.NumberValue.HasValue)
                return value.NumberValue.Value;

            return "";
        }
    }
}

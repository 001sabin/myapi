using Newtonsoft.Json.Linq;

namespace myapi.ExtensionMethods
{
    public static class StringExtension
    {
        public static bool IsValidEmployeeName(this string value)
        {
            return (!string.IsNullOrWhiteSpace(value) &&
           value.Length >= 2 &&
           value.Length <= 100);
        }

        public static bool IsValidEmployee(this string value)=>(!string.IsNullOrWhiteSpace(value) &&
           value.Length >= 2 &&
           value.Length <= 100);
    }
}

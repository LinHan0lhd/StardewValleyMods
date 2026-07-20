#nullable disable
using StardewModdingAPI;

namespace AutoServerPro.Utils
{
    public static class ReflectionHelper
    {
        private static IModHelper _helper;

        public static void Initialize(IModHelper helper) => _helper = helper;

        public static void InvokeMethod(object obj, string methodName, params object[] args)
        {
            _helper?.Reflection.GetMethod(obj, methodName)?.Invoke(args);
        }

        public static T GetFieldValue<T>(object obj, string fieldName)
        {
            return _helper.Reflection.GetField<T>(obj, fieldName).GetValue();
        }

        public static void SetFieldValue<T>(object obj, string fieldName, T value)
        {
            _helper?.Reflection.GetField<T>(obj, fieldName)?.SetValue(value);
        }

        public static T GetPropertyValue<T>(object obj, string propertyName)
        {
            return _helper.Reflection.GetProperty<T>(obj, propertyName).GetValue();
        }

        public static void SetPropertyValue<T>(object obj, string propertyName, T value)
        {
            _helper?.Reflection.GetProperty<T>(obj, propertyName)?.SetValue(value);
        }
    }
}
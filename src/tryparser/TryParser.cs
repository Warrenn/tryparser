using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Reflection;

public static class TryParser
{
    private delegate bool TryParseDelegate<T>(string stringValue, out T instance) where T : struct;

    private static readonly MethodInfo DefaultMethodInfo =
        typeof(TryParser)
            .GetMethod("Default", BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase);

    private static readonly MethodInfo NullableMethodInfo =
        typeof(TryParser)
            .GetMethod("Nullable", BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase);

    private static readonly ConcurrentDictionary<Type, Lazy<Delegate>> TryParseMethods =
        new ConcurrentDictionary<Type, Lazy<Delegate>>();

    private static readonly ConcurrentDictionary<Type, Lazy<MethodInfo>> DefaultMethodInfos =
        new ConcurrentDictionary<Type, Lazy<MethodInfo>>();

    private static readonly ConcurrentDictionary<Type, Lazy<MethodInfo>> NullableMethodInfos =
        new ConcurrentDictionary<Type, Lazy<MethodInfo>>();

    private static readonly string[] Formats =
    {
            "yyyy-MM-dd",
            "yyyy-MM-dd hh:mm:ss tt",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy/MM/dd",
            "yyyy/MM/dd hh:mm:ss tt",
            "yyyy/MM/dd HH:mm:ss",
            "dd/MM/yyyy",
            "dd/MM/yyyy HH:mm:ss",
            "dd/MM/yyyy hh:mm:ss tt",
            "dd-MM-yyyy HH:mm:ss",
            "dd-MM-yyyy hh:mm:ss tt",
            "o"
        };

    private static object InvokeDefault(Type conversionType, object value)
    {
        var methodInfo = DefaultMethodInfos
            .GetOrAdd(conversionType, new Lazy<MethodInfo>(
                () => DefaultMethodInfo.MakeGenericMethod(conversionType))).Value;
        return methodInfo.Invoke(null, new[] { value });
    }

    private static object InvokeNullable(Type conversionType, object value)
    {
        var methodInfo = NullableMethodInfos
            .GetOrAdd(conversionType, new Lazy<MethodInfo>(
                () => NullableMethodInfo.MakeGenericMethod(conversionType))).Value;
        return methodInfo.Invoke(null, new[] { value });
    }

    public static object ChangeType(object value, Type conversionType)
    {
        if (conversionType == typeof(DateTime))
        {
            return DateTime(value) ?? default;
        }
        if (!conversionType.IsValueType)
        {
            return Convert.ChangeType(value, conversionType);
        }
        if (!conversionType.IsGenericType || (conversionType.GetGenericTypeDefinition() != typeof(Nullable<>)))
        {
            return InvokeDefault(conversionType, value);
        }
        return conversionType.GenericTypeArguments[0] == typeof(DateTime)
            ? DateTime(value)
            : InvokeNullable(conversionType.GenericTypeArguments[0], value);
    }

    public static T ChangeType<T>(object value)
    {
        return (T)ChangeType(value, typeof(T));
    }


    public static DateTime? DateTime(object value, string[] formats = null)
    {
        if (value is DateTime)
        {
            return (DateTime?)value;
        }
        var stringValue = string.Format("{0}", value);
        if (string.IsNullOrEmpty(stringValue))
        {
            return null;
        }
        if (formats == null)
        {
            formats = Formats;
        }

        DateTime date;

        if (System.DateTime.TryParseExact(
            stringValue, formats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None, out date))
        {
            return date;
        }

        date = new DateTime(1899, 12, 30);

        double doubleValue;
        if (double.TryParse(stringValue, out doubleValue) &&
            (doubleValue <= 2958465) &&
            (doubleValue >= -693593))
        {
            return date.AddDays(doubleValue);
        }
        return null;
    }

    public static T? Nullable<T>(object value)
        where T : struct
    {
        if (value is T)
        {
            return (T)value;
        }
        var stringValue = string.Format("{0}", value);
        if (string.IsNullOrEmpty(stringValue))
        {
            return null;
        }
        T returnvalue;
        var tryParse = GetDelegate<T>(TryParseMethods);
        if ((tryParse != null) && (tryParse(stringValue, out returnvalue)))
        {
            return returnvalue;
        }
        return null;
    }

    public static T Default<T>(object value)
        where T : struct
    {
        var nullable = Nullable<T>(value) ?? default(T);
        return nullable;
    }

    private static TryParseDelegate<T> GetDelegate<T>(ConcurrentDictionary<Type, Lazy<Delegate>> dictionary)
        where T : struct
    {
        var type = typeof(T);

        if (type.IsEnum)
        {
            return Enum.TryParse<T>;
        }

        var lazy = dictionary
            .GetOrAdd(type, new Lazy<Delegate>(() =>
                {
                    var method =
                        type
                            .GetMethods(BindingFlags.Public | BindingFlags.Static)
                            .FirstOrDefault(m =>
                                            (m.Name == "TryParse") &&
                                            (m.GetParameters().Count() == 2) &&
                                            (m.GetParameters()[0].ParameterType == typeof(string)) &&
                                            (m.GetParameters()[1].IsOut));
                    if (method == null)
                    {
                        return null;
                    }
                    var returnValue =
                        Delegate.CreateDelegate(typeof(TryParseDelegate<T>), method);
                    return returnValue;
                }));
        return (TryParseDelegate<T>)lazy.Value;
    }
}
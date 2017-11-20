using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfigHandler<TOwner>
    {
        public Dictionary<Type, List<Func<object, string>>> TypePrinters =
            new Dictionary<Type, List<Func<object, string>>>();

        public Dictionary<PropertyInfo, List<Func<object, string>>> MemberPrinters =
            new Dictionary<PropertyInfo, List<Func<object, string>>>();

        public Dictionary<Type, CultureInfo> NumericCultures = new Dictionary<Type, CultureInfo>();

        public PrintingConfig<TOwner> PrintingConfig;
    }

    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner>
    {
        PrintingConfigHandler<TOwner> IPropertyPrintingConfig<TOwner>.ConfigDataHandler => configDataHandler;
        private readonly PrintingConfigHandler<TOwner> configDataHandler;
        private readonly PropertyInfo propertyInfo;
        private bool IsMemberSerialization => propertyInfo != null;

        public PropertyPrintingConfig(PrintingConfigHandler<TOwner> configDataHandler)
        {
            this.configDataHandler = configDataHandler;
        }

        public PropertyPrintingConfig(PrintingConfigHandler<TOwner> configDataHandler,
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            this.configDataHandler = configDataHandler;
            propertyInfo = ((MemberExpression) memberSelector.Body).Member as PropertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> printer)
        {
            Func<object, string> objPrinter = x => printer((TPropType) x);

            if (IsMemberSerialization)
            {
                if (!configDataHandler.MemberPrinters.ContainsKey(propertyInfo))
                    configDataHandler.MemberPrinters[propertyInfo] = new List<Func<object, string>>();
                configDataHandler.MemberPrinters[propertyInfo].Add(objPrinter);
            }
            else
            {
                if (!configDataHandler.TypePrinters.ContainsKey(typeof(TPropType)))
                    configDataHandler.TypePrinters[typeof(TPropType)] = new List<Func<object, string>>();
                configDataHandler.TypePrinters[typeof(TPropType)].Add(objPrinter);
            }
            return configDataHandler.PrintingConfig;
        }
    }

    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<PropertyInfo> excludedProperties = new HashSet<PropertyInfo>();
        private readonly PrintingConfigHandler<TOwner> configDataHandler;

        public PrintingConfig()
        {
            configDataHandler = new PrintingConfigHandler<TOwner> {PrintingConfig = this};
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            if (!excludedTypes.Contains(typeof(TPropType)))
                excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this.configDataHandler);
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propInfo = ((MemberExpression) memberSelector.Body).Member as PropertyInfo;
            if (!excludedProperties.Contains(propInfo))
                excludedProperties.Add(propInfo);
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this.configDataHandler, memberSelector);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var value = string.Empty;
            if (TryPrintPrimitiveValue(obj, ref value)) return value;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            foreach (var propertyInfo in type.GetProperties()
                .Where(x => !excludedProperties.Contains(x) && !excludedTypes.Contains(x.PropertyType)))
            {
                value = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
                if (configDataHandler.MemberPrinters.ContainsKey(propertyInfo))
                    value = configDataHandler.MemberPrinters[propertyInfo].Aggregate(value,
                        (current, func) => func(current));

                sb.Append($"{identation}{propertyInfo.Name} = {value}{Environment.NewLine}");
            }
            return sb.ToString();
        }

        private bool TryPrintPrimitiveValue(object obj, ref string s)
        {
            var currentType = obj.GetType();
            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (excludedTypes.Contains(currentType) || !finalTypes.Contains(currentType)) return false;

            if (configDataHandler.TypePrinters.ContainsKey(currentType))
                obj = configDataHandler.TypePrinters[currentType].Aggregate(obj, (current, func) => func(current));

            obj = TryParseNumericObj(obj, currentType);

            s = obj.ToString();
            return true;
        }

        private object TryParseNumericObj(object obj, Type currentType)
        {
            if (!configDataHandler.NumericCultures.ContainsKey(currentType)) return obj;
            var typeCheck = new Dictionary<Type, Func<object>>
            {
                // ReSharper disable AccessToModifiedClosure
                {typeof(int), () => ((int) obj).ToString(configDataHandler.NumericCultures[currentType])},
                {typeof(double), () => ((double) obj).ToString(configDataHandler.NumericCultures[currentType])},
                {typeof(long), () => ((long) obj).ToString(configDataHandler.NumericCultures[currentType])},
                {typeof(float), () => ((float) obj).ToString(configDataHandler.NumericCultures[currentType])},
            };

            obj = typeCheck[currentType]();
            return obj;
        }
    }
}
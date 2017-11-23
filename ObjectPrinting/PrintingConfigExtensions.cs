using System;
using System.Collections.Generic;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, int> printingConfig, CultureInfo info)
        {
            var config = ((IPropertyPrintingConfig<TOwner>) printingConfig).ConfigDataHandler;
            config.NumericCultures[typeof(int)] = info;

            return ((IPropertyPrintingConfig<TOwner>) printingConfig).ConfigDataHandler.PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, double> printingConfig, CultureInfo info)
        {
            var config = ((IPropertyPrintingConfig<TOwner>) printingConfig).ConfigDataHandler;
            config.NumericCultures[typeof(double)] = info;
            return ((IPropertyPrintingConfig<TOwner>) printingConfig).ConfigDataHandler.PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, long> printingConfig, CultureInfo info)
        {
            var config = ((IPropertyPrintingConfig<TOwner>) printingConfig).ConfigDataHandler;
            config.NumericCultures[typeof(long)] = info;

            return ((IPropertyPrintingConfig<TOwner>) printingConfig).ConfigDataHandler.PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, float> printingConfig, CultureInfo info)
        {
            var config = ((IPropertyPrintingConfig<TOwner>) printingConfig).ConfigDataHandler;
            config.NumericCultures[typeof(float)] = info;

            return ((IPropertyPrintingConfig<TOwner>) printingConfig).ConfigDataHandler.PrintingConfig;
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            Func<object, string> trimm = x => x.ToString().Length < maxLen
                ? (string)x
                : ((string)x).Substring(0, maxLen);

            var config = ((IPropertyPrintingConfig<TOwner>) propConfig).ConfigDataHandler;
            if (!config.TypePrinters.ContainsKey(typeof(string)))
                config.TypePrinters[typeof(string)] = new List<Func<object, string>>();
            config.TypePrinters[typeof(string)].Add(trimm);

            return ((IPropertyPrintingConfig<TOwner>) propConfig).ConfigDataHandler.PrintingConfig;
        }
    }
}
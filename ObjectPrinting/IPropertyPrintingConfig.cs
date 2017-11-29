namespace ObjectPrinting
{
    public interface IPropertyPrintingConfig<TOwner>
    {
        PrintingConfigHandler<TOwner> ConfigDataHandler { get; }
    }
}
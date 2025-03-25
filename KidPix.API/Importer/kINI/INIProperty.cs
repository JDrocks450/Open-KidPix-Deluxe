namespace KidPix.API.Importer.kINI
{
    public record INIProperty(string Name, string ValueString) : INIObject
    {
        public override string ToString()
        {
            return $"{Name} = {ValueString}";
        }
    }
}

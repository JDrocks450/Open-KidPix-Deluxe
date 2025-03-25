namespace KidPix.API.Importer.kINI
{
    public record INISection(string Name) : INIObject
    {
        public HashSet<INIObject> Objects { get; } = new();

        public IEnumerable<INIProperty> Properties => Objects.OfType<INIProperty>();

        public IEnumerable<INIComment> Comments => Objects.OfType<INIComment>();

        public override string ToString()
        {
            return $"{Name} {{ Values: {Properties.Count()}, Comments: {Comments.Count()} }}";
        }
    }
}

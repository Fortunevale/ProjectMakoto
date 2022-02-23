namespace Project_Ichigo.Objects;

public class CountryCodes
{
    public Dictionary<string, CountryInfo> List = new();

    public class CountryInfo
    {
        public string Name { get; set; }
        public string ContinentCode { get; set; }
    }
}
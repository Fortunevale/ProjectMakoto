namespace ProjectMakoto.Entities;

public class LanguageCodes
{
    public List<LanguageInfo> List = new();

    public class LanguageInfo
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }
}
namespace ProjectIchigo.Entities;

internal class LibreTranslateTranslation
{
    public string translatedText { get; set; }
    public DetectedLanguage detectedLanguage { get; set; }

    internal class DetectedLanguage
    {
        public decimal confidence { get; set; }
        public string language { get; set; }
    }
}

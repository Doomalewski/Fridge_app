namespace Fridge_app.Services
{
    public class GeminiServiceOptions
    {
        public const string SectionName = "Gemini";
        
        public string ApiKey { get; set; }
        public string ModelName { get; set; } = "gemini-2.0-flash";
    }
}

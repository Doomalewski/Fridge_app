namespace Fridge_app.Services
{
    public class GeminiServiceOptions
    {
        public const string SectionName = "OpenRouter";

        public string ApiKey { get; set; }
        public string ModelName { get; set; } = "deepseek/deepseek-v4-flash";
        public string Endpoint { get; set; } = "https://openrouter.ai/api/v1/chat/completions";
    }
}

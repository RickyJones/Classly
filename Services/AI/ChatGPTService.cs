
using Classly.Models;
using Classly.Models.Config;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System.Text.Json;

namespace Classly.Services.AI
{

    public interface IAIService
    {

        public Task<string>AskAIAsync(List<string> messages);
    }




    public class DeepSeekService: IAIService
    {
        private  HttpClient client;
        private readonly SiteSettings _siteSettings;

        public DeepSeekService(IOptions<SiteSettings> settings)
        {
            _siteSettings = settings.Value;
            client = new HttpClient();
            client.BaseAddress = new Uri("https://api.deepseek.com/v1/");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_siteSettings.AIKey}");
        }

        public async Task<string> AskAIAsync(List<string> messages)
        {
            var payload = new
            {
                model = "deepseek-chat", // or deepseek-coder, deepseek-math, etc.
                messages = messages.Select(m => new { role = "You are a helpful english language tutor AI.", content = m })
            };

            var response = await client.PostAsJsonAsync("chat/completions", payload);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            var output = json.GetProperty("choices")[0]
                             .GetProperty("message")
                             .GetProperty("content")
                             .GetString();

            return output;
        }

    }







    public class ChatGPTService: IAIService
    {
        private static ChatClient client;
        private readonly SiteSettings _siteSettings;
        public ChatGPTService(IOptions<SiteSettings> settings)
        {
            _siteSettings = settings.Value;
            client = new(model: "gpt-3.5-turbo", apiKey: _siteSettings.AIKey);

        }

        public string AskAI(string message)
        {

            ChatCompletion completion = client.CompleteChat(message);

            return completion.Content[0].Text;
        }

        public async Task<string> AskAIAsync(List<string> messages)
        {
            // Build the prompt messages
            var messagesPrompt = new List<ChatMessage>
    {
        ChatMessage.CreateSystemMessage("You are a helpful teaching assistant.")
    };

            foreach (var mes in messages)
            {
                messagesPrompt.Add(ChatMessage.CreateUserMessage(mes));
            }

            // Call the API with the full prompt
            var completion = await client.CompleteChatAsync(messagesPrompt);

            var aiOutput = completion.Value.Content[0].Text;

            return aiOutput;
        }

    }
}


using Classly.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using OpenAI.Chat;

namespace Classly.Services.AI
{
    public static class ChatGPTService
    {
        private static ChatClient client;

        public static void Init(string key)
        {
            client = new(model: "gpt-3.5-turbo", apiKey: key);
        }
        public static string AskAI(string message)
        {

            ChatCompletion completion = client.CompleteChat(message);

            return completion.Content[0].Text;
        }

        public static async Task<string> AskAIAsync(List<ChatMessage> messages) {

            var completion = await client.CompleteChatAsync(messages);
            
            var aiOutput = completion.Value.Content[0].Text;

            return aiOutput;
        }
    }
}

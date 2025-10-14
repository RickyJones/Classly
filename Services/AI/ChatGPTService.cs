
using Classly.Models;
using OpenAI.Chat;

namespace Classly.Services.AI
{
    public static class ChatGPTService
    {
        public static string AskAI(string message)
        {
            var apiKey = TestKeys.AIKey;
            ChatClient client = new(model: "gpt-3.5-turbo", apiKey: apiKey);

            ChatCompletion completion = client.CompleteChat("Say 'this is a test.'");

            return completion.Content[0].Text;
        }
    }
}

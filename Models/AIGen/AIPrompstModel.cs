namespace Classly.Models.AIGen
{

        public class AIPromptsModel
        {
            // Matches <textarea name="AITablePrompt">
            public string AITablePrompt { get; set; }

            // Matches <textarea name="AIHomewrokPrompt">
            public string AIHomewrokPrompt { get; set; }

            // Matches the third <textarea> (but you need a name attribute!)
            public string AILessonPlanPrompt { get; set; }
        }
    }

namespace Classly.Models
{
    public class GeneratedHomeworkModel
    {
        public FillInTheBlanks[] FillInTheBlanksQuestions { get; set; }
        public QuestionWithOptions[] QuestionsWithOptions { get; set; }
        public SentenceMatching SentenceMatching { get; set; }
        public CreateYourOwnSentences[] CreateYourOwnSentences { get; set; }
    }

    public class FillInTheBlanks
    {
        public string SentenceWithBlanks { get; set; }
        public string CorrectSentence { get; set; }
    }

    public class QuestionWithOptions
    {
        public string Question { get; set; }
        public string[] PossibleAnswers { get; set; }
        public string CorrectAnswer { get; set; }
    }

    public class SentenceMatching
    {
        /// <summary>
        /// Each pair contains the first half and second half of a sentence.
        /// </summary>
        public SentencePair[] Pairs { get; set; }
    }

    public class SentencePair
    {
        public string FirstHalf { get; set; }
        public string SecondHalf { get; set; }
    }

    public class CreateYourOwnSentences
    {
        public string WordToInclude { get; set; }
    }

}

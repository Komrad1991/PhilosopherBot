using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTGBot
{
    internal sealed class Question
    {
        public string QuestionText { get; set; }
        public List<string> Options { get; set; }
        public int CorrectOptionIndex { get; set; }

        public Question(string questionText, List<string> options, int correctOptionIndex)
        {
            if (correctOptionIndex < 0 || correctOptionIndex >= options.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(correctOptionIndex), "Индекс правильного ответа должен быть в пределах списка вариантов.");
            }

            QuestionText = questionText;
            Options = options;
            CorrectOptionIndex = correctOptionIndex;
        }

        public bool IsCorrect(int selectedOptionIndex)
        {
            return selectedOptionIndex == CorrectOptionIndex;
        }
    }
}

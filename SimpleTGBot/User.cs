using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace SimpleTGBot
{
    internal sealed class User
    {
        public long chatId { get; private set; }
        public List<int> questionsUsed { get; set; } = new List<int>();
        public int Answers { get; set; } = 0;
        public DateTime LastQuest { get; set; } = DateTime.Now;
        public int score { get; private set; } = 0;

        public User(long ChatId) {
            chatId = ChatId;
        }

        public bool AddQuest(int questID) {
            if (Answers == questionsUsed.Count() && Answers < 5)
            {
                questionsUsed.Add(questID);
                LastQuest = DateTime.Now;
                return true;
            } else if (DateTime.Now.Date > LastQuest.Date)
            {
                questionsUsed.Clear();
                questionsUsed.Add(questID);
                LastQuest = DateTime.Now;
                return true;
            }

            return false;
        }

        public int GetLastQuest()
        {
            if (questionsUsed.Count() > 0 && LastQuest.Date == DateTime.Now)
                return questionsUsed[questionsUsed.Count() - 1];
            else
                return -1;
        }

        public bool AddAnswer(bool isCorrect) {
            if (Answers == questionsUsed.Count())
                return false;

            if (isCorrect)
                score += 10;
            Answers++;

            return true;
        }
    }
}

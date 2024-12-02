using LiteDB;
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
        public long UserId { get; set; }
        public ObjectId Id { get; set; }
        public long chatId { get; private set; }
        //на что отвечал
        public List<int> questionsUsed { get; set; } = new List<int>();
        //всего вопросов
        public int Answers { get; set; } = 0;
        //когда был выдан последний вопрос
        public DateTime LastQuest { get; set; } = DateTime.Now;

        //очки
        public int score { get; private set; } = 0;

        public User() 
        {

        }
        public User(long ChatId, long id) {
            chatId = ChatId;
            UserId = id;
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
            if (questionsUsed.Count() > 0 && LastQuest.Date == DateTime.Now.Date)
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

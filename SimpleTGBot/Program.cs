namespace SimpleTGBot;

using LiteDB;
using System.IO;
using System.Text.Json;
using Telegram.Bot.Types;

public static class Program
{
    const string DBPATH = "users.db";

    public static async Task Main(string[] args)
    {
        User user1 = new User(120453);

        using (var db = new LiteDatabase(DBPATH))
        {
            var users = db.GetCollection<User>("users");
            users.Insert(user1);
        }

        using (var db = new LiteDatabase(DBPATH))
        {
            var users = db.GetCollection<User>("users");
            foreach (var user in users.FindAll())
            {
                Console.WriteLine($"ChatId: {user.chatId}, Score: {user.score}");
            }
        }

        TelegramBot telegramBot = new TelegramBot();
        await telegramBot.Run();
    }
}
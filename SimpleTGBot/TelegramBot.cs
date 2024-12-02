using System.Reflection.Metadata.Ecma335;

namespace SimpleTGBot;

using LiteDB;
using System.Net.Http.Headers;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public class TelegramBot
{
    // Токен TG-бота. Можно получить у @BotFather
    private const string BotToken = "7806168713:AAHcb2dRAlVvrEKr5n_tOnrgvi6nG2PkpI0";
    private ReplyKeyboardMarkup mainMenu = new ReplyKeyboardMarkup(new[] { new[] { new KeyboardButton("Начать игру") },new[] { new KeyboardButton("Моя статистика") }, new[] {new KeyboardButton("Глобальный рейтинг") }});
    private ReplyKeyboardMarkup answers = new ReplyKeyboardMarkup(new[] { new[] { new KeyboardButton("ответ 1") }, new[] { new KeyboardButton("ответ 2") }, new[] { new KeyboardButton("ответ 3") }, new[] { new KeyboardButton("ответ 4") } });
    private ReplyKeyboardMarkup backButton = new ReplyKeyboardMarkup(new KeyboardButton("Назад"));
    private ReplyKeyboardMarkup continueButton = new ReplyKeyboardMarkup(new[] { new[] { new KeyboardButton("Продолжить") }, new[] { new KeyboardButton("В меню") } });

    const string DBPATH = "users.db";


    List<Question> philosophyQuestions = new List<Question>
        {
            new Question(
                "Кто является основателем философии как науки?",
                new List<string> { "Платон", "Сократ", "Аристотель", "Пифагор" },
                1
            ),
            new Question(
                "Что такое 'категорический императив' в философии?",
                new List<string> { "Запрещённый акт", "Принцип в морали", "Логическое правило", "Философия естествознания" },
                1
            ),
            new Question(
                "Какой философ является автором идеи 'Cogito, ergo sum' (Я мыслю, следовательно, существую)?",
                new List<string> { "Фридрих Ницше", "Жан-Поль Сартр", "Рене Декарт", "Иммануил Кант" },
                2
            ),
            new Question(
                "Какой философ развивал идею 'все есть материя'?",
                new List<string> { "Гераклит", "Демокрит", "Платон", "Аристотель" },
                1
            ),
            new Question(
                "Кто из философов написал произведение 'Бытиё и ничто'?",
                new List<string> { "Фридрих Ницше", "Жан-Поль Сартр", "Мартин Хайдеггер", "Мишель Фуко" },
                1
            ),
            new Question(
                "Какое учение было основным у Платона?",
                new List<string> { "Рационализм", "Эмпиризм", "Идеализм", "Скептицизм" },
                2
            ),
            new Question(
                "Какая философская школа акцентирует внимание на поиске удовольствия?",
                new List<string> { "Стоицизм", "Эпикуреизм", "Платонизм", "Кинизм" },
                1
            ),
            new Question(
                "Какая книга является основным произведением Иммануила Канта?",
                new List<string> { "Критика чистого разума", "Древнегреческая философия", "Диалектика", "Бытие и время" },
                0
            ),
            new Question(
                "Какую философскую систему разработал Фридрих Ницше?",
                new List<string> { "Позитивизм", "Нигилизм", "Утилитаризм", "Воля к власти" },
                3
            ),
            new Question(
                "Какое учение утверждает, что вся реальность состоит из мысли?",
                new List<string> { "Идеализм", "Материализм", "Рационализм", "Прагматизм" },
                0
            )
        };
    /// <summary>
    /// Инициализирует и обеспечивает работу бота до нажатия клавиши Esc
    /// </summary>
    public async Task Run()
    {
        

        mainMenu.ResizeKeyboard = true;
        backButton.ResizeKeyboard = true;
        answers.ResizeKeyboard = true;
        continueButton.ResizeKeyboard = true;
        // Инициализируем наш клиент, передавая ему токен.
        var botClient = new TelegramBotClient(BotToken);
        // Служебные вещи для организации правильной работы с потоками
        using CancellationTokenSource cts = new CancellationTokenSource();
        
        // Разрешённые события, которые будет получать и обрабатывать наш бот.
        // Будем получать только сообщения. При желании можно поработать с другими событиями.
        ReceiverOptions receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = new [] { UpdateType.Message }
        };

        // Привязываем все обработчики и начинаем принимать сообщения для бота
        botClient.StartReceiving(
            updateHandler: OnMessageReceived,
            pollingErrorHandler: OnErrorOccured,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );
        
        // Проверяем что токен верный и получаем информацию о боте
        var me = await botClient.GetMeAsync(cancellationToken: cts.Token);
        Console.WriteLine($"Бот @{me.Username} запущен.\nДля остановки нажмите клавишу Esc...");
        
        // Ждём, пока будет нажата клавиша Esc, тогда завершаем работу бота
        while (Console.ReadKey().Key != ConsoleKey.Escape){}

        // Отправляем запрос для остановки работы клиента.
        cts.Cancel();
    }
    
    /// <summary>
    /// Обработчик события получения сообщения.
    /// </summary>
    /// <param name="botClient">Клиент, который получил сообщение</param>
    /// <param name="update">Событие, произошедшее в чате. Новое сообщение, голос в опросе, исключение из чата и т. д.</param>
    /// <param name="cancellationToken">Служебный токен для работы с многопоточностью</param>
    async Task OnMessageReceived(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Работаем только с сообщениями. Остальные события игнорируем
        var message = update.Message;
        
        if (message is null)
        {
            return;
        }
        // Будем обрабатывать только текстовые сообщения.
        // При желании можно обрабатывать стикеры, фото, голосовые и т. д.
        //
        // Обратите внимание на использованную конструкцию. Она эквивалентна проверке на null, приведённой выше.
        // Подробнее об этом синтаксисе: https://medium.com/@mattkenefick/snippets-in-c-more-ways-to-check-for-null-4eb735594c09
        if (message.Text is not { } messageText)
        {
            return;
        }

        // Получаем ID чата, в которое пришло сообщение. Полезно, чтобы отличать пользователей друг от друга.
        var chatId = message.Chat.Id;
        
        // Печатаем на консоль факт получения сообщения
        Console.WriteLine($"Получено сообщение в чате {chatId}: '{messageText}'");
        
        if (messageText == "/start")
        {
            User user1 = new User(chatId);

            using (var db = new LiteDatabase(DBPATH))
            {

                var users = db.GetCollection<User>("users");
                if (users.FindOne(x => x.chatId == chatId) == null)
                {
                    users.Insert(user1);
                }
            }
            await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Приветствую",
                        replyMarkup: mainMenu,
                        cancellationToken: cancellationToken);
            return;
        }
        else if (messageText == "Начать игру" || messageText == "Продолжить")
        {
            using (var db = new LiteDatabase(DBPATH))
            {

                var users = db.GetCollection<User>("users");
                var user = users.FindOne(x => x.chatId == chatId);
                if (user == null)
                {
                    user = new User(chatId);
                    users.Insert(user);
                }
                if (user.GetLastQuest() == -1)
                {
                    Random r = new Random();
                    int id = r.Next(0, 9);
                    while (user.questionsUsed.FindIndex(x => x == id) != -1)
                    {
                        id = r.Next(0, 9);
                    }
                    if (!user.AddQuest(id))
                    {
                        await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Вы ответили на все вопросы сегодня",
                        replyMarkup: mainMenu,
                        cancellationToken: cancellationToken);
                        return;
                    }
                }
                var currQuest = philosophyQuestions[user.GetLastQuest()];
                string options = "";
                for (int i = 0; i < currQuest.Options.Count(); i++)
                {
                    options += $"{i + 1})" + currQuest.Options[i] + "\n";
                }
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"""
                    {currQuest.QuestionText}
                    Варианты:
                    {options}
                    """,
                    replyMarkup: answers,
                    cancellationToken: cancellationToken);
               users.Update(user);
            }

        }
        else if (messageText == "ответ 1")
        {
            using (var db = new LiteDatabase(DBPATH))
            {

                var users = db.GetCollection<User>("users");
                var user = users.FindOne(x => x.chatId == chatId);
                if (user == null)
                {
                    user = new User(chatId);
                    users.Insert(user);
                }
                if (philosophyQuestions[user.GetLastQuest()].IsCorrect(0))
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Ваш ответ верный",
                        replyMarkup: continueButton,
                        cancellationToken: cancellationToken);
                    user.AddAnswer(true);
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Ваш ответ неверный",
                        replyMarkup: continueButton,
                        cancellationToken: cancellationToken);
                    user.AddAnswer(false);
                }
                users.Update(user);
            }
        }
        else if (messageText == "ответ 2")
        {
            using (var db = new LiteDatabase(DBPATH))
            {

                var users = db.GetCollection<User>("users");
                var user = users.FindOne(x => x.chatId == chatId);
                if (user == null)
                {
                    user = new User(chatId);
                    users.Insert(user);
                }
                if (philosophyQuestions[user.GetLastQuest()].IsCorrect(1))
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Ваш ответ верный",
                        replyMarkup: continueButton,
                        cancellationToken: cancellationToken);
                    user.AddAnswer(true);
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Ваш ответ неверный",
                        replyMarkup: continueButton,
                        cancellationToken: cancellationToken);
                    user.AddAnswer(false);
                }
                users.Update(user);
            }
        }
        else if (messageText == "ответ 3")
        {
            using (var db = new LiteDatabase(DBPATH))
            {

                var users = db.GetCollection<User>("users");
                var user = users.FindOne(x => x.chatId == chatId);
                if (user == null)
                {
                    user = new User(chatId);
                    users.Insert(user);
                }
                if (philosophyQuestions[user.GetLastQuest()].IsCorrect(2))
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Ваш ответ верный",
                        replyMarkup: continueButton,
                        cancellationToken: cancellationToken);
                    user.AddAnswer(true);
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Ваш ответ неверный",
                        replyMarkup: continueButton,
                        cancellationToken: cancellationToken);
                    user.AddAnswer(false);
                }
                users.Update(user);
            }
        }
        else if (messageText == "ответ 4")
        {
            using (var db = new LiteDatabase(DBPATH))
            {

                var users = db.GetCollection<User>("users");
                var user = users.FindOne(x => x.chatId == chatId);
                if (user == null)
                {
                    user = new User(chatId);
                    users.Insert(user);
                }
                if (philosophyQuestions[user.GetLastQuest()].IsCorrect(3))
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Ваш ответ верный",
                        replyMarkup: continueButton,
                        cancellationToken: cancellationToken);
                    user.AddAnswer(true);
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Ваш ответ неверный",
                        replyMarkup: continueButton,
                        cancellationToken: cancellationToken);
                    user.AddAnswer(false);
                }
                users.Update(user);
            }
        }
        else if (messageText == "Моя статистика")
        {
            await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"{"cюда стату"}",
            replyMarkup: backButton,
            cancellationToken: cancellationToken);
        }
        else if (messageText == "Глобальный рейтинг")
        {
            var globalRatin = $"{"сюда запихнуть список топа"}";
            await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Ты написал:\n" + messageText,
            replyMarkup: backButton,
            cancellationToken: cancellationToken);
        }
        else if (messageText == "Назад" || messageText == "В меню")
        {
            await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Ты написал:\n" + messageText,
            replyMarkup: mainMenu,
            cancellationToken: cancellationToken);
        }
        else
        {
            await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "ты уверен что такой вариант есть?",
                        replyMarkup: message.ReplyMarkup,
                        cancellationToken: cancellationToken);
        }

    }

    /// <summary>
    /// Обработчик исключений, возникших при работе бота
    /// </summary>
    /// <param name="botClient">Клиент, для которого возникло исключение</param>
    /// <param name="exception">Возникшее исключение</param>
    /// <param name="cancellationToken">Служебный токен для работы с многопоточностью</param>
    /// <returns></returns>
    Task OnErrorOccured(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        // В зависимости от типа исключения печатаем различные сообщения об ошибке
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        
        // Завершаем работу
        return Task.CompletedTask;
    }
}
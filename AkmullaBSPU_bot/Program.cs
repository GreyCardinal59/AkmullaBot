using AkmullaBSPU_bot.Contract;
using AkmullaBSPU_bot.Keyboards;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;


namespace AkmullaBSPU_bot
{
    internal class Program
    {
        static SqlConnection sql = new SqlConnection("");
        static void Main(string[] args)
        {
            var client = new TelegramBotClient(""); // Токен бота
            client.StartReceiving(Update, Error);
            Console.ReadLine();
        }

        async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken cts)
        {
            var msg = update.Message;
            if (msg?.Text != null)
            {
                Console.WriteLine($"{DateTime.Now} | {msg.Chat.FirstName ?? "Анон", -33} | {msg.Text}");

                if (msg.Text == "/start")
                {
                    if (sql.State == ConnectionState.Closed) // Проверяем, закрыто ли sql подключение
                    {
                        sql.Open(); // Открываем sql подключение
                        SqlCommand command1 = new SqlCommand($"if not exists (select * from bspu where chat_id = {msg.Chat.Id}) insert into bspu (chat_id, count) values ('{msg.Chat.Id}', '0')", sql); // Прописываем добавление данных в таблицу
                        await command1.ExecuteNonQueryAsync(); //Выполняем команду с добавлением данных
                        sql.Close();

                        //if (msg.Chat.Id == 1173284165)
                        //{
                        //    await botClient.SendTextMessageAsync(
                        //        chatId: msg.Chat.Id,
                        //        "Добрый день",
                        //        cancellationToken: cts);
                        //}
                        await botClient.SendPhotoAsync(
                            chatId: msg.Chat.Id,
                            photo: "https://idolms.bspu.ru/pluginfile.php/2/course/section/8/header-bg%20%281%29.jpg",
                            caption: "Привет!",
                            replyMarkup: keyboards.StartButtons(),
                            cancellationToken: cts); // Отправляем начальное сообщение
                        return;
                    }
                }

                if (msg.Text == "/get_files")
                {
                    try
                    {
                        await SendFiles(botClient, msg);
                        Console.WriteLine($"{DateTime.Now} | {msg.Chat.FirstName ?? "Анон",-33} | -> Скачал файлы");
                    }
                    catch (Exception)
                    {
                        await botClient.SendTextMessageAsync(
                            msg.Chat.Id,
                            "Вы еще не отправляли свои файлы",
                            cancellationToken: cts);
                    }

                }

                if (sql.State == ConnectionState.Closed) // Проверяем состояние подключения
                {
                    sql.Open();
                    SqlCommand command = new SqlCommand($"Select count From bspu Where chat_id = '{msg.Chat.Id}'", sql);
                    SqlDataReader reader = command.ExecuteReader();
                    await reader.ReadAsync();
                    long count = Convert.ToInt64(reader[0]); //count - обычная long переменная. Мы считали данные с таблицы и присвоили значение второй колонки этой переменной.
                    reader.Close();
                    sql.Close();

                    if (count == 1 & msg.Text != "Отмена") // Проверяем значение переменной и что введенный текст - не 'Отмена'
                    {
                        var answer = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Ответить на вопрос", $"https://t.me/{msg.Chat.Username}")
                            }
                        }); // Создаем кнопку Inline клавиатуры

                        await botClient.SendTextMessageAsync(
                            chatId: 518785094, // Сюда вводим индентификатор 'агента поддержки', т.к. сообщение отправляем ему
                            $"Пришло обращение от пользователя @{msg.Chat.Username ?? "анон"} \n" +
                            $"Текст сообщения:\n «{msg.Text}»",
                            replyMarkup: answer,
                            cancellationToken: cts);

                        await botClient.SendTextMessageAsync(
                            msg.Chat.Id,
                            "Обращение отправлено",
                            replyMarkup: new ReplyKeyboardRemove(),
                            cancellationToken: cts);

                        sql.Open();
                        SqlCommand command2 = new SqlCommand($"Update bspu Set count = '0' where chat_id = '{msg.Chat.Id}'", sql);
                        command2.ExecuteNonQuery(); //Аннулируем вторую колонку в БД.
                        sql.Close();
                    }

                    if (count == 2 & msg.Text != "Отмена")
                    {
                        string message = msg.Text;
                        WordHelper _helper = new WordHelper("Agreement.docx");
                        WordHelper _helper1 = new WordHelper("Contract.docx");

                        var items = new Dictionary<string, string>
                        {
                            { "{name}", message },
                            { "_______{date}_______", DateTime.Now.ToString("D") }
                        };

                        Message act = await botClient.SendTextMessageAsync(
                            msg.Chat.Id,
                            "Создание договора — пожалуйста, подождите...",
                            cancellationToken: cts);

                        _helper.Process(items, msg.Chat.Id.ToString());
                        _helper1.Process(items, msg.Chat.Id.ToString());

                        ConvertAgreement convertAgreement = new ConvertAgreement($"{msg.Chat.Id}Agreement.docx");
                        ConvertContract convertContract = new ConvertContract($"{msg.Chat.Id}Contract.docx");
                        convertAgreement.StartConvert(msg.Chat.Id.ToString());
                        convertContract.StartConvert(msg.Chat.Id.ToString());

                        System.IO.File.Delete(@$"../netcoreapp3.1/{msg.Chat.Id}Agreement.docx");
                        await Task.Delay(100);
                        System.IO.File.Delete(@$"../netcoreapp3.1/{msg.Chat.Id}Contract.docx");

                        await using Stream stream = System.IO.File.OpenRead(@$"../netcoreapp3.1/{msg.Chat.Id}/Agreement.pdf");
                        await botClient.SendDocumentAsync(
                            msg.Chat.Id,
                            document: new InputOnlineFile(content: stream, fileName: "Agreement.pdf"),
                            caption: "Согласие на обработку пресональных данных");

                        await using Stream stream2 = System.IO.File.OpenRead(@$"../netcoreapp3.1/{msg.Chat.Id}/Contract.pdf");
                        await botClient.SendDocumentAsync(
                            msg.Chat.Id,
                            document: new InputOnlineFile(content: stream2, fileName: "Contract.pdf"),
                            caption: "Договор на предоставление услуг");

                        await botClient.SendTextMessageAsync(
                            msg.Chat.Id,
                            "Вы можете отправить заполненный документ в любое время, независимо от того, где вы находитесь в боте.\n" +
                            "Также вы можете получить все свои файлы с помощи команды /get_files",
                            replyMarkup: keyboards.AbiturButtons(),
                            cancellationToken: cts);

                        sql.Open();
                        SqlCommand command2 = new SqlCommand($"Update bspu Set count = '0' where chat_id = '{msg.Chat.Id}'", sql);
                        command2.ExecuteNonQuery(); //Аннулируем вторую колонку в БД.
                        sql.Close();
                    }

                    if (count == 3 & msg.Text != "Отмена")
                    {
                        string keyWord = msg.Text;
                        try
                        {
                            List<string> hrefs = new HtmlWeb().Load(@"https://bspu.ru/").DocumentNode.SelectNodes($"//a[contains(translate(text(), 'АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ', 'абвгдеёжзийклмнопрстуфхцчшщъыьэюя'), '{keyWord.ToLower()}')]").Select(sn => sn.GetAttributeValue("href", null)).ToList();

                            int counter = 0;
                            foreach (var links in hrefs)
                            {
                                counter++;

                                await botClient.SendTextMessageAsync(
                                    msg.Chat.Id,
                                    $"{links}",
                                    disableWebPagePreview: true,
                                    replyMarkup: keyboards.CancelButtons(),
                                    cancellationToken: cts);

                                if (counter == 3) break;
                            }
                        }
                        catch (Exception)
                        {
                            List<string> newsHrefs = new List<string>();
                            var lastPageNews = new HtmlWeb().Load(@"https://bspu.ru/news").DocumentNode.SelectNodes($"//li[@class = 'page-item']/a").Select(sn => sn.InnerText).Where(s => s.All(c => char.IsDigit(c))).Max(s => int.Parse(s));

                            for (int i = 1; i <= lastPageNews; i++)
                            {
                                List<string> nHrefs = new HtmlWeb().Load($@"https://bspu.ru/news?page={i}").DocumentNode.SelectNodes($"//a[contains(@class, 'news-block')]").Where(d => d.InnerText.ToLower().Contains($"{keyWord.ToLower()}")).Select(sm => sm.GetAttributeValue("href", null)).ToList();
                                newsHrefs.AddRange(nHrefs);

                                foreach (var newsLinks in nHrefs)
                                {
                                    await botClient.SendTextMessageAsync(
                                    msg.Chat.Id,
                                    $"{newsLinks}",
                                    disableWebPagePreview: true,
                                    replyMarkup: keyboards.CancelButtons(),
                                    cancellationToken: cts);
                                }

                                if (i == 20) break;
                            }

                            //await botClient.SendTextMessageAsync(
                            //    msg.Chat.Id,
                            //    "Страницы не найдены. Попробуйте еще раз.",
                            //    replyMarkup: keyboards.StartButtons(),
                            //    cancellationToken: cts);
                        }
                    }
                }


                #region АБИТУРИЕНТ

                if (msg.Text == "Абитуриент🙍‍♂️")
                {
                    await botClient.SendTextMessageAsync(msg.Chat.Id, "Здравствуй, абитуриент", replyMarkup: keyboards.AbiturButtons());
                }

                if (msg.Text == "Хочу поступить")
                {
                    var keyboardAbitur = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithUrl("Бакалавриат", "https://abitur.bspu.ru/"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithUrl("Магистратура", "https://abitur.bspu.ru/")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithUrl("Аспирантура", "https://abitur.bspu.ru/"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithUrl("Для иностранных студентов", "https://abitur.bspu.ru/")
                        }
                    });

                    await botClient.SendTextMessageAsync(
                        msg.Chat.Id,
                        "Выберите направление:",
                        replyMarkup: keyboardAbitur,
                        cancellationToken: cts);

                    await botClient.SendTextMessageAsync(
                        msg.Chat.Id,
                        "Чтобы создать договор нажмите на соответствующую кнопку",
                        replyMarkup: keyboards.ActButtons(),
                        cancellationToken: cts);
                }

                if (msg.Text == "Создать договор")
                {
                    if (sql.State == ConnectionState.Closed) //Проверяем состояние подключения
                    {
                        await botClient.SendTextMessageAsync(
                            msg.Chat.Id,
                            "Для генерации договора введите ваше Ф.И.О.:",
                            replyMarkup: keyboards.CancelButtons(),
                            cancellationToken: cts);

                        sql.Open();
                        SqlCommand command = new SqlCommand($"Update bspu Set count = '2' where chat_id = '{msg.Chat.Id}'", sql); // Устанавливаем второй колонке значение 1
                        await command.ExecuteNonQueryAsync();
                        sql.Close();
                    }
                }

                if (msg.Text == "Об университете")
                {
                    await botClient.SendVideoAsync(
                        msg.Chat.Id,
                        video: "https://t.me/sqlprofi/21?comment=2685",
                        caption: "Башкирский государственный педагогический университет им. М. Акмуллы – флагман педагогического образования и науки Республики Башкортостан, один из ведущих педвузов России",
                        supportsStreaming: true,
                        cancellationToken: cts);
                }

                if (msg.Text == "Об институтах и факультетах")
                {
                    return;
                } // TODO

                if (msg.Text == "Мероприятия и курсы для абитуриентов")
                {
                    await botClient.SendTextMessageAsync(
                        msg.Chat.Id,
                        "Выберите направление, которое вас интересует",
                        replyMarkup: keyboards.EventsButtons(),
                        cancellationToken: cts);
                }

                /* Мероприятия и курсы для абитуриентов
                 * Блок содержит обработчики кнопок для мероприятий */

                if (msg.Text == "Курсы")
                {
                    await botClient.SendTextMessageAsync(
                        msg.Chat.Id,
                        "Курсы находятся в разработке, мы будем держать вас в курсе",
                        cancellationToken: cts);
                }

                if (msg.Text == "Олимпиада")
                {
                    var olymp = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                          InlineKeyboardButton.WithUrl("Зарегистрироваться на олимпиаду", "https://distolimp.bspu.ru/reg.php")
                        }
                    });

                    await botClient.SendPhotoAsync(
                        msg.Chat.Id,
                        photo: "https://sun9-north.userapi.com/sun9-85/s/v1/ig2/qn-iq-5m1D9k1qO31E_wDXiR3uLx6_-kbmpK29P1ubUqZgju-pijXn_a2IcPBkz2vsQicqh63WTaaj7ihVE1vlC6.jpg?size=2560x1543&quality=95&type=album",
                        caption: "💡 Акмуллинская олимпиада – это возможность раскрыть и проявить свои знания по разным общеобразовательным дисциплинам, а также получить дополнительные баллы при поступлении в наш университет!",
                        replyMarkup: olymp,
                        cancellationToken: cts);

                    await using Stream stream = System.IO.File.OpenRead(@"../Положение об Акмуллинской Олимпиаде.pdf");
                    await botClient.SendDocumentAsync(
                        chatId: msg.Chat.Id,
                        document: new InputOnlineFile(content: stream, fileName: "Положение об Акмуллинской Олимпиаде.pdf"));
                }

                if (msg.Text == "Ближайшие мероприятия для студентов")
                {
                    // Парсим ссылки на новости и забиваем их в List
                    List<string> hrefs = new HtmlWeb().Load(@"https://bspu.ru/").DocumentNode.SelectNodes("//a[contains(@class, 'ss-item')]").Select(sn => sn.GetAttributeValue("href", null)).ToList();
                    // Парсим и заголовки новостей
                    List<string> text = new HtmlWeb().Load(@"https://bspu.ru/").DocumentNode.SelectNodes("//div[contains(@class, 'emsi-title')]").Select(x => x.InnerText).ToList();

                    int count = 0;
                    foreach (var tuple in hrefs.Zip(text, (x, y) => (x, y))) // Объединяем элементы! двух списков
                    {
                        count++;

                        await botClient.SendTextMessageAsync(
                            msg.Chat.Id,
                            $"<a href='{tuple.Item1}'>{tuple.Item2}</a>", // Выводим гипертекст  с ссылками на новости
                            parseMode: ParseMode.Html,
                            disableWebPagePreview: true,
                            cancellationToken: cts);

                        if (count == 3) break; // Вводим необходимое кол-во получаемых новостей
                    }
                }

                /* Конец блока */

                if (msg.Text == "Свяжитесь с нами")
                {
                    await botClient.SendTextMessageAsync(
                        msg.Chat.Id,
                        "Колледж" +
                        "📞 +7 (347) 246-55-38\n" +
                        "📧 pk_colbgpu@mail.ru\n" +
                        "\nБакалавриат, специалитет и магистратура\n" +
                        "📞 +7 (347) 287-99-99\n" +
                        "📧 pk@bspu.ru\n" +
                        "\nАспирантура" +
                        "📞 +7 (347) 216-50-15\n" +
                        "📧 aspirantbspu@bk.ru\n" +
                        "\nИностранные абитуриенты" +
                        "📞 +7 (347) 246-32-70\n" +
                        "📧 oms_bspu2008@mail.ru",
                        replyMarkup: keyboards.FeedbackButtons(),
                        cancellationToken: cts);
                }

                if (msg.Text == "Связаться в Телеграм")
                {
                    if (sql.State == ConnectionState.Closed) //Проверяем состояние подключения
                    {
                        await botClient.SendTextMessageAsync(
                            msg.Chat.Id,
                            "Введите текст обращения. Оно будет доставлено агенту поддержки приемной комиссии, который ответит вам в личном сообщении",
                            replyMarkup: keyboards.CancelButtons(),
                            cancellationToken: cts);

                        sql.Open();
                        SqlCommand command = new SqlCommand($"Update bspu Set count = '1' where chat_id = '{msg.Chat.Id}'", sql);
                        await command.ExecuteNonQueryAsync();
                        sql.Close();
                    }
                }

                if (msg.Text == "Документы для поступления")
                {
                    await botClient.SendTextMessageAsync(
                        msg.Chat.Id,
                        "Выберите способ подачи докумнтов:",
                        replyMarkup: keyboards.DocsButtons(),
                        cancellationToken: cts);
                }
                if (msg.Text == "В электронном виде")
                {
                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "Личный адрес электронной почты\n" +
                       "Паспорт – вторая и третья страницы (скан/фото)\n" +
                       "Паспорт – разворот с регистрацией по месту жительства (скан/фото)\n" +
                       "Страховое свидетельство обязательного пенсионного страхования (СНИЛС) (скан/фото)\n" +
                       "Медицинская справка 086-у (на все направления 44.03.01, 44.03.02, 44.03.03, 44.03.04, 44.03.05, 44.05.01) (скан/фото)\n" +
                       "Документ о полученном образовании – титульный лист аттестата/диплома (скан/фото)\n" +
                       "Документ о полученном образовании – приложение к аттестату/диплому (скан/фото)\n" +
                       "Подписанное согласие на обработку персональных данных (скан/фото)\n" +
                       "При наличии:\n" +
                       "Дипломы олимпиад и конкурсов за последний год. Условия начисления баллов за индивидуальные достижения (скан/фото)\n" +
                       "Договор о целевом обучении (скан/фото)\n" +
                       "Документы, подтверждающие льготы (скан/фото)\n" +
                       "ИНН (скан/фото)\n",
                       cancellationToken: cts);
                }
                if (msg.Text == "Личная")
                {
                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "Личный адрес электронной почты\n" +
                       "Паспорт\n" +
                       "Страховое свидетельство обязательного пенсионного страхования (СНИЛС)\n" +
                       "Документ об образовании (требуется как минимум аттестат 11-го класса)\n" +
                       "Медицинская справка 086-у (на все направления 44.03.01, 44.03.02, 44.03.03, 44.03.04, 44.03.05, 44.05.01)\n" +
                       "При наличии:\n" +
                       "Дипломы олимпиад и конкурсов за последний год. Условия начисления баллов за индивидуальные достижения\n" +
                       "Договор о целевом обучении\n" +
                       "Документы, подтверждающие льготу\n" +
                       "ИНН\n",
                       cancellationToken: cts);
                }

                if (msg.Text == "FAQ❔")
                {
                    await botClient.SendTextMessageAsync(
                        msg.Chat.Id,
                        "Выберите вопрос, ответ на который вы хотели бы получить.\nТакже вы можете связаться с нами по телефону и электронной почте или обратиться к агенту поддержки института в телеграме с помощью команды /feedback",
                        replyMarkup: keyboards.AbiturFAQButtons(),
                        cancellationToken: cts);
                } // TODO
                if (msg.Text == "До какого числа можно выбрать предметы для сдачи?")
                {
                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "Необходимо сделать свой выбор до 1 февраля, текущего года",
                       cancellationToken: cts);
                }
                if (msg.Text == "Можно ли поступить в вуз без ЕГЭ?")
                {
                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "Да, можно, но это касается только определенных категорий граждан\n" +
                       "Например: иностранные граждане, абитуриенты у которых есть инвалидность, абитуриенты у которых уже имеется профессиональное образование (те кто закончил колледж или университет)",
                       cancellationToken: cts);
                }
                if (msg.Text == "Я заканчивал школу давно, без ЕГЭ, что делать?")
                {
                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "В этом случае необходимо обратится в свой отдел образования по месту проживания. Они вам подскажут всю процедуру и расскажут куда именно нужно подать документы",
                       cancellationToken: cts);
                }
                if (msg.Text == "Сколько действуют результаты ЕГЭ?")
                {
                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "Результаты ЕГЭ действительны в течении 4х лет, после того как вы их сдали",
                       cancellationToken: cts);
                }
                if (msg.Text == "Можно подать документы без результатов ЕГЭ?")
                {
                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "Да, можно. Все результаты сдачи ЕГЭ будут автоматически взяты из федеральной базы ЕГЭ",
                       cancellationToken: cts);
                }
                if (msg.Text == "Я сдавал ЕГЭ несколько раз,что тогда?")
                {
                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "При поступлении будет засчитывается наиболее высокий результат",
                       cancellationToken: cts);
                }
                if (msg.Text == "ЕГЭ по математике является обязательным?")
                {
                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "Нет, но только в том случае, если математика не указана как предмет вступительного испытания, на ту специальность на которую вы поступаете",
                       cancellationToken: cts);
                }
                if (msg.Text == "Можно ли поступить с базовой математикой?")
                {
                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "Нет, в университет можно поступить только с математикой профильного уровня",
                       cancellationToken: cts);
                }
                if (msg.Text == "Какой проходной балл в университет?")
                {
                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "Проходной балл формируется после того, как все абитуриенты сдадут свои документы и будут известны все результаты ЕГЭ",
                       cancellationToken: cts);
                }
                if (msg.Text == "Что нового в приемной компании?")
                {
                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "У абитуриентов есть возможность выбора ЕГЭ, которое он будет сдавать. Например, по некоторым специальностям, какие-то ЕГЭ будут указаны в скобочках – математика(физика). Это означает, что вы можете предоставить результаты ЕГЭ, либо по математике, либо по физике",
                       cancellationToken: cts);
                }
                if (msg.Text == "Как увеличить шансы на поступление?")
                {
                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "Выберите для сдачи ЕГЭ предметы по математике и обществознанию. Это самые популярные предметы, которые присутствуют почти в каждой специальности и направлении",
                       cancellationToken: cts);
                }

                #endregion

                #region СТУДЕНТ
                if (msg.Text == "Студент👨‍🎓")
                {
                    await botClient.SendTextMessageAsync(msg.Chat.Id, "Привет, студент", replyMarkup: keyboards.StudentButtons());
                }

                if (msg.Text == "Расписание занятий")
                {
                    var site = new InlineKeyboardMarkup(new[]
                       {
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Расписание занятий", "https://asu.bspu.ru/Rasp/")
                            },
                    });

                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "Вы можете узнать свое расписание по ссылке",
                       replyMarkup: site,
                       cancellationToken: cts);
                }

                if (msg.Text == "Найти корпус")
                {
                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "Выберите необходимый корпус",
                       replyMarkup: keyboards.СorpusButtuns(),
                       cancellationToken: cts);
                }
                if (msg.Text == "1")
                {
                    await botClient.SendVenueAsync(
                        msg.Chat.Id,
                        latitude: 54.723946,
                        longitude: 55.947800,
                        title: "БГПУ им. М.Акмуллы, 1 корпус",
                        address: "улица Ленина, 20",
                        cancellationToken: cts);
                }
                if (msg.Text == "2")
                {
                    await botClient.SendVenueAsync(
                        msg.Chat.Id,
                        latitude: 54.723335,
                        longitude: 55.948268,
                        title: "БГПУ им. М.Акмуллы, 2 корпус",
                        address: "улица Октябрьской Революции, 3",
                        cancellationToken: cts);
                }
                if (msg.Text == "5")
                {
                    await botClient.SendVenueAsync(
                        msg.Chat.Id,
                        latitude: 54.732581,
                        longitude: 55.929377,
                        title: "БГПУ им. М.Акмуллы, 5 корпус ",
                        address: "улица Чернышевского, 25А",
                        cancellationToken: cts);
                }
                if (msg.Text == "10")
                {
                    await botClient.SendVenueAsync(
                        msg.Chat.Id,
                        latitude: 54.731290,
                        longitude: 55.935146,
                        title: "БГПУ им. М.Акмуллы, 10 корпус",
                        address: "ул. Чернышевского, 49/1",
                        cancellationToken: cts);
                }

                if (msg.Text == "Полезные кабинеты")
                {
                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "В университете имеются:",
                       replyMarkup: keyboards.UsefulCabinetsButtons(),
                       cancellationToken: cts);
                }

                if (msg.Text == "Кафедры")
                {
                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "Выберите факультет",
                       replyMarkup: keyboards.DepartmentButtons(),
                       cancellationToken: cts);
                }
                if (msg.Text == "ИИПСГО")
                {
                    var site = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Обществознания, права и социального управления", "https://bspu.ru/unit/3")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Отечественной истории", "https://bspu.ru/unit/5")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Всеобщей истории и культурного наследия", "https://bspu.ru/unit/10")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Культурологии и социально-экономических дисциплин", "https://bspu.ru/unit/74")
                            }
                        });
                    await botClient.SendTextMessageAsync(
                        msg.Chat.Id,
                        "Выберите кафедру:",
                        replyMarkup: site,
                        cancellationToken: cts);
                }
                if (msg.Text == "ИП")
                {
                    var site = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Музыкального и хореографического образования", "https://bspu.ru/unit/20")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Теорий и методик начального образования", "https://bspu.ru/unit/22")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Дошкольной педагогики и психологии", "https://bspu.ru/unit/23")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Специальной педагогики и психологии", "https://bspu.ru/unit/24")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Педагогики", "https://bspu.ru/unit/107")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Профессионального и социального образования", "https://bspu.ru/unit/16")
                            }
                        });
                    await botClient.SendTextMessageAsync(
                        msg.Chat.Id,
                        "Выберите кафедру:",
                        replyMarkup: site,
                        cancellationToken: cts);

                }
                if (msg.Text == "ИФМЦН")
                {
                    var site = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Информационных технологий", "https://bspu.ru/unit/318")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Математики и статистики", "https://bspu.ru/unit/85")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Физики и нанотехнологий", "https://bspu.ru/unit/91")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Программирования и вычислительной математики", "https://bspu.ru/unit/90")
                            }    
                        });
                    await botClient.SendTextMessageAsync(
                        msg.Chat.Id,
                        "Выберите кафедру:",
                        replyMarkup: site,
                        cancellationToken: cts);

                }
                if (msg.Text == "ИФК")
                {
                    var site = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Теории и методики физ. культуры и спорта", "https://bspu.ru/unit/97")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Физического воспитания и спортивной борьбы", "https://bspu.ru/unit/98")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Охраны здоровья и безопасности жизнед.", "https://bspu.ru/unit/99")
                            }
                        });
                    await botClient.SendTextMessageAsync(
                        msg.Chat.Id,
                        "Выберите кафедру:",
                        replyMarkup: site,
                        cancellationToken: cts);

                }
                if (msg.Text == "ИФОМК")
                {
                    var site = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Английского языка", "https://bspu.ru/unit/34")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Романо-германского языкознания и зарубежной литер.", "https://bspu.ru/unit/98")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Методики преподавания ин. яз. и второго ин. яз.", "https://bspu.ru/unit/99")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Русской литературы", "https://bspu.ru/unit/41")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Русского языка, теор. и прикл. лингвистики", "h")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Татарского языка и литературы", "https://bspu.ru/unit/47")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Иностранных языков", "https://bspu.ru/unit/112")
                            }
                        });
                    await botClient.SendTextMessageAsync(
                        msg.Chat.Id,
                        "Выберите кафедру:",
                        replyMarkup: site,
                        cancellationToken: cts);

                }
                if (msg.Text == "ЕГФ")
                {
                    var site = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Биоэкологии и биологического образования", "https://bspu.ru/unit/62")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Генетики и химии", "https://bspu.ru/unit/67")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Экологии, географии и природопользования", "https://bspu.ru/unit/71")
                            }
                        });
                    await botClient.SendTextMessageAsync(
                        msg.Chat.Id,
                        "Выберите кафедру:",
                        replyMarkup: site,
                        cancellationToken: cts);

                }
                if (msg.Text == "ФБФ")
                {
                    var site = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Башкирского языка и литературы", "https://bspu.ru/unit/103")
                            }
                        });
                    await botClient.SendTextMessageAsync(
                        msg.Chat.Id,
                        "Выберите кафедру:",
                        replyMarkup: site,
                        cancellationToken: cts);

                }
                if (msg.Text == "ФП")
                {
                    var site = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Общей и педагогической психологии", "https://bspu.ru/unit/52")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Возрастной и социальной психологии", "https://bspu.ru/unit/54")
                            }
                        });
                    await botClient.SendTextMessageAsync(
                        msg.Chat.Id,
                        "Выберите кафедру:",
                        replyMarkup: site,
                        cancellationToken: cts);

                }
                if (msg.Text == "ХГФ")
                {
                    var site = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Изобразительного искусства", "https://bspu.ru/unit/78")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Дизайна", "https://bspu.ru/unit/79")
                            }
                        });
                    await botClient.SendTextMessageAsync(
                        msg.Chat.Id,
                        "Выберите кафедру:",
                        replyMarkup: site,
                        cancellationToken: cts);

                }
                if (msg.Text == "Технопарк👨‍🔬")
                {
                    var site = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Узнавай больше", "https://vk.com/technopark_bspu")
                            }
                        });
                    await botClient.SendVideoAsync(
                       msg.Chat.Id,
                       video: "https://t.me/sqlprofi/21?comment=2754",
                       caption: "Технопарк БГПУ им.М.Акмуллы",
                       supportsStreaming: true,
                       replyMarkup: site,
                       cancellationToken: cts);

                }
                if (msg.Text == "Центр карьеры")
                {
                    var site = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Узнать больше", "https://vk.com/ostsom")
                            }
                        });
                    await botClient.SendPhotoAsync(
                        msg.Chat.Id,
                        photo: "https://sun9-45.userapi.com/impg/sctygFKjB1P5NFdzt1RWd4bu_iaAZVyZm5bO5Q/6z5p2XdmBB8.jpg?size=1920x984&quality=95&sign=7c1ee029f73dc9440339493ef3720b30&type=album",
                        caption: "Ярмарка вакансий для молодежи «JOB MARKET»",
                        parseMode: ParseMode.Html,
                        replyMarkup: site,
                        cancellationToken: cts);
                }
                if (msg.Text == "Деканаты")
                {
                    await botClient.SendTextMessageAsync(
                        msg.Chat.Id,
                        "Информация по деканатам уточняется",
                        cancellationToken: cts);
                }

                if (msg.Text == "Внеучебная деятельность")
                {
                    await botClient.SendTextMessageAsync(
                        msg.Chat.Id,
                        "Во внеучебную деятельность входит:",
                        replyMarkup: keyboards.OutUniversityButtons(),
                        cancellationToken: cts);
                }
                if (msg.Text == "Спортивный клуб")
                {
                    var site = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Узнать больше", "https://vk.com/arslan_bspu")
                            }
                        });

                    await botClient.SendVideoAsync(
                       msg.Chat.Id,
                       video: "https://t.me/sqlprofi/21?comment=2745",
                       caption: "Открытие III Арслановских Игр | 2022",
                       replyMarkup: site,
                       supportsStreaming: true,
                       cancellationToken: cts);
                }
                if (msg.Text == "Волонтерский центр")
                {
                    var site = new InlineKeyboardMarkup(new[]
{
                        new[]
                        {
                          InlineKeyboardButton.WithUrl("Узнать больше", "https://vk.com/volunteer_bspu")
                        }
                    });

                    await botClient.SendPhotoAsync(
                        msg.Chat.Id,
                        photo: "https://sun9-74.userapi.com/impf/c849032/v849032343/14888b/esu9mNA0H64.jpg?size=2560x1707&quality=96&sign=c094ce45f87845ffcab346a97a6a18ba&type=album",
                        caption: "Найди себя - стань волонтером! Быть волонтером - модно! Начни с себя - будь волонтером сегодня! Вчера, сегодня, завтра. Волонтер это не напрасно! Развивайся с нами - стань волонтером!",
                        parseMode: ParseMode.Html,
                        replyMarkup: site,
                        cancellationToken: cts);
                }
                if (msg.Text == "Лига КВН")
                {
                    var site = new InlineKeyboardMarkup(new[]
{
                        new[]
                        {
                          InlineKeyboardButton.WithUrl("Узнать больше", "https://vk.com/kvnbspu")
                        }
                    });

                    await botClient.SendPhotoAsync(
                        msg.Chat.Id,
                        photo: "https://sun9-46.userapi.com/impf/c845420/v845420649/1a6237/q6AFLINUDU0.jpg?size=2560x1707&quality=96&sign=0c26ee6afe200024fb6deaf85215ee1c&type=album",
                        caption: "Лига КВН \"Наша\" БГПУ им. М. Акмуллы структурная единица Союза КВН Республики Башкортостан.",
                        parseMode: ParseMode.Html,
                        replyMarkup: site,
                        cancellationToken: cts);
                }
                if (msg.Text == "Студенческие отряды")
                {
                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "Бим бам бом бим",
                       replyMarkup: keyboards.StudentBrigadeButtons(),
                       cancellationToken: cts);
                }
                if (msg.Text == "Отряд проводников")
                {
                    var site = new InlineKeyboardMarkup(new[]
{
                        new[]
                        {
                          InlineKeyboardButton.WithUrl("Узнать больше", "https://vk.com/sop_bgpu")
                        }
                    });

                    await botClient.SendPhotoAsync(
                         msg.Chat.Id,
                         photo: "https://sun7-15.userapi.com/impg/udwkv5Ja_yJHBD9ThSAdH4yU9FavcRBOSvkAgQ/1T_2TBgONXU.jpg?size=736x912&quality=95&sign=2e377935f086b5fcdd2c355475accae5&type=album",
                         caption: "Отряд проводников\"Вокруг света\"",
                         parseMode: ParseMode.Html,
                         replyMarkup: site,
                         cancellationToken: cts);
                }
                if (msg.Text == "Педагогический отряд")
                {
                    var question = new InlineKeyboardMarkup(new[]
{
                        new[]
                        {
                          InlineKeyboardButton.WithUrl("Узнать больше", "https://vk.com/bspufenics")
                        }
                    });

                    await botClient.SendPhotoAsync(
                       msg.Chat.Id,
                       photo: "https://sun9-north.userapi.com/sun9-81/s/v1/ig2/87HuGo9_aWxslz_M6Aw7E5GLGEHTi-ZvKfXqoT7ZRfPD2L0h4YJQ1TL_eJ1Ah6wnJ8CWrphUJkBAKVITO8lrTur4.jpg?size=1080x1080&quality=95&type=album",
                       caption: "СПО «Феникс» им. В. А. Котика",
                       replyMarkup: question,
                       cancellationToken: cts);
                }
                if (msg.Text == "Сервисный отряд")
                {
                    var question = new InlineKeyboardMarkup(new[]
{
                        new[]
                        {
                          InlineKeyboardButton.WithUrl("Узнать больше", "https://vk.com/sservo_south_wind")
                        }
                    });

                    await botClient.SendVideoAsync(
                       msg.Chat.Id,
                       video: "https://t.me/sqlprofi/21?comment=2800", 
                       caption: "Целинные лагеря | ССервО \"Южный ветер\"",
                       supportsStreaming: true,
                       replyMarkup: question,
                       cancellationToken: cts);
                }
                if (msg.Text == "Хореография")
                {
                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "У нас есть:",
                       replyMarkup: keyboards.DanceButtons(),
                       cancellationToken: cts);
                }
                if (msg.Text == "Группа барабанщиц и мажореток")
                {
                    var question = new InlineKeyboardMarkup(new[]
{
                        new[]
                        {
                          InlineKeyboardButton.WithUrl("Узнать больше", "https://vk.com/derzhavaufa")
                        }
                    });

                    await botClient.SendVideoAsync(
                       msg.Chat.Id,
                       video: "https://t.me/sqlprofi/21?comment=2747",
                       caption: "Подготовка нового состава для большой публики идет полным ходом 🥁",
                       supportsStreaming: true,
                       replyMarkup: question,
                       cancellationToken: cts);
                }
                if (msg.Text == "Современный и эстрадный танец")
                {
                    var question = new InlineKeyboardMarkup(new[]
{
                        new[]
                        {
                          InlineKeyboardButton.WithUrl("Узнать больше", "https://vk.com/gracia_bspu")
                        }
                    });

                    await botClient.SendVideoAsync(
                       msg.Chat.Id,
                       video: "https://t.me/sqlprofi/21?comment=2748",
                       caption: "Народный ансамбль современного эстрадного танца \"Грация\"",
                       supportsStreaming: true,
                       replyMarkup: question,
                       cancellationToken: cts);
                }
                if (msg.Text == "Народный танец")
                {
                    var question = new InlineKeyboardMarkup(new[]
{
                        new[]
                        {
                          InlineKeyboardButton.WithUrl("Узнать больше", "https://vk.com/club4410846")
                        }
                    });

                    await botClient.SendVideoAsync(
                       msg.Chat.Id,
                       video: "https://t.me/sqlprofi/21?comment=2749",
                       caption: "Народный ансамбль народного танца \"Кружева\"",
                       supportsStreaming: true,
                       replyMarkup: question,
                       cancellationToken: cts);
                }
                if (msg.Text == "Спортивно-аэробическое шоу")
                {
                    var question = new InlineKeyboardMarkup(new[]
{
                        new[]
                        {
                          InlineKeyboardButton.WithUrl("Узнать больше", "https://vk.com/stradl_bspu")
                        }
                    });

                    await botClient.SendVideoAsync(
                       msg.Chat.Id,
                       video: "https://t.me/sqlprofi/21?comment=2750",
                       caption: "Народный коллектив - спортивно-аэробическое шоу \"Страдл\"",
                       supportsStreaming: true,
                       replyMarkup: question,
                       cancellationToken: cts);
                }
                if (msg.Text == "Театр")
                {
                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "У нас есть:",
                       replyMarkup: keyboards.TheatreButtons(),
                       cancellationToken: cts);
                }
                if (msg.Text == "Театральная студия")
                {
                    var question = new InlineKeyboardMarkup(new[]
{
                        new[]
                        {
                          InlineKeyboardButton.WithUrl("Узнать больше", "https://vk.com/apartebspu")
                        }
                    });

                    await botClient.SendVideoAsync(
                       msg.Chat.Id,
                       video: "https://t.me/sqlprofi/21?comment=2751",
                       caption: "Театральная студия «A parte» БГПУ",
                       supportsStreaming: true,
                       replyMarkup: question,
                       cancellationToken: cts);
                }
                if (msg.Text == "Коллектив народного творчества")
                {
                    var question = new InlineKeyboardMarkup(new[]
{
                        new[]
                        {
                          InlineKeyboardButton.WithUrl("Узнать больше", "https://vk.com/theatr2022")
                        }
                    });

                    await botClient.SendVideoAsync(
                       msg.Chat.Id,
                       video: "https://t.me/sqlprofi/21?comment=2752",
                       caption: "Коллектив народного творчества «ТАБЫН»",
                       supportsStreaming: true,
                       replyMarkup: question,
                       cancellationToken: cts);
                }
                if (msg.Text == "Школа ведущих")
                {
                    var site = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Узнать больше", "https://vk.com/vedushie_bspu")
                            }
                        });

                    await botClient.SendPhotoAsync(
                        msg.Chat.Id,
                        photo: "https://sun9-19.userapi.com/impg/VIDjD5XNsa-Nf9-0i45eGLsyrqvZDB0dVowjFw/go_q0Hrgpb0.jpg?size=2560x1707&quality=95&sign=2dd28cf937e1dada8eef11fb49485db7&type=album",
                        caption: "ШКОЛА ВЕДУЩИХ БГПУ им. М.Акмуллы",
                        parseMode: ParseMode.Html,
                        replyMarkup: site,
                        cancellationToken: cts);
                }
                if (msg.Text == "Медиацентр")
                {
                    var question = new InlineKeyboardMarkup(new[]
{
                        new[]
                        {
                          InlineKeyboardButton.WithUrl("Узнать больше", "https://vk.com/bsputeam")
                        }
                    });

                    await botClient.SendVideoAsync(
                       msg.Chat.Id,
                       video: "https://t.me/sqlprofi/21?comment=2753",
                       caption: "Топ 5 мест для фотосессии по версии \"bsputeam\"",
                       supportsStreaming: true,
                       replyMarkup: question,
                       cancellationToken: cts);
                }
                if (msg.Text == "Студенческое научное общество")
                {
                    var site = new InlineKeyboardMarkup(new[]
{
                        new[]
                        {
                          InlineKeyboardButton.WithUrl("Узнать больше", "https://vk.com/sno_bspu")
                        }
                    });

                    await botClient.SendPhotoAsync(
                        msg.Chat.Id,
                        photo: "https://sun9-63.userapi.com/impg/dn1YNrQUChxja60vBKIpFNVa9yw3E7nvha253Q/H5ZJ3stP7OI.jpg?size=2560x1707&quality=95&sign=afd9bb995db00c9024a570d85a6dd8df&type=album",
                        caption: "Студенческое научное общество - общественная организация студентов БГПУ им. М. Акмуллы, объединившихся на основе общности научных интересов и активно занимающихся научно-исследовательской работой.",
                        parseMode: ParseMode.Html,
                        replyMarkup: site,
                        cancellationToken: cts);
                }

                if (msg.Text == "FAQ❓")
                {
                    await botClient.SendTextMessageAsync(
                        msg.Chat.Id,
                        "Выберите вопрос, ответ на который вы хотели бы получить",
                        replyMarkup: keyboards.StudentFAQButtons(),
                        cancellationToken: cts);
                }
                if (msg.Text == "Как вычисляется оценка и рейтинг?")
                {
                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "- оценки, полученные  за различные виды работ, умножаются на соответствующие весовые коэффициенты для вычисления рейтинга по данной контрольной точке;\n" +
                       "- итоговый рейтинг по дисциплине вычисляется на основе рейтинга по контрольным точкам с учетом веса контрольных точек.",
                       cancellationToken: cts);
                }
                if (msg.Text == "Стипендия при пересдаче по «бегунку»?")
                {
                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "Если пересдача прошла в течение сессии, то стипендию Вы получать будете. При условии, что все экзамены и зачеты сданы и у Вас нет оценок «удовлетворительно».",
                       cancellationToken: cts);
                }
                if (msg.Text == "Как вычислить последний день сессии?")
                {
                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "Дата окончания сессии вычисляется по учебному графику группы как последний день недели, на которой по графику стоит последний экзамен.",
                       cancellationToken: cts);
                }
                if (msg.Text == "Что делать, если в ведомости ошибка?")
                {
                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "Необходимо сообщить преподавателю об ошибке в заполнении ведомости для ее исправления. Если оценки в ведомости не проставлены всем студентам группы, то возможно преподаватель еще не успел внести оценки в ведомость",
                       cancellationToken: cts);
                }
                if (msg.Text == "А если ошибка в закрытой ведомости?")
                {
                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "Необходимо сообщить об ошибке в деканат и преподавателю. В этом случае, с разрешения декана факультета, администратор ИС «Электронные ведомости» может открыть преподавателю ведомость для исправления ошибки.",
                       cancellationToken: cts);
                }
                if (msg.Text == "В списке группы неверная информация")
                {
                    await botClient.SendTextMessageAsync(
                       msg.Chat.Id,
                       "Необходимо, сообщить об этом в деканат данного факультета, для того чтобы данные были исправлены.",
                       cancellationToken: cts);
                }
                #endregion

                #region ПРЕПОДАВАТЕЛЬ
                if (msg.Text == "Преподаватель👨‍🏫")
                {

                }
                #endregion

                if (msg.Text == "Поиск🔎") // TODO
                {
                    if (sql.State == ConnectionState.Closed)
                    {
                        await botClient.SendTextMessageAsync(
                            msg.Chat.Id,
                            "Введите ключевое слово:",
                            replyMarkup: keyboards.CancelButtons(),
                            cancellationToken: cts);

                        sql.Open();
                        SqlCommand command = new SqlCommand($"Update bspu Set count = '3' where chat_id = '{msg.Chat.Id}'", sql); // Устанавливаем второй колонке значение 1
                        await command.ExecuteNonQueryAsync();
                        sql.Close();
                    }
                }

                if (msg.Text == "Квиз💬")
                {
                    var quiz = new InlineKeyboardMarkup(new[]
                    {
                            new[]
                            {
                                InlineKeyboardButton.WithWebApp("Квиз", new WebAppInfo() {Url = "https://aloelose.github.io/webqwiz/"})
                            },
                    });
                    await botClient.SendTextMessageAsync(msg.Chat.Id, "Этнографический диктант", replyMarkup: quiz);
                    return;
                }

                if (msg.Text == "Отмена")
                {
                    await botClient.SendTextMessageAsync(
                            msg.Chat.Id,
                            "Отменено",
                            replyMarkup: keyboards.StartButtons(),
                            cancellationToken: cts);

                    sql.Open();
                    SqlCommand command2 = new SqlCommand($"Update bspu Set count = '0' where chat_id = '{msg.Chat.Id}'", sql);
                    command2.ExecuteNonQuery();
                    sql.Close();
                }

                #region Назад

                if (msg.Text == "Назад⬅️")
                {
                    await botClient.SendTextMessageAsync(
                        msg.Chat.Id,
                        "Назад",
                        replyMarkup: keyboards.AbiturButtons(),
                        cancellationToken: cts);
                }

                if (msg.Text == "Назад◀️")
                {
                    await botClient.SendTextMessageAsync(
                        msg.Chat.Id,
                        "Назад",
                        replyMarkup: keyboards.StudentButtons(),
                        cancellationToken: cts);
                }

                if (msg.Text == "Назад⏪")
                {
                    await botClient.SendTextMessageAsync(
                        msg.Chat.Id,
                        "Назад",
                        replyMarkup: keyboards.OutUniversityButtons(),
                        cancellationToken: cts);
                }

                if (msg.Text == "Назад↪️")
                {
                    await botClient.SendTextMessageAsync(
                        msg.Chat.Id,
                        "Назад",
                        replyMarkup: keyboards.UsefulCabinetsButtons(),
                        cancellationToken: cts);
                }

                #endregion
            }

            if (msg.Type == MessageType.Document)
            {
                var answer = new InlineKeyboardMarkup(new[]
{
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("Рассмотрено", $"https://t.me/{msg.Chat.Username}")
                            }
                        });

                await SaveFiles(botClient, msg);
                Console.WriteLine($"{DateTime.Now} | {msg.Chat.FirstName ?? "Анон",-33} | -> Отправил файл");
                await botClient.SendTextMessageAsync(
                    msg.Chat.Id,
                    "Ваш файл сохранен!",
                    replyToMessageId: msg.MessageId);


                await using Stream stream = System.IO.File.OpenRead(@$"../netcoreapp3.1/{msg.Chat.Id}/Agreement.pdf");
                await botClient.SendDocumentAsync(
                    518785094,
                    document: new InputOnlineFile(content: stream, fileName: "Agreement.pdf"),
                    caption: "Согласие от пользователя");

                await using Stream stream2 = System.IO.File.OpenRead(@$"../netcoreapp3.1/{msg.Chat.Id}/Contract.pdf");
                await botClient.SendDocumentAsync(
                    518785094,
                    document: new InputOnlineFile(content: stream2, fileName: "Contract.pdf"),
                    caption: "Договор от пользователя");

                await botClient.SendTextMessageAsync(
                    518785094,
                    "Для уведомления о рассмотрении нажмите на кнопку",
                    replyMarkup: answer,
                    cancellationToken: cts);
            }
        }

        private static Task Error(ITelegramBotClient botClient, Exception ex, CancellationToken cts)
        {
            var error = ex switch
            {
                ApiRequestException requestException => $"Telegram API Error:\n[{requestException.ErrorCode}]\n{requestException.Message}",
                _ => ex.ToString()
            };

            Console.WriteLine(error);
            return Task.CompletedTask;
        }

        static async Task SaveFiles(ITelegramBotClient botClient, Message msg)
        {
            string fileId = msg.Document.FileId;
            var file = await botClient.GetFileAsync(fileId);
            var filePath = file.FilePath;
            string fileName = msg.Document.FileName;

            try
            {
                string destinationFilePath = $"../{msg.Chat.Id}/{fileName}";
                await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                await botClient.DownloadFileAsync(
                    filePath: filePath,
                    destination: fileStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex);
            }
        }

        static async Task SendFiles(ITelegramBotClient botClient, Message msg)
        {
            var info = new DirectoryInfo($"../{msg.Chat.Id}");
            try
            {
                if (Directory.EnumerateFiles(info.FullName, "*.*", SearchOption.AllDirectories).Any())
                {
                    FileInfo[] fileInfoGroup = info.GetFiles();
                    foreach (var fileInfo in fileInfoGroup)
                    {
                        var fileStream = new FileStream(fileInfo.FullName, FileMode.Open);
                        var file = new InputOnlineFile(fileStream, fileInfo.Name);
                        await botClient.SendDocumentAsync(
                            msg.Chat.Id,
                            file);
                    }
                }
                else
                {
                    await botClient.SendTextMessageAsync(msg.Chat.Id, "Вы еще не отправляли файлы");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:", ex);
            }
        }
    }
}

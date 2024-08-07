using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace AkmullaBSPU_bot.Keyboards
{
    public static class keyboards
    {
        public static IReplyMarkup StartButtons()
        {
            return new ReplyKeyboardMarkup("Main menu")
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                   new List<KeyboardButton> { new KeyboardButton("Абитуриент🙍‍♂️"), new KeyboardButton("Студент👨‍🎓") },
                   new List<KeyboardButton> {  new KeyboardButton("Поиск🔎"), new KeyboardButton("Квиз💬") }
                },
                ResizeKeyboard = true
            };
        }

        public static IReplyMarkup AbiturButtons()
        {
            return new ReplyKeyboardMarkup("Abitur")
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                   new List<KeyboardButton> { new KeyboardButton("Хочу поступить"), new KeyboardButton("Об университете") },
                   new List<KeyboardButton> { new KeyboardButton("Мероприятия и курсы для абитуриентов") },
                   new List<KeyboardButton> { new KeyboardButton("Свяжитесь с нами"), new KeyboardButton("FAQ❔") },
                },
                ResizeKeyboard = true
            };
        }

        public static IReplyMarkup EventsButtons()
        {
            return new ReplyKeyboardMarkup("Events")
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                   new List<KeyboardButton> { new KeyboardButton("Курсы"), new KeyboardButton("Олимпиада") },
                   new List<KeyboardButton> { new KeyboardButton("Ближайшие мероприятия для студентов") },
                   new List<KeyboardButton> { new KeyboardButton("Назад⬅️") },
                },
                ResizeKeyboard = true
            };
        }

        public static IReplyMarkup StudentButtons()
        {
            return new ReplyKeyboardMarkup("Student")
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                   new List<KeyboardButton> { new KeyboardButton("Расписание занятий") },
                   new List<KeyboardButton> { new KeyboardButton("Найти корпус"), new KeyboardButton("Полезные кабинеты") },
                   new List<KeyboardButton> { new KeyboardButton("Внеучебная деятельность"), new KeyboardButton("FAQ❓") },
                },
                ResizeKeyboard = true
            };
        }

        public static IReplyMarkup ActButtons()
        {
            return new ReplyKeyboardMarkup("Act")
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                   new List<KeyboardButton> { new KeyboardButton("Создать договор"), new KeyboardButton("Назад⬅️") }
                },
                ResizeKeyboard = true
            };
        }

        public static IReplyMarkup AbiturFAQButtons()
        {
            return new ReplyKeyboardMarkup("FAQ")
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                   new List<KeyboardButton> { new KeyboardButton("До какого числа можно выбрать предметы для сдачи?") },
                   new List<KeyboardButton> { new KeyboardButton("Можно ли поступить в вуз без ЕГЭ?") },
                   new List<KeyboardButton> { new KeyboardButton("Я заканчивал школу давно, без ЕГЭ, что делать?") },
                   new List<KeyboardButton> { new KeyboardButton("Сколько действуют результаты ЕГЭ?") },
                   new List<KeyboardButton> { new KeyboardButton("Можно подать документы без результатов ЕГЭ?") },
                   new List<KeyboardButton> { new KeyboardButton("Я сдавал ЕГЭ несколько раз,что тогда?") },
                   new List<KeyboardButton> { new KeyboardButton("ЕГЭ по математике является обязательным?") },
                   new List<KeyboardButton> { new KeyboardButton("Можно ли поступить с базовой математикой?") },
                   new List<KeyboardButton> { new KeyboardButton("Какой проходной балл в университет?") },
                   new List<KeyboardButton> { new KeyboardButton("Что нового в приемной компании?") },
                   new List<KeyboardButton> { new KeyboardButton("Как увеличить шансы на поступление?") },
                   new List<KeyboardButton> { new KeyboardButton("Назад⬅️") },

                },
                ResizeKeyboard = true
            };
        }

        public static IReplyMarkup StudentFAQButtons()
        {
            return new ReplyKeyboardMarkup("faq")
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                   new List<KeyboardButton> { new KeyboardButton("Как вычисляется оценка и рейтинг?") },
                   new List<KeyboardButton> { new KeyboardButton("Стипендия при пересдаче по «бегунку»?") },
                   new List<KeyboardButton> { new KeyboardButton("Как вычислить последний день сессии?") },
                   new List<KeyboardButton> { new KeyboardButton("Что делать, если в ведомости ошибка?") },
                   new List<KeyboardButton> { new KeyboardButton("А если ошибка в закрытой ведомости?") },
                   new List<KeyboardButton> { new KeyboardButton("В списке группы неверная информация") },
                   new List<KeyboardButton> { new KeyboardButton("Назад◀️") },
                },
                ResizeKeyboard = true
            };
        }

        public static IReplyMarkup DocsButtons()
        {
            return new ReplyKeyboardMarkup("Документы для поступления")
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                   new List<KeyboardButton> { new KeyboardButton("Личная") },
                   new List<KeyboardButton> { new KeyboardButton("В электронном виде") },
                   new List<KeyboardButton> { new KeyboardButton("Назад") },
                },
                ResizeKeyboard = true
            };
        }

        public static IReplyMarkup OutUniversityButtons()
        {
            return new ReplyKeyboardMarkup("Внеучебная деятельность")
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                   new List<KeyboardButton> { new KeyboardButton("Спортивный клуб"), new KeyboardButton("Волонтерский центр") },
                   new List<KeyboardButton> { new KeyboardButton("Студенческие отряды") },
                   new List<KeyboardButton> { new KeyboardButton("Театр"), new KeyboardButton("Хореография") },
                   new List<KeyboardButton> { new KeyboardButton("Школа ведущих"), new KeyboardButton("Лига КВН") },
                   new List<KeyboardButton> { new KeyboardButton("Медиацентр") },
                   new List<KeyboardButton> { new KeyboardButton("Студенческое научное общество") },
                   new List<KeyboardButton> { new KeyboardButton("Назад◀️") },
                },
                ResizeKeyboard = true
            };
        }

        public static IReplyMarkup StudentBrigadeButtons()
        {
            return new ReplyKeyboardMarkup("Студенческие отряды")
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                   new List<KeyboardButton> { new KeyboardButton("Отряд проводников"), new KeyboardButton("Сервисный отряд") },
                   new List<KeyboardButton> { new KeyboardButton("Педагогический отряд"), new KeyboardButton("Назад⏪") },
                },
                ResizeKeyboard = true
            };
        }

        public static IReplyMarkup DanceButtons()
        {
            return new ReplyKeyboardMarkup("Хореография")
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                   new List<KeyboardButton> { new KeyboardButton("Группа барабанщиц и мажореток") },
                   new List<KeyboardButton> { new KeyboardButton("Современный и эстрадный танец") },
                   new List<KeyboardButton> { new KeyboardButton("Народный танец") },
                   new List<KeyboardButton> { new KeyboardButton("Спортивно-аэробическое шоу"),new KeyboardButton("Назад⏪") },
                },
                ResizeKeyboard = true
            };
        }

        public static IReplyMarkup TheatreButtons()
        {
            return new ReplyKeyboardMarkup("Театр")
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                   new List<KeyboardButton> { new KeyboardButton("Театральная студия") },
                   new List<KeyboardButton> { new KeyboardButton("Коллектив народного творчества"), new KeyboardButton("Назад⏪") },
                },
                ResizeKeyboard = true
            };
        }

        public static IReplyMarkup UsefulCabinetsButtons()
        {
            return new ReplyKeyboardMarkup("Полезные кабинеты")
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                   new List<KeyboardButton> { new KeyboardButton("Кафедры"), new KeyboardButton("Деканаты") },
                   new List<KeyboardButton> { new KeyboardButton("Технопарк👨‍🔬"), new KeyboardButton("Центр карьеры") },
                   new List<KeyboardButton> { new KeyboardButton("Назад◀️") },
                },
                ResizeKeyboard = true
            };
        }

        public static IReplyMarkup DepartmentButtons()
        {
            return new ReplyKeyboardMarkup("Кафедры")
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                   new List<KeyboardButton> { new KeyboardButton("ИИПСГО"), new KeyboardButton("ИП"), new KeyboardButton("ИФК") },
                   new List<KeyboardButton> { new KeyboardButton("ЕГФ"), new KeyboardButton("ФБФ"), new KeyboardButton("ФП") },
                   new List<KeyboardButton> { new KeyboardButton("ХГФ"), new KeyboardButton("ИФМЦН") },
                   new List<KeyboardButton> { new KeyboardButton("Назад↪️") },
                },
                ResizeKeyboard = true
            };
        }
        public static IReplyMarkup СorpusButtuns()
        {
            return new ReplyKeyboardMarkup("Корпуса")
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                   new List<KeyboardButton> { new KeyboardButton("1"), new KeyboardButton("2"), new KeyboardButton("5"), new KeyboardButton("10") },
                   new List<KeyboardButton> { new KeyboardButton("Назад◀️") },
                },
                ResizeKeyboard = true
            };
        }

        public static IReplyMarkup FeedbackButtons()
        {
            return new ReplyKeyboardMarkup("Связь")
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                   new List<KeyboardButton> { new KeyboardButton("Связаться в Телеграм") }
                },
                ResizeKeyboard = true
            };
        }

        public static IReplyMarkup CancelButtons()
        {
            return new ReplyKeyboardMarkup("Cancel")
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                   new List<KeyboardButton> { new KeyboardButton("Отмена") },
                },
                ResizeKeyboard = true
            };
        }
    }
}

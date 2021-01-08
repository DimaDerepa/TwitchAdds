using Microsoft.Win32.TaskScheduler;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace TwitchAdds
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        const int SW_HIDE = 0;
        static ChromeOptions Options()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument(@"--user-data-dir=C:\Users\dimad\AppData\Local\Google\Chrome Dev\User Data\Default\");
            chromeOptions.AddArgument("--profile-directory=Profile 1");
            chromeOptions.AddArgument("--headless");
            chromeOptions.AddArgument("--log-level=3");
            chromeOptions.AddArgument("window-size=1400x900");
            return chromeOptions;
        }
        private static IWebDriver driver = new ChromeDriver(@"C:\Users\dimad\source\repos\twitch\TwitchAdds", Options(), TimeSpan.FromSeconds(180));
        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
            Console.Title = "TwitchAdds";
            Logger("~~~~~~/////////////////////////////~~~~~~~");
            Logger("Программа запущена " + DateTime.Now);
            Logger("~~~~~~~~~~~~~~StreamBoster~~~~~~~~~~~~~~~~");
            List<string> timers = new List<string>();
            driver.Url = "https://streambooster.ru";
            if (LoginSB() == true || OneMore() == true )
            {
                driver.Url = "https://streambooster.ru";
                timers.Add(GetPresent());
                timers.Add(GetGamesBonus());
                int z = GetTotalPoints();
                GetLevel();
                string vipTimer = CheckVip(z);
                if (vipTimer == "7:00:00:00") z = z - 100000;
                timers.Add(vipTimer);
                //string expTimer = CheckExpBonus(z);
                //if (expTimer == "7:00:00:00") z = z - 25000;
                Logger("Баллов на счету - " + z);
                //timers.Add(expTimer);
            }
            else
            {
                Logger("!!!!!!!КРИТИЧЕСКАЯ ОШИБКА ВХОДА StreamBooster!!!!!!!!");
            }
            Logger("~~~~~~~~~~~~~~TwitchMaster~~~~~~~~~~~~~~~~");
            if (LoginTM() == true)
            {
                timers.Add(ButtonInfo());
                VipRebuy();
                PointsInfo();
                LastWinner();
                PositionInfo();
            }
            else
            {
                Logger("!!!!!!КРИТИЧЕСКАЯ ОШИБКА ВХОДА TwitchMaster!!!!!!");
            }
            Logger("~~~~~~~~~~~~~~ChoiceOne~~~~~~~~~~~~~~~~");
            if (LoginCO() == true)
            {
                timers.Add(DailyBonus());
                timers.Add(ExtraBonus());
                FollowersCO();
                Logger("Баллов на счету - " + PointsCO());
            }
            else
            {
                Logger("!!!!!!КРИТИЧЕСКАЯ ОШИБКА ВХОДА ChoiceOne!!!!!!");
            }
            Logger("~~~~~~~~~~~~~~TurboTwitch~~~~~~~~~~~~~~~~");
            if (LoginTT() == true)
            {
                PointsTT();
                PointsTTDay();
                CrystalsTT();
            }
            else
            {
                Logger("!!!!!!КРИТИЧЕСКАЯ ОШИБКА ВХОДА TurboTwitch!!!!!!");
            }
            driver.Quit();
            Logger("~~~~~~~~~~~~~~Планировщик~~~~~~~~~~~~~~~~");
            Sheluder(timers);
            Logger("Выполнение программы завершено " + DateTime.Now);
            Logger("~~~~~~~/////////////////////////~~~~~~~~~");
            GC.Collect();
            return;
        }
        public static void Sheluder(List<string> timers)
        {
            try
            {
                string[] timersSplited = timers.Min().Split(":");
                int seconds = (Int32.Parse(timersSplited[0]) * 3600) + (Int32.Parse(timersSplited[1]) * 60) + (Int32.Parse(timersSplited[2]) + 90);
                DateTime runTask = DateTime.Now.AddSeconds(seconds);
                using (TaskService ts = new TaskService())
                {
                    TaskDefinition td = ts.NewTask();
                    td.RegistrationInfo.Description = "TwitchAdds";
                    td.Triggers.Add(new TimeTrigger() { StartBoundary = Convert.ToDateTime(runTask) });
                    td.Actions.Add(new ExecAction(@"C:\Users\dimad\source\repos\twitch\TwitchAdds\bin\Release\netcoreapp3.1\TwitchAdds.exe", null, null));
                    ts.RootFolder.RegisterTaskDefinition("TwitchAdds", td);
                }
                Logger("Следующий запуск программы запланирован на - " + runTask);
            }
            catch
            {
                Logger("!!!!!!!!Ошибка планировщика!!!!!!!!!!");
            }
        }
        public static void PositionInfo()
        {
            try
            {
                driver.Url = "https://twitchmaster.ru/";
                WaitForPageLoad(driver);
                IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;
                jse.ExecuteScript("window.scrollBy(0, 1000)", "");
                new System.Threading.ManualResetEvent(false).WaitOne(5000);
                for (int i = 1; i <= 10; i++)
                {
                    if (driver.FindElement(By.CssSelector("#chartdiv2 > div > div.amcharts-chart-div > svg > g:nth-child(15) > g:nth-child(1) > text:nth-child(" + i + ") > tspan")).Text == "GRO08Y")
                    {
                        Logger("Позиция в рейтинге зрителей - " + (-i + 11));
                        return;
                    }
                    continue;
                }
                Logger("Отсутсвует профиль в рейтинге зрителей");
            }
            catch
            {
                Logger("!!!!!!Ошибка получения позиции в рейтинге зрителей!!!!!!");
            }
        }
        public static void VipRebuy()
        {
            try
            {
                string vip = driver.FindElement(By.Id("vip-card-expiration-timer")).Text;
                Logger("Времени до окончания Vip-клуба - " + vip);
            }
            catch
            {
                Logger("!!!!!!Ошибка получения времени окончания VIP!!!!!!");
            }
        }
        public static void CrystalsTT()
        {
            try
            {
                driver.FindElement(By.XPath("/html/body/div[1]/table/tbody/tr/td[4]/a")).Click();
                WaitForPageLoad(driver);
                string url = driver.Url;
                driver.Url = url;
                WaitForPageLoad(driver);
                string points = driver.FindElement(By.Id("available_premium_credits")).Text;
                Logger("Энергокристаллов на счету - " + points);
            }
            catch
            {
                Logger("!!!!!!Ошибка получения кристалов!!!!!!");
            }
        }
        public static void PointsTTDay()
        {
            try
            {
                string points = driver.FindElement(By.XPath("/html/body/div[2]/div[2]/table/tbody/tr/td[3]/div[1]/div[2]/p[1]/span")).Text;
                Logger("Заработано за день - " + points + " баллов");
            }
            catch
            {
                Logger("!!!!!!Ошибка получения баллов за день!!!!!!");
            }
        }
        public static void FollowersCO()
        {
            try
            {
                string followers = driver.FindElement(By.XPath("//*[@id='block-twitch-twitch-status']/div/div[3]/a")).Text;
                Logger("Подписчиков с сервиса - " + followers);
            }
            catch
            {
                Logger("!!!!!!Ошибка получения количества подписчиков!!!!!!");
            }
        }
        public static string ButtonInfo()
        {
            try
            {
                string buttonTimer = driver.FindElement(By.XPath("/html/body/div[1]/div/div[3]/div[2]/div/div[2]")).Text;
                buttonTimer = buttonTimer.Substring(buttonTimer.Length - 8, 8);
                DateTime butT = DateTime.Parse(buttonTimer);
                butT = butT.AddMinutes(40);
                string bts = butT.ToString();
                bts = bts.Substring(11);
                Logger("До возможности нажать кнопку - " + bts);
                return bts;
            }
            catch
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[3]/div[2]/div/form/div[2]")).Click();
                    Logger("Кнопка успешно нажата. Следующее нажатие через 24:30:00!!!");
                    return "24:30:00";
                }
                catch
                {
                    Logger("!!!Ошибка получения данных кнопки!!!");
                    return null;
                }
            }
        }
        public static string DailyBonus()
        {
            try
            {
                driver.Url = "https://choiceone.ru/twitch-bonus/day-bonus";
                WaitForPageLoad(driver);
                if (driver.FindElement(By.ClassName("left_time")).Text == "Осталось до следующего бонуса - 0 сек")
                {
                    try
                    {
                        int start = PointsCO();
                        driver.Url = "https://choiceone.ru/twitch-bonus/day-bonus";
                        WaitForPageLoad(driver);
                        driver.FindElement(By.Id("edit-submit")).Click();
                        int end = PointsCO();
                        int getted = end - start;
                        Logger("Дневной бонус - " + getted + " баллов  успешно получен. До следующего бонуса осталось 24 часа!!!");
                        return "24:00:00";
                    }
                    catch
                    {
                        Logger("!!!!!!ChoiceOne Ошибка нажатия кнопки (дневной бонус)  " + DateTime.Now + "!!!!!!!!");
                        return "00:59:00";
                    }
                }
                else
                {
                    string phrase = driver.FindElement(By.ClassName("left_time")).Text;
                    phrase = phrase.Substring(32);
                    string hours;
                    string minutes = "59:00";
                    if (phrase.IndexOf("час") == -1)
                    {
                        hours = "00:";
                    }
                    else
                    {
                        hours = phrase.Substring(0, 2);
                        if (hours.IndexOf(" ") != -1)
                        {
                            hours = hours.Substring(0, 1);
                            hours = "0" + hours;
                        }
                        hours = hours + ":";
                    }
                    Logger("Дневной бонус был получен. До следующего нажатия - " + hours + minutes);
                    return hours + minutes;
                }
            }
            catch
            {
                Logger("!!!!!!ChoiceOne дневной бонус глобальная ошибка " + DateTime.Now + "!!!!!!!!");
                return "00:59:00";
            }
        }
        public static string ExtraBonus()
        {
            try
            {
                driver.Url = "https://choiceone.ru/twitch-bonus/extra-bonus";
                WaitForPageLoad(driver);
                if (driver.FindElement(By.ClassName("left_time")).Text == "Осталось до следующего бонуса - 0 сек")
                {
                    try
                    {
                        int start = PointsCO();
                        driver.Url = "https://choiceone.ru/twitch-bonus/extra-bonus";
                        WaitForPageLoad(driver);
                        driver.FindElement(By.CssSelector("#edit-submit")).Click();
                        int end = PointsCO();
                        int getted = end - start;
                        Logger("Экстра бонус - " + getted + " баллов успешно получен. До следующего бонуса осталось 24 часа!!!");
                        return "24:00:00";
                    }
                    catch
                    {
                        Logger("!!!!!!ChoiceOne Ошибка нажатия кнопки (экстра бонус)  " + DateTime.Now + "!!!!!!!!");
                        return "59:00";
                    }
                }
                else
                {
                    string phrase = driver.FindElement(By.ClassName("left_time")).Text;
                    phrase = phrase.Substring(32);
                    string hours;
                    string minutes = "59:00";
                    if (phrase.IndexOf("час") == -1)
                    {
                        hours = "";
                    }
                    else
                    {
                        hours = phrase.Substring(0, 2);
                        if (hours.IndexOf(" ") != -1)
                        {
                            hours = hours.Substring(0, 1);
                            hours = "0" + hours;
                        }
                        hours = hours + ":";
                    }
                    Logger("Экстра бонус был получен. До следующего нажатия - " + hours + minutes);
                    return hours + minutes;
                }
            }
            catch
            {
                Logger("!!!!!!ChoiceOne экстра бонус глобальная ошибка " + DateTime.Now + "!!!!!!!!");
                return "59:00";
            }
        }
        public static int PointsCO()
        {
            try
            {
                driver.Url = "https://choiceone.ru/kontakty";
                WaitForPageLoad(driver);
                string points = driver.FindElement(By.XPath("/html/body/div[2]/div/div[1]/div/div[3]/div/div/div[1]/div/div[1]/a")).Text;
                return Int32.Parse(points);
            }
            catch
            {
                Logger("!!!!!!!!!!!!!!!Ошибка получения баллов СhoiceOne!!!!!!!!!!");
                return 0;
            }
        }
        public static bool LoginCO()
        {
            try
            {
                driver.Url = "https://choiceone.ru/user";
                WaitForPageLoad(driver);
                try
                {
                    if (driver.FindElement(By.XPath("/html/body/div[2]/div/header/div/nav/ul/li[10]/a")).Text == "Регистрация")
                    {
                        driver.FindElement(By.Id("edit-name")).SendKeys("GRO08Y");
                        driver.FindElement(By.Id("edit-pass")).SendKeys("quakederepa123");
                        new System.Threading.ManualResetEvent(false).WaitOne(3000);
                        driver.FindElement(By.Id("edit-submit")).Click();
                        new System.Threading.ManualResetEvent(false).WaitOne(5000);
                        if (driver.FindElement(By.XPath("/html/body/div[2]/div/header/div/nav[1]/ul/li[1]/a")).Text == "Моя учётная запись")
                        {
                            return true;
                        }
                        else
                        {
                            Logger("~~~~~~~~~~~~~~~~~~~!!!!!Ошибка логина ChoiceOne №1 " + DateTime.Now + " !!!~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ ");
                            return false;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                catch
                {
                    return true;
                }
            }
            catch
            {
                Logger("~~~~~~~~~~~~~~~~~~~!!!!!Ошибка логина №2 ChoiceOne " + DateTime.Now + " !!!~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ ");
                return false;
            }
        }

        public static bool LoginTT()
        {
            try
            {
                driver.Url = "https://turbotwitch.ru/";
                driver.Manage().Cookies.AddCookie(new Cookie("_ym_d", "1604517965"));
                driver.Manage().Cookies.AddCookie(new Cookie("_ym_isad", "1"));
                driver.Manage().Cookies.AddCookie(new Cookie("_ym_uid", "1604517965652975587"));
                driver.Manage().Cookies.AddCookie(new Cookie("_ym_visorc", "w"));
                driver.Manage().Cookies.AddCookie(new Cookie("sid", "ViHASCrnyCtu3OT0mOXo38t0yOovor2Rbk5qVkQhSx3Gc0q7"));
                driver.Manage().Cookies.GetCookieNamed("_ym_d");
                driver.Manage().Cookies.GetCookieNamed("_ym_isad");
                driver.Manage().Cookies.GetCookieNamed("_ym_uid");
                driver.Manage().Cookies.GetCookieNamed("_ym_visorc");
                driver.Manage().Cookies.GetCookieNamed("sid");
                driver.Url = "https://turbotwitch.ru/";
                if (driver.FindElement(By.ClassName("cap_username")).Text == "gro08y") return true;
                else
                {
                    Logger("!!!Ошибка входа в TurboTwich №1!!!");
                    return false;
                }
            }
            catch
            {
                Logger("!!!Ошибка входа в TurboTwich №2!!!");
                return false;
            }
        }
        public static void PointsTT()
        {
            try
            {
                driver.FindElement(By.ClassName("cap_avatar")).Click();
                WaitForPageLoad(driver);
                string pointsTT = driver.FindElement(By.XPath("/html/body/div[2]/div[2]/table/tbody/tr/td[3]/div[1]/div[1]/p[1]/span")).Text;
                Logger("На счету - " + pointsTT + " баллов");
            }
            catch
            {
                Logger("!!!Ошибка получения баллов!!!");
            }
        }
        public static void PointsInfo()
        {
            try
            {
                string points = driver.FindElement(By.XPath("/html/body/div[1]/div/div[3]/div[1]/div[1]/div[1]")).Text;
                Logger("На счету - " + points + " баллов");
            }
            catch
            {
                Logger("!!!Ошибка получения баллов!!!");
            }
        }
        public static void LastWinner()
        {
            try
            {
                driver.Url = "https://twitchmaster.ru/static/contest-results";
                WaitForPageLoad(driver);
                string who = driver.FindElement(By.XPath("/html/body/div[1]/div/div[2]/div/table/tbody/tr[2]/td[3]/a[1]")).Text;
                string day = driver.FindElement(By.XPath("/html/body/div[1]/div/div[2]/div/table/tbody/tr[2]/td[4]")).Text;
                Logger("Победитель  " + who + " за " + day);
            }
            catch
            {
                Logger("!!!Ошибка получения победителя акции!!!");
            }
        }
        public static bool LoginTM()
        {
            try
            {
                try
                {
                    driver.Url = "https://twitchmaster.ru/static/profile?t=1";
                    new System.Threading.ManualResetEvent(false).WaitOne(30000);
                    WaitForPageLoad(driver);
                    if (driver.FindElement(By.ClassName("menu-popup")).Text == "GRO08Y") ;
                    return true;
                }
                catch
                {
                    driver.Url = "https://twitchmaster.ru/";
                    WaitForPageLoad(driver);
                    string login = "GRO08Y";
                    string password = "quakederepa";
                    driver.FindElement(By.XPath("/html/body/div[1]/div[1]/div/div[2]/span[2]/a[2]")).Click();
                    new System.Threading.ManualResetEvent(false).WaitOne(2000);
                    driver.FindElement(By.Name("login")).SendKeys(login);
                    driver.FindElement(By.Name("mypw")).SendKeys(password);
                    new System.Threading.ManualResetEvent(false).WaitOne(2000);
                    driver.FindElement(By.XPath("/html/body/div[4]/div[2]/form/div[5]/div")).Click();
                    driver.Url = "https://twitchmaster.ru/static/profile?t=1";
                    WaitForPageLoad(driver);
                    if (driver.FindElement(By.ClassName("menu-popup")).Text == "GRO08Y")
                    {
                        Logger("Вход успешен");
                        return true;
                    }
                    else
                    {
                        Logger("!!!Ошибка логина №1!!!");
                        return false;
                    }
                }
            }
            catch
            {
                Logger("!!!Ошибка логина №2!!!");
                return false;
            }
        }
        protected static void WaitForPageLoad(IWebDriver driver)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(120));
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
        }
        public static void GetLevel()
        {
            try
            {
                string level = driver.FindElement(By.XPath("//*[@id='current-level']/div/div[1]/div[1]/div")).Text;
                Logger("Уровень - " + level);
            }
            catch
            {
                Logger("Ошибка получения уровня");
            }
        }
        public static int GetTotalPoints()
        {
            driver.Url = "https://streambooster.ru";
            new System.Threading.ManualResetEvent(false).WaitOne(1000);
            try
            {
                string pointsGrime = driver.FindElement(By.Id("current-credits")).Text;
                pointsGrime = pointsGrime.Substring(0, pointsGrime.Length - 3);
                pointsGrime = pointsGrime.Replace(",", string.Empty);
                int points = Int32.Parse(pointsGrime);
                return points;
            }
            catch
            {
                Logger("!!!Ошибка получения баллов!!!");
                return -100000000;
            }
        }
        public static string GetGamesBonus()
        {
            driver.Url = "https://streambooster.ru";
            new System.Threading.ManualResetEvent(false).WaitOne(1000);
            try
            {
                int was = GetTotalPoints();
                driver.FindElement(By.XPath("//div[@id='free-spins']/div/div[2]/form/a")).Click();
                new System.Threading.ManualResetEvent(false).WaitOne(1500);
                for (int i = 1; i < 16; i++)
                {
                    driver.FindElement(By.Id("spin-the-slot")).Click();
                    new System.Threading.ManualResetEvent(false).WaitOne(5000);
                }
                int now = GetTotalPoints();
                int bonus = now - was;
                Logger("Бесплатные игры успешно получены. Заработано - " + bonus + " баллов. До следующих игр осталось 24 часа!!!");
                return "24:00:00";
            }
            catch
            {
                try
                {
                    string gameTimer = driver.FindElement(By.XPath("//div[@id='free-spins']/div/div[2]/span[2]")).Text;
                    Logger("До следующих игр осталось - " + gameTimer);
                    return gameTimer;
                }
                catch
                {
                    Logger("!!!Ошибка бесплатных игр или времени до повтора!!!");
                    return null;
                }
            }
        }
        public static string GetPresent()
        {
            driver.Url = "https://streambooster.ru";
            new System.Threading.ManualResetEvent(false).WaitOne(1000);
            try
            {
                int was = GetTotalPoints();
                driver.FindElement(By.XPath("//div[@id='random-gift']/div/div[2]/a")).Click();
                new System.Threading.ManualResetEvent(false).WaitOne(1500);
                driver.FindElement(By.XPath("/html/body/div[3]/div[2]/form/a")).Click();
                new System.Threading.ManualResetEvent(false).WaitOne(8000);
                int now = GetTotalPoints();
                int goldGiven = now - was;
                Logger("Подарок получен успешно - " + goldGiven + "баллов. До следующего подарка осталось 6 часов!!!");
                return "06:00:00";
            }
            catch
            {
                try
                {
                    string presentTimer = driver.FindElement(By.XPath("//div[@id='random-gift']/div/div[2]/span[2]")).Text;
                    Logger("Подарок не получен. До следующего подарка осталось " + presentTimer);
                    return presentTimer;
                }
                catch
                {
                    Logger("!!!Ошибка получения подарка или времени до повтора!!!");
                    return null;
                }
            }
        }
        public static string CheckVip(int points)
        {
            driver.Url = "https://streambooster.ru/dashboard/upgrades";
            new System.Threading.ManualResetEvent(false).WaitOne(1500);
            try
            {
                if (driver.FindElement(By.XPath("/html/body/div[1]/div/div[2]/div[2]/div/div/div[1]/div[2]/div/div[2]/form/a")).Text == "Активировать улучшение")
                {
                    Logger("Доступно VIP улучшение");
                    if (points >= 100000)
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[2]/div[2]/div/div/div[1]/div[2]/div/div[2]/form/a")).Click();
                        Logger("VIP улучшение активировано!!!");
                        return "7:00:00:00";
                    }
                    else
                    {
                        int need = 100000 - points;
                        Logger("Невозможно активировать VIP, недостаточно " + need + " баллов");
                        return null;
                    }
                }
                return null;
            }
            catch
            {
                try
                {
                    string timeVip = driver.FindElement(By.XPath("//span[@id='upgrade-expiration-4']")).Text;
                    Logger("До повторной активации VIP - " + timeVip);
                    return timeVip;

                }
                catch
                {
                    Logger("Невозможно получить оставшееся время VIP или недалось найти кнопку.");
                    return null;
                }
            }
        }
        public static string CheckExpBonus(int points)
        {
            driver.Url = "https://streambooster.ru/dashboard/upgrades";
            new System.Threading.ManualResetEvent(false).WaitOne(1500);
            try
            {
                if (driver.FindElement(By.XPath("/html/body/div[1]/div/div[2]/div[2]/div/div/div[1]/div[3]/form/a")).Text == "Активировать улучшение")
                {
                    Logger("Доступно EXP улучшение");
                    if (points >= 25000)
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[2]/div[2]/div/div/div[1]/div[3]/form/a")).Click();
                        Logger("EXP улучшение активировано!!!!!!!!!!");
                        return "7:00:00:00";
                    }
                    else
                    {
                        int need = 25000 - points;
                        Logger("Невозможно активировать EXP, недостаточно " + need + " баллов");
                        return null;
                    }
                }
                return null;
            }
            catch
            {
                try
                {
                    string timeExp = driver.FindElement(By.XPath("//span[@id='upgrade-expiration-3']")).Text;
                    Logger("До повторной активации EXP - " + timeExp);
                    return timeExp;

                }
                catch
                {
                    Logger("Невозможно получить оставшееся время EXP или недалось найти кнопку.");
                    return null;
                }
            }
        }
        public static void Logger(string message)
        {
            string pathLogs = @"C:\Users\dimad\source\repos\twitch\TwitchAddsLog" + DateTime.Now.Day + "d" + DateTime.Now.Month + ".txt";
            using (StreamWriter sw = new StreamWriter(pathLogs, true, System.Text.Encoding.Default))
            {
                sw.WriteLine(message);
            }
        }
        public static bool LoginSB()
        {
            try
            {
                string[] sesId = File.ReadAllLines(@"C:\Users\dimad\source\repos\twitch\StreamBooster\sesid.txt");
                driver.Url = "https://streambooster.ru/";
                driver.Manage().Cookies.AddCookie(new Cookie("PHPSESSID", sesId[0]));
                driver.Manage().Cookies.AddCookie(new Cookie("_ga", "GA1.2.403533739.1606816484"));
                driver.Manage().Cookies.AddCookie(new Cookie("_gat_gtag_UA_7160809_4", "1"));
                driver.Manage().Cookies.AddCookie(new Cookie("_gid", "GA1.2.1705362827.1608254732"));
                driver.Manage().Cookies.AddCookie(new Cookie("_ym_d", "1606816484"));
                driver.Manage().Cookies.AddCookie(new Cookie("_ym_uid", "160681648492267251"));
                driver.Manage().Cookies.GetCookieNamed("PHPSESSID");
                driver.Manage().Cookies.GetCookieNamed("_ga");
                driver.Manage().Cookies.GetCookieNamed("_gat_gtag_UA_7160809_4");
                driver.Manage().Cookies.GetCookieNamed("_gid");
                driver.Manage().Cookies.GetCookieNamed("_ym_d");
                driver.Manage().Cookies.GetCookieNamed("_ym_uid");
                driver.Url = "https://streambooster.ru/";
                new System.Threading.ManualResetEvent(false).WaitOne(1000);
                if (driver.FindElement(By.XPath("/html/body/div/div[1]/div[1]/div/div[2]/div[1]/div[1]/a")).Text == "GRO08Y")
                {
                    Logger("Аккаунт был подключен");
                    return true;
                }
            }
            catch { }
            try
            {
                driver.Url = "https://streambooster.ru/login";
                new System.Threading.ManualResetEvent(false).WaitOne(1500);
                driver.FindElement(By.XPath("//a[@id='oauth-trigger']")).Click();
                new System.Threading.ManualResetEvent(false).WaitOne(1000);
            }
            catch
            {
                Logger("!!!Ошибка входа №1!!!");
                return false;
            }
            try
            {
                new System.Threading.ManualResetEvent(false).WaitOne(1500);
                if (driver.FindElement(By.XPath("/html/body/div/div[1]/div[1]/div/div[2]/div[1]/div[1]/a")).Text == "GRO08Y")
                {
                    Logger("Вход успешен");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                Logger("!!!Ошибка входа №2!!!!");
                return false;
            }
        }
        public static bool LoginSBNewTab()
        {
            try
            {
                var chromeOptions = new ChromeOptions();
                chromeOptions.AddArgument(@"--user-data-dir=C:\Users\dimad\AppData\Local\Google\Chrome Dev\User Data\Default\");
                chromeOptions.AddArgument("--profile-directory=Profile 1");
                chromeOptions.AddArgument("--log-level=3");
                chromeOptions.AddArgument("window-size=1400x900");
                IWebDriver driver2 = new ChromeDriver(@"C:\Users\dimad\source\repos\twitch\TwitchAdds", chromeOptions, TimeSpan.FromSeconds(180));
                try
                {
                    driver2.Url = "https://streambooster.ru/login";
                    new System.Threading.ManualResetEvent(false).WaitOne(1500);
                    driver2.FindElement(By.XPath("//a[@id='oauth-trigger']")).Click();
                    new System.Threading.ManualResetEvent(false).WaitOne(1000);
                    if (driver2.FindElement(By.XPath("/html/body/div/div[1]/div[1]/div/div[2]/div[1]/div[1]/a")).Text == "GRO08Y")
                    {
                        string newSesId = driver2.Manage().Cookies.GetCookieNamed("PHPSESSID").Value;
                        using (StreamWriter sw = new StreamWriter(@"C:\Users\dimad\source\repos\twitch\StreamBooster\sesid.txt", false, System.Text.Encoding.Default))
                        {
                            sw.WriteLine(newSesId);
                        }
                        Logger("Аккаунт был подключен в новой вкладке");
                        driver2.Quit();
                        return true;
                    }
                    else
                    {
                        Logger("!!!!Ошибка подключения в новой вкладке №1!!!!");
                        driver2.Quit();
                        return false;
                    }
                }
                catch
                {
                    if (driver2.FindElement(By.XPath("/html/body/div/div[1]/div[1]/div/div[2]/div[1]/div[1]/a")).Text == "GRO08Y")
                    {
                        Logger("Аккаунт был подключен");
                        driver2.Quit();
                        return true;
                    }
                    else
                    {
                        Logger("!!!!Ошибка подключения в новой вкладке №2!!!!");
                        driver2.Quit();
                        return false;
                    }
                }
            }
            catch
            {
                Logger("!!!Глобальная ошибка входа в новой вкладке!!!");
                return false;
            }
        }
        public static bool OneMore()
        {
            if (LoginSBNewTab() == true | LoginSB() == true) return true;
            else return false;
        }
        
    }
}

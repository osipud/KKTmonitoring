using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HtmlAgilityPack;
using System.Net;
using System.Net.Mail;
using KKT_STATUS_WEB.Models;

namespace KKT_STATUS_WEB.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }
        public List<Device> devlist = new List<Device>();

        HtmlWeb web = new HtmlWeb();

        List<string> ips = new List<string>() {
        "109.73.13.157", "94.72.4.38"};
        //List<string> ips = new List<string>() {
        //    "192.168.10.201",
        //    "192.168.10.202",
        //    "192.168.10.203",
        //    "192.168.10.204",
        //    "192.168.10.205",
        //    "192.168.10.206",
        //    "192.168.10.207",
        //    "192.168.10.208",
        //    "192.168.10.209",
        //    "192.168.10.210",
        //    "192.168.10.211",
        //    "192.168.10.212",
        //    "192.168.10.213",
        //    "192.168.10.214",
        //    "192.168.10.215",
        //    "192.168.10.216",
        //    "192.168.10.217",
        //    "192.168.10.218",
        //};


        int index = 1;
        public void OnGet()
        {
            RefreshData();
        }
        public void RefreshData()
        {
            foreach (string ip in ips)
            {
                try
                {
                    var html = string.Concat("http://", ip, "/index.shtm");
                    var htmlDoc = web.Load(html);
                    web.PreRequest = delegate (HttpWebRequest webRequest)
                    {
                        webRequest.Timeout = 5000;
                        return true;
                    };
                    devlist.Add(new Device()
                    {
                        DeviceId = index,
                        SpentResource = GetSpentResource(htmlDoc),
                        LastDocumentDate = GetLastDocumentDate(htmlDoc),
                        LastDocumentNumber = GetLastDocumentNumber(htmlDoc),
                        ResidualResource = GetResidualResource(htmlDoc),
                        ResidualProcent = GetResidualProcent(htmlDoc),
                        SoftwareVersion = GetSoftwareVersion(htmlDoc),
                        FiscalStoreNumber = CheckCorrectFiscalNumber(GetSoftwareVersion(htmlDoc), htmlDoc),
                        LastDocumentDateTimeSpan = LastDocumentDateTimeSpanCalculate(GetLastDocumentDate(htmlDoc)),
                        FiscalStoreStatus = FiscalStoreExist(GetSoftwareVersion(htmlDoc), GetLastDocumentNumber(htmlDoc))
                    });
                    index++;
                }
                catch (Exception ex)
                {
                    devlist.Add(new Device()
                    {
                        DeviceId = index,
                        SpentResource = 0,
                        FiscalStoreNumber = "-",
                        LastDocumentDate = Convert.ToDateTime(Convert.ToString("01.01.2000")),
                        LastDocumentNumber = 0,
                        ResidualResource = 0,
                        ResidualProcent = 0,
                        SoftwareVersion = "-"

                    });
                    SendEmailAsync(index).GetAwaiter(); //Функция отправки письма в случае сработки условия try/catch
                    index++;
                }
            }
        }

        private int GetResidualResource(HtmlDocument htmlDoc)
        {
            int ResidualResource = 0;

            try
            {
                if (htmlDoc.DocumentNode.SelectSingleNode("//table//*[td='Номер последнего ФД']/td[1]/following::td[1]") != null)
                {
                    ResidualResource = 249000 - int.Parse(htmlDoc.DocumentNode.SelectSingleNode("//table//*[td='Номер последнего ФД']/td[1]/following::td[1]").InnerText);
                }
                else if (htmlDoc.DocumentNode.SelectSingleNode("//table//*[td='Номер последнего ФД']/td[1]/following::td[1]") == null)
                {
                    ResidualResource = 249000 - (int.Parse(htmlDoc.DocumentNode.SelectSingleNode("//tr/td//tr[9]/td[2]").InnerText));
                }
            }
            catch
            {
                ResidualResource = 0;
            }
            return ResidualResource;

        }
        private double GetResidualProcent(HtmlDocument htmlDoc)
        {
            double ResidualProcent = 0;
            try
            {
                if (htmlDoc.DocumentNode.SelectSingleNode("//table//*[td='Номер последнего ФД']/td[1]/following::td[1]") != null)
                {
                    ResidualProcent = (100 - ((249000 - (int.Parse(htmlDoc.DocumentNode.SelectSingleNode("//table//*[td='Номер последнего ФД']/td[1]/following::td[1]").InnerText))) / (249000 / 100)));
                }
                else if (htmlDoc.DocumentNode.SelectSingleNode("//table//*[td='Номер последнего ФД']/td[1]/following::td[1]") == null)
                {
                    ResidualProcent = (100 - ((249000 - (int.Parse(htmlDoc.DocumentNode.SelectSingleNode("//tr/td//tr[9]/td[2]").InnerText))) / (249000 / 100)));
                }

            }
            catch
            {
                ResidualProcent = 0;
            }

            return ResidualProcent;
        }
        private double LastDocumentDateTimeSpanCalculate(DateTime LastDocumentDate)
        {

            TimeSpan LastDocumentDateTimeSpan = DateTime.Now.Subtract(Convert.ToDateTime(LastDocumentDate));
            double LDT = LastDocumentDateTimeSpan.TotalMinutes;
            return LDT;
        }
        private DateTime GetLastDocumentDate(HtmlDocument htmlDoc)
        {
            DateTime LastDocumentDate = Convert.ToDateTime(Convert.ToString("01.01.2000"));

            try
            {
                if (htmlDoc.DocumentNode.SelectSingleNode("//table//*[td='Номер последнего ФД']/td[1]/following::td[1]") != null)
                {
                    LastDocumentDate = Convert.ToDateTime(Convert.ToString(htmlDoc.DocumentNode.SelectSingleNode("//table//*[td='Дата и время']/td[1]/following::td[1]").InnerText));
                }
                else if (htmlDoc.DocumentNode.SelectSingleNode("//table//*[td='Номер последнего ФД']/td[1]/following::td[1]") == null)
                {
                    LastDocumentDate = Convert.ToDateTime(Convert.ToString(htmlDoc.DocumentNode.SelectSingleNode("//tr[1]/td[2]//tr[7]/td[2]").InnerText));
                }
            }
            catch
            {
                LastDocumentDate = Convert.ToDateTime(Convert.ToString("01.01.2000"));
            }

            return LastDocumentDate;
        }
        private int GetSpentResource(HtmlDocument htmlDoc)
        {
            int SpentResource = 0;
            try
            {
                if (htmlDoc.DocumentNode.SelectSingleNode("//table//*[td='Номер последнего ФД']/td[1]/following::td[1]") != null)
                {
                    SpentResource = int.Parse(htmlDoc.DocumentNode.SelectSingleNode("//table//*[td='Номер последнего ФД']/td[1]/following::td[1]").InnerText);
                }
                else if (htmlDoc.DocumentNode.SelectSingleNode("//table//*[td='Номер последнего ФД']/td[1]/following::td[1]") == null)
                {
                    SpentResource = int.Parse(htmlDoc.DocumentNode.SelectSingleNode("//tr/td//tr[9]/td[2]").InnerText);
                }

            }
            catch
            {
                SpentResource = 0;
            }

            return SpentResource;
        }


        private string CheckCorrectFiscalNumber(string SoftwareVersion, HtmlDocument htmlDoc)
        {
            string FiscalStoreNumber = "FiscalStoreNumber";
            try
            {
                if (htmlDoc.DocumentNode.SelectSingleNode("//table//*[td='Номер ФН']/td[1]/following::td[1]") != null)
                {
                    FiscalStoreNumber = Convert.ToString(htmlDoc.DocumentNode.SelectSingleNode("//table//*[td='Номер ФН']/td[1]/following::td[1]").InnerText);
                }
                else if (htmlDoc.DocumentNode.SelectSingleNode("//table//*[td='Версия прошивки']/td[1]/following::td[1]") == null)
                {
                    FiscalStoreNumber = Convert.ToString(htmlDoc.DocumentNode.SelectSingleNode("//html/body/div/div[2]/table//table[2]//tr[8]/td[2]").InnerText);
                }
            }
            catch
            {
                FiscalStoreNumber = "Error";
            }
            return FiscalStoreNumber;
        }

        private bool FiscalStoreExist(string SoftwareVersion, int LastDocumentNumber)
        {
            bool ExistFlag = true;
            if (SoftwareVersion != "error" && LastDocumentNumber > 0)
            {
                ExistFlag = true;
            }
            else
            {
                ExistFlag = false;
            }
            return ExistFlag;
        }

        private string GetSoftwareVersion(HtmlDocument htmlDoc)
        {
            string SoftwareVersion = "SoftwareVersion";

            try
            {

                if (htmlDoc.DocumentNode.SelectSingleNode("//table//*[td='Версия сборки']/td[1]/following::td[1]") != null)
                {
                    SoftwareVersion = Convert.ToString(htmlDoc.DocumentNode.SelectSingleNode("//table//*[td='Версия сборки']/td[1]/following::td[1]").InnerText);
                }
                else if (htmlDoc.DocumentNode.SelectSingleNode("//table//*[td='Версия прошивки']/td[1]/following::td[1]") != null)
                {
                    SoftwareVersion = Convert.ToString(htmlDoc.DocumentNode.SelectSingleNode("//table//*[td='Версия прошивки']/td[1]/following::td[1]").InnerText);
                }
                else
                {
                    SoftwareVersion = Convert.ToString(htmlDoc.DocumentNode.SelectSingleNode("//td[1]/table[1]//tr[4]/td[2]").InnerText);
                }
            }
            catch
            {
                SoftwareVersion = "Error";
                //td[1]/table[1]//tr[4]/td[2]
            }
            return SoftwareVersion;
        }

        private int GetLastDocumentNumber(HtmlDocument htmlDoc)
        {
            int lastDocumentNumber = 0;
            try
            {
                if (htmlDoc.DocumentNode.SelectSingleNode("//table//*[td='Номер последнего ФД']/td[1]/following::td[1]") != null)
                {
                    lastDocumentNumber = int.Parse(htmlDoc.DocumentNode.SelectSingleNode("//table//*[td='Номер последнего ФД']/td[1]/following::td[1]").InnerText);
                }
                else if (htmlDoc.DocumentNode.SelectSingleNode("//table//*[td='Номер последнего ФД']/td[1]/following::td[1]") == null)
                {
                    lastDocumentNumber = int.Parse(htmlDoc.DocumentNode.SelectSingleNode("//tr/td//tr[9]/td[2]").InnerText);
                }
                else
                {
                    lastDocumentNumber = int.Parse(htmlDoc.DocumentNode.SelectSingleNode("/html/body/div/div[2]/table/tbody/tr/td[2]/table[2]/tbody/tr[9]/td[2]").InnerText);
                }
            }
            catch
            {
                lastDocumentNumber = 0;
            }

            return lastDocumentNumber;
        }

        private static async Task SendEmailAsync(int index)
        {
            //MailAddress from = new MailAddress("Почта для отправки", "KKT INFO");
            //MailAddress to1 = new MailAddress("Адресат 1");
            //MailAddress to2 = new MailAddress("Адресат 2");
            //MailMessage m1 = new MailMessage(from, to1);
            //MailMessage m2 = new MailMessage(from, to2);
            //m1.Subject = "МОНИТОРИНГ ККТ";
            //m1.Body = $"<b>ИГЪТИБАР!</b><br>Требуется внимание!<br>ККТ №{index} не отвечает на сетевые запросы!";
            //m1.IsBodyHtml = true;
            //m2.Subject = "МОНИТОРИНГ ККТ";
            //m2.Body = $"<b>ИГЪТИБАР!</b><br>Требуется внимание!<br>ККТ №{index} не отвечает на сетевые запросы!";
            //m2.IsBodyHtml = true;
            //SmtpClient smtp = new("SMTP сервер почты", порт SMTP сервера);
            //smtp.Credentials = new NetworkCredential("Почта для отправки", "Пароль для отправки");
            //smtp.EnableSsl = true;
            //await smtp.SendMailAsync(m1);
            //await smtp.SendMailAsync(m2);
            //Console.WriteLine($"{DateTime.Now}: Письмо отправлено");
        }
    }
}

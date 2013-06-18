using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Security.Cryptography;
using System.IO;
using RSA;

namespace services
{
    /// <summary>
    /// Summary description for rightcolor
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class rightcolor : System.Web.Services.WebService
    {
        DataClassesDataContext Data = new DataClassesDataContext();

        [WebMethod]
        public List<Record> GetRecords(RecordType Level)
        {
            List<Record> Records = new List<Record>();
            if (Level == RecordType.Forever)
            {
                Records = (from inc in Data.Records orderby inc.Point descending select inc).Take(10).ToList();
            }
            if (Level == RecordType.Month)
            {
                Records = (from inc in Data.Records where inc.AddDate > DateTime.Now.AddMonths(-1) orderby inc.Point descending select inc).Take(10).ToList();
            }
            if (Level == RecordType.Week)
            {
                Records = (from inc in Data.Records where inc.AddDate > DateTime.Now.AddDays(-7) orderby inc.Point descending select inc).Take(10).ToList();
            }
            if (Level == RecordType.Day)
            {
                Records = (from inc in Data.Records where inc.AddDate > DateTime.Now.AddDays(-1) orderby inc.Point descending select inc).Take(10).ToList();
            }
            return Records;
        }

        public enum RecordType
        {
            None,
            Forever,
            Month,
            Week,
            Day
        }

        [WebMethod]
        public List<long> GetHighestRecords()
        {
            long ForeverMax = (from inc in Data.Records orderby inc.Point descending select inc.Point).Skip(9).Take(1).SingleOrDefault();
            long MonthMax = (from inc in Data.Records where inc.AddDate > DateTime.Now.AddMonths(-1) orderby inc.Point descending select inc.Point).Skip(9).Take(1).SingleOrDefault();
            long WeekMax = (from inc in Data.Records where inc.AddDate > DateTime.Now.AddDays(-7) orderby inc.Point descending select inc.Point).Skip(9).Take(1).SingleOrDefault();
            long DayMax = (from inc in Data.Records where inc.AddDate > DateTime.Now.AddDays(-1) orderby inc.Point descending select inc.Point).Skip(9).Take(1).SingleOrDefault();

            List<long> AllRecords = new List<long>();
            AllRecords.Add(ForeverMax);
            AllRecords.Add(MonthMax);
            AllRecords.Add(WeekMax);
            AllRecords.Add(DayMax);
            return AllRecords;
        }

        [WebMethod]
        public RecordType AddRecord(string OName, string OPoint)
        {
            byte[] Name = Convert.FromBase64String(OName);
            byte[] Point = Convert.FromBase64String(OPoint);
            StreamReader sr = new StreamReader(Server.MapPath("App_Data\rightcolor_private.xml"));
            string ALL = sr.ReadToEnd();
            sr.Close();

            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(2048);
            RSA.FromXmlString(ALL);

            string RealName = System.Text.Encoding.UTF8.GetString(RSA.Decrypt(Name, false));
            long RealPoint = long.Parse(System.Text.Encoding.UTF8.GetString(RSA.Decrypt(Point, false)));

            RecordType AllowInsert = RecordType.None;
            long ForeverMax = (from inc in Data.Records orderby inc.Point descending select inc.Point).Skip(9).Take(1).SingleOrDefault();
            if (RealPoint > ForeverMax)
            {
                AllowInsert = RecordType.Forever;
                goto Insert;
            }
            long MonthMax = (from inc in Data.Records where inc.AddDate > DateTime.Now.AddMonths(-1) orderby inc.Point descending select inc.Point).Skip(9).Take(1).SingleOrDefault();
            if (RealPoint > MonthMax)
            {
                AllowInsert = RecordType.Month;
                goto Insert;
            }
            long WeekMax = (from inc in Data.Records where inc.AddDate > DateTime.Now.AddDays(-7) orderby inc.Point descending select inc.Point).Skip(9).Take(1).SingleOrDefault();
            if (RealPoint > WeekMax)
            {
                AllowInsert = RecordType.Week;
                goto Insert;
            }
            long DayMax = (from inc in Data.Records where inc.AddDate > DateTime.Now.AddDays(-1) orderby inc.Point descending select inc.Point).Skip(9).Take(1).SingleOrDefault();
            if (RealPoint > DayMax)
            {
                AllowInsert = RecordType.Day;
            }
        Insert:
            if (AllowInsert != RecordType.None)
            {
                Record NewRecord = new Record();
                NewRecord.Person = RealName;
                NewRecord.Point = RealPoint;
                NewRecord.AddDate = DateTime.Now;
                Data.Records.InsertOnSubmit(NewRecord);
                Data.SubmitChanges();
            }
            return AllowInsert;
        }
    }
}

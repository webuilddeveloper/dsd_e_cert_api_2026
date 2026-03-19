using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using OfficeOpenXml;

namespace master_api.Controllers
{
    public class Rude : Identity
    {
        public Rude()
        {
        }

        public string replace { get; set; }
    }

    [Route("[controller]")]
    public class RudeController : Controller
    {
        public RudeController() { }

        #region main

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> RudeRead([FromBody] Criteria value)
        {
            try
            {

                var col = new Database().MongoClient<Rude>("mrude");
                var filter = Builders<Rude>.Filter.Eq("isActive", true) & Builders<Rude>.Filter.Ne("status", "D");

                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter &= Builders<Rude>.Filter.Regex("title", value.keySearch);
                }

                if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<Rude>.Filter.Eq("code", value.code); }

                var docs = col.Find(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.title, c.docDate, c.createBy, c.updateBy, c.createDate, c.updateDate }).ToList();

                return new Response { status = "S", message = "success", jsonData = "", objectData = docs, totalData = col.Find(filter).Project(c => new { c.code, c.title }).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        #endregion

        // POST /initial
        [HttpGet("initialSwearWords")]
        public ActionResult<Response> InitialSwearWords()
        {
            var col = new Database().MongoClient("mrude");
            var doc = new BsonDocument();
            var model = new List<Rude>
            {
                new Rude { code = "1", title = "เหี้ย" },
                new Rude { code = "2", title = "สัด" },
                new Rude { code = "3", title = "หมา" },
                new Rude { code = "4", title = "ควาย" },
                new Rude { code = "5", title = "แรด" },
                new Rude { code = "6", title = "กระทิง" },
                new Rude { code = "7", title = "ฟาย" },
                new Rude { code = "8", title = "หมาบ้า" },
                new Rude { code = "9", title = "มัจจุราช" },
                new Rude { code = "10", title = "เควี่ย" },
                new Rude { code = "11", title = "เชี่ย" },
                new Rude { code = "12", title = "เงี่ยน" },
                new Rude { code = "13", title = "ส้นตีน" },
                new Rude { code = "14", title = "ชาติชั่ว" },
                new Rude { code = "15", title = "สถุล" },
                new Rude { code = "16", title = "ระยำ" },
                new Rude { code = "17", title = "สันดาน" },
                new Rude { code = "18", title = "ไอ้สัตว์นรก" },
                new Rude { code = "19", title = "พ่อมึงตาย" },
                new Rude { code = "20", title = "แม่มึงตาย" },
                new Rude { code = "21", title = "ชิงหมาเกิด" },
                new Rude { code = "22", title = "จัญไร" },
                new Rude { code = "23", title = "ใจหมา" },
                new Rude { code = "24", title = "ช้างเย็ด" },
                new Rude { code = "25", title = "อัปปรี" },
                new Rude { code = "26", title = "ชาติหมา" },
                new Rude { code = "27", title = "อีวาฬ" },
                new Rude { code = "28", title = "หน้าส้นตีน" },
                new Rude { code = "29", title = "ไอเหี้ย" },
                new Rude { code = "30", title = "ไอสัด" },
                new Rude { code = "31", title = "ไอ้หมา" },
                new Rude { code = "32", title = "ไอ้ควาย" },
                new Rude { code = "33", title = "นรกแดกกบาล" },
                new Rude { code = "34", title = "เศษนรก" },
                new Rude { code = "35", title = "กวนส้นตีน" },
                new Rude { code = "36", title = "ล่อกัน" },
                new Rude { code = "37", title = "เอากัน" },
                new Rude { code = "38", title = "แทงกัน" },
                new Rude { code = "39", title = "ยิงกัน" },
                new Rude { code = "40", title = "ปี้กัน" },
                new Rude { code = "41", title = "ตีกัน" },
                new Rude { code = "42", title = "หน้าตัวเมืย" },
                new Rude { code = "43", title = "หน้าหี" },
                new Rude { code = "44", title = "หน้าควย" },
                new Rude { code = "45", title = "ไอ้ขึ้หมา" },
                new Rude { code = "46", title = "แมงดา" },
                new Rude { code = "47", title = "กระดอ" },
                new Rude { code = "48", title = "อีหน้าหี" },
                new Rude { code = "49", title = "กระสัน" },
                new Rude { code = "50", title = "ไอเหี้ยหน้าหี" },
                new Rude { code = "51", title = "อีเหี้ย" },
                new Rude { code = "52", title = "อีสัด" },
                new Rude { code = "53", title = "อีหมา" },
                new Rude { code = "54", title = "อีควาย" },
                new Rude { code = "55", title = "อีปลาวาฬ" },
                new Rude { code = "56", title = "อีหน้าหมา" },
                new Rude { code = "57", title = "สาด" },
                new Rude { code = "58", title = "อีฟันหมาบ้า" },
                new Rude { code = "59", title = "อีหน้าควาย" },
                new Rude { code = "60", title = "กินขี้ปี้เยี่ยว" },
                new Rude { code = "61", title = "อีปลาเงือก" },
                new Rude { code = "62", title = "ไอ้ส้นตีน" },
                new Rude { code = "63", title = "เสือกพะยูน" },
                new Rude { code = "64", title = "พ่อมึง" },
                new Rude { code = "65", title = "แม่มึง" },
                new Rude { code = "66", title = "พ่อง" },
                new Rude { code = "67", title = "แม่ง" },
                new Rude { code = "68", title = "ควย" },
                new Rude { code = "69", title = "หี" },
                new Rude { code = "70", title = "เจี๊ยว" },
                new Rude { code = "71", title = "แดก" },
                new Rude { code = "72", title = "ขี้" },
                new Rude { code = "73", title = "เยี่ยว" },
                new Rude { code = "74", title = "ขึ้แตก" },
                new Rude { code = "75", title = "จรวย" },
                new Rude { code = "76", title = "ไอเข้" },
                new Rude { code = "77", title = "ไอ้สัส" },
                new Rude { code = "78", title = "ขายตัว" },
                new Rude { code = "79", title = "ลูกอีกะหรี่" },
                new Rude { code = "80", title = "ลูกอีดอกทอง" },
                new Rude { code = "81", title = "ลูกอีสาด" },
                new Rude { code = "82", title = "ยัดแม่" },
                new Rude { code = "83", title = "ฟักยู" },
                new Rude { code = "84", title = "อีอับปรี" },
                new Rude { code = "85", title = "อีกระหรี่" },
                new Rude { code = "86", title = "อีกะหรี" },
                new Rude { code = "87", title = "อีชาติชั้ว" },
                new Rude { code = "88", title = "อีช้างเย๊ด" },
                new Rude { code = "89", title = "อีห่า" },
                new Rude { code = "90", title = "อยากเอาหญิง" },
                new Rude { code = "91", title = "ทมิฬ" },
                new Rude { code = "92", title = "ลูกโสเภณี" },
                new Rude { code = "93", title = "หน้าโง่" },
                new Rude { code = "94", title = "สัส" },
                new Rude { code = "95", title = "บ้าเอ๋ย" },
                new Rude { code = "96", title = "ไอบ้า" },
                new Rude { code = "97", title = "ตอแหล" },
                new Rude { code = "98", title = "กะหรี่" },
                new Rude { code = "99", title = "สาระเลว" },
                new Rude { code = "100", title = "อีต่ำทราม" },
                new Rude { code = "101", title = "จังไร" },
                new Rude { code = "102", title = "บัดสบ" },
                new Rude { code = "103", title = "เหี้ยขี้" },
                new Rude { code = "104", title = "ไอสัส" },
                new Rude { code = "105", title = "ไอเชี่ย" },
                new Rude { code = "106", title = "ห่ากิน" },
                new Rude { code = "107", title = "ควยเอ้ย" },
                new Rude { code = "108", title = "อีสัส" },
                new Rude { code = "109", title = "อีหน้าด้าน" },
                new Rude { code = "110", title = "เป็นเหี้ยอะไรสัส" },
                new Rude { code = "111", title = "FUCK YOU" },
                new Rude { code = "112", title = "อีย้อย" },
                new Rude { code = "113", title = "ไอ้ซา" },
                new Rude { code = "114", title = "อีเรณู" },
                new Rude { code = "115", title = "อีพิไล" },
                new Rude { code = "116", title = "ใครซ้อมึงไอ้ซา" },
                new Rude { code = "117", title = "บันเด้าหักแหละพวกมึงอีย้อย" },
                new Rude { code = "118", title = "มึงรู้จักอีย้อยน้อยไปแล้ว" },
                new Rude { code = "119", title = "มึงวอนเรื่องกูใช้ไหมอีเรณู" },
                new Rude { code = "120", title = "คนอย่างมึงต้องเจอแน่" },
                new Rude { code = "121", title = "กู" },
                new Rude { code = "122", title = "มึง" },
                new Rude { code = "123", title = "ไอ้" },
                new Rude { code = "124", title = "อี" },
                new Rude { code = "125", title = "สัตว์" },
                new Rude { code = "126", title = "ตีน" },
                new Rude { code = "127", title = "จู๋" },
                new Rude { code = "128", title = "จิ๋ม" },
                new Rude { code = "129", title = "หำ" },
                new Rude { code = "130", title = "ตด" },
                new Rude { code = "131", title = "เยียว" },
                new Rude { code = "132", title = "ขี้แตก" },
                new Rude { code = "133", title = "ตดแตก" },
                new Rude { code = "134", title = "เยี่ยวแตก" },
                new Rude { code = "135", title = "เออ" },
                new Rude { code = "136", title = "โว่ย" },
                new Rude { code = "137", title = "โว้ย" },
                new Rude { code = "138", title = "ตุ๊กแก" },
                new Rude { code = "139", title = "ไอ้เข้" },
                new Rude { code = "140", title = "เย็ด" },
                new Rude { code = "141", title = "หมอย" },
                new Rude { code = "142", title = "แตด" },
                new Rude { code = "143", title = "ชิบหาย" },
                new Rude { code = "144", title = "ดอกทอง" },
                new Rude { code = "145", title = "เย็ดเป็ด" },
                new Rude { code = "146", title = "เสือก" },
                new Rude { code = "147", title = "แอ๊บใสๆ" },
                new Rude { code = "148", title = "ถ่อย" },
                new Rude { code = "149", title = "ครวย" },
                new Rude { code = "150", title = "อี๋" },
                new Rude { code = "151", title = "เย้ดดดดดดด~!" },
                new Rude { code = "152", title = "kuy" },
                new Rude { code = "153", title = "เอี้ย" },
                new Rude { code = "154", title = "ป้อเมิงจิ" },
                new Rude { code = "155", title = "หน้าหนังหีสังกะสีบาดแตด" },
                new Rude { code = "156", title = "กรู" },
                new Rude { code = "157", title = "เมิง" },
                new Rude { code = "158", title = "กุ" },
                new Rude { code = "159", title = "มืง" },
                new Rude { code = "160", title = "มรึง" },
                new Rude { code = "161", title = "มึงนั่นแหละ !" },
                new Rude { code = "162", title = "สรัด" },
                new Rude { code = "163", title = "ค-ว-า-ย" },
                new Rude { code = "164", title = "แม่ม" },
                new Rude { code = "165", title = "สัตหมา" },
                new Rude { code = "166", title = "เหมี้ย" },
                new Rude { code = "167", title = "อีดอก" },
                new Rude { code = "168", title = "กรี่" },
                new Rude { code = "169", title = "คุวย" },
                new Rude { code = "170", title = "omg" },
                new Rude { code = "171", title = "lol" },
                new Rude { code = "172", title = "wtf" },
                new Rude { code = "173", title = "อีสัตว์" },
                new Rude { code = "174", title = "อีตอแหล" },
                new Rude { code = "175", title = "ไอ้ระยำ" },
                new Rude { code = "176", title = "ไอ้เบื๊อก" },
                new Rude { code = "177", title = "ไอ้ตัวแสบ" },
                new Rude { code = "178", title = "เฮงซวย" },
                new Rude { code = "179", title = "ผู้หญิงต่ำๆ" },
                new Rude { code = "180", title = "พระหน้าผี" },
                new Rude { code = "181", title = "พระหน้าเปรต" },
                new Rude { code = "182", title = "มารศาสนา" },
                new Rude { code = "183", title = "ไอ้หน้าโง่" },
                new Rude { code = "184", title = "อีร้อย" },
                new Rude { code = "185", title = "อีดอกทอง" },
                new Rude { code = "186", title = "Shut the F..ck up" },
                new Rude { code = "187", title = "Suck my dick" },
                new Rude { code = "188", title = "Go f..ck yourself" },
                new Rude { code = "189", title = "Get the F..ck out" },
                new Rude { code = "190", title = "Damn!!" },
                new Rude { code = "191", title = "God damn it!!!!" },
                new Rude { code = "192", title = "Oh my F..cking God!!!" },
                new Rude { code = "193", title = "Oh shit!!" },
                new Rude { code = "194", title = "Oh F..ck" },
                new Rude { code = "195", title = "Holy shit, Holy F..ck, Holy hell" },
                new Rude { code = "196", title = "Hell Yeah" },
                new Rude { code = "197", title = "F..ck You" },
                new Rude { code = "198", title = "Damn you" },
                new Rude { code = "199", title = "Go to F..cking hell" },
                new Rude { code = "200", title = "Don’t kiss my ass" },
                new Rude { code = "201", title = "Bitch , Slut" },
                new Rude { code = "202", title = "Sons of bitch" },
                new Rude { code = "203", title = "am in deep shit" },
                new Rude { code = "204", title = "I F..cked up" },
                new Rude { code = "205", title = "Screw you" },
                new Rude { code = "206", title = "Screw up" },
                new Rude { code = "207", title = "Chicken" },
                new Rude { code = "208", title = "Noob, Loser" },
                new Rude { code = "209", title = "noob" },
                new Rude { code = "210", title = "Dick" },
                new Rude { code = "211", title = "Dick head" },
                new Rude { code = "212", title = "Jerk" },
                new Rude { code = "213", title = "Go jerk yourself" },
                new Rude { code = "214", title = "ass" },
                new Rude { code = "215", title = "ass hole" },
                new Rude { code = "216", title = "Pervert" },
                new Rude { code = "217", title = "Jackass" },
                new Rude { code = "218", title = "You twat, You are a twat" },
                new Rude { code = "219", title = "Low life, Low level of intellegence" },

            };

            try
            {
                model.ForEach(c =>
                {
                    var filter = Builders<BsonDocument>.Filter.Eq("code", c.code);
                    if (!col.Find(filter).Any())
                    {
                        doc = new BsonDocument
                    {
                        { "code", c.code },
                        { "title", c.title },
                        { "createBy", "system" },
                        { "createDate", DateTime.Now.toStringFromDate() },
                        { "createTime", DateTime.Now.toTimeStringFromDate() },
                        { "updateBy", "system" },
                        { "updateDate", DateTime.Now.toStringFromDate() },
                        { "updateTime", DateTime.Now.toTimeStringFromDate() },
                        { "docDate", DateTime.Now.Date.AddHours(7) },
                        { "docTime", DateTime.Now.toTimeStringFromDate() },
                        { "isActive", true }
                    };
                        col.InsertOne(doc);
                    }
                });

                return new Response { status = "S", message = "success" };
            }
            catch (Exception ex)
            {
                return new Response { status = "S", message = ex.Message };
            }

            //using (var package = new ExcelPackage(new FileInfo("Book1.xlsx")))
            //{
            //    var firstSheet = package.Workbook.Worksheets["Sheet1"];
            //    Console.WriteLine("Sheet 1 Data");
            //    Console.WriteLine($"Cell A2 Value   : {firstSheet.Cells["A2"].Text}");
            //    Console.WriteLine($"Cell A2 Color   : {firstSheet.Cells["A2"].Style.Font.Color.LookupColor()}");
            //    Console.WriteLine($"Cell B2 Formula : {firstSheet.Cells["B2"].Formula}");
            //    Console.WriteLine($"Cell B2 Value   : {firstSheet.Cells["B2"].Text}");
            //    Console.WriteLine($"Cell B2 Border  : {firstSheet.Cells["B2"].Style.Border.Top.Style}");
            //    Console.WriteLine("");

            //    var secondSheet = package.Workbook.Worksheets["Second Sheet"];
            //    Console.WriteLine($"Sheet 2 Data");
            //    Console.WriteLine($"Cell A2 Formula : {secondSheet.Cells["A2"].Formula}");
            //    Console.WriteLine($"Cell A2 Value   : {secondSheet.Cells["A2"].Text}");
            //}
        }

    }
}
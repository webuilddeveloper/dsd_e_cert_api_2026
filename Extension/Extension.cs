using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using cms_api.Models;
using Jose;
using master_api.Controllers;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace cms_api.Extension
{
    public static class Extension
    {   
        public static string toCode(this string param)
        {
            return $"{DateTime.Now.ToString("yyyyMMddHHmmss")}-{DateTime.Now.Millisecond.ToString()}-{new Random().Next(100, 999)}";
        }
        public static string getRandom(this string param)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var stringChars = new char[8];
            Random random = new Random();
            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return $"{new string(stringChars)}";
        }

        public static DateTime toDateFromString(this string param)
        {
            if (param.Length == 8) //20200131
            {
                return new DateTime(Int16.Parse(param.Substring(0, 4)), Int16.Parse(param.Substring(4, 2)), Int16.Parse(param.Substring(6, 2)));
            }
            else if (param.Length == 14) //20200131120000
            {
                return new DateTime(Int16.Parse(param.Substring(0, 4)), Int16.Parse(param.Substring(4, 2)), Int16.Parse(param.Substring(6, 2)), Int16.Parse(param.Substring(8, 2)), Int16.Parse(param.Substring(10, 2)), Int16.Parse(param.Substring(12, 2)));
            }

            return DateTime.Now;
        }

        public static BetweenDate toBetweenDate(this DateTime param)
        {
            return new BetweenDate { start = new DateTime(param.Year, param.Month, param.Day, 0, 0, 0), end = new DateTime(param.Year, param.Month, param.Day, 23, 59, 59) };
        }

        public static string toStringFromDate(this DateTime param)
        {
            var d = param.ToString("yyyyMMddHHmmss");
            return d;
        }

        public static string toTimeStringFromDate(this DateTime param)
        {
            return new TimeSpan(param.Hour, param.Minute, param.Second).ToString();
        }

        public static string toEncode(this string param)
        {
            var secretKey = new byte[] { 164, 60, 194, 0, 161, 189, 41, 38, 130, 89, 141, 164, 45, 170, 159, 209, 69, 137, 243, 216, 191, 131, 47, 250, 32, 107, 231, 117, 37, 158, 225, 234 };
            return JWT.Encode(param, secretKey, JwsAlgorithm.HS256);
        }

        public static string toDecode(this string param)
        {
            var secretKey = new byte[] { 164, 60, 194, 0, 161, 189, 41, 38, 130, 89, 141, 164, 45, 170, 159, 209, 69, 137, 243, 216, 191, 131, 47, 250, 32, 107, 231, 117, 37, 158, 225, 234 };
            return JWT.Decode(param, secretKey, JwsAlgorithm.HS256);
        }

        public static void logCreate(this object param, string title, string description)
        {
            var doc = new BsonDocument();
            var col = new Database().MongoClient("log_" + title);

            try
            {
                doc = new BsonDocument
                {
                    { "code", "".toCode() },
                    { "title", title },
                    { "description", description },
                    { "raw", param.ToJson() },
                    { "createBy", param.GetType().GetProperty("createBy").GetValue(param, null).ToString() },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", param.GetType().GetProperty("updateBy").GetValue(param, null).ToString() },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true }
                };
                col.InsertOne(doc);
            }
            catch (Exception ex)
            {
                doc = new BsonDocument
                {
                    { "code", "".toCode() },
                    { "title", title },
                    { "description", description },
                    { "raw", ex.Message },
                    { "createBy", param.GetType().GetProperty("createBy").GetValue(param, null).ToString() },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", param.GetType().GetProperty("updateBy").GetValue(param, null).ToString() },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true }
                };
                col.InsertOne(doc);
            }
        }

        public static void statisticsCreate(this Criteria value, string page)
        {
            try
            {
                value.databaseName = "khub_dee_prod_statistics";
                value.page = page;

                HttpClient client = new HttpClient();
                var json = JsonConvert.SerializeObject(value);
                HttpContent httpContent = new StringContent(json);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                //var response = client.PostAsync("http://opec.we-builds.com/statistic-api/statistics/create", httpContent);
                //var response = client.PostAsync("http://localhost:6100/statistics/create", httpContent);
                var response = client.PostAsync("http://122.155.223.63/td-statistics-api/statistics/create", httpContent);

            }
            catch
            {

            }
        }

        public static FilterDefinition<T> filterPermission<T>(this string param, string field)
        {
            var permissionFilter = Builders<T>.Filter.Ne("status", "D");

            var permission = param.Split(",");
            for (int i = 0; i < permission.Length; i++)
            {
                if (i == 0)
                    permissionFilter = Builders<T>.Filter.Eq(field, permission[i]);
                else
                    permissionFilter |= Builders<T>.Filter.Eq(field, permission[i]);
            }

            return permissionFilter;
        }

        public static FilterDefinition<T> filterOrganization<T>(this string param, string field)
        {
            var organizationFilter = Builders<T>.Filter.Ne("status", "D");

            var organization = param.Split(",");
            for (int i = 0; i < organization.Length; i++)
            {
                if (i == 0)
                    organizationFilter = Builders<T>.Filter.Regex(field, organization[i]);
                else
                    organizationFilter |= Builders<T>.Filter.Regex(field, organization[i]);
            }

            return organizationFilter;
        }

        public static FilterDefinition<T> filterOrganizationOld<T>(this Criteria param)
        {
            // (status != 'D' & ( (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '') | ( (lv0 = 'xxx') & (lv1 = 'xxx') & (lv2 = 'xxx') & (lv3 = 'xxx') ) ) )

            // (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '')
            var publicFilter = Builders<T>.Filter.Ne("status", "D");

            if (param.organization.Count == 0)
            {
                return publicFilter = Builders<T>.Filter.Ne("status", "D");
            }
            else
            {
                publicFilter = (Builders<T>.Filter.Eq("lv0", "x") & Builders<T>.Filter.Eq("lv1", "x") & Builders<T>.Filter.Eq("lv2", "x") & Builders<T>.Filter.Eq("lv3", "x") & Builders<T>.Filter.Eq("lv4", "x"));

                param.organization.ForEach(c =>
                {
                    if (c.status == "A")
                    {
                        // (lv0 = 'xxx' & lv1 = 'xxx' & lv2 = 'xxx' & lv3 = 'xxx')
                        // Use 'Regex' because where lv like in content

                        var organizationFilter = Builders<T>.Filter.Ne("status", "D");

                        if (!string.IsNullOrEmpty(c.lv5))
                        {
                            var organization = c.lv5.Split(",");
                            for (int i = 0; i < organization.Length; i++)
                            {
                                if (i == 0)
                                    organizationFilter = Builders<T>.Filter.Regex("lv5", organization[i]);
                                else
                                    organizationFilter |= Builders<T>.Filter.Regex("lv5", organization[i]);
                            }

                            publicFilter |= (organizationFilter);
                        }

                        if (!string.IsNullOrEmpty(c.lv4))
                        {
                            var organization = c.lv4.Split(",");
                            for (int i = 0; i < organization.Length; i++)
                            {
                                if (i == 0)
                                    organizationFilter = Builders<T>.Filter.Regex("lv4", organization[i]);
                                else
                                    organizationFilter |= Builders<T>.Filter.Regex("lv4", organization[i]);
                            }

                            publicFilter |= (organizationFilter);
                        }
                        else if (!string.IsNullOrEmpty(c.lv3))
                        {
                            var organization = c.lv3.Split(",");
                            for (int i = 0; i < organization.Length; i++)
                            {
                                if (i == 0)
                                    organizationFilter = Builders<T>.Filter.Regex("lv3", organization[i]);
                                else
                                    organizationFilter |= Builders<T>.Filter.Regex("lv3", organization[i]);
                            }

                            publicFilter |= (organizationFilter);
                        }
                        else if (!string.IsNullOrEmpty(c.lv2))
                        {
                            var organization = c.lv2.Split(",");
                            for (int i = 0; i < organization.Length; i++)
                            {
                                if (i == 0)
                                    organizationFilter = Builders<T>.Filter.Regex("lv2", organization[i]);
                                else
                                    organizationFilter |= Builders<T>.Filter.Regex("lv2", organization[i]);
                            }

                            publicFilter |= (organizationFilter);
                        }
                        else if (!string.IsNullOrEmpty(c.lv1))
                        {
                            var organization = c.lv1.Split(",");
                            for (int i = 0; i < organization.Length; i++)
                            {
                                if (i == 0)
                                    organizationFilter = Builders<T>.Filter.Regex("lv1", organization[i]);
                                else
                                    organizationFilter |= Builders<T>.Filter.Regex("lv1", organization[i]);
                            }

                            publicFilter |= (organizationFilter);
                        }
                        else if (!string.IsNullOrEmpty(c.lv0))
                        {
                            var organization = c.lv0.Split(",");
                            for (int i = 0; i < organization.Length; i++)
                            {
                                if (i == 0)
                                    organizationFilter = Builders<T>.Filter.Regex("lv0", organization[i]);
                                else
                                    organizationFilter |= Builders<T>.Filter.Regex("lv0", organization[i]);
                            }

                            publicFilter |= (organizationFilter);
                        }
                    }
                });
            }

            // (status != 'D' & ( (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '') | ( (lv0 = 'xxx') & (lv1 = 'xxx') & (lv2 = 'xxx') & (lv3 = 'xxx') ) ) )
            //return ( publicFilter | ( (lv0Filter) & (lv1Filter) & (lv2Filter) & (lv3Filter) & (lv4Filter) & (lv5Filter) ) );
            return (publicFilter);
        }

        public static FilterDefinition<T> filterOrganization<T>(this Criteria param)
        {
            // (status != 'D' & ( (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '') | ( (lv0 = 'xxx') & (lv1 = 'xxx') & (lv2 = 'xxx') & (lv3 = 'xxx') ) ) )

            // (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '')
            var publicFilter = (Builders<T>.Filter.Eq("lv0", "") & Builders<T>.Filter.Eq("lv1", "") & Builders<T>.Filter.Eq("lv2", "") & Builders<T>.Filter.Eq("lv3", "") & Builders<T>.Filter.Eq("lv4", "") & Builders<T>.Filter.Eq("lv5", ""));

            if (param.organization.Count == 0)
            {
                publicFilter = Builders<T>.Filter.Eq("status", "A");
                return publicFilter;
            }
            else
            {
                param.organization.ForEach(c =>
                {
                    if (c.status == "A")
                    {
                        if (!string.IsNullOrEmpty(c.lv5))
                        {
                            var organization = c.lv5.Split(",");
                            for (int i = 0; i < organization.Count(); i++)
                            {
                                publicFilter |= Builders<T>.Filter.Regex("lv5", organization[i]);
                            }
                        }
                        else if (!string.IsNullOrEmpty(c.lv4))
                        {
                            var organization = c.lv4.Split(",");
                            for (int i = 0; i < organization.Count(); i++)
                            {
                                //if (i == 0)
                                //    organizationFilter |= Builders<T>.Filter.Regex("lv4", organization[i]);
                                //else
                                publicFilter |= Builders<T>.Filter.Regex("lv4", organization[i]);
                            }

                            //publicFilter |= (organizationFilter);
                        }
                        else if (!string.IsNullOrEmpty(c.lv3))
                        {
                            var organization = c.lv3.Split(",");
                            for (int i = 0; i < organization.Count(); i++)
                            {
                                //if (i == 0)
                                //    organizationFilter |= Builders<T>.Filter.Regex("lv3", organization[i]);
                                //else
                                publicFilter |= Builders<T>.Filter.Regex("lv3", organization[i]);
                            }

                            //publicFilter |= (organizationFilter);
                        }
                        else if (!string.IsNullOrEmpty(c.lv2))
                        {
                            var organization = c.lv2.Split(",");
                            for (int i = 0; i < organization.Count(); i++)
                            {
                                //if (i == 0)
                                //    organizationFilter |= Builders<T>.Filter.Regex("lv2", organization[i]);
                                //else
                                publicFilter |= Builders<T>.Filter.Regex("lv2", organization[i]);
                            }

                            //publicFilter |= (organizationFilter);
                        }
                        else if (!string.IsNullOrEmpty(c.lv1))
                        {
                            var organization = c.lv1.Split(",");
                            for (int i = 0; i < organization.Count(); i++)
                            {
                                //if (i == 0)
                                //    organizationFilter |= Builders<T>.Filter.Regex("lv1", organization[i]);
                                //else
                                publicFilter |= Builders<T>.Filter.Regex("lv1", organization[i]);
                            }

                            //publicFilter |= (organizationFilter);
                        }
                        else if (!string.IsNullOrEmpty(c.lv0))
                        {
                            var organization = c.lv0.Split(",");
                            for (int i = 0; i < organization.Count(); i++)
                            {
                                //if (i == 0)
                                //    organizationFilter |= Builders<T>.Filter.Regex("lv0", organization[i]);
                                //else
                                publicFilter |= Builders<T>.Filter.Regex("lv0", organization[i]);
                            }

                            //publicFilter |= (organizationFilter);
                        }
                    }
                });
            }

            // (status != 'D' & ( (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '') | ( (lv0 = 'xxx') & (lv1 = 'xxx') & (lv2 = 'xxx') & (lv3 = 'xxx') ) ) )
            //return ( publicFilter | ( (lv0Filter) & (lv1Filter) & (lv2Filter) & (lv3Filter) & (lv4Filter) & (lv5Filter) ) );
            return (publicFilter);
        }

        public static FilterDefinition<T> filterOrganization<T>(this EventCalendar param)
        {
            // (status != 'D' & ( (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '') | ( (lv0 = 'xxx') & (lv1 = 'xxx') & (lv2 = 'xxx') & (lv3 = 'xxx') ) ) )

            // (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '')
            var publicFilter = Builders<T>.Filter.Ne("status", "D") & Builders<T>.Filter.Eq("isPublic", true);

            if (param.organization.Count == 0)
            {
                return publicFilter;
            }
            else
            {
                //publicFilter = (Builders<T>.Filter.Eq("lv0", "x") & Builders<T>.Filter.Eq("lv1", "x") & Builders<T>.Filter.Eq("lv2", "x") & Builders<T>.Filter.Eq("lv3", "x") & Builders<T>.Filter.Eq("lv4", "x"));

                param.organization.ForEach(c =>
                {
                    if (c.status == "A")
                    {
                        // (lv0 = 'xxx' & lv1 = 'xxx' & lv2 = 'xxx' & lv3 = 'xxx')
                        // Use 'Regex' because where lv like in content

                        var organizationFilter = Builders<T>.Filter.Eq("isPublic", true);

                        if (!string.IsNullOrEmpty(c.lv5))
                        {
                            var organization = c.lv5.Split(",");
                            for (int i = 0; i < organization.Count(); i++)
                            {
                                //if (i == 0)
                                //    organizationFilter |= Builders<T>.Filter.Regex("lv5", organization[i]);
                                //else
                                organizationFilter |= Builders<T>.Filter.Regex("lv5", organization[i]);
                            }

                            publicFilter |= (organizationFilter);
                        }
                        else if (!string.IsNullOrEmpty(c.lv4))
                        {
                            var organization = c.lv4.Split(",");
                            for (int i = 0; i < organization.Count(); i++)
                            {
                                //if (i == 0)
                                //    organizationFilter |= Builders<T>.Filter.Regex("lv4", organization[i]);
                                //else
                                organizationFilter |= Builders<T>.Filter.Regex("lv4", organization[i]);
                            }

                            publicFilter |= (organizationFilter);
                        }
                        else if (!string.IsNullOrEmpty(c.lv3))
                        {
                            var organization = c.lv3.Split(",");
                            for (int i = 0; i < organization.Count(); i++)
                            {
                                //if (i == 0)
                                //    organizationFilter |= Builders<T>.Filter.Regex("lv3", organization[i]);
                                //else
                                organizationFilter |= Builders<T>.Filter.Regex("lv3", organization[i]);
                            }

                            publicFilter |= (organizationFilter);
                        }
                        else if (!string.IsNullOrEmpty(c.lv2))
                        {
                            var organization = c.lv2.Split(",");
                            for (int i = 0; i < organization.Count(); i++)
                            {
                                //if (i == 0)
                                //    organizationFilter |= Builders<T>.Filter.Regex("lv2", organization[i]);
                                //else
                                organizationFilter |= Builders<T>.Filter.Regex("lv2", organization[i]);
                            }

                            publicFilter |= (organizationFilter);
                        }
                        else if (!string.IsNullOrEmpty(c.lv1))
                        {
                            var organization = c.lv1.Split(",");
                            for (int i = 0; i < organization.Count(); i++)
                            {
                                //if (i == 0)
                                //    organizationFilter |= Builders<T>.Filter.Regex("lv1", organization[i]);
                                //else
                                organizationFilter |= Builders<T>.Filter.Regex("lv1", organization[i]);
                            }

                            publicFilter |= (organizationFilter);
                        }
                        else if (!string.IsNullOrEmpty(c.lv0))
                        {
                            var organization = c.lv0.Split(",");
                            for (int i = 0; i < organization.Count(); i++)
                            {
                                //if (i == 0)
                                //    organizationFilter |= Builders<T>.Filter.Regex("lv0", organization[i]);
                                //else
                                organizationFilter |= Builders<T>.Filter.Regex("lv0", organization[i]);
                            }

                            publicFilter |= (organizationFilter);
                        }
                    }
                });
            }

            // (status != 'D' & ( (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '') | ( (lv0 = 'xxx') & (lv1 = 'xxx') & (lv2 = 'xxx') & (lv3 = 'xxx') ) ) )
            //return ( publicFilter | ( (lv0Filter) & (lv1Filter) & (lv2Filter) & (lv3Filter) & (lv4Filter) & (lv5Filter) ) );
            return (publicFilter);
        }

        //public static FilterDefinition<T> filterOrganizationOld<T>(this Criteria param)
        //{
        //    // (status != 'D' & ( (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '') | ( (lv0 = 'xxx') & (lv1 = 'xxx') & (lv2 = 'xxx') & (lv3 = 'xxx') ) ) )


        //    // (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '')
        //    var publicFilter = (Builders<T>.Filter.Eq("lv0", "")
        //        & Builders<T>.Filter.Eq("lv1", "")
        //        & Builders<T>.Filter.Eq("lv2", "")
        //        & Builders<T>.Filter.Eq("lv3", "")
        //        & Builders<T>.Filter.Eq("lv4", "")
        //        & Builders<T>.Filter.Eq("lv5", ""));




        //    // (lv0 = 'xxx' & lv1 = 'xxx' & lv2 = 'xxx' & lv3 = 'xxx')
        //    // Use 'Regex' because where lv like in content
        //    var lv5Filter = Builders<T>.Filter.Ne("status", "D");
        //    if (!string.IsNullOrEmpty(param.lv5))
        //    {
        //        var organization = param.lv5.Split(",");
        //        for (int i = 0; i < organization.Length; i++)
        //        {
        //            if (i == 0)
        //                lv5Filter = Builders<T>.Filter.Regex("lv5", organization[i]);
        //            else
        //                lv5Filter |= Builders<T>.Filter.Regex("lv5", organization[i]);
        //        }

        //        return (lv5Filter);
        //    }
        //    var lv4Filter = Builders<T>.Filter.Ne("status", "D");
        //    if (!string.IsNullOrEmpty(param.lv4))
        //    {
        //        var organization = param.lv4.Split(",");
        //        for (int i = 0; i < organization.Length; i++)
        //        {
        //            if (i == 0)
        //                lv4Filter = Builders<T>.Filter.Regex("lv4", organization[i]);
        //            else
        //                lv4Filter |= Builders<T>.Filter.Regex("lv4", organization[i]);
        //        }

        //        return (lv4Filter);
        //    }
        //    var lv3Filter = Builders<T>.Filter.Ne("status", "D");
        //    if (!string.IsNullOrEmpty(param.lv3))
        //    {
        //        var organization = param.lv3.Split(",");
        //        for (int i = 0; i < organization.Length; i++)
        //        {
        //            if (i == 0)
        //                lv3Filter = Builders<T>.Filter.Regex("lv3", organization[i]);
        //            else
        //                lv3Filter |= Builders<T>.Filter.Regex("lv3", organization[i]);
        //        }

        //        return (lv3Filter);
        //    }
        //    var lv2Filter = Builders<T>.Filter.Ne("status", "D");
        //    if (!string.IsNullOrEmpty(param.lv2))
        //    {
        //        var organization = param.lv2.Split(",");
        //        for (int i = 0; i < organization.Length; i++)
        //        {
        //            if (i == 0)
        //                lv2Filter = Builders<T>.Filter.Regex("lv2", organization[i]);
        //            else
        //                lv2Filter |= Builders<T>.Filter.Regex("lv2", organization[i]);
        //        }

        //        return (lv2Filter);
        //    }
        //    var lv1Filter = Builders<T>.Filter.Ne("status", "D");
        //    if (!string.IsNullOrEmpty(param.lv1))
        //    {
        //        var organization = param.lv1.Split(",");
        //        for (int i = 0; i < organization.Length; i++)
        //        {
        //            if (i == 0)
        //                lv1Filter = Builders<T>.Filter.Regex("lv1", organization[i]);
        //            else
        //                lv1Filter |= Builders<T>.Filter.Regex("lv1", organization[i]);
        //        }

        //        return (lv1Filter);
        //    }
        //    var lv0Filter = Builders<T>.Filter.Ne("status", "D");
        //    if (!string.IsNullOrEmpty(param.lv0))
        //    {
        //        var organization = param.lv0.Split(",");
        //        for (int i = 0; i < organization.Length; i++)
        //        {
        //            if (i == 0)
        //                lv0Filter = Builders<T>.Filter.Regex("lv0", organization[i]);
        //            else
        //                lv0Filter |= Builders<T>.Filter.Regex("lv0", organization[i]);
        //        }

        //        return (lv0Filter);
        //    }


        //    // (status != 'D' & ( (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '') | ( (lv0 = 'xxx') & (lv1 = 'xxx') & (lv2 = 'xxx') & (lv3 = 'xxx') ) ) )
        //    //return ( publicFilter | ( (lv0Filter) & (lv1Filter) & (lv2Filter) & (lv3Filter) & (lv4Filter) & (lv5Filter) ) );
        //    return (publicFilter);
        //}

        public static FilterDefinition<T> filterOrganizationOld<T>(this EventCalendar param)
        {
            // (status != 'D' & ( (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '') | ( (lv0 = 'xxx') & (lv1 = 'xxx') & (lv2 = 'xxx') & (lv3 = 'xxx') ) ) )

            // (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '')
            var publicFilter = Builders<T>.Filter.Ne("status", "D"); // <----- ผิดตรงนี้

            if (param.organization.Count == 0)
                return publicFilter = Builders<T>.Filter.Ne("status", "D");

            param.organization.ForEach(c =>
            {
                if (c.status == "A")
                {
                    // (lv0 = 'xxx' & lv1 = 'xxx' & lv2 = 'xxx' & lv3 = 'xxx')
                    // Use 'Regex' because where lv like in content

                    var organizationFilter = Builders<T>.Filter.Ne("status", "D");

                    if (!string.IsNullOrEmpty(c.lv4))
                    {
                        var organization = c.lv4.Split(",");
                        for (int i = 0; i < organization.Length; i++)
                        {
                            if (i == 0)
                                organizationFilter = Builders<T>.Filter.Regex("lv4", organization[i]);
                            else
                                organizationFilter |= Builders<T>.Filter.Regex("lv4", organization[i]);
                        }

                        publicFilter |= (organizationFilter); // <----- ผิดตรงนี้
                    }
                    else if (!string.IsNullOrEmpty(c.lv3))
                    {
                        var organization = c.lv3.Split(",");
                        for (int i = 0; i < organization.Length; i++)
                        {
                            if (i == 0)
                                organizationFilter = Builders<T>.Filter.Regex("lv3", organization[i]);
                            else
                                organizationFilter |= Builders<T>.Filter.Regex("lv3", organization[i]);
                        }

                        publicFilter |= (organizationFilter);
                    }
                    else if (!string.IsNullOrEmpty(c.lv2))
                    {
                        var organization = c.lv2.Split(",");
                        for (int i = 0; i < organization.Length; i++)
                        {
                            if (i == 0)
                                organizationFilter = Builders<T>.Filter.Regex("lv2", organization[i]);
                            else
                                organizationFilter |= Builders<T>.Filter.Regex("lv2", organization[i]);
                        }

                        publicFilter |= (organizationFilter);
                    }
                    else if (!string.IsNullOrEmpty(c.lv1))
                    {
                        var organization = c.lv1.Split(",");
                        for (int i = 0; i < organization.Length; i++)
                        {
                            if (i == 0)
                                organizationFilter = Builders<T>.Filter.Regex("lv1", organization[i]);
                            else
                                organizationFilter |= Builders<T>.Filter.Regex("lv1", organization[i]);
                        }

                        publicFilter |= (organizationFilter);
                    }
                    else if (!string.IsNullOrEmpty(c.lv0))
                    {
                        var organization = c.lv0.Split(",");
                        for (int i = 0; i < organization.Length; i++)
                        {
                            if (i == 0)
                                organizationFilter = Builders<T>.Filter.Regex("lv0", organization[i]);
                            else
                                organizationFilter |= Builders<T>.Filter.Regex("lv0", organization[i]);
                        }

                        publicFilter |= (organizationFilter);
                    }
                }
            });


            // (status != 'D' & ( (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '') | ( (lv0 = 'xxx') & (lv1 = 'xxx') & (lv2 = 'xxx') & (lv3 = 'xxx') ) ) )
            //return ( publicFilter | ( (lv0Filter) & (lv1Filter) & (lv2Filter) & (lv3Filter) & (lv4Filter) & (lv5Filter) ) );
            return (publicFilter);
        }

        public static Criteria filterQrganizationAuto(this List<Organization> param)
        {
            var value = new Criteria();

            foreach (var item in param)
            {
                if (!string.IsNullOrEmpty(item.lv5))
                {
                    value.lv0 = string.IsNullOrEmpty(value.lv0) ? item.lv0 : value.lv0 += "," + item.lv0;
                    value.lv1 = string.IsNullOrEmpty(value.lv1) ? item.lv1 : value.lv1 += "," + item.lv1;
                    value.lv2 = string.IsNullOrEmpty(value.lv2) ? item.lv2 : value.lv2 += "," + item.lv2;
                    value.lv3 = string.IsNullOrEmpty(value.lv3) ? item.lv3 : value.lv3 += "," + item.lv3;
                    value.lv4 = string.IsNullOrEmpty(value.lv4) ? item.lv4 : value.lv4 += "," + item.lv4;
                    var split = item.lv5.Split(",");
                    for (int i = 0; i < split.Count(); i++)
                    {
                        if (string.IsNullOrEmpty(value.lv5))
                            value.lv5 = split[i];
                        else
                            value.lv5 = value.lv5 + "," + split[i];

                        //var col2 = new Database().MongoClient<News>("organization");
                        //var filter2 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("code", split[i]) & Builders<News>.Filter.Eq("category", "lv5");
                        //var doc2 = col2.Find(filter2).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.description, c.descriptionEN, c.titleEN, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.view, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.sequence, c.status, c.lv0, c.lv1, c.lv2, c.lv3 }).FirstOrDefault();
                        //if (doc2 != null)
                        //{
                        //    if (string.IsNullOrEmpty(value.lv5))
                        //        value.lv5 = split[i];
                        //    else
                        //        value.lv5 = value.lv5 + "," + split[i];
                        //}
                    }
                }
                else if (!string.IsNullOrEmpty(item.lv4))
                {
                    value.lv0 = string.IsNullOrEmpty(value.lv0) ? item.lv0 : value.lv0 += "," + item.lv0;
                    value.lv1 = string.IsNullOrEmpty(value.lv1) ? item.lv1 : value.lv1 += "," + item.lv1;
                    value.lv2 = string.IsNullOrEmpty(value.lv2) ? item.lv2 : value.lv2 += "," + item.lv2;
                    value.lv3 = string.IsNullOrEmpty(value.lv3) ? item.lv3 : value.lv3 += "," + item.lv3;
                    var split = item.lv4.Split(",");
                    for (int i = 0; i < split.Count(); i++)
                    {
                        if (string.IsNullOrEmpty(value.lv4))
                            value.lv4 = split[i];
                        else
                            value.lv4 = value.lv4 + "," + split[i];

                        //Get Lv5 From Lv4
                        var colLv5 = new Database().MongoClient<News>("organization");
                        var filterLv5 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv4", split[i]) & Builders<News>.Filter.Eq("category", "lv5");
                        var docLv5 = colLv5.Find(filterLv5).Project(c => new { c.code }).ToList();
                        if (docLv5.Count() > 0)
                        {
                            if (string.IsNullOrEmpty(value.lv5))
                                value.lv5 = string.Join(",", docLv5.Select(c => c.code).ToList());
                            else
                                value.lv5 = value.lv5 + "," + string.Join(",", docLv5.Select(c => c.code).ToList());
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(item.lv3))
                {
                    value.lv0 = string.IsNullOrEmpty(value.lv0) ? item.lv0 : value.lv0 += "," + item.lv0;
                    value.lv1 = string.IsNullOrEmpty(value.lv1) ? item.lv1 : value.lv1 += "," + item.lv1;
                    value.lv2 = string.IsNullOrEmpty(value.lv2) ? item.lv2 : value.lv2 += "," + item.lv2;
                    var split = item.lv3.Split(",");
                    for (int i = 0; i < split.Count(); i++)
                    {
                        if (string.IsNullOrEmpty(value.lv3))
                            value.lv3 = split[i];
                        else
                            value.lv3 = value.lv3 + "," + split[i];

                        //Get Lv4 From Lv3
                        var colLv4 = new Database().MongoClient<News>("organization");
                        var filterLv4 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv3", split[i]) & Builders<News>.Filter.Eq("category", "lv4");
                        var docLv4 = colLv4.Find(filterLv4).Project(c => new { c.code }).ToList();
                        if (docLv4.Count() > 0)
                        {
                            if (string.IsNullOrEmpty(value.lv4))
                                value.lv4 = string.Join(",", docLv4.Select(c => c.code).ToList());
                            else
                                value.lv4 = value.lv4 + "," + string.Join(",", docLv4.Select(c => c.code).ToList());
                        }

                        //Copy This
                        docLv4.ForEach(i =>
                        {
                            //Get Lv5 From Lv4
                            var colLv5 = new Database().MongoClient<News>("organization");
                            var filterLv5 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv4", i.code) & Builders<News>.Filter.Eq("category", "lv5");
                            var docLv5 = colLv5.Find(filterLv5).Project(c => new { c.code }).ToList();
                            if (docLv5.Count() > 0)
                            {
                                if (string.IsNullOrEmpty(value.lv5))
                                    value.lv5 = string.Join(",", docLv5.Select(c => c.code).ToList());
                                else
                                    value.lv5 = value.lv5 + "," + string.Join(",", docLv5.Select(c => c.code).ToList());
                            }
                        });
                    }
                }
                else if (!string.IsNullOrEmpty(item.lv2))
                {
                    value.lv0 = string.IsNullOrEmpty(value.lv0) ? item.lv0 : value.lv0 += "," + item.lv0;
                    value.lv1 = string.IsNullOrEmpty(value.lv1) ? item.lv1 : value.lv1 += "," + item.lv1;
                    var split = item.lv2.Split(",");
                    for (int i = 0; i < split.Count(); i++)
                    {
                        if (string.IsNullOrEmpty(value.lv2))
                            value.lv2 = split[i];
                        else
                            value.lv2 = value.lv2 + "," + split[i];

                        //Get Lv3 From Lv2
                        var colLv3 = new Database().MongoClient<News>("organization");
                        var filterLv3 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv2", split[i]) & Builders<News>.Filter.Eq("category", "lv3");
                        var docLv3 = colLv3.Find(filterLv3).Project(c => new { c.code }).ToList();
                        if (docLv3.Count() > 0)
                        {
                            if (string.IsNullOrEmpty(value.lv3))
                                value.lv3 = string.Join(",", docLv3.Select(c => c.code).ToList());
                            else
                                value.lv3 = value.lv3 + "," + string.Join(",", docLv3.Select(c => c.code).ToList());
                        }

                        //Copy This
                        docLv3.ForEach(i =>
                        {
                            //Get Lv4 From Lv3
                            var colLv4 = new Database().MongoClient<News>("organization");
                            var filterLv4 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv3", i.code) & Builders<News>.Filter.Eq("category", "lv4");
                            var docLv4 = colLv4.Find(filterLv4).Project(c => new { c.code }).ToList();
                            if (docLv4.Count() > 0)
                            {
                                if (string.IsNullOrEmpty(value.lv4))
                                    value.lv4 = string.Join(",", docLv4.Select(c => c.code).ToList());
                                else
                                    value.lv4 = value.lv4 + "," + string.Join(",", docLv4.Select(c => c.code).ToList());
                            }

                            //Copy This
                            docLv4.ForEach(j =>
                            {
                                //Get Lv5 From Lv4
                                var colLv5 = new Database().MongoClient<News>("organization");
                                var filterLv5 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv4", j.code) & Builders<News>.Filter.Eq("category", "lv5");
                                var docLv5 = colLv5.Find(filterLv5).Project(c => new { c.code }).ToList();
                                if (docLv5.Count() > 0)
                                {
                                    if (string.IsNullOrEmpty(value.lv5))
                                        value.lv5 = string.Join(",", docLv5.Select(c => c.code).ToList());
                                    else
                                        value.lv5 = value.lv5 + "," + string.Join(",", docLv5.Select(c => c.code).ToList());
                                }
                            });
                        });
                    }
                }
                else if (!string.IsNullOrEmpty(item.lv1))
                {
                    value.lv0 = string.IsNullOrEmpty(value.lv0) ? item.lv0 : value.lv0 += "," + item.lv0;
                    var split = item.lv1.Split(",");
                    for (int i = 0; i < split.Count(); i++)
                    {
                        if (string.IsNullOrEmpty(value.lv1))
                            value.lv1 = split[i];
                        else
                            value.lv1 = value.lv1 + "," + split[i];

                        //Get Lv2 From Lv1
                        var colLv2 = new Database().MongoClient<News>("organization");
                        var filterLv2 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv1", split[i]) & Builders<News>.Filter.Eq("category", "lv2");
                        var docLv2 = colLv2.Find(filterLv2).Project(c => new { c.code }).ToList();
                        if (docLv2.Count() > 0)
                        {
                            if (string.IsNullOrEmpty(value.lv2))
                                value.lv2 = string.Join(",", docLv2.Select(c => c.code).ToList());
                            else
                                value.lv2 = value.lv2 + "," + string.Join(",", docLv2.Select(c => c.code).ToList());
                        }

                        //Copy This
                        docLv2.ForEach(i =>
                        {
                            //Get Lv3 From Lv2
                            var colLv3 = new Database().MongoClient<News>("organization");
                            var filterLv3 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv2", i.code) & Builders<News>.Filter.Eq("category", "lv3");
                            var docLv3 = colLv3.Find(filterLv3).Project(c => new { c.code }).ToList();
                            if (docLv3.Count() > 0)
                            {
                                if (string.IsNullOrEmpty(value.lv3))
                                    value.lv3 = string.Join(",", docLv3.Select(c => c.code).ToList());
                                else
                                    value.lv3 = value.lv3 + "," + string.Join(",", docLv3.Select(c => c.code).ToList());
                            }

                            //Copy This
                            docLv3.ForEach(j =>
                            {
                                //Get Lv4 From Lv3
                                var colLv4 = new Database().MongoClient<News>("organization");
                                var filterLv4 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv3", j.code) & Builders<News>.Filter.Eq("category", "lv4");
                                var docLv4 = colLv4.Find(filterLv4).Project(c => new { c.code }).ToList();
                                if (docLv4.Count() > 0)
                                {
                                    if (string.IsNullOrEmpty(value.lv4))
                                        value.lv4 = string.Join(",", docLv4.Select(c => c.code).ToList());
                                    else
                                        value.lv4 = value.lv4 + "," + string.Join(",", docLv4.Select(c => c.code).ToList());
                                }

                                //Copy This
                                docLv4.ForEach(k =>
                                {
                                    //Get Lv5 From Lv4
                                    var colLv5 = new Database().MongoClient<News>("organization");
                                    var filterLv5 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv4", k.code) & Builders<News>.Filter.Eq("category", "lv5");
                                    var docLv5 = colLv5.Find(filterLv5).Project(c => new { c.code }).ToList();
                                    if (docLv5.Count() > 0)
                                    {
                                        if (string.IsNullOrEmpty(value.lv5))
                                            value.lv5 = string.Join(",", docLv5.Select(c => c.code).ToList());
                                        else
                                            value.lv5 = value.lv5 + "," + string.Join(",", docLv5.Select(c => c.code).ToList());
                                    }
                                });
                            });
                        });
                    }
                }
                else if (!string.IsNullOrEmpty(item.lv0))
                {
                    var split = item.lv0.Split(",");
                    for (int i = 0; i < split.Count(); i++)
                    {
                        if (string.IsNullOrEmpty(value.lv0))
                            value.lv0 = split[i];
                        else
                            value.lv0 = value.lv0 + "," + split[i];

                        //Get Lv1 From Lv0
                        var colLv1 = new Database().MongoClient<News>("organization");
                        var filterLv1 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv0", split[i]) & Builders<News>.Filter.Eq("category", "lv1");
                        var docLv1 = colLv1.Find(filterLv1).Project(c => new { c.code }).ToList();
                        if (docLv1.Count() > 0)
                        {
                            if (string.IsNullOrEmpty(value.lv1))
                                value.lv1 = string.Join(",", docLv1.Select(c => c.code).ToList());
                            else
                                value.lv1 = value.lv1 + "," + string.Join(",", docLv1.Select(c => c.code).ToList());
                        }

                        //Copy This
                        docLv1.ForEach(i =>
                        {
                            //Get Lv2 From Lv1
                            var colLv2 = new Database().MongoClient<News>("organization");
                            var filterLv2 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv1", i.code) & Builders<News>.Filter.Eq("category", "lv2");
                            var docLv2 = colLv2.Find(filterLv2).Project(c => new { c.code }).ToList();
                            if (docLv2.Count() > 0)
                            {
                                if (string.IsNullOrEmpty(value.lv2))
                                    value.lv2 = string.Join(",", docLv2.Select(c => c.code).ToList());
                                else
                                    value.lv2 = value.lv2 + "," + string.Join(",", docLv2.Select(c => c.code).ToList());
                            }

                            //Copy This
                            docLv2.ForEach(j =>
                            {
                                //Get Lv3 From Lv2
                                var colLv3 = new Database().MongoClient<News>("organization");
                                var filterLv3 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv2", j.code) & Builders<News>.Filter.Eq("category", "lv3");
                                var docLv3 = colLv3.Find(filterLv3).Project(c => new { c.code }).ToList();
                                if (docLv3.Count() > 0)
                                {
                                    if (string.IsNullOrEmpty(value.lv3))
                                        value.lv3 = string.Join(",", docLv3.Select(c => c.code).ToList());
                                    else
                                        value.lv3 = value.lv3 + "," + string.Join(",", docLv3.Select(c => c.code).ToList());
                                }

                                //Copy This
                                docLv3.ForEach(k =>
                                {
                                    //Get Lv4 From Lv3
                                    var colLv4 = new Database().MongoClient<News>("organization");
                                    var filterLv4 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv3", k.code) & Builders<News>.Filter.Eq("category", "lv4");
                                    var docLv4 = colLv4.Find(filterLv4).Project(c => new { c.code }).ToList();
                                    if (docLv4.Count() > 0)
                                    {
                                        if (string.IsNullOrEmpty(value.lv4))
                                            value.lv4 = string.Join(",", docLv4.Select(c => c.code).ToList());
                                        else
                                            value.lv4 = value.lv4 + "," + string.Join(",", docLv4.Select(c => c.code).ToList());
                                    }

                                    //Copy This
                                    docLv4.ForEach(l =>
                                    {
                                        //Get Lv5 From Lv4
                                        var colLv5 = new Database().MongoClient<News>("organization");
                                        var filterLv5 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv4", l.code) & Builders<News>.Filter.Eq("category", "lv5");
                                        var docLv5 = colLv5.Find(filterLv5).Project(c => new { c.code }).ToList();
                                        if (docLv5.Count() > 0)
                                        {
                                            if (string.IsNullOrEmpty(value.lv5))
                                                value.lv5 = string.Join(",", docLv5.Select(c => c.code).ToList());
                                            else
                                                value.lv5 = value.lv5 + "," + string.Join(",", docLv5.Select(c => c.code).ToList());
                                        }
                                    });
                                });
                            });
                        });
                    }
                }
            }

            return value;
        }

        public static List<BsonValue> ConvertToCaseInsensitiveRegexList(this IEnumerable<string> source)
        {
            return source.Select(range => new BsonRegularExpression("/^" + range.Replace("+", @"\+") + "$/i")).Cast<BsonValue>().ToList();
        }

        public static List<BsonValue> ConvertToEndsWithRegexList(this IEnumerable<string> source)
        {
            return source.Select(range => new BsonRegularExpression("/" + range.Replace("+", @"\+") + "$/i")).Cast<BsonValue>().ToList();
        }

        public static BsonRegularExpression ToCaseInsensitiveRegex(this string source)
        {
            return new BsonRegularExpression("/^" + source.Replace("+", @"\+") + "$/i");
        }

        public static string verifyRude(this string param)
        {
            var col = new Database().MongoClient<Rude>("mrude");
            var filter = Builders<Rude>.Filter.Eq("isActive", true);
            var docs = col.Find(filter).Project(c => new Rude { code = c.code, title = c.title }).ToList();

            docs.ForEach(c =>
            {
                param = System.Text.RegularExpressions.Regex.Unescape(param.Replace(c.title, "(***)").ToString());
            });

            return param;
        }

        public static void createQrganizationAuto(this List<Organization> param, string code, string collection)
        {
            var value = new Criteria();

            foreach (var item in param)
            {
                if (!string.IsNullOrEmpty(item.lv5))
                {
                    value.lv0 = string.IsNullOrEmpty(value.lv0) ? item.lv0 : value.lv0 += "," + item.lv0;
                    value.lv1 = string.IsNullOrEmpty(value.lv1) ? item.lv1 : value.lv1 += "," + item.lv1;
                    value.lv2 = string.IsNullOrEmpty(value.lv2) ? item.lv2 : value.lv2 += "," + item.lv2;
                    value.lv3 = string.IsNullOrEmpty(value.lv3) ? item.lv3 : value.lv3 += "," + item.lv3;
                    value.lv4 = string.IsNullOrEmpty(value.lv4) ? item.lv4 : value.lv4 += "," + item.lv4;
                    var split = item.lv5.Split(",");
                    for (int i = 0; i < split.Count(); i++)
                    {
                        if (string.IsNullOrEmpty(value.lv5))
                            value.lv5 = split[i];
                        else
                            value.lv5 = value.lv5 + "," + split[i];

                        //var col2 = new Database().MongoClient<News>("organization");
                        //var filter2 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("code", split[i]) & Builders<News>.Filter.Eq("category", "lv5");
                        //var doc2 = col2.Find(filter2).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.description, c.descriptionEN, c.titleEN, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.view, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.sequence, c.status, c.lv0, c.lv1, c.lv2, c.lv3 }).FirstOrDefault();
                        //if (doc2 != null)
                        //{
                        //    if (string.IsNullOrEmpty(value.lv5))
                        //        value.lv5 = split[i];
                        //    else
                        //        value.lv5 = value.lv5 + "," + split[i];
                        //}
                    }
                }
                else if (!string.IsNullOrEmpty(item.lv4))
                {
                    value.lv0 = string.IsNullOrEmpty(value.lv0) ? item.lv0 : value.lv0 += "," + item.lv0;
                    value.lv1 = string.IsNullOrEmpty(value.lv1) ? item.lv1 : value.lv1 += "," + item.lv1;
                    value.lv2 = string.IsNullOrEmpty(value.lv2) ? item.lv2 : value.lv2 += "," + item.lv2;
                    value.lv3 = string.IsNullOrEmpty(value.lv3) ? item.lv3 : value.lv3 += "," + item.lv3;
                    var split = item.lv4.Split(",");
                    for (int i = 0; i < split.Count(); i++)
                    {
                        if (string.IsNullOrEmpty(value.lv4))
                            value.lv4 = split[i];
                        else
                            value.lv4 = value.lv4 + "," + split[i];

                        //Get Lv5 From Lv4
                        var colLv5 = new Database().MongoClient<News>("organization");
                        var filterLv5 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv4", split[i]) & Builders<News>.Filter.Eq("category", "lv5");
                        var docLv5 = colLv5.Find(filterLv5).Project(c => new { c.code }).ToList();
                        if (docLv5.Count() > 0)
                        {
                            if (string.IsNullOrEmpty(value.lv5))
                                value.lv5 = string.Join(",", docLv5.Select(c => c.code).ToList());
                            else
                                value.lv5 = value.lv5 + "," + string.Join(",", docLv5.Select(c => c.code).ToList());
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(item.lv3))
                {
                    value.lv0 = string.IsNullOrEmpty(value.lv0) ? item.lv0 : value.lv0 += "," + item.lv0;
                    value.lv1 = string.IsNullOrEmpty(value.lv1) ? item.lv1 : value.lv1 += "," + item.lv1;
                    value.lv2 = string.IsNullOrEmpty(value.lv2) ? item.lv2 : value.lv2 += "," + item.lv2;
                    var split = item.lv3.Split(",");
                    for (int i = 0; i < split.Count(); i++)
                    {
                        if (string.IsNullOrEmpty(value.lv3))
                            value.lv3 = split[i];
                        else
                            value.lv3 = value.lv3 + "," + split[i];

                        //Get Lv4 From Lv3
                        var colLv4 = new Database().MongoClient<News>("organization");
                        var filterLv4 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv3", split[i]) & Builders<News>.Filter.Eq("category", "lv4");
                        var docLv4 = colLv4.Find(filterLv4).Project(c => new { c.code }).ToList();
                        if (docLv4.Count() > 0)
                        {
                            if (string.IsNullOrEmpty(value.lv4))
                                value.lv4 = string.Join(",", docLv4.Select(c => c.code).ToList());
                            else
                                value.lv4 = value.lv4 + "," + string.Join(",", docLv4.Select(c => c.code).ToList());
                        }

                        //Copy This
                        docLv4.ForEach(i =>
                        {
                            //Get Lv5 From Lv4
                            var colLv5 = new Database().MongoClient<News>("organization");
                            var filterLv5 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv4", i.code) & Builders<News>.Filter.Eq("category", "lv5");
                            var docLv5 = colLv5.Find(filterLv5).Project(c => new { c.code }).ToList();
                            if (docLv5.Count() > 0)
                            {
                                if (string.IsNullOrEmpty(value.lv5))
                                    value.lv5 = string.Join(",", docLv5.Select(c => c.code).ToList());
                                else
                                    value.lv5 = value.lv5 + "," + string.Join(",", docLv5.Select(c => c.code).ToList());
                            }
                        });
                    }
                }
                else if (!string.IsNullOrEmpty(item.lv2))
                {
                    value.lv0 = string.IsNullOrEmpty(value.lv0) ? item.lv0 : value.lv0 += "," + item.lv0;
                    value.lv1 = string.IsNullOrEmpty(value.lv1) ? item.lv1 : value.lv1 += "," + item.lv1;
                    var split = item.lv2.Split(",");
                    for (int i = 0; i < split.Count(); i++)
                    {
                        if (string.IsNullOrEmpty(value.lv2))
                            value.lv2 = split[i];
                        else
                            value.lv2 = value.lv2 + "," + split[i];

                        //Get Lv3 From Lv2
                        var colLv3 = new Database().MongoClient<News>("organization");
                        var filterLv3 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv2", split[i]) & Builders<News>.Filter.Eq("category", "lv3");
                        var docLv3 = colLv3.Find(filterLv3).Project(c => new { c.code }).ToList();
                        if (docLv3.Count() > 0)
                        {
                            if (string.IsNullOrEmpty(value.lv3))
                                value.lv3 = string.Join(",", docLv3.Select(c => c.code).ToList());
                            else
                                value.lv3 = value.lv3 + "," + string.Join(",", docLv3.Select(c => c.code).ToList());
                        }

                        //Copy This
                        docLv3.ForEach(i =>
                        {
                            //Get Lv4 From Lv3
                            var colLv4 = new Database().MongoClient<News>("organization");
                            var filterLv4 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv3", i.code) & Builders<News>.Filter.Eq("category", "lv4");
                            var docLv4 = colLv4.Find(filterLv4).Project(c => new { c.code }).ToList();
                            if (docLv4.Count() > 0)
                            {
                                if (string.IsNullOrEmpty(value.lv4))
                                    value.lv4 = string.Join(",", docLv4.Select(c => c.code).ToList());
                                else
                                    value.lv4 = value.lv4 + "," + string.Join(",", docLv4.Select(c => c.code).ToList());
                            }

                            //Copy This
                            docLv4.ForEach(j =>
                            {
                                //Get Lv5 From Lv4
                                var colLv5 = new Database().MongoClient<News>("organization");
                                var filterLv5 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv4", j.code) & Builders<News>.Filter.Eq("category", "lv5");
                                var docLv5 = colLv5.Find(filterLv5).Project(c => new { c.code }).ToList();
                                if (docLv5.Count() > 0)
                                {
                                    if (string.IsNullOrEmpty(value.lv5))
                                        value.lv5 = string.Join(",", docLv5.Select(c => c.code).ToList());
                                    else
                                        value.lv5 = value.lv5 + "," + string.Join(",", docLv5.Select(c => c.code).ToList());
                                }
                            });
                        });
                    }
                }
                else if (!string.IsNullOrEmpty(item.lv1))
                {
                    value.lv0 = string.IsNullOrEmpty(value.lv0) ? item.lv0 : value.lv0 += "," + item.lv0;
                    var split = item.lv1.Split(",");
                    for (int i = 0; i < split.Count(); i++)
                    {
                        if (string.IsNullOrEmpty(value.lv1))
                            value.lv1 = split[i];
                        else
                            value.lv1 = value.lv1 + "," + split[i];

                        //Get Lv2 From Lv1
                        var colLv2 = new Database().MongoClient<News>("organization");
                        var filterLv2 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv1", split[i]) & Builders<News>.Filter.Eq("category", "lv2");
                        var docLv2 = colLv2.Find(filterLv2).Project(c => new { c.code }).ToList();
                        if (docLv2.Count() > 0)
                        {
                            if (string.IsNullOrEmpty(value.lv2))
                                value.lv2 = string.Join(",", docLv2.Select(c => c.code).ToList());
                            else
                                value.lv2 = value.lv2 + "," + string.Join(",", docLv2.Select(c => c.code).ToList());
                        }

                        //Copy This
                        docLv2.ForEach(i =>
                        {
                            //Get Lv3 From Lv2
                            var colLv3 = new Database().MongoClient<News>("organization");
                            var filterLv3 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv2", i.code) & Builders<News>.Filter.Eq("category", "lv3");
                            var docLv3 = colLv3.Find(filterLv3).Project(c => new { c.code }).ToList();
                            if (docLv3.Count() > 0)
                            {
                                if (string.IsNullOrEmpty(value.lv3))
                                    value.lv3 = string.Join(",", docLv3.Select(c => c.code).ToList());
                                else
                                    value.lv3 = value.lv3 + "," + string.Join(",", docLv3.Select(c => c.code).ToList());
                            }

                            //Copy This
                            docLv3.ForEach(j =>
                            {
                                //Get Lv4 From Lv3
                                var colLv4 = new Database().MongoClient<News>("organization");
                                var filterLv4 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv3", j.code) & Builders<News>.Filter.Eq("category", "lv4");
                                var docLv4 = colLv4.Find(filterLv4).Project(c => new { c.code }).ToList();
                                if (docLv4.Count() > 0)
                                {
                                    if (string.IsNullOrEmpty(value.lv4))
                                        value.lv4 = string.Join(",", docLv4.Select(c => c.code).ToList());
                                    else
                                        value.lv4 = value.lv4 + "," + string.Join(",", docLv4.Select(c => c.code).ToList());
                                }

                                //Copy This
                                docLv4.ForEach(k =>
                                {
                                    //Get Lv5 From Lv4
                                    var colLv5 = new Database().MongoClient<News>("organization");
                                    var filterLv5 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv4", k.code) & Builders<News>.Filter.Eq("category", "lv5");
                                    var docLv5 = colLv5.Find(filterLv5).Project(c => new { c.code }).ToList();
                                    if (docLv5.Count() > 0)
                                    {
                                        if (string.IsNullOrEmpty(value.lv5))
                                            value.lv5 = string.Join(",", docLv5.Select(c => c.code).ToList());
                                        else
                                            value.lv5 = value.lv5 + "," + string.Join(",", docLv5.Select(c => c.code).ToList());
                                    }
                                });
                            });
                        });
                    }
                }
                else if (!string.IsNullOrEmpty(item.lv0))
                {
                    var split = item.lv0.Split(",");
                    for (int i = 0; i < split.Count(); i++)
                    {
                        if (string.IsNullOrEmpty(value.lv0))
                            value.lv0 = split[i];
                        else
                            value.lv0 = value.lv0 + "," + split[i];

                        //Get Lv1 From Lv0
                        var colLv1 = new Database().MongoClient<News>("organization");
                        var filterLv1 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv0", split[i]) & Builders<News>.Filter.Eq("category", "lv1");
                        var docLv1 = colLv1.Find(filterLv1).Project(c => new { c.code }).ToList();
                        if (docLv1.Count() > 0)
                        {
                            if (string.IsNullOrEmpty(value.lv1))
                                value.lv1 = string.Join(",", docLv1.Select(c => c.code).ToList());
                            else
                                value.lv1 = value.lv1 + "," + string.Join(",", docLv1.Select(c => c.code).ToList());
                        }

                        //Copy This
                        docLv1.ForEach(i =>
                        {
                            //Get Lv2 From Lv1
                            var colLv2 = new Database().MongoClient<News>("organization");
                            var filterLv2 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv1", i.code) & Builders<News>.Filter.Eq("category", "lv2");
                            var docLv2 = colLv2.Find(filterLv2).Project(c => new { c.code }).ToList();
                            if (docLv2.Count() > 0)
                            {
                                if (string.IsNullOrEmpty(value.lv2))
                                    value.lv2 = string.Join(",", docLv2.Select(c => c.code).ToList());
                                else
                                    value.lv2 = value.lv2 + "," + string.Join(",", docLv2.Select(c => c.code).ToList());
                            }

                            //Copy This
                            docLv2.ForEach(j =>
                            {
                                //Get Lv3 From Lv2
                                var colLv3 = new Database().MongoClient<News>("organization");
                                var filterLv3 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv2", j.code) & Builders<News>.Filter.Eq("category", "lv3");
                                var docLv3 = colLv3.Find(filterLv3).Project(c => new { c.code }).ToList();
                                if (docLv3.Count() > 0)
                                {
                                    if (string.IsNullOrEmpty(value.lv3))
                                        value.lv3 = string.Join(",", docLv3.Select(c => c.code).ToList());
                                    else
                                        value.lv3 = value.lv3 + "," + string.Join(",", docLv3.Select(c => c.code).ToList());
                                }

                                //Copy This
                                docLv3.ForEach(k =>
                                {
                                    //Get Lv4 From Lv3
                                    var colLv4 = new Database().MongoClient<News>("organization");
                                    var filterLv4 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv3", k.code) & Builders<News>.Filter.Eq("category", "lv4");
                                    var docLv4 = colLv4.Find(filterLv4).Project(c => new { c.code }).ToList();
                                    if (docLv4.Count() > 0)
                                    {
                                        if (string.IsNullOrEmpty(value.lv4))
                                            value.lv4 = string.Join(",", docLv4.Select(c => c.code).ToList());
                                        else
                                            value.lv4 = value.lv4 + "," + string.Join(",", docLv4.Select(c => c.code).ToList());
                                    }

                                    //Copy This
                                    docLv4.ForEach(l =>
                                    {
                                        //Get Lv5 From Lv4
                                        var colLv5 = new Database().MongoClient<News>("organization");
                                        var filterLv5 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv4", l.code) & Builders<News>.Filter.Eq("category", "lv5");
                                        var docLv5 = colLv5.Find(filterLv5).Project(c => new { c.code }).ToList();
                                        if (docLv5.Count() > 0)
                                        {
                                            if (string.IsNullOrEmpty(value.lv5))
                                                value.lv5 = string.Join(",", docLv5.Select(c => c.code).ToList());
                                            else
                                                value.lv5 = value.lv5 + "," + string.Join(",", docLv5.Select(c => c.code).ToList());
                                        }
                                    });
                                });
                            });
                        });
                    }
                }
            }

            //update
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient(collection);
                var filter = Builders<BsonDocument>.Filter.Eq("code", code);

                doc = col.Find(filter).FirstOrDefault();
                //var model = BsonSerializer.Deserialize<object>(doc);
                doc["status"] = doc["isActive"].ToBoolean() ? "A" : "N";
                doc["lv0"] = value.lv0;
                doc["lv1"] = value.lv1;
                doc["lv2"] = value.lv2;
                doc["lv3"] = value.lv3;
                doc["lv4"] = value.lv4;
                doc["lv5"] = value.lv5;
                col.ReplaceOne(filter, doc);
            }
            catch
            {
                
            }
        }


        public static CertificateTeacherModel CheckCertificateType(this String code)
        {
            var result = new CertificateTeacherModel{
                certificateTypeName = "",
                color = "0xFF707070",
                statusName = "รอใบประกอบวิชาชีพ",
                };
            switch (code)
            {
                case "0":
                    result.certificateTypeName = "ใบประกอบวิชาชีพ";
                    result.color = "0xFF408C40";
                    result.statusName = "มีใบประกอบวิชาชีพ";
                    break;
                case "1":
                    result.certificateTypeName = "ใบอนุญาตปฎิบัติการสอน";
                    result.color = "0xFF408C40";
                    result.statusName = "มีใบประกอบวิชาชีพ";
                    break;
                case "7":
                    result.certificateTypeName = "ใบประกอบวิชาชีพครูโดยไม่มี ใบอนุญาตประกอบวิชาชีพ";
                    result.color = "0xFFEEBA33";
                    result.statusName = "ประกอบวิชาชีพชั่วคราว";
                    break;

                default:
                    break;
            }
            return result;
        }

        public static String CheckTeachDegreeLevelID(this String code)
        {
            String name = "";
            var data = new string[] { "เตรียมอนุบาล", "ก่อนประถมศึกษา", "ประถมศึกษา", "มัธยมศึกษาตอนต้น", "มัธยมศึกษาตอนปลาย" };

            switch (code)
            {
                case "12":
                    name = data[0];
                    break;
                case "1":
                    name = data[1];
                    break;
                case "2":
                    name = data[2];
                    break;
                case "3":
                    name = data[3];
                    break;
                case "4":
                    name = data[4];
                    break;

                default:
                    break;
            }
            return name;
        }

        public static String CheckTeachSubjectID(this String code)
        {
            String name = "";
            var data = new string[] { "สาระการเรียนรู้ภาษาไทย", "สาระการเรียนรู้ศิลปะศึกษา", "สาระการเรียนรู้วิทยาศาสตร์", "สาระการเรียนรู้คณิตศาสตร์", "สาระการเรียนรู้ ภาษาต่างประเทศ", "สาระการเรียนรู้สุขศึกษาและ พละศึกษา", "สาระการเรียนรู้สังคม ศาสนา", "สาระการเรียนรู้การงานและ อาชีพ" };

            switch (code)
            {
                case "1":
                    name = data[0];
                    break;
                case "3":
                    name = data[1];
                    break;
                case "4":
                    name = data[2];
                    break;
                case "5":
                    name = data[3];
                    break;
                case "6":
                    name = data[4];
                    break;
                case "7":
                    name = data[5];
                    break;
                case "8":
                    name = data[6];
                    break;
                case "9":
                    name = data[7];
                    break;
                default:
                    break;
            }
            return name;
        }
    }
}

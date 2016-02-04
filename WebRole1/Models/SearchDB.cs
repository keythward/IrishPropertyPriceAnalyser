// model class for the search page

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebRole1.DatabaseConn;

namespace WebRole1.Models
{
    public class SearchDB
    {

        public string County { get; set; }
        public string Year { get; set; }
        public string Value { get; set; }
        public string Dwelling { get; set; }
        public string MarketPrice { get; set; }
        public string PostCode { get; set; }
        public string KeyWord { get; set; }
        public string Dates { get; set; }


        public static string[] Counties
        {
            get
            {
                return new string[] { "Kerry", "Cork", "Limerick", "Tipperary", "Waterford", "Kilkenny", "Wexford", "Laois",
                                      "Carlow", "Kildare", "Wicklow","Offaly", "Dublin", "Meath", "Westmeath", "Louth",
                                      "Monaghan", "Cavan", "Longford", "Donegal", "Leitrim", "Sligo", "Roscommon", "Mayo",
                                      "Galway", "Clare" };
            }
        }
        public static string[] Years
        {
            get
            {
                return new string[] { "2010", "2011", "2012", "2013", "2014", "2015", "2016" };
            }
        }

        public static string[] DwellingTypes
        {
            get
            {
                return new string[] { "New Property","Second Hand Property" };
            }
        }

        public static string[] ValueRange
        {
            get
            {
                return new string[] { "<€50,000", "€50,000 - €100,000", "€100,000 - €150,000", "€150,000 - €200,000",
                                      "€200,000 - €250,000","€250,000 - €300,000","€350,000 - €400,000","€400,000 - €450,000",
                                      "€450,000 - €500,000","€500,000 - €550,000","€550,000 - €600,000","€600,000 - €650,000",
                                      "€650,000 - €700,000","€700,000 - €750,000",">€750,000"};
            }
        }

        public static string[] MarketPriceDecision
        {
            get
            {
                return new string[] { "Yes", "No" };
            }
        }

        public static string[] PostalCodes
        {
            get
            {
                return new string[] { "1", "2","3","4","5","6","6w","7","8","9","10","11","12","13","14","15","16","17","18",
                                      "20","22","24","county dublin" };
            }
        }

        public static string[] DatesBetween
        {
            get
            {
                return new string[] { "all year", "first 6 months","last 6 months"};
            }
        }

        public List<ListObject> Found
        {
            get
            {
                List<ListObject> list = new List<ListObject>();
                string doc_id="";
                DBRecord test = null;
                if (County.Equals("Dublin"))
                {

                }
                else
                {
                    if(Dates.Equals("all year"))
                    {
                        doc_id = County + Year + "_A";
                        test = DatabaseConnect2.ReadDocument(doc_id);
                        list = test.records;
                        doc_id = County + Year + "_B";
                        test = DatabaseConnect2.ReadDocument(doc_id);
                        list.AddRange(test.records);
                    }
                    else if(Dates.Equals("first 6 months"))
                    {
                        doc_id = County + Year + "_A";
                        test = DatabaseConnect2.ReadDocument(doc_id);
                        list = test.records;
                    }
                    else
                    {
                        doc_id = County + Year + "_B";
                        test = DatabaseConnect2.ReadDocument(doc_id);
                        list = test.records;
                    }
                }
                return list;
            }
        }

        









    }
}

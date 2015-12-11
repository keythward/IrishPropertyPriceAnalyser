// add altered records to proper database documents

using System;
using System.Collections.Generic;
using WorkerRole1.DatabaseConnections;
using WorkerRole1.Records;

namespace WorkerRole1.Data
{
    public class AddToDatabase
    {
        public List<AlteredRecord> templist;
        public List<AlteredRecord> templist1;
        public List<AlteredRecord> templist2;

        public AddToDatabase()
        {
            templist = new List<AlteredRecord>();
            templist1 = new List<AlteredRecord>();
            templist2 = new List<AlteredRecord>();
        }
        // list of counties
        public enum County
        {
            Kerry, Cork, Limerick, Tipperary, Waterford, Kilkenny, Wexford, Laois, Carlow, Kildare, Wicklow,
            Offaly, Dublin, Meath, Westmeath, Louth, Monaghan, Cavan, Longford, Donegal, Leitrim, Sligo, Roscommon, Mayo, Galway, Clare
        };

        // divide list up into counties and add to database document
        public void AddList(List<AlteredRecord> list,DateTime date)
        {
            // loop through every county 1 at a time
            for (County co = County.Kerry; co <= County.Clare; co++)
            {
                if (co == County.Dublin) // Dublin is too large and needs to be uploaded to database in months
                {
                    foreach (AlteredRecord ar in list)
                    {
                        if (ar.County.Equals(co.ToString()))
                        {
                            templist.Add(ar);
                        }
                    }
                    // only need to update around date of last update, so:
                    int day = date.Day;
                    int month = date.Month;
                    string year = date.Year.ToString();
                    if (day <= 10) // if in 1st 10 days of month update month + last month
                    {
                        List<AlteredRecord> templist_a = new List<AlteredRecord>(); // this month
                        List<AlteredRecord> templist_b = new List<AlteredRecord>(); // last month
                        foreach (AlteredRecord ar in templist)
                        {
                            // this month
                            if (ar.SoldOn.Month == month)
                            {
                                templist_a.Add(ar);
                            }
                            // last month
                            if (month == 1) // if month is january last month needs to be december
                            {
                                if (ar.SoldOn.Month == 12)
                                {
                                    templist_b.Add(ar);
                                }
                            }
                            else // otherwise take last month
                            {
                                if (ar.SoldOn.Month == month-1)
                                {
                                    templist_b.Add(ar);
                                }
                            }
                        }
                        // create database connection with list and county
                        DatabaseConnect dba = new DatabaseConnect(co.ToString(), templist_a);
                        DatabaseConnect dbb = new DatabaseConnect(co.ToString(), templist_b);
                        // update the document with new data
                        dba.ModifyDocumentDublin(year, month);
                        if (month == 1) // year needs to be last year
                        {
                            int year2 = date.Year - 1;
                            string lastYear = year2.ToString();
                            dbb.ModifyDocumentDublin(lastYear, 12);
                        }
                        else
                        {
                            dbb.ModifyDocumentDublin(year, month-1);
                        }
                    }
                    else // otherwise just update month
                    {
                        List<AlteredRecord> templist_a = new List<AlteredRecord>();
                        foreach (AlteredRecord ar in templist)
                        {
                            if (ar.SoldOn.Month == month)
                            {
                                templist_a.Add(ar);
                            }
                        }
                        // create database connection with list and county
                        DatabaseConnect dba = new DatabaseConnect(co.ToString(), templist_a);
                        // update the document with new data
                        dba.ModifyDocumentDublin(year,month);
                    }
                    // clear the list
                    templist.Clear();
                }
                /*else // rest of ireland
                {
                    foreach (AlteredRecord ar in list)
                    {
                        if (ar.County.Equals(co.ToString()))
                        {
                            templist.Add(ar);
                        }
                    }
                    foreach (AlteredRecord ar in templist) // divide in 2
                    {
                        if (ar.SoldOn.Month == 1 || ar.SoldOn.Month == 2 || ar.SoldOn.Month == 3 || ar.SoldOn.Month == 4 ||
                            ar.SoldOn.Month == 5 || ar.SoldOn.Month == 6)
                        {
                            templist1.Add(ar);
                        }
                        else
                        {
                            templist2.Add(ar);
                        }
                    }
                    // save templist in proper document
                    DatabaseConnect db = new DatabaseConnect(co.ToString(), templist1);
                    DatabaseConnect db2 = new DatabaseConnect(co.ToString(), templist2);
                    string year = "2014_A";
                    //db.CreateDocument(year);
                    year = "2014_B";
                    //db2.CreateDocument(year);
                    //db.ModifyDocument(co.ToString());
                    // empty templists
                    templist.Clear();
                    templist1.Clear();
                    templist2.Clear();
                }*/
            }

        }
    }
}

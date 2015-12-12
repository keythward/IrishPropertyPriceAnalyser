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
        public List<AlteredRecord> templist3;

        public AddToDatabase()
        {
            templist = new List<AlteredRecord>();
            templist1 = new List<AlteredRecord>();
            templist2 = new List<AlteredRecord>();
            templist3 = new List<AlteredRecord>();
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
            DateTime lastDate = WorkerRole.dateTimeUpdatedTo;
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
                        // get the last date in the list so know were database has been updated to
                        if (ar.SoldOn > lastDate)
                        {
                            lastDate = ar.SoldOn;
                        }
                    }
                    // set date updated too as last date in list
                    WorkerRole.dateTimeUpdatedTo = lastDate;
                    // only need to update around date of last update, so:
                    int day = date.Day;
                    int month = date.Month;
                    string year = date.Year.ToString();
                    if (day <= 10) // if in 1st 10 days of month update month + last month
                    {
                        foreach (AlteredRecord ar in templist)
                        {
                            // this month
                            if (ar.SoldOn.Month == month)
                            {
                                templist1.Add(ar);
                            }
                            // last month
                            if (month == 1) // if month is january last month needs to be december
                            {
                                // needs data from last year so it needs to be downloaded and then updated
                                int year2 = date.Year - 1;
                                string lastYear = year2.ToString();
                                WorkerRole.MainTasks(lastYear);
                            }
                            else // otherwise take last month
                            {
                                if (ar.SoldOn.Month == month-1)
                                {
                                    templist2.Add(ar);
                                }
                            }
                        }
                        // create database connection with list and county
                        DatabaseConnect dba = new DatabaseConnect(co.ToString(), templist1);
                        DatabaseConnect dbb = new DatabaseConnect(co.ToString(), templist2);
                        // update the document with new data
                        dba.ModifyDocumentDublin(year, month);
                        if (month == 1) // year needs to be last year
                        {
                            // do nothing being taken care of
                        }
                        else
                        {
                            dbb.ModifyDocumentDublin(year, month-1);
                        }
                    }
                    else // otherwise just update month
                    {
                        foreach (AlteredRecord ar in templist)
                        {
                            if (ar.SoldOn.Month == month)
                            {
                                templist1.Add(ar);
                            }
                        }
                        // create database connection with list and county
                        DatabaseConnect dba = new DatabaseConnect(co.ToString(), templist1);
                        // update the document with new data
                        dba.ModifyDocumentDublin(year,month);
                    }
                    // if any data for next month deal with it
                    if (lastDate.Month > month) 
                    {
                        foreach (AlteredRecord ar in templist) // if any data for next month is in the list
                        {
                            if (ar.SoldOn.Month == lastDate.Month)
                            {
                                templist3.Add(ar);
                            }
                        }
                        // create database connection with list and county
                        DatabaseConnect dbd = new DatabaseConnect(co.ToString(), templist3);
                        // update the document with new data
                        dbd.ModifyDocumentDublin(year, lastDate.Month);
                    }
                    // clear the lists
                    templist.Clear();
                    templist1.Clear();
                    templist2.Clear();
                    templist3.Clear();
                }
                else // rest of Ireland
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
                    // depending on date send appropriate list to database
                    int day = date.Day;
                    int month = date.Month;
                    string year = date.Year.ToString();
                    if (day <= 10 && (month==1 || month==7)) // start of month january or july, update both groups
                    {
                        if (month == 7) // july
                        {
                            DatabaseConnect db = new DatabaseConnect(co.ToString(), templist1);
                            db.ModifyDocumentBoggers(year, 'A');
                            DatabaseConnect db2 = new DatabaseConnect(co.ToString(), templist2);
                            db2.ModifyDocumentBoggers(year, 'B');
                        }
                        else // january
                        {
                            DatabaseConnect db = new DatabaseConnect(co.ToString(), templist1);
                            db.ModifyDocumentBoggers(year, 'A');
                            // needs data from last year so it needs to be downloaded and then updated
                            int year2 = date.Year - 1;
                            string lastYear = year2.ToString();
                            WorkerRole.MainTasks(lastYear);
                        }
                    }
                    else // update 1 group only
                    {
                        if (month <= 6) // first part of year - temp list 1
                        {
                            DatabaseConnect db = new DatabaseConnect(co.ToString(), templist1);
                            db.ModifyDocumentBoggers(year,'A');
                        }
                        else // 2nd part of year - temp list 2
                        {
                            DatabaseConnect db = new DatabaseConnect(co.ToString(), templist2);
                            db.ModifyDocumentBoggers(year,'B');
                        }
                    }
                    // clear temp lists
                    templist.Clear();
                    templist1.Clear();
                    templist2.Clear();
                }
            }

        }
        // adding to database any changes from the previous year
        public void AddListLastYear(List<AlteredRecord> list,string year)
        {
            // loop through every county 1 at a time
            for (County co = County.Kerry; co <= County.Clare; co++)
            {
                if (co == County.Dublin) // Dublin only december
                {
                    foreach (AlteredRecord ar in list)
                    {
                        if ((ar.County.Equals(co.ToString()))&&(ar.SoldOn.Month==12))
                        {
                            templist.Add(ar);
                        }
                    }
                    // create database connection with list and county
                    DatabaseConnect dba = new DatabaseConnect(co.ToString(), templist);
                    dba.ModifyDocumentDublin(year, 12);
                    // clear temp list
                    templist.Clear();
                }
                else // rest of country
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
                    // create database connection with list and county
                    DatabaseConnect dba = new DatabaseConnect(co.ToString(), templist2);
                    dba.ModifyDocumentBoggers(year,'B');
                    // clear temp list
                    templist.Clear();
                    templist1.Clear();
                    templist2.Clear();
                }
            }
        }
    }
}

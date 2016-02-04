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
        public void AddList(List<AlteredRecord> list,DateTime date) // date parameter is were updated to
        {
            DateTime lastDate = date;
            // find date the update file is updated to
            foreach (AlteredRecord ar in list)
            {
                // get the last date in the list so know were database has been updated to
                if (ar.SoldOn > lastDate)
                {
                    lastDate = ar.SoldOn;
                }
            }
            // set date updated too as last date in list
            WorkerRole.dateTimeUpdatedTo = lastDate;
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
                    dba.ModifyDocumentDublin(year, month);                    
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
                    if (month>=1 && month<=6) // first 6 months update group A
                    {
                        DatabaseConnect db = new DatabaseConnect(co.ToString(), templist1);
                        db.ModifyDocumentBoggers(year, 'A');
                    }
                    else // update last 6 months in group B
                    {
                        DatabaseConnect db2 = new DatabaseConnect(co.ToString(), templist2);
                        db2.ModifyDocumentBoggers(year, 'B');
                    }
                    // clear temp lists
                    templist.Clear();
                    templist1.Clear();
                    templist2.Clear();
                }
            }

        }
    }
}

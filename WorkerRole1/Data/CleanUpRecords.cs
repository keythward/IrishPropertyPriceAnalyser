// take the list of records, clean up every record object into a list of altered record objects

using System;
using System.Collections.Generic;
using System.Linq;
using WorkerRole1.Records;

namespace WorkerRole1.Data
{
    public class CleanUpRecords
    {
        // filter the records into new altered record objects and return in list
        public List<AlteredRecord> CleanRecords(List<Record> list)
        {
            List<AlteredRecord> altered = new List<AlteredRecord>();
            foreach (Record r in list)
            {
                AlteredRecord ar = new AlteredRecord();
                ar.SoldOn = r.SoldDate;
                ar.County = r.County;
                // take '?' of front of price and change to double
                // no need to catch format exception
                string sub = r.Price.Substring(1);
                double price = Convert.ToDouble(sub);
                ar.Price = price;
                // take first char from string for NFMP
                ar.NotFullMP = r.NMFP[0];
                // shorten description to char of N/S (new/second hand)
                if (r.Description.StartsWith("N"))
                {
                    ar.Description = 'N';
                }
                if (r.Description.StartsWith("S"))
                {
                    ar.Description = 'S';
                }
                // take vat column out and phase it into the price (if No then take 13.5% off price)
                if (r.VAT.StartsWith("N"))
                {
                    price = price * 0.865;
                    ar.Price = price;
                }
                // sort address and postal code if Dublin
                AddressHolder addresssorted = CleanAddress(r.Address, r.County, r.PostalCode);
                ar.Address = addresssorted.Address;
                ar.PostCode = addresssorted.PostalCode;
                // add altered record to altered record list
                altered.Add(ar);
            }
            return altered;
        }

        // clean address and postal code
        public AddressHolder CleanAddress(string address, string county, string pc)
        {
            AddressHolder add = new AddressHolder();
            // break the address up into its parts separated by a comma, make all lowercase
            string[] parts = address.Split(',').Select(sValue => sValue.Trim().ToLower()).ToArray();
            // change to lowercase
            string countyLower = county.ToLower();
            string pcLower = pc.ToLower();
            add.PostalCode = pcLower;
            // number elements in array
            int num = parts.Length;
            // not for Dublin check if last element of address matches the county, if so get rid of
            if (!countyLower.Equals("dublin"))
            {
                if (parts[num - 1].Equals(countyLower) || parts[num - 1].Equals("co " + countyLower) ||
                    parts[num - 1].Equals("co. " + countyLower) || parts[num - 1].Equals("county " + countyLower)
                    || parts[num - 1].Equals("co." + countyLower))
                {
                    num--; // take last element away
                }
            }
            else // Sort out Dublin
            {
                if (parts[num - 1].Equals(countyLower) || parts[num - 1].Equals("co " + countyLower) ||
                    parts[num - 1].Equals("co. " + countyLower) || parts[num - 1].Equals("county " + countyLower)
                    || parts[num - 1].Equals(pcLower) || parts[num - 1].Equals("co." + countyLower))
                {
                    if (parts[num - 1].Any(char.IsDigit)) // if it contains numbers its the postal code
                    {
                        add.PostalCode = parts[num - 1];
                    }
                    num--;
                }
                // try find postal code from address if does not already have a value for it
                if (add.PostalCode.Equals(""))
                {
                    if (FindPostalCode(parts[num - 1]) != null)
                    {
                        add.PostalCode = FindPostalCode(parts[num - 1]);
                    }
                    else // check if post code is in address at end of address line
                    {
                        string last = parts[num - 1];
                        int lastLenght = last.Length;
                        if (lastLenght >= 6)
                        {
                            last = parts[num - 1].Substring(0, 6);
                        }
                        if (last.Equals("dublin"))
                        {
                            add.PostalCode = parts[num - 1];
                            num--;
                        }
                    }
                }
            }
            // put address back together
            string complete = "";
            for (int i = 0; i < num; i++)
            {
                complete += parts[i] + ",";
            }
            add.Address = complete.Remove(complete.Length - 1); // take away last comma
            // return address
            return add;
        }

        // container to hold address and postal code
        public struct AddressHolder
        {
            public string Address { get; set; }
            public string PostalCode { get; set; }
        }

        // find postal code or return null
        public string FindPostalCode(string line)
        {
            string found = null;
            if (postalCodes.ContainsKey(line))
            {
                found = postalCodes[line];
            }
            return found;
        }

        // postal codes matching areas
        public static Dictionary<string, string> postalCodes = new Dictionary<string, string>()
            { {"north wall", "dublin 1"},
                { "summerhill","dublin 1"},
                { "parnell street", "dublin 1"},
                { "templebar", "dublin 1"},
                { "ballybough","dublin 3"},
                { "cloniffe","dublin 3"},
                { "clontarf","dublin 3"},
                { "dollymount", "dublin 3"},
                { "east wall", "dublin 3"},
                { "fairview", "dublin 3"},
                { "marino","dublin 3"},
                { "killester", "dublin 3"},
                { "college green", "dublin 2"},
                { "merrion square", "dublin 2"},
                { "st. stephens green", "dublin 2"},
                { "ballsbridge", "dublin 4"},
                { "donnybrook", "dublin 4"},
                { "irishtown", "dublin 4"},
                { "merrion", "dublin 4"},
                { "pembroke", "dublin 4"},
                { "ringsend", "dublin 4"},
                { "sandymount", "dublin 4"},
                { "north strand", "dublin 3"},
                { "artane", "dublin 5"},
                { "harmonstown", "dublin 5"},
                { "donnycarney", "dublin 5"},
                { "raheny", "dublin 5"},
                { "dartry", "dublin 6"},
                { "ranelagh", "dublin 6"},
                { "rathmines", "dublin 6"},
                { "rathgar", "dublin 6"},
                { "harolds cross", "dublin 6w"},
                { "templeogue", "dublin 6w"},
                { "terenure", "dublin 6w"},
                { "arbour hill", "dublin 7"},
                { "cabra", "dublin 7"},
                { "phibsboro", "dublin 7"},
                { "four courts", "dublin 7"},
                { "navan road", "dublin 7"},
                { "dolphins barn", "dublin 8"},
                { "rialto", "dublin 8"},
                { "inchicore", "dublin 8"},
                { "island bridge", "dublin 8"},
                { "kilmainham", "dublin 8"},
                { "portobello", "dublin 8"},
                { "the coombe", "dublin 8"},
                { "beaumont", "dublin 9"},
                { "drumcondra", "dublin 9"},
                { "santry", "dublin 9"},
                { "whitehall", "dublin 9"},
                { "glasnevin", "dublin 9"},
                { "ballyfermot", "dublin 10"},
                { "ballygall", "dublin 11"},
                { "cappagh", "dublin 11"},
                { "cremore", "dublin 11"},
                { "dubber", "dublin 11"},
                { "finglas", "dublin 11"},
                { "jamestown", "dublin 11"},
                { "kilshane", "dublin 11"},
                { "wadelai", "dublin 11"},
                { "bluebell", "dublin 12"},
                { "crumlin", "dublin 12"},
                { "drimnagh", "dublin 12"},
                { "walkinstown", "dublin 12"},
                { "baldoyle","dublin 13"},
                { "donaghmede", "dublin 13"},
                { "sutton", "dublin 13"},
                { "howth", "dublin 13"},
                { "churchtown", "dublin 14"},
                { "dundrum", "dublin 14"},
                { "goatstown", "dublin 14"},
                { "roebuck", "dublin 14"},
                { "windy arbour", "dublin 14"},
                { "clonskeagh", "dublin 14"},
                { "rathfarnham", "dublin 14"},
                { "blanchardstown", "dublin 15"},
                { "castleknock", "dublin 15"},
                { "clonee", "dublin 15"},
                { "clonsilla", "dublin 15"},
                { "corduff", "dublin 15"},
                { "mulhuddart", "dublin 15"},
                { "tyrrelstown", "dublin 15"},
                { "ballinteer", "dublin 16"},
                { "kilmashogue", "dublin 16"},
                { "knocklyon", "dublin 16"},
                { "rockbrook", "dublin 16"},
                { "whitechurch", "dublin 16"},
                { "belcamp", "dublin 17"},
                { "balgriffin", "dublin 17"},
                { "clonshaugh", "dublin 17"},
                { "darndale", "dublin 17"},
                { "riverside", "dublin 17"},
                { "clare hall", "dublin 17"},
                { "cabinteely", "dublin 18"},
                { "carrickmines", "dublin 18"},
                { "foxrock","dublin 18"},
                { "kilternan", "dublin 18"},
                { "sandyford", "dublin 18"},
                { "ticknock", "dublin 18"},
                { "ballyedmonduff", "dublin 18"},
                { "stepaside", "dublin 18"},
                { "leopardstown", "dublin 18"},
                { "loughlinstown", "dublin 18"},
                { "lucan", "dublin 20"},
                { "chapelizod", "dublin 20"},
                { "palmerstown", "dublin 20"},
                { "adamstown", "dublin 20"},
                { "neilstown", "dublin 22"},
                { "clondalkin", "dublin 22"},
                { "bawnogue", "dublin 22"},
                { "firhouse", "dublin 24"},
                { "jobstown", "dublin 24"},
                { "kilnamanagh", "dublin 24"},
                { "oldbawn", "dublin 24"},
                { "tallaght", "dublin 24"},
                { "springfield", "dublin 24"},
                { "booterstown", "county dublin"},
                { "williamstown", "county dublin"},
                { "salthill", "county dublin"},
                { "monkstown", "county dublin"},
                { "mt. merrion", "county dublin"},
                { "blackrock", "county dublin"},
                { "stillorgan", "county dublin"},
                { "kilmacud", "county dublin"},
                { "deans grange", "county dublin"},
                { "newtown park", "county dublin"},
                { "sandycove","county dublin"},
                { "mounttown", "county dublin"},
                { "sallynoggin", "county dublin"},
                { "glasthule", "county dublin"},
                { "dun laoghaire", "county dublin"},
                { "dunlaoghaire", "county dublin"},
                { "glenageary","county dublin"},
                { "kill-o-the-grange", "county dublin"},
                { "dalkey", "county dublin"},
                { "killiney", "county dublin"},
                { "ballybrack", "county dublin"},
                { "dunlaoire", "county dublin"},
                { "malahide", "county dublin"},
                { "swords", "county dublin"},
                { "newcastle", "county dublin"},
                { "balbriggan", "county dublin"},
                { "donabate", "county dublin"},
                { "skerries", "county dublin"},
                { "portmarnock", "county dublin"},
                { "rathcoole", "county dublin"},
                { "saggart", "county dublin"},
                { "shankill", "county dublin"}};
    }
}

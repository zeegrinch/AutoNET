using System;
using System.Collections.Generic;
using CHO.Next.Global.Enumerations;

namespace CHO.Next.Global.Queries
{
    /// <summary>
    /// Query used to specify filtering for a specific Tee Sheets
    /// This is container ONLY. 
    /// </summary>
    [Serializable]
    public class TeeSheetFilterQuery
    {
        public TeeSheetFilterQuery()
        {
            Courses = new List<int>();
        }

        private DateTime _date;
        //Current selected Date
        public DateTime Date 
        {
            get { //if (_date == DateTime.MinValue) return Convert.ToDateTime("1/1/1900");
                return _date;}
            set { _date = value; }
        }

        //Tee Sheet Date
        public TimeOfDay TimeOfDay { get; set; }

        //List of course IDs
        public List<int> Courses { get; set; }

        //True if available TeeTimes
        public bool Available { get; set; }

        //The company being searched
        public int CompanyId { get; set; }

        //The city being searched
        public int CityId { get; set; }

        //The state/province being searched
        public int StateId { get; set; }

        //The country being searched
        public int CountryId { get; set; }

        //The type of query that is expected to be executed
        public QueryTypeEnum QueryType { get; set; }

        //The number of teetime spots - ONLY FOR TeeTime.Mobile
        public int NumberPlayers { get; set; }


        //the types of queries available - note not all options are being used when executing the queries as this enum was added later.
        [Flags]
        public enum QueryTypeEnum
        {
            NotSet = 0,  //including not set so as to not break existing code as this is the default value when not set.
            Rcn = 1,
            Global = 2,
            Public = 4,
            Club = 8,
            Group = 16

        }
    }
}

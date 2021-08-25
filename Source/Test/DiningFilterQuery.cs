using System;


namespace CHO.Next.Global
{
    /// <summary>
    /// Defines the option for which day period to return (usually used for Tee Times)
    /// </summary>
    public enum TimeOfDay
    {
        /// <summary>
        /// All Bookings
        /// </summary>
        AllDay = 0,
        /// <summary>
        /// From First booking until 12 AM
        /// </summary>
        Morning = 1,
        /// <summary>
        /// From 11 AM until 5 PM
        /// </summary>
        Afternoon = 2,
        /// <summary>
        /// FROM 4 PM until last booking.
        /// </summary>
        Evening = 3
    }


    [Serializable]
    public class DiningFilterQuery
    {
        private DateTime _date;
        
        //Current selected Date
        public DateTime Date 
        {
            get { if (_date == DateTime.MinValue) return Convert.ToDateTime("1/1/1900");
                return _date;}
            set { _date = value; }
        }

        //Facility Id
        public int? Room { get; set; }

        //The number of people to reserve for
        public int Guests { get; set; }

        //The time of the day
        public TimeOfDay TimeOfDay { get; set; }

        public bool Complete { get; set; }
    }
}

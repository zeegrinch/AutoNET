using System;
using System.Collections.Generic;
using CHO.Next.Global.Enumerations;

namespace CHO.Next.Global.Queries
{
    /// <summary>
    /// Query used to specify filtering for a specific event
    /// </summary>
    [Serializable]
    public class EventFilterQuery:BaseQuery
    {
        
        /// <summary>
        /// Constructor
        /// </summary>
        public EventFilterQuery()
        {
           EventGenderRestriction = new List<Gender>();
        }
        /// <summary>
        /// Company the filter is running for.
        /// </summary>
        public int? CompanyId { get; set; }
        
        /// <summary>
        /// The name of the company.  This is used in a contains search
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Start date
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Whether the event is registerable or not (ever)
        /// </summary>
        public bool? IsRegisterable { get; set; }

        /// <summary>
        /// The category to search for. 
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// The member of the person doing this search
        /// </summary>
        public int? MemberId { get; set; }

        /// <summary>
        /// Indicates whether to return events that are available to the member.
        /// </summary>
        public bool? Availability { get; set; }

        /// <summary>
        /// Indicates whether to return events that are to be shown in Calendar at a Glance.
        /// </summary>
        public bool? ShowInCalendarAtGlance { get; set; }

        /// <summary>
        /// Search for events that allow or don't allow guests.
        /// </summary>
        public bool? AllowGuests { get; set; }


        /// <summary>
        /// Return event times that I've booked for i have reservations for.
        /// </summary>
        public bool? MyBookings { get; set; }


        /// <summary>
        /// Return hidden events.
        /// </summary>
        public bool? HiddenEvents { get; set; }

        /// <summary>
        /// Specific search for underaged
        /// </summary>
        public bool? AllowUnderageSpecific { get; set; }

        /// <summary>
        /// Return admin events.
        /// </summary>
        public bool? HideAdminEvents { get; set; }
        

        /// <summary>
        /// Sets/Gets a date range
        /// </summary>
        public DateRange Range { get; set; }

        #region BL to DA specific
        /// <summary>
        /// Search for events that allow underage or not.  This are filled in based on Availability setting.
        /// </summary>
        public bool? AllowUnderage { get; set; }

        /// <summary>
        /// List of gender restrictions to search for.  This are filled in based on Availability setting.
        /// </summary>
        public List<Gender> EventGenderRestriction { get; set; }

        /// <summary>
        /// If false then search for events that are not full.
        /// </summary>
        public bool? EventIsFull { get; set; }
        #endregion BL to DA specific

        /// <summary>
        /// Sets the filter date range based on range and timezoneId.
        /// </summary>
        public void SetDateRange(Enumerations.DateRange range, string TimeZoneId)
        {
            DateTime today = AppDateTime.GetTimeZoneToday(TimeZoneId);
            switch (range)
            {
                case Enumerations.DateRange.NoRange:
                    StartDate = null;
                    EndDate = null;
                    break;
                case Enumerations.DateRange.Next30Days:
                    StartDate = today;
                    EndDate = today.AddDays(30);
                    break;
                case Enumerations.DateRange.StartingToday:
                    StartDate = today;
                    EndDate = null;
                    break;
                case Enumerations.DateRange.ThisWeekend:
                    StartDate = AppDateTime.GetLastDayOfWeek(today);
                    EndDate = StartDate.Value.AddDays(1);
                    break;
                case Enumerations.DateRange.NextWeekend:
                    StartDate = AppDateTime.GetLastDayOfWeek(today).AddDays(7);
                    EndDate = StartDate.Value.AddDays(1);
                    break;
                case Enumerations.DateRange.ThisWeek:
                    StartDate = AppDateTime.GetFirstDayOfWeek(today);
                    EndDate = AppDateTime.GetLastDayOfWeek(today);
                    break;
                case Enumerations.DateRange.NextWeek:
                    StartDate = AppDateTime.GetFirstDayOfWeek(today).AddDays(7);
                    EndDate = AppDateTime.GetLastDayOfWeek(today).AddDays(7);
                    break;
                case Enumerations.DateRange.ThisMonth:
                    StartDate = AppDateTime.GetFirstDayOfMonth(today);
                    EndDate = AppDateTime.GetLastDayOfMonth(today);
                    break;
                case Enumerations.DateRange.NextMonth:
                    StartDate = AppDateTime.GetNextMonth(today);
                    EndDate = AppDateTime.GetLastDayOfMonth(AppDateTime.GetNextMonth(today));
                    break;
                case Enumerations.DateRange.NextSeason:
                    StartDate = AppDateTime.GetFirstDayOfNextQuarter(today);
                    EndDate = AppDateTime.GetLastDayOfNextQuarter(today);
                    break;
                case Enumerations.DateRange.ThisYear:
                    StartDate = AppDateTime.GetFirstDayOfYear(today);
                    EndDate = AppDateTime.GetLastDayOfYear(today);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("range");
            }
            if (EndDate.HasValue)
                EndDate = AppDateTime.GetEndofDay(EndDate);
        }

        /// <summary>
        /// Clone the filterquery and copy the genders.
        /// </summary>        
        public override object Clone()
        {
            EventFilterQuery o = (EventFilterQuery)this.MemberwiseClone();
            o.EventGenderRestriction= new List<Gender>();
            foreach (var gender in EventGenderRestriction)
            {
                o.EventGenderRestriction.Add(gender);
            }
            return o;
        }
        

    }
}

using System;

namespace CHO.Next.Global.Queries
{
    /// <summary>
    /// Query used to specify filtering for a specific match
    /// This is container ONLY. 
    /// All property values should be modified on business layer
    /// </summary>
    [Serializable]
    public class MatchPlayFilterQuery
    {
        /// <summary>
        /// Filter status
        /// </summary>
        public enum eStatus
        {
            None,
            Published,
            Unpublished,
            Deleted
        }
        /// <summary>
        /// Filter period of time
        /// </summary>
        public enum ePeriod
        {
            None,
            Upcoming,
            Past
        }

        //by Company ID 
        public int CompanyId { get; set; }

        //by Mathch Name
        public string MatchName { get; set; }

        //by match start date
        public DateTime? StartDate { get; set; }

        //by match end date
        public DateTime? EndDate { get; set; }

        //if the match is deleted
        public bool? IsDeleted { get; set; }

        //Date of publish start date
        public DateTime? PublishedStartDate { get; set; }
        
        //Date of publish end date
        public DateTime? PublishedEndDate { get; set; }

        //by period of time
        //Should accept values such as string/number presentation of ePeriod
        public ePeriod period;
        public string Period 
        {
            set { period = value.ConvertTo(ePeriod.None); }
            get { return period.ToString(); }
        } 

        //my match status 
        //Should accept values such as string/number presentation of eStatus
        public eStatus status;
        public string Status
        {
            set { status = value.ConvertTo(eStatus.None); }
            get { return status.ToString(); }
        } 
        
    }
}

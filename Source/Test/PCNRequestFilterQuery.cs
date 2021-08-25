using System;


namespace CHO.Next.Global
{

    public enum RequestStatus
    {
        Unknown,
        InitialRequest,
        Pending,
        Rejected,
        Approved
    };

    [Serializable]
    public class PCNRequestFilterQuery
    {
        private string _identifier = "1F48F456-706B-44D5-8539-1BCD87DD4469";

        //only a GET-er
        public string ID
        {
            get { return _identifier;  }
        }

        /// <summary>
        /// Search from date
        /// </summary>
        public DateTime? FromDate { get; set; }
        /// <summary>
        /// Search to date
        /// </summary>
        public DateTime? ToDate { get; set; }
        /// <summary>
        /// The id of the pcn club that is being requested
        /// </summary>
        public int? RequestedPCNClub { get; set; }
        /// <summary>
        /// The id of the pcn club that the requesting member belongs to.
        /// </summary>
        public int? MemberPCNClub { get; set; }
        /// <summary>
        /// The status
        /// </summary>
        public RequestStatus Status { get; set; }
        /// <summary>
        /// The member number of the requesting member.
        /// </summary>
        public string MemberNumber { get; set; }

        /// <summary>
        /// Override to string from the query.
        /// </summary>
        public override string ToString()
        {
            return
                string.Format(
                    "PCNQuery with From Date:{0}, ToDate:{1}, RequestedPCNClubId:{2}, MemberPCNClubId:{3}, Status:{4}, MemberNumber: {5}.",
                    FromDate.HasValue ? FromDate.Value.ToShortDateString() : "-empty-",
                    ToDate.HasValue ? ToDate.Value.ToShortDateString() : "-empty-", 
                    RequestedPCNClub.HasValue ? RequestedPCNClub.Value.ToString() : "-empty-", 
                    MemberPCNClub.HasValue ? MemberPCNClub.Value.ToString() : "-empty-", 
                    Status.ToString(), string.IsNullOrEmpty(MemberNumber)?"-empty-":MemberNumber );
        }

    }
}

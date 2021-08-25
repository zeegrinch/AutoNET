using System;

namespace CHO.Next.Global.Queries
{

    /// <summary>
    /// Base class
    /// </summary>
    [Serializable]
    public class BaseQuery:ICloneable
    {
        /// <summary>
        /// Clone object
        /// </summary>
        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }
        /// <summary>
        /// Maximum rows to return (for paging)
        /// </summary>
        public int MaxRows { get; set; }

        /// <summary>
        /// Current record to start from (for paging)
        /// </summary>
        public int CurrentIndex { get; set; }
    }
}

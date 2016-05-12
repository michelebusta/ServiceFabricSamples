using System;
using System.Runtime.Serialization;

namespace Shared.Models
{
    public enum TransactionTypes
    {
        Purchase,
        Cancellation
    }

    [Serializable]
    [DataContract]
    public class EntityTransaction
    {
        [DataMember]
        public DateTime TransactionDate { get; set; }
        [DataMember]
        public TransactionTypes TransactionType { get; set; }
        [DataMember]
        public EntityTypes EntityType { get; set; }
        [DataMember]
        public int BusinessKey { get; set; }
        [DataMember]
        public int SoldItems { get; set; }
        [DataMember]
        public double Revenue { get; set; }
        [DataMember]
        public double Tax { get; set; }
        [DataMember]
        public double Shipping { get; set; }
    }
}

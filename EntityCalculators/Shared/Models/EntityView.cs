using System.Runtime.Serialization;

namespace Shared.Models
{
    [DataContract]
    public class EntityView
    {
        [DataMember]
        public EntityTypes Type { get; set; }
        [DataMember]
        public int BusinessKey { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Purchases { get; set; }
        [DataMember]
        public int Cancellations { get; set; }
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

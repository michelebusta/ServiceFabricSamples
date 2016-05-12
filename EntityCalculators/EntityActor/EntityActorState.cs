using Shared.Models;
using System.Runtime.Serialization;

namespace EntityActor
{
    /// <summary>
    /// This class contains each actor's replicated state.
    /// Each instance of this class is serialized and replicated every time an actor's state is saved.
    /// For more information, see http://aka.ms/servicefabricactorsstateserialization
    /// </summary>
    [DataContract]
    public class EntityActorState
    {
        // A argument-less constructor is needed 
        public EntityActorState()
        {
        }

        [DataMember]
        public Entity Entity { get; set; }

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

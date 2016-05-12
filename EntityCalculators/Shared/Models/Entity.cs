using System;
using System.Runtime.Serialization;

namespace Shared.Models
{
    public enum EntityTypes
    {
        Global,
        Region,
        Country,
        SalesOffice,
        Unknown
    }

    [DataContract]
    public class Entity : IComparable<Entity>, IEquatable<Entity>
    {
        public Entity()
        {
            Type = EntityTypes.Unknown;
            BusinessKey = -1;
            Name = "";
            Parent = null;
        }

        public Entity(EntityTypes type, int key)
        {
            Type = type;
            BusinessKey = key;
            Name = "";
            Parent = null;
        }

        [DataMember]
        public EntityTypes Type { get; set; }

        [DataMember]
        public int BusinessKey { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Entity Parent{ get; set; }

        // This requires an explanation:
        // I am making the actor id the combination of both the entity type and the business key
        // so that when an actor starts from no state (i.e. the first time), it know how to initialize
        // itself with proper state. It uses the entity type and business key to load the Entity from the 
        // OLTP connector 
        public string GetPartitionKey()
        {
            return (int)Type + "|" + BusinessKey;
        }

        // No longer use
        public long GetPartitionKey(bool ignore)
        {
            var bytes = System.Text.Encoding.Unicode.GetBytes("" + (int)Type + BusinessKey);
            return (long)BitConverter.ToInt64(bytes, 0);
        }

        public bool Equals(Entity other)
        {
            return (
                this.Type.Equals(other.Type) &&
                this.BusinessKey == other.BusinessKey
                );
        }

        public int CompareTo(Entity other)
        {
            if ((int)this.Type > (int)other.Type)
            {
                return 1;
            }

            if ((int)this.Type < (int)other.Type)
            {
                return -1;
            }

            if ((int)this.Type == (int)other.Type)
            {
                return 0;
            }

            return 0;
        }
    }
}

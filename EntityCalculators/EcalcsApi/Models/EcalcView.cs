using System.Collections.Generic;

namespace EcalcsApi.Models
{
    public class EcalcsView
    {
        public EcalcsView(HierarchyView hView)
        {
            ThisView = new EntityView();
            ChildrenViews = new List<EntityView>();

            if (hView != null)
            {
                ThisView = hView.ThisView;
                ChildrenViews = new List<EntityView>();
                foreach (var item in hView.ChildrenViews)
                {
                    ChildrenViews.Add(item.Value);
                }
            }
        }

        public EntityView ThisView { get; set; }
        public List<EntityView> ChildrenViews { get; set; }
    }

    public class HierarchyView
    {
        public string ParentName { get; set; }
        public EntityView ParentView { get; set; }
        public EntityView ThisView { get; set; }
        public Dictionary<string, EntityView> ChildrenViews { get; set; }
    }

    public class EntityView
    {
        public int Type { get; set; }
        public int BusinessKey { get; set; }
        public string Name { get; set; }
        public int Purchases { get; set; }
        public int Cancellations { get; set; }
        public int SoldItems { get; set; }
        public double Revenue { get; set; }
        public double Tax { get; set; }
        public double Shipping { get; set; }
    }

    public enum EntityTypes
    {
        Global,
        Region,
        Country,
        SalesOffice,
        Unknown
    }

    public class Entity 
    {
        public Entity()
        {
            Type = EntityTypes.Unknown;
            BusinessKey = -1;
            Name = "";
        }

        public Entity(EntityTypes type, int key, string name)
        {
            Type = type;
            BusinessKey = key;
            Name = name;
        }

        public EntityTypes Type { get; set; }

        public int BusinessKey { get; set; }

        public string Name { get; set; }
    }
}
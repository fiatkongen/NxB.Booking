using System;
using Newtonsoft.Json;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class AutoAddState : ITenantEntity
    {
        public Guid Id { get; set; } 
        public Guid TenantId { get; set; }
        public string Json { get; set; }
        public int Version { get; set; }

        private AutoAddState() { }

        public AutoAddState(Guid tenantId, AutoAdd autoAdd, int version) : this(autoAdd.Id, tenantId, Serialize(autoAdd), version)
        {}

        public AutoAddState(Guid id, Guid tenantId, string json, int version)
        {
            Id = id;
            TenantId = tenantId;
            Version = version;

            if (json != null)
            {
                Json = json;
            }
        }

        public AutoAdd GetModel()
        {
            return Deserialize(this.Json);
        }

        public void SetModel(AutoAdd autoAdd)
        {
            this.Json = Serialize(autoAdd);
            this.Id = autoAdd.Id;
        }

        public static AutoAdd Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            return JsonConvert.DeserializeObject<AutoAdd>(json);
        }

        public static string Serialize(AutoAdd autoAdd)
        {
            if (autoAdd == null) return null;
            return JsonConvert.SerializeObject(autoAdd);
        }
    }


    [Serializable]
    public class AutoAdd 
    {
        public Guid Id { get; set; }
        public AutoCondition Condition { get; set; }
        public AutoAction Action { get; set; }
        public int ExecutionStrategy { get; set; }
        public bool IsDeleted { get; set; }

        private AutoAdd() { }

        public AutoAdd(Guid id)
        {
            Id = id;
        }
    }

    [Serializable]
    public class AutoCondition
    {
        public int ConditionTrigger { get; set; }
        public Guid TriggerId { get; set; }
    }

    [Serializable]
    public class AutoAction
    {
        public int ActionType { get; set; }
        public Guid ActionId { get; set; }
        public string Parameter { get; set; }
    }
}

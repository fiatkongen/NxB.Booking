using System;

namespace NxB.Dto.OrderingApi
{
    public class CreateAutoAddDto
    {
        public AutoConditionDto Condition { get; set; }
        public AutoActionDto Action { get; set; }
        public int ExecutionStrategy { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class AutoAddDto : CreateAutoAddDto
    {
        public Guid Id { get; set; }
    }

    public class AutoConditionDto
    {
        public int ConditionTrigger { get; set; }
        public Guid TriggerId { get; set; }
    }

    public class AutoActionDto
    {
        public int ActionType { get; set; }
        public Guid ActionId { get; set; }
        public string Parameter { get; set; }
    }
}

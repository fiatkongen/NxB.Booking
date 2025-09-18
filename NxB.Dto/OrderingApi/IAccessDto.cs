using System;

namespace NxB.Dto.OrderingApi
{
    public interface IAccessDto
    {
        uint? Code { get; set; }
        bool IsKeyCode { get; set; }
        Guid SubOrderId { get; set; }
    }
}
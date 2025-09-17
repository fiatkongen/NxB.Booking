using NxB.Domain.Common.Dto;

public interface IKeyCodeGenerator
{
    Task<int> Next(ClaimsProviderDto overrideClaimsProviderDto);
}
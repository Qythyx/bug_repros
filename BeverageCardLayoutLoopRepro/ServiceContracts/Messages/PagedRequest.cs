
namespace Beerbox.Service.Contracts.Messages;

public record PagedRequest(int Page, int PageSize) : IMessage;

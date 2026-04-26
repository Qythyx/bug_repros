
namespace Beerbox.Service.Contracts.Messages;

public record PagedResponse<T>(IEnumerable<T> Items, int Total) : IMessage;

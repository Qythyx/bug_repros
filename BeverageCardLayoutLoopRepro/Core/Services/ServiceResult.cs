using System.Net;

namespace Beerbox.App.Core.Services;

public record ServiceResult(HttpStatusCode Status)
{
	public bool IsSuccessStatusCode => StatusCodeIsSuccess(Status);

	public static bool StatusCodeIsSuccess(HttpStatusCode status) => ((int)status >= 200) && ((int)status <= 299);
}

public record ServiceResult<T> : ServiceResult
{
	public T Document { get; init; }

	/// <summary>
	/// Creates a new <see cref="ServiceResult{T}"/> with a <see cref="HttpStatusCode.OK"/> status.
	/// </summary>
	/// <param name="document">The document.</param>
	public ServiceResult(T document)
		: base(HttpStatusCode.OK) => Document = document;

	/// <summary>Creates a new <see cref="ServiceResult{T}"/> with no document.</summary>
	/// <param name="status">The status</param>
	public ServiceResult(HttpStatusCode status)
		: base(status) => Document = default!;
}

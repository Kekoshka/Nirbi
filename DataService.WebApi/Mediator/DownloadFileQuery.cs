using DataService.WebApi.Interfaces;
using MediatR;

namespace DataService.WebApi.Mediator;

public sealed record DownloadFileQuery(Guid FileId) : IRequest<FileDownloadResult?>;

public sealed class DownloadFileQueryHandler : IRequestHandler<DownloadFileQuery, FileDownloadResult?>
{
    private readonly IDataObjectService _data;

    public DownloadFileQueryHandler(IDataObjectService data) => _data = data;

    public Task<FileDownloadResult?> Handle(DownloadFileQuery request, CancellationToken cancellationToken) =>
        _data.OpenReadAsync(request.FileId, cancellationToken);
}

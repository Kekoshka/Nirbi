using DataService.DataAccess.Postgres.Models;
using DataService.WebApi.Common.DTO;

namespace DataService.WebApi.Common
{
    public static class StoredFileMapper
    {
        public static List<FileMetadataDto> ToDto(this IEnumerable<StoredFile> f) =>
            f.Select(f =>  new FileMetadataDto(f.Id, f.FileCollectionId, f.SortOrder, f.ContentType, f.SizeBytes, f.OriginalFileName, f.CreatedAtUtc)).ToList();
        public static FileMetadataDto ToDto(this StoredFile f) =>
            new FileMetadataDto(f.Id, f.FileCollectionId, f.SortOrder, f.ContentType, f.SizeBytes, f.OriginalFileName, f.CreatedAtUtc);

    }
}

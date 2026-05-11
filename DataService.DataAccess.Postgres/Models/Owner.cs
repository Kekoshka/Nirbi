namespace DataService.DataAccess.Postgres.Models
{
    public class Owner
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public ICollection<StoredFile> StoredFiles { get; set; }
        public ICollection<FileCollection> FileCollections { get; set; }

    }
}

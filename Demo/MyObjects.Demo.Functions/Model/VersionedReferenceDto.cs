namespace MyObjects.Demo.Functions.Model;

public class VersionedReferenceDto<T> where T : IEntity
{
    public int Id { get; set; }
    public int Version { get; set; }

    public static implicit operator VersionedReference<T>(VersionedReferenceDto<T> dto)
    {
        return new VersionedReference<T>(dto.Id, dto.Version);
    }
}

public static class VersionReferenceExtensions
{
    public static VersionedReferenceDto<T> ToDto<T>(this VersionedReference<T> @ref) where T : IEntity
    {
        return new VersionedReferenceDto<T>
        {
            Id = @ref.Id,
            Version = @ref.Version
        };
    }
}
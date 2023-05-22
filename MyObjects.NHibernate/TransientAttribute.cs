using NHibernate;
using NHibernate.Mapping.ByCode;

namespace MyObjects.NHibernate;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class TransientAttribute : Attribute
{
}

public abstract class FetchAttribute : Attribute
{
    public abstract CollectionFetchMode Mode { get; }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class FetchWithSubselectAttribute : FetchAttribute
{
    public override CollectionFetchMode Mode => CollectionFetchMode.Subselect;
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class FetchWithJoinAttribute : FetchAttribute
{
    public override CollectionFetchMode Mode => CollectionFetchMode.Join;
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class BatchAttribute : Attribute
{
    public int Size { get; }

    public BatchAttribute(int size)
    {
        this.Size = size;
    }
}
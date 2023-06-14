using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace MyObjects.Demo.Functions;

public class ProjectedQuery<T, TDto>
{
    private readonly Mapper mapper;
    private readonly IReadonlySession session;

    public ProjectedQuery(IReadonlySession session, Mapper mapper)
    {
        this.session = session;
        this.mapper = mapper;
    }

    public IQueryable<TDto> Run(int? skip = null, int? take = null)
    {
        var query = this.session.Query<T>();
        if (skip.HasValue)
            query = query.Skip(skip.Value);
        query = query.Take(take ?? 1000);
        return query.ProjectTo<TDto>(this.mapper.ConfigurationProvider);
    }
}
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

    public IQueryable<TDto> Run()
    {
        return this.session.Query<T>().ProjectTo<TDto>(this.mapper.ConfigurationProvider);
    }
}
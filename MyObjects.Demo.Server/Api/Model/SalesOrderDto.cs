using System.Collections.Generic;

namespace MyObjects.Demo.Api.Model
{
    public class SalesOrderDto
    {
        public IEnumerable<SalesOrderLineDto> Lines { get; set; }
    }
}
using MyObjects.Demo.Model.Orders.Commands;

namespace MyObjects.Demo.Functions.Infrastructure;

public class NumberSequence : INumberSequence
{
    private int i = 1;
    public int Next()
    {
        return i++;
    }
}
using Xunit;
using System.Collections.Generic;
using System.Linq;



public class ClaimLogicTests
{
    [Fact]
    public void CalculateTotalAmount_ShouldReturnCorrectTotal()
    {
        var hourlyRate = 150.00m;
        var details = new List<ClaimDetailViewModel>
        {
            new ClaimDetailViewModel { HoursWorked = 5.0m },
            new ClaimDetailViewModel { HoursWorked = 3.5m }
        };

        var totalHours = details.Sum(d => d.HoursWorked);
        var totalAmount = totalHours * hourlyRate;

        Assert.Equal(8.5m, totalHours);
        Assert.Equal(1275.00m, totalAmount);
    }
}
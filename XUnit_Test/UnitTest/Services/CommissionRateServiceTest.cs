using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.ServiceContracts;
using Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace XUnit_Test.UnitTest.Services;

public class CommissionRateServiceTest
{
    // private readonly ICommissionRateService _commissionRateServiceInterface;
    private readonly CommissionRateService _commissionRateService;
    
    private readonly Mock<ICommissionRateRepository> _commissionRateRepositoryMock;
    private readonly Mock<IExchangeValueService> _exchangeValueServiceMock;
    private readonly IMemoryCache _memoryCache;

    public CommissionRateServiceTest()
    {
        _commissionRateRepositoryMock = new Mock<ICommissionRateRepository>();
        _exchangeValueServiceMock = new Mock<IExchangeValueService>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        
        _commissionRateService = new CommissionRateService(_commissionRateRepositoryMock.Object, _exchangeValueServiceMock.Object, _memoryCache);
    }
    
    #region GetCRateByUSDAmount

    [Fact]
    public async Task GetCRateByUSDAmount_ShouldReturnNull_WhenCommissionRatesListIsEmpty()
    {
        // Arrange
        decimal amount = Decimal.Zero;
        var commissionRatesList = new List<CommissionRate>();    
        
        // Mocking the Required Methods
        _commissionRateRepositoryMock.Setup(e => e.GetAllCommissionRatesAsync())
                                     .ReturnsAsync(commissionRatesList);  
        
        // Act
        var cRate = await _commissionRateService.GetCRateByUSDAmount(amount);
        
        // Assert
        cRate.Should().BeNull();
    }
    
    
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(99)]
    [InlineData(100)]
    public async Task GetCRateByUSDAmount_ShouldReturnCorrectCRate_WhenCommissionRatesListHasOneObject(decimal amount)
    {
        // Arrange
        var commissionRatesList = new List<CommissionRate>()
        {
            new() {Id = 1, CRate = 0.3m, MaxUSDRange = 100}
        };    
        
        // Mocking the Required Methods
        _commissionRateRepositoryMock.Setup(e => e.GetAllCommissionRatesAsync())
                                     .ReturnsAsync(commissionRatesList);  
        
        // Act
        var cRate = await _commissionRateService.GetCRateByUSDAmount(amount);
        
        // Assert
        var expectedCRate = commissionRatesList[0].CRate;
        cRate.Should().Be(expectedCRate);
    }
    
    
    [Fact]
    public async Task GetCRateByUSDAmount_ShouldReturnNull_WhenInputAmountIsOutsideAndAboveMaxRange()
    {
        // Arrange
        decimal amount = 501;
        var commissionRatesList = new List<CommissionRate>()
        {
            new() {Id = 1, CRate = 0.3m, MaxUSDRange = 100},
            new() {Id = 2, CRate = 0.1m, MaxUSDRange = 500},
            new() {Id = 3, CRate = 0.4m, MaxUSDRange = 200}
        };    
        
        // Mocking the Required Methods
        _commissionRateRepositoryMock.Setup(e => e.GetAllCommissionRatesAsync())
            .ReturnsAsync(commissionRatesList);  
        
        // Act
        var cRate = await _commissionRateService.GetCRateByUSDAmount(amount);
        
        // Assert
        cRate.Should().BeNull();
    }  
    
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(99)]
    public async Task GetCRateByUSDAmount_ShouldReturnSmallestRangeCRate_WhenInputAmountIsOutsideAndUnderMinRange(decimal amount)
    {
        // Arrange
        var commissionRatesList = new List<CommissionRate>()
        {
            new() {Id = 1, CRate = 0.3m, MaxUSDRange = 100},
            new() {Id = 2, CRate = 0.1m, MaxUSDRange = 500},
            new() {Id = 3, CRate = 0.4m, MaxUSDRange = 200}
        };    
        
        // Mocking the Required Methods
        _commissionRateRepositoryMock.Setup(e => e.GetAllCommissionRatesAsync())
                                     .ReturnsAsync(commissionRatesList);  
        
        // Act
        var cRate = await _commissionRateService.GetCRateByUSDAmount(amount);
        
        // Assert
        var expectedCRate = commissionRatesList.OrderBy(c => c.MaxUSDRange).First().CRate;
        cRate.Should().Be(expectedCRate);    
    }
    
    
    [Theory]
    [InlineData(100)]
    [InlineData(200)]
    [InlineData(500)]
    public async Task GetCRateByUSDAmount_ShouldReturnCorrectCRate_WhenInputAmountIsExactlyFound(decimal amount)
    {
        // Arrange
        var commissionRatesList = new List<CommissionRate>()
        {
            new() {Id = 1, CRate = 0.3m, MaxUSDRange = 100},
            new() {Id = 2, CRate = 0.1m, MaxUSDRange = 500},
            new() {Id = 3, CRate = 0.4m, MaxUSDRange = 200}
        };    
        
        // Mocking the Required Methods
        _commissionRateRepositoryMock.Setup(e => e.GetAllCommissionRatesAsync())
            .ReturnsAsync(commissionRatesList);  
        
        // Act
        var cRate = await _commissionRateService.GetCRateByUSDAmount(amount);
        
        // Assert
        var expectedCRate = commissionRatesList.First(c => c.MaxUSDRange == amount).CRate;
        cRate.Should().Be(expectedCRate);    
    }  

    
    [Theory]
    [InlineData(99, 0.3)]
    [InlineData(150, 0.2)]
    [InlineData(200, 0.2)]
    [InlineData(450, 0.1)]    
    [InlineData(501, 0.4)]    
    public async Task GetCRateByUSDAmount_ShouldReturnCorrectCRate_WhenCommissionRatesListIsNotOrdered(decimal amount, decimal expectedCRate)
    {
        // Arrange
        var commissionRatesList = new List<CommissionRate>()
        {
            new() {Id = 1, CRate = 0.3m, MaxUSDRange = 100},
            new() {Id = 2, CRate = 0.1m, MaxUSDRange = 500},
            new() {Id = 4, CRate = 0.4m, MaxUSDRange = 800},
            new() {Id = 3, CRate = 0.2m, MaxUSDRange = 200}
        };    
        
        // Mocking the Required Methods
        _commissionRateRepositoryMock.Setup(e => e.GetAllCommissionRatesAsync())
            .ReturnsAsync(commissionRatesList);  
        
        // Act
        var cRate = await _commissionRateService.GetCRateByUSDAmount(amount);
        
        // Assert
        cRate.Should().Be(expectedCRate);    
    }  
    
    
    [Theory]
    [InlineData(-5, 0.666)]
    [InlineData(0.00001, 0.666)]
    [InlineData(0, 0.666)]
    [InlineData(99.99999, 0.666)]
    [InlineData(150, 0.2001)]
    [InlineData(200, 0.2001)]
    [InlineData(200.00000000, 0.2001)]
    [InlineData(200.00000001, 0.982)]
    [InlineData(450, 0.982)]    
    [InlineData(501.040204, 0.345689123)]    
    [InlineData(799.999999999, 0.00001)]    
    public async Task GetCRateByUSDAmount_ShouldReturnCorrectCRate_WhenThereAreUnorderedVariousCRatesAndRangesInList(decimal amount, decimal expectedCRate)
    {
        // Arrange
        var commissionRatesList = new List<CommissionRate>()
        {
            new() {Id = 1, CRate = 0.666m, MaxUSDRange = 100},
            new() {Id = 2, CRate = 0.345689123m, MaxUSDRange = 600},
            new() {Id = 3, CRate = 0.982m, MaxUSDRange = 500},
            new() {Id = 4, CRate = 0.2001m, MaxUSDRange = 500},
            new() {Id = 5, CRate = 0.00001m, MaxUSDRange = 800}
        };    
        
        // Mocking the Required Methods
        _commissionRateRepositoryMock.Setup(e => e.GetAllCommissionRatesAsync())
            .ReturnsAsync(commissionRatesList);  
        
        // Act
        var cRate = await _commissionRateService.GetCRateByUSDAmount(amount);
        
        // Assert
        cRate.Should().Be(expectedCRate);    
    }  
    
    #endregion
}
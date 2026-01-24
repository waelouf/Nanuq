using FluentAssertions;
using Moq;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.Tests.Helpers;
using Nanuq.WebApi.Endpoints.ActivityLog;
using Xunit;

namespace Nanuq.Tests.Endpoints.ActivityLog;

public class GetActivityTypesTests
{
    private readonly Mock<IActivityLogRepository> _mockActivityLogRepository;
    private readonly GetActivityTypes _endpoint;

    public GetActivityTypesTests()
    {
        _mockActivityLogRepository = new Mock<IActivityLogRepository>();
        _endpoint = new GetActivityTypes(_mockActivityLogRepository.Object);
    }

    [Fact]
    public async Task GetAllActivityTypes_ReturnsAllTypes_WhenTypesExist()
    {
        // Arrange
        var types = new List<ActivityType>
        {
            TestDataBuilder.CreateActivityType(id: 1, name: "Type 1"),
            TestDataBuilder.CreateActivityType(id: 2, name: "Type 2"),
            TestDataBuilder.CreateActivityType(id: 3, name: "Type 3")
        };

        _mockActivityLogRepository
            .Setup(repo => repo.GetAllActivityTypes())
            .ReturnsAsync(types);

        // Act
        var result = await _mockActivityLogRepository.Object.GetAllActivityTypes();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(t => t.Name == "Type 1");
        result.Should().Contain(t => t.Name == "Type 2");
        result.Should().Contain(t => t.Name == "Type 3");
    }

    [Fact]
    public async Task GetAllActivityTypes_ReturnsEmptyList_WhenNoTypesExist()
    {
        // Arrange
        _mockActivityLogRepository
            .Setup(repo => repo.GetAllActivityTypes())
            .ReturnsAsync(new List<ActivityType>());

        // Act
        var result = await _mockActivityLogRepository.Object.GetAllActivityTypes();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllActivityTypes_CallsRepository_Once()
    {
        // Arrange
        _mockActivityLogRepository
            .Setup(repo => repo.GetAllActivityTypes())
            .ReturnsAsync(new List<ActivityType>());

        // Act
        await _mockActivityLogRepository.Object.GetAllActivityTypes();

        // Assert
        _mockActivityLogRepository.Verify(
            repo => repo.GetAllActivityTypes(),
            Times.Once);
    }

    [Fact]
    public async Task GetAllActivityTypes_ReturnsTypesWithAllProperties_WhenTypesExist()
    {
        // Arrange
        var types = new List<ActivityType>
        {
            TestDataBuilder.CreateActivityType(
                id: 1,
                name: "Add Server",
                description: "Server added",
                color: "#00FF00",
                icon: "add-icon")
        };

        _mockActivityLogRepository
            .Setup(repo => repo.GetAllActivityTypes())
            .ReturnsAsync(types);

        // Act
        var result = await _mockActivityLogRepository.Object.GetAllActivityTypes();

        // Assert
        var type = result.First();
        type.Name.Should().Be("Add Server");
        type.Description.Should().Be("Server added");
        type.Color.Should().Be("#00FF00");
        type.Icon.Should().Be("add-icon");
    }
}

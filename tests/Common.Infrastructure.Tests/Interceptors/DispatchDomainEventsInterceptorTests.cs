using Common.Infrastructure;
using Common.Infrastructure.Interceptors;
using Common.Mediator;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Common.Infrastructure.Tests.Interceptors;

public class DispatchDomainEventsInterceptorTests
{
    private class TestEvent : BaseEvent { }

    private class TestEntity : BaseEntity
    {
        public int Id { get; set; }
    }

    private class TestDbContext : DbContext
    {
        public DbSet<TestEntity> Entities { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
    }

    [Fact]
    public async Task DispatchDomainEvents_WithEvents_PublishesAndClears()
    {
        var publisher = new Mock<IPublisher>();
        var interceptor = new DispatchDomainEventsInterceptor(publisher.Object);

        using var context = new TestDbContext();
        var entity = new TestEntity { Id = 1 };
        var evt1 = new TestEvent();
        var evt2 = new TestEvent();
        entity.AddDomainEvent(evt1);
        entity.AddDomainEvent(evt2);
        context.Entities.Add(entity);

        await interceptor.DispatchDomainEvents(context);

        publisher.Verify(p => p.Publish(It.Is<BaseEvent>(e => e == evt1), It.IsAny<CancellationToken>()), Times.Once);
        publisher.Verify(p => p.Publish(It.Is<BaseEvent>(e => e == evt2), It.IsAny<CancellationToken>()), Times.Once);
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task DispatchDomainEvents_NoEvents_DoesNotPublish()
    {
        var publisher = new Mock<IPublisher>();
        var interceptor = new DispatchDomainEventsInterceptor(publisher.Object);

        using var context = new TestDbContext();
        var entity = new TestEntity { Id = 1 };
        context.Entities.Add(entity);

        await interceptor.DispatchDomainEvents(context);

        publisher.Verify(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DispatchDomainEvents_NullContext_DoesNotThrow()
    {
        var publisher = new Mock<IPublisher>();
        var interceptor = new DispatchDomainEventsInterceptor(publisher.Object);

        var act = () => interceptor.DispatchDomainEvents(null);
        await act.Should().NotThrowAsync();
    }
}

namespace StackerBot.Services;

public sealed class Repository(IDbContextFactory<DatabaseContext> factory, ILogger<Repository> logger) : IRepository {
  public ValueTask<Union<YouTubeSubscriptionModel, DatabaseError>> AddYouTubeSubscription(YouTubeSubscriptionModel model, CancellationToken cancellationToken) {
    return Executor(action, "add_youtube_subscription", cancellationToken);

    async Task<YouTubeSubscriptionModel> action(DatabaseContext context) {
      await context.YouTubeSubscriptions.AddAsync(model, cancellationToken);
      await context.SaveChangesAsync(cancellationToken);
      return model;
    }
  }

  public ValueTask<Union<YouTubeSubscriptionModel, NotFound, DatabaseError>> FindYouTubeSubscription(string channelName, CancellationToken cancellationToken) {
    return Executor(action, "find_youtube_subscription", cancellationToken);

    async Task<Union<YouTubeSubscriptionModel, NotFound>> action(DatabaseContext context) {
      var subscription = await context.YouTubeSubscriptions.FirstOrDefaultAsync(x => x.ChannelName == channelName, cancellationToken);

      if (subscription is null) {
        return NotFound.Instance;
      }

      return subscription;
    }
  }

  public ValueTask<Union<Success, NotFound, DatabaseError>> RemoveYouTubeSubscription(Guid id, CancellationToken cancellationToken) {
    return Executor(action, "remove_youtube_subscription", cancellationToken);

    async Task<Union<Success, NotFound>> action(DatabaseContext context) {
      var subscription = await context.YouTubeSubscriptions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

      if (subscription is null) {
        return NotFound.Instance;
      }

      context.YouTubeSubscriptions.Remove(subscription);
      await context.SaveChangesAsync(cancellationToken);
      return Success.Instance;
    }
  }

  public async ValueTask<Union<List<YouTubeSubscriptionModel>, DatabaseError>> GetYouTubeSubscriptions(CancellationToken cancellationToken) {
    return await Executor(action, "get_youtube_subscriptions", cancellationToken);

    async Task<List<YouTubeSubscriptionModel>> action(DatabaseContext context) {
      return await context.YouTubeSubscriptions.ToListAsync(cancellationToken);
    }
  }

  private async ValueTask<Union<T, DatabaseError>> Executor<T>(Func<DatabaseContext, Task<T>> action, string operation, CancellationToken cancellationToken) {
    return await Executor(wrappedAction, operation, cancellationToken);

    async Task<Union<T, None>> wrappedAction(DatabaseContext context) {
      return await action(context);
    }
  }

  private async ValueTask<Union<T1, T2, DatabaseError>> Executor<T1, T2>(Func<DatabaseContext, Task<Union<T1, T2>>> action, string operation, CancellationToken cancellationToken) {
    try {
      await using var context = await factory.CreateDbContextAsync(cancellationToken);
      return await action(context);
    } catch (Exception error) {
      logger.LogError(error, "An exception occurred on database operation [{Operation}]", operation);
      return DatabaseError.Instance;
    }
  }
}

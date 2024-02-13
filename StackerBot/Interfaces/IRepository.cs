namespace StackerBot.Interfaces;

public interface IRepository {
  ValueTask<Union<YouTubeSubscriptionModel, DatabaseError>> AddYouTubeSubscription(YouTubeSubscriptionModel model, CancellationToken cancellationToken);
  ValueTask<Union<YouTubeSubscriptionModel, NotFound, DatabaseError>> FindYouTubeSubscription(string channelName, CancellationToken cancellationToken);
  ValueTask<Union<Success, NotFound, DatabaseError>> RemoveYouTubeSubscription(Guid id, CancellationToken cancellationToken);
  ValueTask<Union<List<YouTubeSubscriptionModel>, DatabaseError>> GetYouTubeSubscriptions(CancellationToken cancellationToken);
}

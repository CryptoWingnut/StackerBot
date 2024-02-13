using Coravel.Invocable;

namespace StackerBot.Tasks;

public sealed class DailyYouTubeReSubscribe(IRepository repository, IExternals externals) : IInvocable {
  public async Task Invoke() {
    var channelsResults = await repository.GetYouTubeSubscriptions(CancellationToken.None);

    if (channelsResults.IsType(typeof(DatabaseError))) {
      return;
    }

    foreach (var channel in channelsResults.GetT1) {
      await externals.SubscribeToYouTubeChannel(channel.ChannelId);
    }
  }
}

namespace BrasilCompete.Worker.Domain;

public sealed record DiscardedEvent(SportEvent Event, DiscardReason Reason);

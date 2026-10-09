namespace BrasilCompete.Worker.History;

public sealed record HistoryUpdateResult(int New, int Changed, int Unchanged, int Missing);

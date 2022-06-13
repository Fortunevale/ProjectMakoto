namespace ProjectIchigo.Events;

internal class CollectionUpdates
{
    internal CollectionUpdates(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal NotifyCollectionChangedEventHandler CrosspostCollectionUpdated(KeyValuePair<ulong, Guilds.ServerSettings> b)
    {
        return (s, e) =>
        {
            _ = _bot._databaseClient.SyncDatabase();
        };
    }

    internal NotifyCollectionChangedEventHandler AuditLogCollectionUpdated(KeyValuePair<ulong, Guilds.ServerSettings> b)
    {
        return (s, e) =>
        {
            if (b.Value.ProcessedAuditLogs.Count > 50)
                b.Value.ProcessedAuditLogs.Remove(b.Value.ProcessedAuditLogs[0]);

            _ = _bot._databaseClient.SyncDatabase();
        };
    }
}

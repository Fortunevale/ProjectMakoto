﻿namespace Project_Ichigo.Objects;

internal class CrosspostSettings
{
    private int _DelayBeforePosting { get; set; } = 0;
    public int DelayBeforePosting { get => _DelayBeforePosting; set { _DelayBeforePosting = value; _ = Bot.DatabaseClient.SyncDatabase(); } }

    public ObservableCollection<ulong> CrosspostChannels { get; set; } = new();
}

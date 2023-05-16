// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public class Translations
{
    public Dictionary<string, int> Progress = new();

    
    public commands Commands;
    public class commands
    {
        public config Config;
        public class config
        {
            public guildLanguage GuildLanguage;
            public class guildLanguage
            {
                public SingleTranslationKey Selector;
                public SingleTranslationKey DisableOverride;
                public SingleTranslationKey Response;
                public SingleTranslationKey Disclaimer;
                public SingleTranslationKey Title;
            }

            public prefixConfigCommand PrefixConfigCommand;
            public class prefixConfigCommand
            {
                public SingleTranslationKey NewPrefix;
                public SingleTranslationKey NewPrefixModalTitle;
                public SingleTranslationKey ChangePrefix;
                public SingleTranslationKey TogglePrefixCommands;
                public SingleTranslationKey CurrentPrefix;
                public SingleTranslationKey PrefixDisabled;
                public SingleTranslationKey Title;
            }

            public autoCrosspost AutoCrosspost;
            public class autoCrosspost
            {
                public SingleTranslationKey NoCrosspostChannels;
                public SingleTranslationKey ChannelLimit;
                public SingleTranslationKey DurationLimit;
                public SingleTranslationKey RemoveChannelButton;
                public SingleTranslationKey AddChannelButton;
                public SingleTranslationKey ToggleExcludeBotsButton;
                public SingleTranslationKey SetDelayButton;
                public SingleTranslationKey DelayBeforePosting;
                public SingleTranslationKey ExcludeBots;
                public SingleTranslationKey Title;
            }

            public actionLog ActionLog;
            public class actionLog
            {
                public SingleTranslationKey NoOptions;
                public SingleTranslationKey OptionInaccurate;
                public SingleTranslationKey ChangeFilterButton;
                public SingleTranslationKey ChangeChannelButton;
                public SingleTranslationKey SetChannelButton;
                public SingleTranslationKey DisableActionLogButton;
                public SingleTranslationKey InviteModifications;
                public SingleTranslationKey VoiceChannelUpdates;
                public SingleTranslationKey ChannelModifications;
                public SingleTranslationKey ServerModifications;
                public SingleTranslationKey BanUpdates;
                public SingleTranslationKey RoleUpdates;
                public SingleTranslationKey MessageModifications;
                public SingleTranslationKey MessageDeletions;
                public SingleTranslationKey UserProfileUpdates;
                public SingleTranslationKey UserRoleUpdates;
                public SingleTranslationKey UserStateUpdates;
                public SingleTranslationKey AttemptGatheringMoreDetails;
                public SingleTranslationKey ActionLogChannel;
                public SingleTranslationKey ActionlogDisabled;
                public SingleTranslationKey Title;
            }

        }

        public moderation Moderation;
        public class moderation
        {
            public unban Unban;
            public class unban
            {
                public SingleTranslationKey Failed;
                public SingleTranslationKey Removed;
                public SingleTranslationKey Removing;
            }

            public timeout Timeout;
            public class timeout
            {
                public SingleTranslationKey Invalid;
                public SingleTranslationKey Failed;
                public SingleTranslationKey TimedOut;
                public SingleTranslationKey TimingOut;
                public SingleTranslationKey AuditLog;
            }

            public softban Softban;
            public class softban
            {
                public SingleTranslationKey Errored;
                public SingleTranslationKey Banned;
                public SingleTranslationKey Banning;
                public SingleTranslationKey AuditLog;
            }

            public removeTimeout RemoveTimeout;
            public class removeTimeout
            {
                public SingleTranslationKey Failed;
                public SingleTranslationKey Removed;
                public SingleTranslationKey Removing;
            }

            public purge Purge;
            public class purge
            {
                public SingleTranslationKey Failed;
                public SingleTranslationKey Deleted;
                public SingleTranslationKey NoMessages;
                public SingleTranslationKey Fetched;
                public SingleTranslationKey Fetching;
            }

            public poll Poll;
            public class poll
            {
                public SingleTranslationKey Votes;
                public SingleTranslationKey NoVotes;
                public SingleTranslationKey Results;
                public SingleTranslationKey PollEnded;
                public SingleTranslationKey NoPerms;
                public SingleTranslationKey VoteUpdated;
                public SingleTranslationKey Voted;
                public SingleTranslationKey TotalVotes;
                public SingleTranslationKey PollEnding;
                public SingleTranslationKey Poll;
                public SingleTranslationKey EndPollEarly;
                public SingleTranslationKey VoteOnThisPoll;
                public SingleTranslationKey OptionExists;
                public SingleTranslationKey Description;
                public SingleTranslationKey Title;
                public SingleTranslationKey ModalTitle;
                public SingleTranslationKey DontPing;
                public SingleTranslationKey NoOptions;
                public SingleTranslationKey MaximumVotes;
                public SingleTranslationKey MinimumVotes;
                public SingleTranslationKey Role;
                public SingleTranslationKey DueTime;
                public SingleTranslationKey SelectedChannel;
                public SingleTranslationKey AvailableOptions;
                public SingleTranslationKey PollContent;
                public SingleTranslationKey SelectMultiSelectButton;
                public SingleTranslationKey SetTimeButton;
                public SingleTranslationKey RemoveOptionButton;
                public SingleTranslationKey NewOptionButton;
                public SingleTranslationKey SelectPollContentButton;
                public SingleTranslationKey SelectChannelButton;
                public SingleTranslationKey SelectRoleButton;
                public SingleTranslationKey InvalidOptionLimit;
                public SingleTranslationKey InvalidTime;
                public SingleTranslationKey PollLimitReached;
            }

            public move Move;
            public class move
            {
                public SingleTranslationKey Moved;
                public SingleTranslationKey Moving;
                public SingleTranslationKey VcEmpty;
                public SingleTranslationKey NotAVc;
            }

            public manualBump ManualBump;
            public class manualBump
            {
                public SingleTranslationKey Warning;
                public SingleTranslationKey NotSetUp;
            }

            public kick Kick;
            public class kick
            {
                public SingleTranslationKey Errored;
                public SingleTranslationKey Kicked;
                public SingleTranslationKey Kicking;
                public SingleTranslationKey AuditLog;
            }

            public guildPurge GuildPurge;
            public class guildPurge
            {
                public SingleTranslationKey Ended;
                public SingleTranslationKey Deleting;
                public SingleTranslationKey Scanning;
            }

            public followUpdates FollowUpdates;
            public class followUpdates
            {
                public SingleTranslationKey Failed;
                public SingleTranslationKey Followed;
            }

            public customEmbed CustomEmbed;
            public class customEmbed
            {
                public SingleTranslationKey NoValidChannels;
                public SingleTranslationKey InlineField;
                public SingleTranslationKey ModifyingField;
                public SingleTranslationKey TextField;
                public SingleTranslationKey ModifyingFooterText;
                public SingleTranslationKey SetTextButton;
                public SingleTranslationKey ColorField;
                public SingleTranslationKey ModifyingColor;
                public SingleTranslationKey DescriptionField;
                public SingleTranslationKey ModifyingDescription;
                public SingleTranslationKey UserIdField;
                public SingleTranslationKey ModifyingAuthorbyUserId;
                public SingleTranslationKey ModifyingAuthorUrl;
                public SingleTranslationKey NameField;
                public SingleTranslationKey ModifyingAuthorName;
                public SingleTranslationKey SetAsServer;
                public SingleTranslationKey SetAsUserButton;
                public SingleTranslationKey SetIconButton;
                public SingleTranslationKey SetUrlButton;
                public SingleTranslationKey SetNameButton;
                public SingleTranslationKey UrlField;
                public SingleTranslationKey TitleField;
                public SingleTranslationKey ModifyingTitle;
                public SingleTranslationKey ImportSizeError;
                public SingleTranslationKey ImportingUpload;
                public SingleTranslationKey UploadImage;
                public SingleTranslationKey ContinueTimer;
                public SingleTranslationKey SendEmbedButton;
                public SingleTranslationKey RemoveFieldButton;
                public SingleTranslationKey ModifyFieldButton;
                public SingleTranslationKey AddFieldButton;
                public SingleTranslationKey SetFooterButton;
                public SingleTranslationKey SetTimestampButton;
                public SingleTranslationKey SetColorButton;
                public SingleTranslationKey SetImageButton;
                public SingleTranslationKey SetDescriptionButton;
                public SingleTranslationKey SetThumbnailButton;
                public SingleTranslationKey SetAuthorButton;
                public SingleTranslationKey SetTitleButton;
                public SingleTranslationKey New;
                public SingleTranslationKey UploadNotice;
            }

            public clearBackup ClearBackup;
            public class clearBackup
            {
                public SingleTranslationKey Deleted;
                public SingleTranslationKey IsOnServer;
            }

            public ban Ban;
            public class ban
            {
                public SingleTranslationKey Errored;
                public SingleTranslationKey Banned;
                public SingleTranslationKey Banning;
                public SingleTranslationKey AuditLog;
            }

            public SingleTranslationKey NoReason;
        }

        public music Music;
        public class music
        {
            public playlists Playlists;
            public class playlists
            {
                public modify Modify;
                public class modify
                {
                    public SingleTranslationKey DeleteNote;
                    public SingleTranslationKey HexHelp;
                    public SingleTranslationKey NewPlaylistColorPrompt;
                    public SingleTranslationKey NewPlaylistColor;
                    public SingleTranslationKey ThumbnailError;
                    public SingleTranslationKey ThumbnailSizeError;
                    public SingleTranslationKey ImportingThumbnail;
                    public SingleTranslationKey UploadThumbnail;
                    public SingleTranslationKey AddSong;
                    public SingleTranslationKey TrackLimit;
                    public SingleTranslationKey ModifyingPlaylist;
                    public SingleTranslationKey Track;
                    public SingleTranslationKey CurrentTrackCount;
                    public SingleTranslationKey RemoveDuplicates;
                    public SingleTranslationKey RemoveTracks;
                    public SingleTranslationKey AddTracks;
                    public SingleTranslationKey ChangeThumbnail;
                    public SingleTranslationKey ChangeColor;
                    public SingleTranslationKey ChangeName;
                }

                public createPlaylist CreatePlaylist;
                public class createPlaylist
                {
                    public SingleTranslationKey Created;
                    public SingleTranslationKey Creating;
                    public SingleTranslationKey SupportedAddType;
                    public SingleTranslationKey SetFirstTracks;
                    public SingleTranslationKey SetPlaylistName;
                    public SingleTranslationKey FirstTracks;
                    public SingleTranslationKey PlaylistName;
                    public SingleTranslationKey CreatePlaylist;
                    public SingleTranslationKey ChangeTracks;
                    public SingleTranslationKey ChangeName;
                }

                public import Import;
                public class import
                {
                    public SingleTranslationKey ImportFailed;
                    public SingleTranslationKey Importing;
                    public SingleTranslationKey UploadExport;
                    public SingleTranslationKey Created;
                    public SingleTranslationKey Creating;
                    public SingleTranslationKey NotLoaded;
                    public SingleTranslationKey PlaylistUrl;
                    public SingleTranslationKey ImportPlaylist;
                    public SingleTranslationKey ImportMethod;
                    public SingleTranslationKey ExportedPlaylist;
                    public SingleTranslationKey Link;
                }

                public delete Delete;
                public class delete
                {
                    public SingleTranslationKey Deleted;
                    public SingleTranslationKey Deleting;
                }

                public export Export;
                public class export
                {
                    public SingleTranslationKey Exported;
                }

                public share Share;
                public class share
                {
                    public MultiTranslationKey Shared;
                }

                public addToQueue AddToQueue;
                public class addToQueue
                {
                    public SingleTranslationKey Adding;
                }

                public manage Manage;
                public class manage
                {
                    public SingleTranslationKey PlaylistSelectorDelete;
                    public SingleTranslationKey PlaylistSelectorModify;
                    public SingleTranslationKey PlaylistSelectorExport;
                    public SingleTranslationKey PlaylistSelectorShare;
                    public SingleTranslationKey PlaylistSelectorQueue;
                    public SingleTranslationKey DeleteButton;
                    public SingleTranslationKey ModifyButton;
                    public SingleTranslationKey CreateNewButton;
                    public SingleTranslationKey SaveCurrentButton;
                    public SingleTranslationKey ImportButton;
                    public SingleTranslationKey ExportButton;
                    public SingleTranslationKey ShareButton;
                    public SingleTranslationKey AddToQueueButton;
                    public SingleTranslationKey NoPlaylists;
                }

                public loadShare LoadShare;
                public class loadShare
                {
                    public SingleTranslationKey Imported;
                    public SingleTranslationKey Importing;
                    public SingleTranslationKey ImportButton;
                    public SingleTranslationKey CreatedBy;
                    public SingleTranslationKey PlaylistName;
                    public SingleTranslationKey Found;
                    public SingleTranslationKey NotFound;
                    public SingleTranslationKey Loading;
                }

                public SingleTranslationKey ThumbnailModerationNote;
                public SingleTranslationKey NameModerationNote;
                public SingleTranslationKey Tracks;
                public SingleTranslationKey NoPlaylist;
                public SingleTranslationKey PlayListLimit;
                public SingleTranslationKey Title;
            }

            public skip Skip;
            public class skip
            {
                public SingleTranslationKey VoteButton;
                public SingleTranslationKey VoteStarted;
                public SingleTranslationKey Skipped;
                public SingleTranslationKey AlreadyVoted;
            }

            public shuffle Shuffle;
            public class shuffle
            {
                public SingleTranslationKey Off;
                public SingleTranslationKey On;
            }

            public repeat Repeat;
            public class repeat
            {
                public SingleTranslationKey Off;
                public SingleTranslationKey On;
            }

            public removeQueue RemoveQueue;
            public class removeQueue
            {
                public SingleTranslationKey Removed;
                public SingleTranslationKey NoSong;
                public SingleTranslationKey OutOfRange;
            }

            public join Join;
            public class join
            {
                public SingleTranslationKey AlreadyUsed;
                public SingleTranslationKey Joined;
            }

            public queue Queue;
            public class queue
            {
                public SingleTranslationKey NoSong;
                public SingleTranslationKey CurrentlyPlaying;
                public SingleTranslationKey Track;
                public SingleTranslationKey QueueCount;
            }

            public play Play;
            public class play
            {
                public SingleTranslationKey SearchSuccess;
                public SingleTranslationKey NoMatches;
                public SingleTranslationKey FailedToLoad;
                public SingleTranslationKey PlatformSelect;
                public SingleTranslationKey LookingForPlatform;
                public SingleTranslationKey LookingFor;
                public SingleTranslationKey Duration;
                public SingleTranslationKey Uploader;
                public SingleTranslationKey QueuePosition;
                public SingleTranslationKey QueuePositions;
                public SingleTranslationKey QueuedSingle;
                public SingleTranslationKey QueuedMultiple;
                public SingleTranslationKey Preparing;
            }

            public pause Pause;
            public class pause
            {
                public SingleTranslationKey Resumed;
                public SingleTranslationKey Paused;
            }

            public forceSkip ForceSkip;
            public class forceSkip
            {
                public SingleTranslationKey Skipped;
            }

            public forceDisconnect ForceDisconnect;
            public class forceDisconnect
            {
                public SingleTranslationKey Disconnected;
            }

            public forceClearQueue ForceClearQueue;
            public class forceClearQueue
            {
                public SingleTranslationKey Cleared;
            }

            public disconnect Disconnect;
            public class disconnect
            {
                public SingleTranslationKey VoteButton;
                public SingleTranslationKey VoteStarted;
                public SingleTranslationKey Disconnected;
                public SingleTranslationKey AlreadyVoted;
            }

            public clearQueue ClearQueue;
            public class clearQueue
            {
                public SingleTranslationKey VoteButton;
                public SingleTranslationKey VoteStarted;
                public SingleTranslationKey Cleared;
                public SingleTranslationKey AlreadyVoted;
            }

            public SingleTranslationKey DjRole;
            public SingleTranslationKey NotSameChannel;
        }

        public scoreSaber ScoreSaber;
        public class scoreSaber
        {
            public unlink Unlink;
            public class unlink
            {
                public SingleTranslationKey NoLink;
                public SingleTranslationKey Unlinked;
            }

            public search Search;
            public class search
            {
                public SingleTranslationKey NoSearchResult;
                public SingleTranslationKey FoundCount;
                public SingleTranslationKey SelectPlayer;
                public SingleTranslationKey Searching;
                public SingleTranslationKey SelectedCountry;
                public SingleTranslationKey SelectCountry;
                public SingleTranslationKey SelectContinent;
                public SingleTranslationKey NextStep;
                public SingleTranslationKey StartSearch;
                public SingleTranslationKey SelectCountryDropdown;
                public SingleTranslationKey SelectContinentDropdown;
                public SingleTranslationKey NoCountryFilter;
            }

            public profile Profile;
            public class profile
            {
                public SingleTranslationKey InvalidId;
                public SingleTranslationKey UserDoesNotExist;
                public SingleTranslationKey Placement;
                public SingleTranslationKey GraphToday;
                public SingleTranslationKey GraphDays;
                public SingleTranslationKey ReplaysWatched;
                public SingleTranslationKey TotalScore;
                public SingleTranslationKey TotalPlayCount;
                public SingleTranslationKey AverageRankedAccuracy;
                public SingleTranslationKey TotalRankedScore;
                public SingleTranslationKey RankedPlayCount;
                public SingleTranslationKey MapLeaderboard;
                public SingleTranslationKey RecentScores;
                public SingleTranslationKey TopScores;
                public MultiTranslationKey LinkSuccessful;
                public SingleTranslationKey OpenInBrowser;
                public SingleTranslationKey LinkProfileToAccount;
                public SingleTranslationKey ShowRecentScores;
                public SingleTranslationKey ShowTopScores;
                public SingleTranslationKey ShowProfile;
                public SingleTranslationKey LoadingPlayer;
                public SingleTranslationKey NoProfile;
                public SingleTranslationKey NoUser;
                public SingleTranslationKey InvalidInput;
            }

            public mapLeaderboard MapLeaderboard;
            public class mapLeaderboard
            {
                public SingleTranslationKey Profile;
                public SingleTranslationKey PageNotExist;
                public SingleTranslationKey ScoreboardNotExist;
                public SingleTranslationKey LoadingScoreboard;
            }

            public SingleTranslationKey ForbiddenError;
            public SingleTranslationKey InternalServerError;
        }

        public social Social;
        public class social
        {
            public slap Slap;
            public class slap
            {
                public MultiTranslationKey Self;
                public MultiTranslationKey Other;
            }

            public pat Pat;
            public class pat
            {
                public MultiTranslationKey Self;
                public MultiTranslationKey Other;
            }

            public kiss Kiss;
            public class kiss
            {
                public MultiTranslationKey Self;
                public MultiTranslationKey Other;
            }

            public kill Kill;
            public class kill
            {
                public MultiTranslationKey Self;
                public MultiTranslationKey Other;
            }

            public hug Hug;
            public class hug
            {
                public MultiTranslationKey Self;
                public MultiTranslationKey Other;
            }

            public highFive HighFive;
            public class highFive
            {
                public MultiTranslationKey Self;
                public MultiTranslationKey Other;
            }

            public cuddle Cuddle;
            public class cuddle
            {
                public MultiTranslationKey Self;
                public MultiTranslationKey Other;
            }

            public boop Boop;
            public class boop
            {
                public MultiTranslationKey Self;
                public MultiTranslationKey Other;
            }

            public afk Afk;
            public class afk
            {
                public SingleTranslationKey SetAfk;
                public SingleTranslationKey Title;
                public events Events;
                public class events
                {
                    public SingleTranslationKey CurrentlyAfk;
                    public SingleTranslationKey AndMore;
                    public SingleTranslationKey MessageListing;
                    public SingleTranslationKey Message;
                    public SingleTranslationKey MissedTitle;
                    public SingleTranslationKey NoLongerAfk;
                }

            }

        }

        public utility Utility;
        public class utility
        {
            public voiceChannelCreator VoiceChannelCreator;
            public class voiceChannelCreator
            {
                public unban Unban;
                public class unban
                {
                    public SingleTranslationKey VictimUnbanned;
                    public SingleTranslationKey VictimNotBanned;
                }

                public open Open;
                public class open
                {
                    public SingleTranslationKey Success;
                }

                public name Name;
                public class name
                {
                    public SingleTranslationKey Success;
                    public SingleTranslationKey Cooldown;
                }

                public limit Limit;
                public class limit
                {
                    public SingleTranslationKey Success;
                    public SingleTranslationKey OutsideRange;
                }

                public kick Kick;
                public class kick
                {
                    public SingleTranslationKey Success;
                    public SingleTranslationKey CannotKickSelf;
                }

                public invite Invite;
                public class invite
                {
                    public SingleTranslationKey VictimMessage;
                    public SingleTranslationKey Success;
                    public SingleTranslationKey PartialSuccess;
                    public SingleTranslationKey AlreadyPresent;
                    public SingleTranslationKey CannotInviteSelf;
                }

                public close Close;
                public class close
                {
                    public SingleTranslationKey Success;
                }

                public changeOwner ChangeOwner;
                public class changeOwner
                {
                    public SingleTranslationKey Success;
                    public SingleTranslationKey ForceAssign;
                    public SingleTranslationKey AlreadyOwner;
                }

                public ban Ban;
                public class ban
                {
                    public SingleTranslationKey VictimBanned;
                    public SingleTranslationKey VictimAlreadyBanned;
                    public SingleTranslationKey CannotBanSelf;
                }

                public events Events;
                public class events
                {
                    public SingleTranslationKey DefaultChannelName;
                }

                public SingleTranslationKey VictimIsBot;
                public SingleTranslationKey VictimNotPresent;
                public SingleTranslationKey NotAVccChannelOwner;
                public SingleTranslationKey NotAVccChannel;
            }

            public data Data;
            public class data
            {
                public @object Object;
                public class @object
                {
                    public SingleTranslationKey ProfileDeletionScheduled;
                    public SingleTranslationKey SecondaryConfirm;
                    public MultiTranslationKey ObjectionDisclaimer;
                    public SingleTranslationKey DeletionScheduleReversed;
                    public SingleTranslationKey DeletionAlreadyScheduled;
                    public SingleTranslationKey EnablingDataProcessingSuccess;
                    public SingleTranslationKey EnablingDataProcessingError;
                    public SingleTranslationKey EnablingDataProcessing;
                    public SingleTranslationKey ProfileAlreadyDeleted;
                }

                public policy Policy;
                public class policy
                {
                    public SingleTranslationKey NoPolicy;
                }

                public request Request;
                public class request
                {
                    public SingleTranslationKey DmNotice;
                    public SingleTranslationKey Confirm;
                    public SingleTranslationKey Fetching;
                    public SingleTranslationKey TimeError;
                }

            }

            public userInfo UserInfo;
            public class userInfo
            {
                public SingleTranslationKey FetchUserError;
                public SingleTranslationKey TimedOutUntil;
                public SingleTranslationKey Status;
                public SingleTranslationKey Competing;
                public SingleTranslationKey Watching;
                public SingleTranslationKey ListeningTo;
                public SingleTranslationKey Streaming;
                public SingleTranslationKey Playing;
                public SingleTranslationKey Activities;
                public SingleTranslationKey DoNotDisturb;
                public SingleTranslationKey Idle;
                public SingleTranslationKey Offline;
                public SingleTranslationKey Online;
                public SingleTranslationKey Web;
                public SingleTranslationKey Mobile;
                public SingleTranslationKey Desktop;
                public SingleTranslationKey Presence;
                public SingleTranslationKey BannerColor;
                public SingleTranslationKey Pronouns;
                public SingleTranslationKey ServerBoosterSince;
                public SingleTranslationKey AccountCreationDate;
                public SingleTranslationKey FirstJoinDate;
                public SingleTranslationKey ServerLeaveDate;
                public SingleTranslationKey ServerJoinDate;
                public SingleTranslationKey ShowProfileInviter;
                public SingleTranslationKey UsersInvited;
                public SingleTranslationKey NoInviter;
                public SingleTranslationKey InvitedBy;
                public SingleTranslationKey BanDetails;
                public SingleTranslationKey GlobalBanDate;
                public SingleTranslationKey GlobalBanMod;
                public SingleTranslationKey GlobalBanReason;
                public SingleTranslationKey NoReason;
                public SingleTranslationKey BotNotes;
                public SingleTranslationKey Backup;
                public SingleTranslationKey Roles;
                public SingleTranslationKey PendingMembership;
                public SingleTranslationKey DiscordPartner;
                public SingleTranslationKey VerifiedBotDeveloper;
                public SingleTranslationKey CertifiedMod;
                public SingleTranslationKey DiscordStaff;
                public SingleTranslationKey Owner;
                public SingleTranslationKey BotStaff;
                public SingleTranslationKey BotOwner;
                public SingleTranslationKey GlobalBanned;
                public SingleTranslationKey JoinedBefore;
                public SingleTranslationKey IsBanned;
                public SingleTranslationKey NeverJoined;
                public SingleTranslationKey Bot;
                public SingleTranslationKey System;
            }

            public urbanDictionary UrbanDictionary;
            public class urbanDictionary
            {
                public SingleTranslationKey Example;
                public SingleTranslationKey Definition;
                public SingleTranslationKey WrittenBy;
                public SingleTranslationKey NotExist;
                public SingleTranslationKey LookupFail;
                public SingleTranslationKey LookingUp;
                public SingleTranslationKey AdultContentWarning;
                public SingleTranslationKey AdultContentError;
            }

            public upload Upload;
            public class upload
            {
                public SingleTranslationKey Uploaded;
                public SingleTranslationKey TimedOut;
                public SingleTranslationKey AlreadyUploaded;
                public SingleTranslationKey NoInteraction;
            }

            public translateMessage TranslateMessage;
            public class translateMessage
            {
                public SingleTranslationKey Translated;
                public MultiTranslationKey Queue;
                public SingleTranslationKey Translating;
                public SingleTranslationKey SelectTargetDropdown;
                public SingleTranslationKey SelectTarget;
                public SingleTranslationKey SelectSourceDropdown;
                public SingleTranslationKey SelectSource;
                public SingleTranslationKey SelectProvider;
                public SingleTranslationKey NoContent;
            }

            public reportHost ReportHost;
            public class reportHost
            {
                public SingleTranslationKey SubmissionCreated;
                public SingleTranslationKey CreatingSubmission;
                public SingleTranslationKey SubmissionError;
                public SingleTranslationKey SubmissionCheck;
                public SingleTranslationKey DatabaseError;
                public SingleTranslationKey DatabaseCheck;
                public SingleTranslationKey ConfirmHost;
                public SingleTranslationKey InvalidHost;
                public SingleTranslationKey GuildBan;
                public SingleTranslationKey UserBan;
                public SingleTranslationKey LimitError;
                public SingleTranslationKey CooldownError;
                public SingleTranslationKey Processing;
                public SingleTranslationKey TosChangedNotice;
                public MultiTranslationKey Tos;
                public SingleTranslationKey AcceptTos;
                public SingleTranslationKey Title;
            }

            public reminders Reminders;
            public class reminders
            {
                public SingleTranslationKey DateTime;
                public SingleTranslationKey Description;
                public SingleTranslationKey SetDateTime;
                public SingleTranslationKey SetDescription;
                public SingleTranslationKey InvalidDateTime;
                public SingleTranslationKey Notice;
                public SingleTranslationKey DueTime;
                public SingleTranslationKey Created;
                public SingleTranslationKey Count;
                public SingleTranslationKey DeleteReminder;
                public SingleTranslationKey NewReminder;
                public SingleTranslationKey Title;
            }

            public rank Rank;
            public class rank
            {
                public SingleTranslationKey Progress;
                public SingleTranslationKey Other;
                public SingleTranslationKey Self;
                public SingleTranslationKey Title;
            }

            public leaderboard Leaderboard;
            public class leaderboard
            {
                public SingleTranslationKey NoPoints;
                public SingleTranslationKey Placement;
                public SingleTranslationKey Level;
                public SingleTranslationKey Fetching;
                public SingleTranslationKey Disabled;
                public SingleTranslationKey Title;
            }

            public guildInfo GuildInfo;
            public class guildInfo
            {
                public SingleTranslationKey NoGuildFound;
                public SingleTranslationKey Mee6Notice;
                public SingleTranslationKey GuildWidgetNotice;
                public SingleTranslationKey GuildPreviewNotice;
                public SingleTranslationKey JoinServer;
                public SingleTranslationKey GuildFeatures;
                public SingleTranslationKey SystemMessagesSetupTips;
                public SingleTranslationKey SystemMessagesRoleSticker;
                public SingleTranslationKey SystemMessagesRole;
                public SingleTranslationKey SystemMessagesBoost;
                public SingleTranslationKey SystemMessagesWelcomeStickers;
                public SingleTranslationKey SystemMessagesWelcome;
                public SingleTranslationKey SystemMessages;
                public SingleTranslationKey InactiveTimeout;
                public SingleTranslationKey InactiveChannel;
                public SingleTranslationKey CommunityUpdates;
                public SingleTranslationKey Rules;
                public SingleTranslationKey SpecialChannels;
                public SingleTranslationKey DefaultNotificationsMentions;
                public SingleTranslationKey DefaultNotificationsAll;
                public SingleTranslationKey DefaultNotifications;
                public SingleTranslationKey NsfwQuestionable;
                public SingleTranslationKey NsfwSafe;
                public SingleTranslationKey NsfwExplicit;
                public SingleTranslationKey NsfwNoRating;
                public SingleTranslationKey Nsfw;
                public SingleTranslationKey ExplicitContentEveryone;
                public SingleTranslationKey ExplicitContentNoRoles;
                public SingleTranslationKey ExplicitContentNone;
                public SingleTranslationKey ExplicitContent;
                public SingleTranslationKey VerificationHighest;
                public SingleTranslationKey VerificationHigh;
                public SingleTranslationKey VerificationMedium;
                public SingleTranslationKey VerificationLow;
                public SingleTranslationKey VerificationNone;
                public SingleTranslationKey Verification;
                public SingleTranslationKey WelcomeScreen;
                public SingleTranslationKey Screening;
                public SingleTranslationKey MultiFactor;
                public SingleTranslationKey Security;
                public SingleTranslationKey Community;
                public SingleTranslationKey Widget;
                public SingleTranslationKey BoostsTierThree;
                public SingleTranslationKey BoostsTierTwo;
                public SingleTranslationKey BoostsTierOne;
                public SingleTranslationKey BoostsNone;
                public SingleTranslationKey Boosts;
                public SingleTranslationKey Locale;
                public SingleTranslationKey Creation;
                public SingleTranslationKey Owner;
                public SingleTranslationKey GuildTitle;
                public SingleTranslationKey MaxMembers;
                public SingleTranslationKey OnlineMembers;
                public SingleTranslationKey MemberTitle;
                public SingleTranslationKey Fetching;
            }

            public emojiStealer EmojiStealer;
            public class emojiStealer
            {
                public SingleTranslationKey SuccessChat;
                public SingleTranslationKey SendingZipChat;
                public SingleTranslationKey SendingZipDm;
                public SingleTranslationKey PreparingZip;
                public SingleTranslationKey SuccessDmMain;
                public SingleTranslationKey SuccessDm;
                public SingleTranslationKey SendingDm;
                public SingleTranslationKey SuccessAdded;
                public SingleTranslationKey NoMoreRoom;
                public SingleTranslationKey AddToServerLoadingNotice;
                public SingleTranslationKey AddToServerLoading;
                public SingleTranslationKey AddToServerStickerError;
                public SingleTranslationKey CurrentChatZip;
                public SingleTranslationKey DirectMessageSingle;
                public SingleTranslationKey DirectMessageZip;
                public SingleTranslationKey AddToServer;
                public SingleTranslationKey ToggleStickers;
                public SingleTranslationKey ReceivePrompt;
                public SingleTranslationKey NoSuccessfulDownload;
                public SingleTranslationKey DownloadingStickers;
                public SingleTranslationKey DownloadingEmojis;
                public SingleTranslationKey NoEmojis;
                public SingleTranslationKey DownloadingPre;
                public SingleTranslationKey Sticker;
                public SingleTranslationKey Emoji;
            }

            public credits Credits;
            public class credits
            {
                public MultiTranslationKey Credits;
                public SingleTranslationKey Fetching;
            }

            public banner Banner;
            public class banner
            {
                public SingleTranslationKey NoBanner;
                public SingleTranslationKey Banner;
            }

            public avatar Avatar;
            public class avatar
            {
                public SingleTranslationKey ShowUserProfile;
                public SingleTranslationKey ShowServerProfile;
                public SingleTranslationKey Avatar;
            }

            public language Language;
            public class language
            {
                public SingleTranslationKey Selector;
                public SingleTranslationKey DisableOverride;
                public SingleTranslationKey Response;
                public SingleTranslationKey Disclaimer;
            }

            public help Help;
            public class help
            {
                public SingleTranslationKey MissingCommand;
                public SingleTranslationKey Disclaimer;
                public SingleTranslationKey Module;
            }

        }

        public moduleNames ModuleNames;
        public class moduleNames
        {
            public SingleTranslationKey Unknown;
            public SingleTranslationKey Configuration;
            public SingleTranslationKey Moderation;
            public SingleTranslationKey Music;
            public SingleTranslationKey Social;
            public SingleTranslationKey Utility;
        }

        public common Common;
        public class common
        {
            public SingleTranslationKey DirectMessageRedirect;
            public SingleTranslationKey InteractionFinished;
            public SingleTranslationKey InteractionTimeout;
            public SingleTranslationKey UsedByFooter;
            public cooldown Cooldown;
            public class cooldown
            {
                public SingleTranslationKey WaitingForCooldown;
                public SingleTranslationKey CancelCommand;
                public SingleTranslationKey SlowDown;
            }

            public prompts Prompts;
            public class prompts
            {
                public SingleTranslationKey DateTimeYear;
                public SingleTranslationKey DateTimeMonth;
                public SingleTranslationKey DateTimeDay;
                public SingleTranslationKey DateTimeHour;
                public SingleTranslationKey DateTimeMinute;
                public SingleTranslationKey SelectADateTime;
                public SingleTranslationKey TimespanDays;
                public SingleTranslationKey TimespanHours;
                public SingleTranslationKey TimespanMinutes;
                public SingleTranslationKey TimespanSeconds;
                public SingleTranslationKey SelectATimeSpan;
                public SingleTranslationKey WaitingForModalResponse;
                public SingleTranslationKey ReOpenModal;
                public SingleTranslationKey SelectAnOption;
                public SingleTranslationKey SelectAChannel;
                public SingleTranslationKey CreateChannelForMe;
                public SingleTranslationKey SelectedRoleUnavailable;
                public SingleTranslationKey SelectEveryone;
                public SingleTranslationKey SelectARole;
                public SingleTranslationKey CreateRoleForMe;
                public SingleTranslationKey Disable;
                public SingleTranslationKey ConfirmSelection;
            }

            public errors Errors;
            public class errors
            {
                public SingleTranslationKey NoChannels;
                public SingleTranslationKey NoRoles;
                public SingleTranslationKey UploadInProgress;
                public SingleTranslationKey DirectMessage;
                public SingleTranslationKey BotPermissions;
                public SingleTranslationKey Data;
                public SingleTranslationKey ExclusiveApp;
                public SingleTranslationKey ExclusivePrefix;
                public SingleTranslationKey GuildBan;
                public SingleTranslationKey UserBan;
                public SingleTranslationKey VoiceChannel;
                public SingleTranslationKey BotOwner;
                public SingleTranslationKey NoMember;
                public SingleTranslationKey Generic;
            }

        }

    }

    public common Common;
    public class common
    {
        public SingleTranslationKey Reason;
        public SingleTranslationKey NotSelected;
        public SingleTranslationKey Refresh;
        public SingleTranslationKey NextPage;
        public SingleTranslationKey PreviousPage;
        public SingleTranslationKey Page;
        public SingleTranslationKey Back;
        public SingleTranslationKey Cancel;
        public SingleTranslationKey Submit;
        public SingleTranslationKey Deny;
        public SingleTranslationKey Confirm;
        public SingleTranslationKey Off;
        public SingleTranslationKey On;
        public SingleTranslationKey No;
        public SingleTranslationKey Yes;
    }

}
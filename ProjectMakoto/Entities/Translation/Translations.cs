﻿namespace ProjectMakoto.Entities;

public class Translations
{
    public Dictionary<string, int> Progress = new();

    public common Common;
    public class common
    {
        public SingleTranslationKey Yes;
        public SingleTranslationKey No;
        public SingleTranslationKey On;
        public SingleTranslationKey Off;
        public SingleTranslationKey Confirm;
        public SingleTranslationKey Deny;
        public SingleTranslationKey Submit;
        public SingleTranslationKey Cancel;
        public SingleTranslationKey Back;
        public SingleTranslationKey Page;
        public SingleTranslationKey PreviousPage;
        public SingleTranslationKey NextPage;
        public SingleTranslationKey Refresh;
        public SingleTranslationKey NotSelected;
        public SingleTranslationKey Reason;
    }

    public _commands Commands;
    public class _commands
    {
        public common Common;
        public class common
        {
            public errors Errors;
            public class errors
            {
                public SingleTranslationKey Generic;
                public SingleTranslationKey NoMember;
                public SingleTranslationKey BotOwner;
                public SingleTranslationKey VoiceChannel;
                public SingleTranslationKey UserBan;
                public SingleTranslationKey GuildBan;
                public SingleTranslationKey ExclusivePrefix;
                public SingleTranslationKey ExclusiveApp;
                public SingleTranslationKey Data;
                public SingleTranslationKey BotPermissions;
                public SingleTranslationKey DirectMessage;
            }

            public prompts Prompts;
            public class prompts
            {
                public SingleTranslationKey ConfirmSelection;
                public SingleTranslationKey Disable;

                public SingleTranslationKey CreateRoleForMe;
                public SingleTranslationKey SelectEveryone;
                public SingleTranslationKey SelectARole;
                public SingleTranslationKey SelectedRoleUnavailable;

                public SingleTranslationKey CreateChannelForMe;
                public SingleTranslationKey SelectAChannel;

                public SingleTranslationKey SelectAnOption;

                public SingleTranslationKey ReOpenModal;
                public SingleTranslationKey WaitingForModalResponse;
                
                public SingleTranslationKey SelectATimeSpan;
                public SingleTranslationKey TimespanSeconds;
                public SingleTranslationKey TimespanMinutes;
                public SingleTranslationKey TimespanHours;
                public SingleTranslationKey TimespanDays;

                public SingleTranslationKey SelectADateTime;
                public SingleTranslationKey DateTimeMinute;
                public SingleTranslationKey DateTimeHour;
                public SingleTranslationKey DateTimeDay;
                public SingleTranslationKey DateTimeMonth;
                public SingleTranslationKey DateTimeYear;
            }

            public cooldown Cooldown;
            public class cooldown
            {
                public SingleTranslationKey SlowDown;
                public SingleTranslationKey CancelCommand;
                public SingleTranslationKey WaitingForCooldown;
            }

            public SingleTranslationKey UsedByFooter;
            public SingleTranslationKey InteractionTimeout;
            public SingleTranslationKey InteractionFinished;
            public SingleTranslationKey DirectMessageRedirect;
        }

        public modules ModuleNames;
        public class modules
        {
            public SingleTranslationKey Utility;
            public SingleTranslationKey Social;
            public SingleTranslationKey Music;
            public SingleTranslationKey Moderation;
            public SingleTranslationKey Configuration;
            public SingleTranslationKey Unknown;
        }

        public utilityCommands Utility;
        public class utilityCommands
        {
            public help Help;
            public class help
            {
                public SingleTranslationKey Module;
                public SingleTranslationKey Disclaimer;
                public SingleTranslationKey MissingCommand;
            }

            public language Language;
            public class language
            {
                public SingleTranslationKey Disclaimer;
                public SingleTranslationKey Response;
                public SingleTranslationKey DisableOverride;
                public SingleTranslationKey Selector;
            }

            public avatar Avatar;
            public class avatar
            {
                public SingleTranslationKey Avatar;
                public SingleTranslationKey ShowServerProfile;
                public SingleTranslationKey ShowUserProfile;
            }

            public banner Banner;
            public class banner
            {
                public SingleTranslationKey Banner;
                public SingleTranslationKey NoBanner;
            }

            public credits Credits;
            public class credits
            {
                public SingleTranslationKey Fetching;
                public MultiTranslationKey Credits;
            }

            public emojiStealer EmojiStealer;
            public class emojiStealer
            {
                public SingleTranslationKey Emoji;
                public SingleTranslationKey Sticker;
                public SingleTranslationKey DownloadingPre;
                public SingleTranslationKey NoEmojis;
                public SingleTranslationKey DownloadingEmojis;
                public SingleTranslationKey DownloadingStickers;
                public SingleTranslationKey NoSuccessfulDownload;
                public SingleTranslationKey ReceivePrompt;
                public SingleTranslationKey AddToServer;
                public SingleTranslationKey ToggleStickers;
                public SingleTranslationKey DirectMessageZip;
                public SingleTranslationKey DirectMessageSingle;
                public SingleTranslationKey CurrentChatZip;
                public SingleTranslationKey AddToServerStickerError;
                public SingleTranslationKey AddToServerLoading;
                public SingleTranslationKey AddToServerLoadingNotice;
                public SingleTranslationKey NoMoreRoom;
                public SingleTranslationKey SuccessAdded;
                public SingleTranslationKey SendingDm;
                public SingleTranslationKey SuccessDm;
                public SingleTranslationKey SuccessDmMain;
                public SingleTranslationKey PreparingZip;
                public SingleTranslationKey SendingZipDm;
                public SingleTranslationKey SendingZipChat;
                public SingleTranslationKey SuccessChat;
            }

            public guildInfo GuildInfo;
            public class guildInfo
            {
                public SingleTranslationKey Fetching;
                public SingleTranslationKey MemberTitle;
                public SingleTranslationKey OnlineMembers;
                public SingleTranslationKey MaxMembers;
                public SingleTranslationKey GuildTitle;
                public SingleTranslationKey Owner;
                public SingleTranslationKey Creation;
                public SingleTranslationKey Locale;
                public SingleTranslationKey Boosts;
                public SingleTranslationKey BoostsNone;
                public SingleTranslationKey BoostsTierOne;
                public SingleTranslationKey BoostsTierTwo;
                public SingleTranslationKey BoostsTierThree;
                public SingleTranslationKey Widget;
                public SingleTranslationKey Community;
                public SingleTranslationKey Security;
                public SingleTranslationKey MultiFactor;
                public SingleTranslationKey Screening;
                public SingleTranslationKey WelcomeScreen;
                public SingleTranslationKey Verification;
                public SingleTranslationKey VerificationNone;
                public SingleTranslationKey VerificationLow;
                public SingleTranslationKey VerificationMedium;
                public SingleTranslationKey VerificationHigh;
                public SingleTranslationKey VerificationHighest;
                public SingleTranslationKey ExplicitContent;
                public SingleTranslationKey ExplicitContentNone;
                public SingleTranslationKey ExplicitContentNoRoles;
                public SingleTranslationKey ExplicitContentEveryone;
                public SingleTranslationKey Nsfw;
                public SingleTranslationKey NsfwNoRating;
                public SingleTranslationKey NsfwExplicit;
                public SingleTranslationKey NsfwSafe;
                public SingleTranslationKey NsfwQuestionable;
                public SingleTranslationKey DefaultNotifications;
                public SingleTranslationKey DefaultNotificationsAll;
                public SingleTranslationKey DefaultNotificationsMentions;
                public SingleTranslationKey SpecialChannels;
                public SingleTranslationKey Rules;
                public SingleTranslationKey CommunityUpdates;
                public SingleTranslationKey InactiveChannel;
                public SingleTranslationKey InactiveTimeout;
                public SingleTranslationKey SystemMessages;
                public SingleTranslationKey SystemMessagesWelcome;
                public SingleTranslationKey SystemMessagesWelcomeStickers;
                public SingleTranslationKey SystemMessagesBoost;
                public SingleTranslationKey SystemMessagesRole;
                public SingleTranslationKey SystemMessagesRoleSticker;
                public SingleTranslationKey SystemMessagesSetupTips;
                public SingleTranslationKey GuildFeatures;
                public SingleTranslationKey JoinServer;
                public SingleTranslationKey GuildPreviewNotice;
                public SingleTranslationKey GuildWidgetNotice;
                public SingleTranslationKey Mee6Notice;
                public SingleTranslationKey NoGuildFound;
            }

            public leaderboard Leaderboard;
            public class leaderboard
            {
                public SingleTranslationKey Title;
                public SingleTranslationKey Disabled;
                public SingleTranslationKey Fetching;
                public SingleTranslationKey Level;
                public SingleTranslationKey Placement;
                public SingleTranslationKey NoPoints;
            }
            public rank Rank;
            public class rank
            {
                public SingleTranslationKey Title;
                public SingleTranslationKey Self;
                public SingleTranslationKey Other;
                public SingleTranslationKey Progress;
            }
            public reminders Reminders;
            public class reminders
            {
                public SingleTranslationKey Title;
                public SingleTranslationKey NewReminder;
                public SingleTranslationKey DeleteReminder;
                public SingleTranslationKey Count;
                public SingleTranslationKey Created;
                public SingleTranslationKey DueTime;
                public SingleTranslationKey Notice;
                public SingleTranslationKey InvalidDateTime;
                public SingleTranslationKey SetDescription;
                public SingleTranslationKey SetDateTime;
                public SingleTranslationKey Description;
                public SingleTranslationKey DateTime;
            }
            public reportHost ReportHost;
            public class reportHost
            {
                public SingleTranslationKey Title;
                public SingleTranslationKey AcceptTos;
                public MultiTranslationKey Tos;
                public SingleTranslationKey TosChangedNotice;
                public SingleTranslationKey Processing;
                public SingleTranslationKey CooldownError;
                public SingleTranslationKey LimitError;
                public SingleTranslationKey UserBan;
                public SingleTranslationKey GuildBan;
                public SingleTranslationKey InvalidHost;
                public SingleTranslationKey ConfirmHost;
                public SingleTranslationKey DatabaseCheck;
                public SingleTranslationKey DatabaseError;
                public SingleTranslationKey SubmissionCheck;
                public SingleTranslationKey SubmissionError;
                public SingleTranslationKey CreatingSubmission;
                public SingleTranslationKey SubmissionCreated;
            }

            public translateMessage TranslateMessage;
            public class translateMessage
            {
                public SingleTranslationKey NoContent;
                public SingleTranslationKey SelectProvider;
                public SingleTranslationKey SelectSource;
                public SingleTranslationKey SelectSourceDropdown;
                public SingleTranslationKey SelectTarget;
                public SingleTranslationKey SelectTargetDropdown;
                public SingleTranslationKey Translating;
                public MultiTranslationKey Queue;
                public SingleTranslationKey Translated;
            }

            public upload Upload;
            public class upload
            {
                public SingleTranslationKey NoInteraction;
                public SingleTranslationKey AlreadyUploaded;
                public SingleTranslationKey TimedOut;
                public SingleTranslationKey Uploaded;
            }

            public urbanDictionary UrbanDictionary;
            public class urbanDictionary
            {
                public SingleTranslationKey AdultContentError;
                public SingleTranslationKey AdultContentWarning;
                public SingleTranslationKey LookingUp;
                public SingleTranslationKey LookupFail;
                public SingleTranslationKey NotExist;
                public SingleTranslationKey WrittenBy;
                public SingleTranslationKey Definition;
                public SingleTranslationKey Example;
            }

            public userInfo UserInfo;
            public class userInfo
            {
                public SingleTranslationKey System;
                public SingleTranslationKey Bot;
                public SingleTranslationKey NeverJoined;
                public SingleTranslationKey IsBanned;
                public SingleTranslationKey JoinedBefore;
                public SingleTranslationKey GlobalBanned;
                public SingleTranslationKey BotOwner;
                public SingleTranslationKey BotStaff;
                public SingleTranslationKey Owner;
                public SingleTranslationKey DiscordStaff;
                public SingleTranslationKey CertifiedMod;
                public SingleTranslationKey VerifiedBotDeveloper;
                public SingleTranslationKey DiscordPartner;
                public SingleTranslationKey PendingMembership;
                public SingleTranslationKey Roles;
                public SingleTranslationKey Backup;
                public SingleTranslationKey BotNotes;
                public SingleTranslationKey NoReason;
                public SingleTranslationKey GlobalBanReason;
                public SingleTranslationKey GlobalBanMod;
                public SingleTranslationKey GlobalBanDate;
                public SingleTranslationKey BanDetails;
                public SingleTranslationKey InvitedBy;
                public SingleTranslationKey NoInviter;
                public SingleTranslationKey UsersInvited;
                public SingleTranslationKey ShowProfileInviter;
                public SingleTranslationKey ServerJoinDate;
                public SingleTranslationKey ServerLeaveDate;
                public SingleTranslationKey FirstJoinDate;
                public SingleTranslationKey AccountCreationDate;
                public SingleTranslationKey ServerBoosterSince;
                public SingleTranslationKey Pronouns;
                public SingleTranslationKey BannerColor;
                public SingleTranslationKey Presence;
                public SingleTranslationKey Desktop;
                public SingleTranslationKey Mobile;
                public SingleTranslationKey Web;
                public SingleTranslationKey Online;
                public SingleTranslationKey Offline;
                public SingleTranslationKey Idle;
                public SingleTranslationKey DoNotDisturb;
                public SingleTranslationKey Activities;
                public SingleTranslationKey Playing;
                public SingleTranslationKey Streaming;
                public SingleTranslationKey ListeningTo;
                public SingleTranslationKey Watching;
                public SingleTranslationKey Competing;
                public SingleTranslationKey Status;
                public SingleTranslationKey TimedOutUntil;
                public SingleTranslationKey FetchUserError;
            }

            public data Data;
            public class data
            {
                public request Request;
                public class request
                {
                    public SingleTranslationKey TimeError;
                    public SingleTranslationKey Fetching;
                    public SingleTranslationKey Confirm;
                    public SingleTranslationKey DmNotice;
                }

                public policy Policy;
                public class policy
                {
                    public SingleTranslationKey NoPolicy;
                }

                public objectCmd Object;
                public class objectCmd
                {
                    public SingleTranslationKey ProfileAlreadyDeleted;
                    public SingleTranslationKey EnablingDataProcessing;
                    public SingleTranslationKey EnablingDataProcessingError;
                    public SingleTranslationKey EnablingDataProcessingSuccess;
                    public SingleTranslationKey DeletionAlreadyScheduled;
                    public SingleTranslationKey DeletionScheduleReversed;
                    public MultiTranslationKey ObjectionDisclaimer;
                    public SingleTranslationKey SecondaryConfirm;
                    public SingleTranslationKey ProfileDeletionScheduled;
                }
            }

            public voiceChannelCreator VoiceChannelCreator;
            public class voiceChannelCreator
            {
                public SingleTranslationKey NotAVccChannel;
                public SingleTranslationKey NotAVccChannelOwner;
                public SingleTranslationKey VictimNotPresent;
                public SingleTranslationKey VictimIsBot;

                public events Events;
                public class events
                {
                    public SingleTranslationKey DefaultChannelName;
                }

                public ban Ban;
                public class ban
                {
                    public SingleTranslationKey CannotBanSelf;
                    public SingleTranslationKey VictimAlreadyBanned;
                    public SingleTranslationKey VictimBanned;
                }

                public changeOwner ChangeOwner;
                public class changeOwner
                {
                    public SingleTranslationKey AlreadyOwner;
                    public SingleTranslationKey ForceAssign;
                    public SingleTranslationKey Success;
                }

                public close Close;
                public class close
                {
                    public SingleTranslationKey Success;
                }

                public invite Invite;
                public class invite
                {
                    public SingleTranslationKey CannotInviteSelf;
                    public SingleTranslationKey AlreadyPresent;
                    public SingleTranslationKey PartialSuccess;
                    public SingleTranslationKey Success;
                    public SingleTranslationKey VictimMessage;
                }

                public kick Kick;
                public class kick
                {
                    public SingleTranslationKey CannotKickSelf;
                    public SingleTranslationKey Success;
                }

                public limit Limit;
                public class limit
                {
                    public SingleTranslationKey OutsideRange;
                    public SingleTranslationKey Success;
                }

                public name Name;
                public class name
                {
                    public SingleTranslationKey Cooldown;
                    public SingleTranslationKey Success;
                }

                public open Open;
                public class open
                {
                    public SingleTranslationKey Success;
                }

                public unban Unban;
                public class unban
                {
                    public SingleTranslationKey VictimNotBanned;
                    public SingleTranslationKey VictimUnbanned;
                }
            }
        }

        public socialCommands Social;
        public class socialCommands
        {
            public afk Afk;
            public class afk
            {
                public SingleTranslationKey Title;
                public SingleTranslationKey SetAfk;
            }

            public boop Boop;
            public class boop
            {
                public MultiTranslationKey Other;
                public MultiTranslationKey Self;
            }

            public cuddle Cuddle;
            public class cuddle
            {
                public MultiTranslationKey Other;
                public MultiTranslationKey Self;
            }

            public highFive HighFive;
            public class highFive
            {
                public MultiTranslationKey Other;
                public MultiTranslationKey Self;
            }

            public hug Hug;
            public class hug
            {
                public MultiTranslationKey Other;
                public MultiTranslationKey Self;
            }

            public kill Kill;
            public class kill
            {
                public MultiTranslationKey Other;
                public MultiTranslationKey Self;
            }

            public kiss Kiss;
            public class kiss
            {
                public MultiTranslationKey Other;
                public MultiTranslationKey Self;
            }

            public pat Pat;
            public class pat
            {
                public MultiTranslationKey Other;
                public MultiTranslationKey Self;
            }

            public slap Slap;
            public class slap
            {
                public MultiTranslationKey Other;
                public MultiTranslationKey Self;
            }
        }

        public scoreSaberCommands ScoreSaber;
        public class scoreSaberCommands
        {
            public SingleTranslationKey InternalServerError;
            public SingleTranslationKey ForbiddenError;

            public mapLeaderboard MapLeaderboard;
            public class mapLeaderboard
            {
                public SingleTranslationKey LoadingScoreboard;
                public SingleTranslationKey ScoreboardNotExist;
                public SingleTranslationKey PageNotExist;
                public SingleTranslationKey Profile;
            }

            public profile Profile;
            public class profile
            {
                public SingleTranslationKey InvalidInput;
                public SingleTranslationKey NoUser;
                public SingleTranslationKey NoProfile;
                public SingleTranslationKey LoadingPlayer;
                public SingleTranslationKey ShowProfile;
                public SingleTranslationKey ShowTopScores;
                public SingleTranslationKey ShowRecentScores;
                public SingleTranslationKey LinkProfileToAccount;
                public SingleTranslationKey OpenInBrowser;
                public MultiTranslationKey LinkSuccessful;
                public SingleTranslationKey TopScores;
                public SingleTranslationKey RecentScores;
                public SingleTranslationKey MapLeaderboard;
                public SingleTranslationKey RankedPlayCount;
                public SingleTranslationKey TotalRankedScore;
                public SingleTranslationKey AverageRankedAccuracy;
                public SingleTranslationKey TotalPlayCount;
                public SingleTranslationKey TotalScore;
                public SingleTranslationKey ReplaysWatched;
                public SingleTranslationKey GraphDays;
                public SingleTranslationKey GraphToday;
                public SingleTranslationKey Placement;
                public SingleTranslationKey UserDoesNotExist;
                public SingleTranslationKey InvalidId;
            }

            public search Search;
            public class search
            {
                public SingleTranslationKey NoCountryFilter;
                public SingleTranslationKey SelectContinentDropdown;
                public SingleTranslationKey SelectCountryDropdown;
                public SingleTranslationKey StartSearch;
                public SingleTranslationKey NextStep;
                public SingleTranslationKey SelectContinent;
                public SingleTranslationKey SelectCountry;
                public SingleTranslationKey SelectedCountry;
                public SingleTranslationKey Searching;
                public SingleTranslationKey SelectPlayer;
                public SingleTranslationKey FoundCount;
                public SingleTranslationKey NoSearchResult;
            }

            public unlink Unlink;
            public class unlink
            {
                public SingleTranslationKey Unlinked;
                public SingleTranslationKey NoLink;
            }
        }

        public musicCommands Music;
        public class musicCommands
        {
            public SingleTranslationKey DjRole;
            public SingleTranslationKey NotSameChannel;

            public clearQueue ClearQueue;
            public class clearQueue
            {
                public SingleTranslationKey AlreadyVoted;
                public SingleTranslationKey Cleared;
                public SingleTranslationKey VoteStarted;
                public SingleTranslationKey VoteButton;
            }

            public disconnect Disconnect;
            public class disconnect
            {
                public SingleTranslationKey AlreadyVoted;
                public SingleTranslationKey Disconnected;
                public SingleTranslationKey VoteStarted;
                public SingleTranslationKey VoteButton;
            }

            public forceClearQueue ForceClearQueue;

            public class forceClearQueue
            {
                public SingleTranslationKey Cleared;
            }

            public forceDisconnect ForceDisconnect;

            public class forceDisconnect
            {
                public SingleTranslationKey Disconnected;
            }


            public forceSkip ForceSkip;

            public class forceSkip
            {
                public SingleTranslationKey Skipped;
            }


            public pause Pause;

            public class pause
            {
                public SingleTranslationKey Paused;
                public SingleTranslationKey Resumed;
            }


            public play Play;

            public class play
            {
                public SingleTranslationKey Preparing;
                public SingleTranslationKey QueuedMultiple;
                public SingleTranslationKey QueuedSingle;
                public SingleTranslationKey QueuePositions;
                public SingleTranslationKey QueuePosition;
                public SingleTranslationKey Uploader;
                public SingleTranslationKey Duration;
                public SingleTranslationKey LookingFor;
                public SingleTranslationKey LookingForPlatform;
                public SingleTranslationKey PlatformSelect;
                public SingleTranslationKey FailedToLoad;
                public SingleTranslationKey NoMatches;
                public SingleTranslationKey SearchSuccess;
            }

            public queue Queue;
            public class queue
            {
                public SingleTranslationKey QueueCount;
                public SingleTranslationKey Track;
                public SingleTranslationKey CurrentlyPlaying;
                public SingleTranslationKey NoSong;
            }

            public join Join;
            public class join
            {
                public SingleTranslationKey Joined;
                public SingleTranslationKey AlreadyUsed;
            }

            public removeQueue RemoveQueue;
            public class removeQueue
            {
                public SingleTranslationKey OutOfRange;
                public SingleTranslationKey NoSong;
                public SingleTranslationKey Removed;
            }

            public repeat Repeat;
            public class repeat
            {
                public SingleTranslationKey On;
                public SingleTranslationKey Off;
            }

            public shuffle Shuffle;
            public class shuffle
            {
                public SingleTranslationKey On;
                public SingleTranslationKey Off;
            }

            public skip Skip;
            public class skip
            {
                public SingleTranslationKey AlreadyVoted;
                public SingleTranslationKey Skipped;
                public SingleTranslationKey VoteStarted;
                public SingleTranslationKey VoteButton;
            }

            public playlists Playlists;
            public class playlists
            {
                public SingleTranslationKey Title;
                public SingleTranslationKey PlayListLimit;
                public SingleTranslationKey NoPlaylist;
                public SingleTranslationKey Tracks;

                public loadShare LoadShare;
                public class loadShare
                {
                    public SingleTranslationKey Loading;
                    public SingleTranslationKey NotFound;
                    public SingleTranslationKey Found;
                    public SingleTranslationKey PlaylistName;
                    public SingleTranslationKey CreatedBy;
                    public SingleTranslationKey ImportButton;
                    public SingleTranslationKey Importing;
                    public SingleTranslationKey Imported;
                }

                public manage Manage;
                public class manage
                {
                    public SingleTranslationKey NoPlaylists;
                    public SingleTranslationKey AddToQueueButton;
                    public SingleTranslationKey ShareButton;
                    public SingleTranslationKey ExportButton;
                    public SingleTranslationKey ImportButton;
                    public SingleTranslationKey SaveCurrentButton;
                    public SingleTranslationKey CreateNewButton;
                    public SingleTranslationKey ModifyButton;
                    public SingleTranslationKey DeleteButton;

                    public SingleTranslationKey PlaylistSelectorQueue;
                    public SingleTranslationKey PlaylistSelectorShare;
                    public SingleTranslationKey PlaylistSelectorExport;
                    public SingleTranslationKey PlaylistSelectorModify;
                    public SingleTranslationKey PlaylistSelectorDelete;
                }

                public addToQueue AddToQueue;
                public class addToQueue
                {
                    public SingleTranslationKey Adding;
                }
                
                public share Share;
                public class share
                {
                    public MultiTranslationKey Shared;
                }

                public export Export;
                public class export
                {
                    public SingleTranslationKey Exported;
                }
            }
        }

        public configurationCommands Config;
        public class configurationCommands
        {
            public prefixConfig PrefixConfigCommand;
            public class prefixConfig
            {
                public SingleTranslationKey Title;
                public SingleTranslationKey PrefixDisabled;
                public SingleTranslationKey CurrentPrefix;
                public SingleTranslationKey TogglePrefixCommands;
                public SingleTranslationKey ChangePrefix;
                public SingleTranslationKey NewPrefixModalTitle;
                public SingleTranslationKey NewPrefix;
            }

            public guildlanguage GuildLanguage;
            public class guildlanguage
            {
                public SingleTranslationKey Title;
                public SingleTranslationKey Disclaimer;
                public SingleTranslationKey Response;
                public SingleTranslationKey DisableOverride;
                public SingleTranslationKey Selector;
            }
        }
    }
}

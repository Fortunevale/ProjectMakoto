namespace ProjectIchigo.Entities;

public class Translations
{
    public Dictionary<string, int> Progress = new();

    public common Common;
    public class common
    {
        public TranslationKey Yes;
        public TranslationKey No;
        public TranslationKey On;
        public TranslationKey Off;
        public TranslationKey Confirm;
        public TranslationKey Deny;
        public TranslationKey Submit;
        public TranslationKey Cancel;
        public TranslationKey Back;
        public TranslationKey PreviousPage;
        public TranslationKey NextPage;
        public TranslationKey NotSelected;
        public TranslationKey Reason;
    }

    public _commands Commands;
    public class _commands
    {
        public language Language;
        public class language
        {
            public TranslationKey Disclaimer;
            public TranslationKey Response;
            public TranslationKey DisableOverride;
            public TranslationKey Selector;
        }

        public common Common;
        public class common
        {
            public errors Errors;
            public class errors
            {
                public TranslationKey Generic;
                public TranslationKey NoMember;
                public TranslationKey BotOwner;
                public TranslationKey VoiceChannel;
                public TranslationKey UserBan;
                public TranslationKey GuildBan;
                public TranslationKey ExclusivePrefix;
                public TranslationKey ExclusiveApp;
                public TranslationKey Data;
                public TranslationKey BotPermissions;
                public TranslationKey DirectMessage;
            }

            public prompts Prompts;
            public class prompts
            {
                public TranslationKey ConfirmSelection;
                public TranslationKey Disable;

                public TranslationKey CreateRoleForMe;
                public TranslationKey SelectEveryone;
                public TranslationKey SelectARole;
                public TranslationKey SelectedRoleUnavailable;

                public TranslationKey CreateChannelForMe;
                public TranslationKey SelectAChannel;

                public TranslationKey SelectAnOption;

                public TranslationKey ReOpenModal;
                public TranslationKey WaitingForModalResponse;
                
                public TranslationKey SelectATimeSpan;
                public TranslationKey TimespanSeconds;
                public TranslationKey TimespanMinutes;
                public TranslationKey TimespanHours;
                public TranslationKey TimespanDays;

                public TranslationKey SelectADateTime;
                public TranslationKey DateTimeMinute;
                public TranslationKey DateTimeHour;
                public TranslationKey DateTimeDay;
                public TranslationKey DateTimeMonth;
                public TranslationKey DateTimeYear;
            }

            public TranslationKey UsedByFooter;
            public TranslationKey InteractionTimeout;
            public TranslationKey InteractionFinished;
            public TranslationKey DirectMessageRedirect;
        }

        public modules ModuleNames;
        public class modules
        {
            public TranslationKey Utility;
            public TranslationKey Social;
            public TranslationKey Music;
            public TranslationKey Moderation;
            public TranslationKey Configuration;
            public TranslationKey Unknown;
        }

        public help Help;
        public class help
        {
            public TranslationKey Module;
            public TranslationKey Disclaimer;
            public TranslationKey MissingCommand;
        }

        public avatar Avatar;
        public class avatar
        {
            public TranslationKey Avatar;
            public TranslationKey ShowServerProfile;
            public TranslationKey ShowUserProfile;
        }

        public banner Banner;
        public class banner
        {
            public TranslationKey Banner;
            public TranslationKey NoBanner;
        }

        public credits Credits;
        public class credits
        {
            public TranslationKey Fetching;
            public TranslationKey Credits;
        }

        public emojiStealer EmojiStealer;
        public class emojiStealer
        {
            public TranslationKey Emoji;
            public TranslationKey Sticker;
            public TranslationKey DownloadingPre;
            public TranslationKey NoEmojis;
            public TranslationKey DownloadingEmojis;
            public TranslationKey DownloadingStickers;
            public TranslationKey NoSuccessfulDownload;
            public TranslationKey ReceivePrompt;
            public TranslationKey AddToServer;
            public TranslationKey ToggleStickers;
            public TranslationKey DirectMessageZip;
            public TranslationKey DirectMessageSingle;
            public TranslationKey CurrentChatZip;
            public TranslationKey AddToServerStickerError;
            public TranslationKey AddToServerLoading;
            public TranslationKey AddToServerLoadingNotice;
            public TranslationKey NoMoreRoom;
            public TranslationKey SuccessAdded;
            public TranslationKey SendingDm;
            public TranslationKey SuccessDm;
            public TranslationKey SuccessDmMain;
            public TranslationKey PreparingZip;
            public TranslationKey SendingZipDm;
            public TranslationKey SendingZipChat;
            public TranslationKey SuccessChat;
        }

        public guildInfo GuildInfo;
        public class guildInfo
        {
            public TranslationKey Fetching;
            public TranslationKey MemberTitle;
            public TranslationKey OnlineMembers;
            public TranslationKey MaxMembers;
            public TranslationKey GuildTitle;
            public TranslationKey Owner;
            public TranslationKey Creation;
            public TranslationKey Locale;
            public TranslationKey Boosts;
            public TranslationKey BoostsNone;
            public TranslationKey BoostsTierOne;
            public TranslationKey BoostsTierTwo;
            public TranslationKey BoostsTierThree;
            public TranslationKey Widget;
            public TranslationKey Community;
            public TranslationKey Security;
            public TranslationKey MultiFactor;
            public TranslationKey Screening;
            public TranslationKey WelcomeScreen;
            public TranslationKey Verification;
            public TranslationKey VerificationNone;
            public TranslationKey VerificationLow;
            public TranslationKey VerificationMedium;
            public TranslationKey VerificationHigh;
            public TranslationKey VerificationHighest;
            public TranslationKey ExplicitContent;
            public TranslationKey ExplicitContentNone;
            public TranslationKey ExplicitContentNoRoles;
            public TranslationKey ExplicitContentEveryone;
            public TranslationKey Nsfw;
            public TranslationKey NsfwNoRating;
            public TranslationKey NsfwExplicit;
            public TranslationKey NsfwSafe;
            public TranslationKey NsfwQuestionable;
            public TranslationKey DefaultNotifications;
            public TranslationKey DefaultNotificationsAll;
            public TranslationKey DefaultNotificationsMentions;
            public TranslationKey SpecialChannels;
            public TranslationKey Rules;
            public TranslationKey CommunityUpdates;
            public TranslationKey InactiveChannel;
            public TranslationKey InactiveTimeout;
            public TranslationKey SystemMessages;
            public TranslationKey SystemMessagesWelcome;
            public TranslationKey SystemMessagesWelcomeStickers;
            public TranslationKey SystemMessagesBoost;
            public TranslationKey SystemMessagesRole;
            public TranslationKey SystemMessagesRoleSticker;
            public TranslationKey SystemMessagesSetupTips;
            public TranslationKey GuildFeatures;
            public TranslationKey JoinServer;
            public TranslationKey GuildPreviewNotice;
            public TranslationKey GuildWidgetNotice;
            public TranslationKey Mee6Notice;
            public TranslationKey NoGuildFound;
        }
        
        public leaderboard Leaderboard;
        public class leaderboard
        {
            public TranslationKey Title;
            public TranslationKey Disabled;
            public TranslationKey Fetching;
            public TranslationKey Level;
            public TranslationKey Placement;
            public TranslationKey NoPoints;
        }        
        public rank Rank;
        public class rank
        {
            public TranslationKey Title;
            public TranslationKey Self;
            public TranslationKey Other;
            public TranslationKey Progress;
        }
        public reminders Reminders;
        public class reminders
        {
            public TranslationKey Title;
            public TranslationKey NewReminder;
            public TranslationKey DeleteReminder;
            public TranslationKey Count;
            public TranslationKey Created;
            public TranslationKey DueTime;
            public TranslationKey Notice;
            public TranslationKey InvalidDateTime;
            public TranslationKey SetDescription;
            public TranslationKey SetDateTime;
            public TranslationKey Description;
            public TranslationKey DateTime;
        }
        public reportHost ReportHost;
        public class reportHost
        {
            public TranslationKey Title;
            public TranslationKey AcceptTos;
            public TranslationKey Tos;
            public TranslationKey TosChangedNotice;
            public TranslationKey Processing;
            public TranslationKey CooldownError;
            public TranslationKey LimitError;
            public TranslationKey UserBan;
            public TranslationKey GuildBan;
            public TranslationKey InvalidHost;
            public TranslationKey ConfirmHost;
            public TranslationKey DatabaseCheck;
            public TranslationKey DatabaseError;
            public TranslationKey SubmissionCheck;
            public TranslationKey SubmissionError;
            public TranslationKey CreatingSubmission;
            public TranslationKey SubmissionCreated;
        }

        public translateMessage TranslateMessage;
        public class translateMessage
        {
            public TranslationKey NoContent;
            public TranslationKey SelectProvider;
            public TranslationKey SelectSource;
            public TranslationKey SelectSourceDropdown;
            public TranslationKey SelectTarget;
            public TranslationKey SelectTargetDropdown;
            public TranslationKey Translating;
            public TranslationKey Queue;
            public TranslationKey Translated;
        }

        public upload Upload;
        public class upload
        {
            public TranslationKey NoInteraction;
            public TranslationKey AlreadyUploaded;
            public TranslationKey TimedOut;
            public TranslationKey Uploaded;
        }

        public urbanDictionary UrbanDictionary;
        public class urbanDictionary
        {
            public TranslationKey AdultContentError;
            public TranslationKey AdultContentWarning;
            public TranslationKey LookingUp;
            public TranslationKey LookupFail;
            public TranslationKey NotExist;
            public TranslationKey WrittenBy;
            public TranslationKey Definition;
            public TranslationKey Example;
        }

        public userInfo UserInfo;
        public class userInfo
        {
            public TranslationKey System;
            public TranslationKey Bot;
            public TranslationKey NeverJoined;
            public TranslationKey IsBanned;
            public TranslationKey JoinedBefore;
            public TranslationKey GlobalBanned;
            public TranslationKey BotOwner;
            public TranslationKey BotStaff;
            public TranslationKey Owner;
            public TranslationKey DiscordStaff;
            public TranslationKey CertifiedMod;
            public TranslationKey VerifiedBotDeveloper;
            public TranslationKey DiscordPartner;
            public TranslationKey PendingMembership;
            public TranslationKey Roles;
            public TranslationKey Backup;
            public TranslationKey BotNotes;
            public TranslationKey NoReason;
            public TranslationKey GlobalBanReason;
            public TranslationKey GlobalBanMod;
            public TranslationKey GlobalBanDate;
            public TranslationKey BanDetails;
            public TranslationKey InvitedBy;
            public TranslationKey NoInviter;
            public TranslationKey UsersInvited;
            public TranslationKey ShowProfileInviter;
            public TranslationKey ServerJoinDate;
            public TranslationKey ServerLeaveDate;
            public TranslationKey FirstJoinDate;
            public TranslationKey AccountCreationDate;
            public TranslationKey ServerBoosterSince;
            public TranslationKey Pronouns;
            public TranslationKey BannerColor;
            public TranslationKey Presence;
            public TranslationKey Desktop;
            public TranslationKey Mobile;
            public TranslationKey Web;
            public TranslationKey Online;
            public TranslationKey Offline;
            public TranslationKey Idle;
            public TranslationKey DoNotDisturb;
            public TranslationKey Activities;
            public TranslationKey Playing;
            public TranslationKey Streaming;
            public TranslationKey ListeningTo;
            public TranslationKey Watching;
            public TranslationKey Competing;
            public TranslationKey Status;
            public TranslationKey TimedOutUntil;
            public TranslationKey FetchUserError;
        }

        public data Data;
        public class data
        {
            public request Request;
            public class request
            {
                public TranslationKey Fetching;
                public TranslationKey Confirm;
                public TranslationKey DmNotice;
            }

            public policy Policy;
            public class policy
            {
                public TranslationKey NoPolicy;
            }
        }
    }
}

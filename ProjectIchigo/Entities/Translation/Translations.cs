namespace ProjectIchigo.Entities;

public class Translations
{
    public _common Common;
    public class _common
    {
        public TranslationKey Yes;
        public TranslationKey No;
        public TranslationKey On;
        public TranslationKey Off;
        public TranslationKey Confirm;
        public TranslationKey Deny;
        public TranslationKey PreviousPage;
        public TranslationKey NextPage;
    }

    public _commands Commands;
    public class _commands
    {
        public _common Common;
        public class _common
        {
            public _errors Errors;
            public class _errors
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

            public TranslationKey UsedByFooter;
            public TranslationKey InteractionTimeout;
            public TranslationKey InteractionFinished;
        }

        public _modules ModuleNames;
        public class _modules
        {
            public TranslationKey Utility;
            public TranslationKey Social;
            public TranslationKey Music;
            public TranslationKey Moderation;
            public TranslationKey Configuration;
            public TranslationKey Unknown;
        }

        public _help Help;
        public class _help
        {
            public TranslationKey Module;
            public TranslationKey Disclaimer;
            public TranslationKey MissingCommand;
        }

        public _avatar Avatar;
        public class _avatar
        {
            public TranslationKey Avatar;
            public TranslationKey ShowServerProfile;
            public TranslationKey ShowUserProfile;
        }

        public _banner Banner;
        public class _banner
        {
            public TranslationKey Banner;
            public TranslationKey NoBanner;
        }

        public _credits Credits;
        public class _credits
        {
            public TranslationKey Fetching;
            public TranslationKey Credits;
        }

        public _emojistealer EmojiStealer;
        public class _emojistealer
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

        public _guildinfo GuildInfo;
        public class _guildinfo
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
    }
}

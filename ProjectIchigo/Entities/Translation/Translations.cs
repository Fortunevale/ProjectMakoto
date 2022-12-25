namespace ProjectIchigo.Entities;

public class Translations
{
    public _common Common;
    public class _common
    {
        public TranslationKey Yes;
        public TranslationKey No;
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
            public TranslationKey MemberTitle;
            public TranslationKey OnlineMembers;
            public TranslationKey MaxMembers;
            public TranslationKey GuildTitle;
            public TranslationKey Owner;
            public TranslationKey Creation;
            public TranslationKey Locale;
            public TranslationKey Boosts;
            public TranslationKey Widget;
            public TranslationKey Community;
        }
    }
}

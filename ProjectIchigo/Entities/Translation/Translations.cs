namespace ProjectIchigo.Entities;

public class Translations
{
    public _common common;
    public class _common
    {
        public TranslationKey yes;
        public TranslationKey no;
        public TranslationKey confirm;
        public TranslationKey deny;
        public TranslationKey previous_page;
        public TranslationKey next_page;
    }

    public _commands commands;
    public class _commands
    {
        public _common common;
        public class _common
        {
            public _errors errors;
            public class _errors
            {
                public TranslationKey generic;
                public TranslationKey nomember;
                public TranslationKey botowner;
                public TranslationKey voicechannel;
                public TranslationKey userban;
                public TranslationKey guildban;
                public TranslationKey exclusiveprefix;
                public TranslationKey exclusiveapp;
                public TranslationKey data;
                public TranslationKey botperms;
            }

            public TranslationKey usedbyfoot;
            public TranslationKey timeout;
            public TranslationKey finished;
        }

        public _modules modules;
        public class _modules
        {
            public TranslationKey utility;
            public TranslationKey social;
            public TranslationKey music;
            public TranslationKey moderation;
            public TranslationKey configuration;
            public TranslationKey unknown;
        }

        public _help help;
        public class _help
        {
            public TranslationKey module;
            public TranslationKey disclaimer;
            public TranslationKey nocmd;
        }

        public _avatar avatar;
        public class _avatar
        {
            public TranslationKey avatar;
            public TranslationKey show_server_profile;
            public TranslationKey show_user_profile;
        }

        public _banner banner;
        public class _banner
        {
            public TranslationKey banner;
            public TranslationKey nobanner;
        }

        public _credits credits;
        public class _credits
        {
            public TranslationKey fetching;
            public TranslationKey credits;
        }
    }
}

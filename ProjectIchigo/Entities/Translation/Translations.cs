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
            public TranslationKey usedbyfoot;
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
    }
}

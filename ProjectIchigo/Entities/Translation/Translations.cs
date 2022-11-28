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
        public _help help;
        public class _help
        {
            public TranslationKey disclaimer;
            public TranslationKey nocmd;
        }
    }
}

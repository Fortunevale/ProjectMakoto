<h1 align="center">Makoto</h1>
<p align="center"><img src="ProjectMakoto/Assets/Prod.png" width=250 align="center"></p>
<p align="center" style="font-weight:bold;">A feature packed discord bot!</p>

<p align="center"><img src="https://github.com/Fortunevale/ProjectMakoto/actions/workflows/dev.yml/badge.svg?branch=dev" align="center">
<p align="center"><img src="https://img.shields.io/github/contributors/Fortunevale/ProjectMakoto" align="center"> <img src="https://img.shields.io/github/issues-raw/Fortunevale/ProjectMakoto" align="center"></p>
<p align="center"><img src="https://wakatime.com/badge/github/Fortunevale/ProjectMakoto.svg" align="center"></p>

<p align="center"><img src="https://img.shields.io/github/stars/Fortunevale/ProjectMakoto?style=social" align="center"> <img src="https://img.shields.io/github/watchers/Fortunevale/ProjectMakoto?style=social" align="center"></p>

## Creating/modifying Translations

- All translation can be found in `ProjectMakoto/Translations/strings.json`.
- When adding a new command, add a new entry to the `CommandList` key in `Commands`.
    - Remember to include all subcommands, options and choices.
    - You can find an example [here](#command-list-reference).
- You should use the [Translation Generator](#translation-generator), if not you need to manually update `ProjectMakoto/Entities/Translation/Translations.cs`.
  - If you add, remove, rename or change the type of keys.
- Adding a new translation is quite simple and you do not need the Translation Generator to do so: Simply locate the key you want to add a translation for, add a comma at the end of the last translation and add a valid locale as key name.

A group usually looks like this:
```json
"TranslationGroup": {
  "SingleTranslationKey": { // Single Translation Keys are usually used for single-line translations.
    "en": "Example",
    "de": "Beispiel"
  },
  "MultiTranslationKey": { // Multi Translation Keys are usually used for multi-line translations or variations.
    "en": [
      "Example Line 1",
      "Example Line 2"
    ],
    "de": [
      "Beispielzeile 1",
      "Beispielzeile 2"
    ]
  }
}
```

Placeholders can be added via `{Placeholder}`, these are values that get replaced at runtime.

## Translation Generator

- Makoto has a tool allowing you to automatically generate the class used to reference the translations.
- You can build and start this tool by using the `RunTranslationGenerator.sh` in `ProjectMakoto` or it's official plugins.

### Command List Reference

- Valid Locales are: `en`, `de`, `da`, `fr`, `hr`, `it`, `lt`, `hu`, `nl`, `no`, `pl`, `ro`, `fi`, `vi`, `tr`, `cs`, `el`, `bg`, `ru`, `uk`, `hi`, `th`, `ja`, `ko`, `pt-BR`, `sv-SE`, `zh-CN`, `zh-TW`, `es-ES`.
    - `en-GB` and `en-US` use `en`.
    - You can also find a (probably) more up to date version [here](https://docs.dcs.aitsys.dev/articles/modules/application_commands/translations/reference#valid-locales).

```json
{
  "Type": 1, // 1 = Slash Command, 2 = User Context Menu, 3 = Message Context Menu
  "Names": {
    "en": "example",
    "de": "beispiel"
  },
  "Descriptions": { // If the type is not 1, Descriptions will not be sent to Discord, but are still required for the help command.
    "en": "Example Description",
    "de": "Beispiel Beschreibung"
  },
  "Options": [ // Options are user-selected input.
    {
      "Names": {
        "en": "example1",
        "de": "beispiel1"
      },
      "Descriptions": {
        "en": "Example Description 2",
        "de": "Beispiel Beschreibung 2"
      },
      "Choices": [ // Choices are enums, provided by the bot.
        {
          "Names": {
            "en": "example",
            "de": "beispiel"
          }
        }
      ]
    },
    {
      "Names": {
        "en": "example2",
        "de": "beispiel2"
      },
      "Descriptions": {
        "en": "Example Description 2",
        "de": "Beispiel Beschreibung 2"
      }
    }
  ],
  "Commands": [ // Commands are Sub-Commands of a group.
    {
      "Names": {
        "en": "example1",
        "de": "beispiel1"
      },
      "Descriptions": {
        "en": "Example Description 2",
        "de": "Beispiel Beschreibung 2"
      }
    },
    {
      "Names": {
        "en": "example2",
        "de": "beispiel2"
      },
      "Descriptions": {
        "en": "Example Description 2",
        "de": "Beispiel Beschreibung 2"
      }
    }
  ]
}
```
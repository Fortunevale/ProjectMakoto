cd ../Tools/TranslationSourceGenerator
dotnet restore
dotnet run -- ../../ProjectMakoto/Translations/strings.json ../../ProjectMakoto/Entities/Translation/Translations.cs ProjectMakoto.Entities
sleep 60
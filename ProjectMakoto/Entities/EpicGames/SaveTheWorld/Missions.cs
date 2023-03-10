namespace ProjectMakoto.Entities.EpicGames;

internal class Missions
{
    public Theater[] theaters { get; set; }
    public Mission[] missions { get; set; }
    public MissionAlert[] missionAlerts { get; set; }

    public class Theater
    {
        public string uniqueId { get; set; }
        public TranslatedString displayName { get; set; }
        public TranslatedString description { get; set; }
        public int theaterSlot { get; set; }
        public bool bIsTestTheater { get; set; }
        public bool bHideLikeTestTheater { get; set; }
        public string requiredEventFlag { get; set; }
        public string missionRewardNamedWeightsRowName { get; set; }
        
        public Runtimeinfo runtimeInfo { get; set; }
        public Tile[] tiles { get; set; }
        public Region[] regions { get; set; }
    }

    public class TranslatedString
    {
        public string de { get; set; }
        public string ru { get; set; }
        public string ko { get; set; }
        public string zhhant { get; set; }
        public string ptbr { get; set; }
        public string en { get; set; }
        public string it { get; set; }
        public string fr { get; set; }
        public string zhcn { get; set; }
        public string es { get; set; }
        public string ar { get; set; }
        public string ja { get; set; }
        public string pl { get; set; }
        public string es419 { get; set; }
        public string tr { get; set; }
    }

    public class Runtimeinfo
    {
        public string theaterType { get; set; }
        public Theatertags theaterTags { get; set; }
        public Eventdependenttheatertag[] eventDependentTheaterTags { get; set; }
        public Theatervisibilityrequirements theaterVisibilityRequirements { get; set; }
        public Requirements requirements { get; set; }
        public string requiredSubGameForVisibility { get; set; }
        public bool bOnlyMatchLinkedQuestsToTiles { get; set; }
        public string worldMapPinClass { get; set; }
        public string theaterImage { get; set; }
        public Theaterimages theaterImages { get; set; }
        public Theatercolorinfo theaterColorInfo { get; set; }
        public string socket { get; set; }
        public Missionalertrequirements missionAlertRequirements { get; set; }
        public Missionalertcategoryrequirement[] missionAlertCategoryRequirements { get; set; }
        public Gameplaymodifierlist[] gameplayModifierList { get; set; }
    }

    public class Theatertags
    {
        public Gameplaytag[] gameplayTags { get; set; }
    }

    public class Gameplaytag
    {
        public string tagName { get; set; }
    }

    public class Theatervisibilityrequirements
    {
        public int commanderLevel { get; set; }
        public int personalPowerRating { get; set; }
        public int maxPersonalPowerRating { get; set; }
        public int partyPowerRating { get; set; }
        public int maxPartyPowerRating { get; set; }
        public string[] activeQuestDefinitions { get; set; }
        public string questDefinition { get; set; }
        public Objectivestathandle objectiveStatHandle { get; set; }
        public string uncompletedQuestDefinition { get; set; }
        public string itemDefinition { get; set; }
        public string eventFlag { get; set; }
    }

    public class Objectivestathandle
    {
        public string dataTable { get; set; }
        public string rowName { get; set; }
    }

    public class Requirements
    {
        public int commanderLevel { get; set; }
        public int personalPowerRating { get; set; }
        public int maxPersonalPowerRating { get; set; }
        public int partyPowerRating { get; set; }
        public int maxPartyPowerRating { get; set; }
        public object[] activeQuestDefinitions { get; set; }
        public string questDefinition { get; set; }
        public Objectivestathandle1 objectiveStatHandle { get; set; }
        public string uncompletedQuestDefinition { get; set; }
        public string itemDefinition { get; set; }
        public string eventFlag { get; set; }
    }

    public class Objectivestathandle1
    {
        public string dataTable { get; set; }
        public string rowName { get; set; }
    }

    public class Theaterimages
    {
        public Brush_XXS brush_XXS { get; set; }
        public Brush_XS brush_XS { get; set; }
        public Brush_S brush_S { get; set; }
        public Brush_M brush_M { get; set; }
        public Brush_L brush_L { get; set; }
        public Brush_XL brush_XL { get; set; }
    }

    public class Brush_XXS
    {
        public bool bIsDynamicallyLoaded { get; set; }
        public string drawAs { get; set; }
        public string tiling { get; set; }
        public string mirroring { get; set; }
        public string imageType { get; set; }
        public Imagesize imageSize { get; set; }
        public Margin margin { get; set; }
        public Tintcolor tintColor { get; set; }
        public Outlinesettings outlineSettings { get; set; }
        public string resourceObject { get; set; }
        public string resourceName { get; set; }
        public Uvregion uVRegion { get; set; }
    }

    public class Imagesize
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class Margin
    {
        public int left { get; set; }
        public int top { get; set; }
        public int right { get; set; }
        public int bottom { get; set; }
    }

    public class Tintcolor
    {
        public Specifiedcolor specifiedColor { get; set; }
        public string colorUseRule { get; set; }
    }

    public class Specifiedcolor
    {
        public int r { get; set; }
        public int g { get; set; }
        public int b { get; set; }
        public int a { get; set; }
    }

    public class Outlinesettings
    {
        public Cornerradii cornerRadii { get; set; }
        public Color color { get; set; }
        public int width { get; set; }
        public string roundingType { get; set; }
        public bool bUseBrushTransparency { get; set; }
    }

    public class Cornerradii
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
        public int w { get; set; }
    }

    public class Color
    {
        public Specifiedcolor1 specifiedColor { get; set; }
        public string colorUseRule { get; set; }
    }

    public class Specifiedcolor1
    {
        public int r { get; set; }
        public int g { get; set; }
        public int b { get; set; }
        public int a { get; set; }
    }

    public class Uvregion
    {
        public Min min { get; set; }
        public Max max { get; set; }
        public int bIsValid { get; set; }
    }

    public class Min
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class Max
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class Brush_XS
    {
        public bool bIsDynamicallyLoaded { get; set; }
        public string drawAs { get; set; }
        public string tiling { get; set; }
        public string mirroring { get; set; }
        public string imageType { get; set; }
        public Imagesize1 imageSize { get; set; }
        public Margin1 margin { get; set; }
        public Tintcolor1 tintColor { get; set; }
        public Outlinesettings1 outlineSettings { get; set; }
        public string resourceObject { get; set; }
        public string resourceName { get; set; }
        public Uvregion1 uVRegion { get; set; }
    }

    public class Imagesize1
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class Margin1
    {
        public int left { get; set; }
        public int top { get; set; }
        public int right { get; set; }
        public int bottom { get; set; }
    }

    public class Tintcolor1
    {
        public Specifiedcolor2 specifiedColor { get; set; }
        public string colorUseRule { get; set; }
    }

    public class Specifiedcolor2
    {
        public int r { get; set; }
        public int g { get; set; }
        public int b { get; set; }
        public int a { get; set; }
    }

    public class Outlinesettings1
    {
        public Cornerradii1 cornerRadii { get; set; }
        public Color1 color { get; set; }
        public int width { get; set; }
        public string roundingType { get; set; }
        public bool bUseBrushTransparency { get; set; }
    }

    public class Cornerradii1
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
        public int w { get; set; }
    }

    public class Color1
    {
        public Specifiedcolor3 specifiedColor { get; set; }
        public string colorUseRule { get; set; }
    }

    public class Specifiedcolor3
    {
        public int r { get; set; }
        public int g { get; set; }
        public int b { get; set; }
        public int a { get; set; }
    }

    public class Uvregion1
    {
        public Min1 min { get; set; }
        public Max1 max { get; set; }
        public int bIsValid { get; set; }
    }

    public class Min1
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class Max1
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class Brush_S
    {
        public bool bIsDynamicallyLoaded { get; set; }
        public string drawAs { get; set; }
        public string tiling { get; set; }
        public string mirroring { get; set; }
        public string imageType { get; set; }
        public Imagesize2 imageSize { get; set; }
        public Margin2 margin { get; set; }
        public Tintcolor2 tintColor { get; set; }
        public Outlinesettings2 outlineSettings { get; set; }
        public string resourceObject { get; set; }
        public string resourceName { get; set; }
        public Uvregion2 uVRegion { get; set; }
    }

    public class Imagesize2
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class Margin2
    {
        public int left { get; set; }
        public int top { get; set; }
        public int right { get; set; }
        public int bottom { get; set; }
    }

    public class Tintcolor2
    {
        public Specifiedcolor4 specifiedColor { get; set; }
        public string colorUseRule { get; set; }
    }

    public class Specifiedcolor4
    {
        public int r { get; set; }
        public int g { get; set; }
        public int b { get; set; }
        public int a { get; set; }
    }

    public class Outlinesettings2
    {
        public Cornerradii2 cornerRadii { get; set; }
        public Color2 color { get; set; }
        public int width { get; set; }
        public string roundingType { get; set; }
        public bool bUseBrushTransparency { get; set; }
    }

    public class Cornerradii2
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
        public int w { get; set; }
    }

    public class Color2
    {
        public Specifiedcolor5 specifiedColor { get; set; }
        public string colorUseRule { get; set; }
    }

    public class Specifiedcolor5
    {
        public int r { get; set; }
        public int g { get; set; }
        public int b { get; set; }
        public int a { get; set; }
    }

    public class Uvregion2
    {
        public Min2 min { get; set; }
        public Max2 max { get; set; }
        public int bIsValid { get; set; }
    }

    public class Min2
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class Max2
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class Brush_M
    {
        public bool bIsDynamicallyLoaded { get; set; }
        public string drawAs { get; set; }
        public string tiling { get; set; }
        public string mirroring { get; set; }
        public string imageType { get; set; }
        public Imagesize3 imageSize { get; set; }
        public Margin3 margin { get; set; }
        public Tintcolor3 tintColor { get; set; }
        public Outlinesettings3 outlineSettings { get; set; }
        public string resourceObject { get; set; }
        public string resourceName { get; set; }
        public Uvregion3 uVRegion { get; set; }
    }

    public class Imagesize3
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class Margin3
    {
        public int left { get; set; }
        public int top { get; set; }
        public int right { get; set; }
        public int bottom { get; set; }
    }

    public class Tintcolor3
    {
        public Specifiedcolor6 specifiedColor { get; set; }
        public string colorUseRule { get; set; }
    }

    public class Specifiedcolor6
    {
        public int r { get; set; }
        public int g { get; set; }
        public int b { get; set; }
        public int a { get; set; }
    }

    public class Outlinesettings3
    {
        public Cornerradii3 cornerRadii { get; set; }
        public Color3 color { get; set; }
        public int width { get; set; }
        public string roundingType { get; set; }
        public bool bUseBrushTransparency { get; set; }
    }

    public class Cornerradii3
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
        public int w { get; set; }
    }

    public class Color3
    {
        public Specifiedcolor7 specifiedColor { get; set; }
        public string colorUseRule { get; set; }
    }

    public class Specifiedcolor7
    {
        public int r { get; set; }
        public int g { get; set; }
        public int b { get; set; }
        public int a { get; set; }
    }

    public class Uvregion3
    {
        public Min3 min { get; set; }
        public Max3 max { get; set; }
        public int bIsValid { get; set; }
    }

    public class Min3
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class Max3
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class Brush_L
    {
        public bool bIsDynamicallyLoaded { get; set; }
        public string drawAs { get; set; }
        public string tiling { get; set; }
        public string mirroring { get; set; }
        public string imageType { get; set; }
        public Imagesize4 imageSize { get; set; }
        public Margin4 margin { get; set; }
        public Tintcolor4 tintColor { get; set; }
        public Outlinesettings4 outlineSettings { get; set; }
        public string resourceObject { get; set; }
        public string resourceName { get; set; }
        public Uvregion4 uVRegion { get; set; }
    }

    public class Imagesize4
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class Margin4
    {
        public int left { get; set; }
        public int top { get; set; }
        public int right { get; set; }
        public int bottom { get; set; }
    }

    public class Tintcolor4
    {
        public Specifiedcolor8 specifiedColor { get; set; }
        public string colorUseRule { get; set; }
    }

    public class Specifiedcolor8
    {
        public int r { get; set; }
        public int g { get; set; }
        public int b { get; set; }
        public int a { get; set; }
    }

    public class Outlinesettings4
    {
        public Cornerradii4 cornerRadii { get; set; }
        public Color4 color { get; set; }
        public int width { get; set; }
        public string roundingType { get; set; }
        public bool bUseBrushTransparency { get; set; }
    }

    public class Cornerradii4
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
        public int w { get; set; }
    }

    public class Color4
    {
        public Specifiedcolor9 specifiedColor { get; set; }
        public string colorUseRule { get; set; }
    }

    public class Specifiedcolor9
    {
        public int r { get; set; }
        public int g { get; set; }
        public int b { get; set; }
        public int a { get; set; }
    }

    public class Uvregion4
    {
        public Min4 min { get; set; }
        public Max4 max { get; set; }
        public int bIsValid { get; set; }
    }

    public class Min4
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class Max4
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class Brush_XL
    {
        public bool bIsDynamicallyLoaded { get; set; }
        public string drawAs { get; set; }
        public string tiling { get; set; }
        public string mirroring { get; set; }
        public string imageType { get; set; }
        public Imagesize5 imageSize { get; set; }
        public Margin5 margin { get; set; }
        public Tintcolor5 tintColor { get; set; }
        public Outlinesettings5 outlineSettings { get; set; }
        public string resourceObject { get; set; }
        public string resourceName { get; set; }
        public Uvregion5 uVRegion { get; set; }
    }

    public class Imagesize5
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class Margin5
    {
        public int left { get; set; }
        public int top { get; set; }
        public int right { get; set; }
        public int bottom { get; set; }
    }

    public class Tintcolor5
    {
        public Specifiedcolor10 specifiedColor { get; set; }
        public string colorUseRule { get; set; }
    }

    public class Specifiedcolor10
    {
        public int r { get; set; }
        public int g { get; set; }
        public int b { get; set; }
        public int a { get; set; }
    }

    public class Outlinesettings5
    {
        public Cornerradii5 cornerRadii { get; set; }
        public Color5 color { get; set; }
        public int width { get; set; }
        public string roundingType { get; set; }
        public bool bUseBrushTransparency { get; set; }
    }

    public class Cornerradii5
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
        public int w { get; set; }
    }

    public class Color5
    {
        public Specifiedcolor11 specifiedColor { get; set; }
        public string colorUseRule { get; set; }
    }

    public class Specifiedcolor11
    {
        public int r { get; set; }
        public int g { get; set; }
        public int b { get; set; }
        public int a { get; set; }
    }

    public class Uvregion5
    {
        public Min5 min { get; set; }
        public Max5 max { get; set; }
        public int bIsValid { get; set; }
    }

    public class Min5
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class Max5
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class Theatercolorinfo
    {
        public bool bUseDifficultyToDetermineColor { get; set; }
        public Color6 color { get; set; }
    }

    public class Color6
    {
        public Specifiedcolor12 specifiedColor { get; set; }
        public string colorUseRule { get; set; }
    }

    public class Specifiedcolor12
    {
        public float r { get; set; }
        public float g { get; set; }
        public float b { get; set; }
        public int a { get; set; }
    }

    public class Missionalertrequirements
    {
        public int commanderLevel { get; set; }
        public int personalPowerRating { get; set; }
        public int maxPersonalPowerRating { get; set; }
        public int partyPowerRating { get; set; }
        public int maxPartyPowerRating { get; set; }
        public object[] activeQuestDefinitions { get; set; }
        public string questDefinition { get; set; }
        public Objectivestathandle2 objectiveStatHandle { get; set; }
        public string uncompletedQuestDefinition { get; set; }
        public string itemDefinition { get; set; }
        public string eventFlag { get; set; }
    }

    public class Objectivestathandle2
    {
        public string dataTable { get; set; }
        public string rowName { get; set; }
    }

    public class Eventdependenttheatertag
    {
        public string requiredEventFlag { get; set; }
        public Relatedtag relatedTag { get; set; }
    }

    public class Relatedtag
    {
        public string tagName { get; set; }
    }

    public class Missionalertcategoryrequirement
    {
        public string missionAlertCategoryName { get; set; }
        public bool bRespectTileRequirements { get; set; }
        public bool bAllowQuickplay { get; set; }
    }

    public class Gameplaymodifierlist
    {
        public string eventFlagName { get; set; }
        public string gameplayModifier { get; set; }
    }

    public class Tile
    {
        public string tileType { get; set; }
        public string zoneTheme { get; set; }
        public Requirements1 requirements { get; set; }
        public Linkedquest[] linkedQuests { get; set; }
        public int xCoordinate { get; set; }
        public int yCoordinate { get; set; }
        public Missionweightoverride[] missionWeightOverrides { get; set; }
        public Difficultyweightoverride[] difficultyWeightOverrides { get; set; }
        public bool canBeMissionAlert { get; set; }
        public Tiletags tileTags { get; set; }
        public bool bDisallowQuickplay { get; set; }
    }

    public class Requirements1
    {
        public int commanderLevel { get; set; }
        public int personalPowerRating { get; set; }
        public int maxPersonalPowerRating { get; set; }
        public int partyPowerRating { get; set; }
        public int maxPartyPowerRating { get; set; }
        public string[] activeQuestDefinitions { get; set; }
        public string questDefinition { get; set; }
        public Objectivestathandle3 objectiveStatHandle { get; set; }
        public string uncompletedQuestDefinition { get; set; }
        public string itemDefinition { get; set; }
        public string eventFlag { get; set; }
    }

    public class Objectivestathandle3
    {
        public string dataTable { get; set; }
        public string rowName { get; set; }
    }

    public class Tiletags
    {
        public Gameplaytag1[] gameplayTags { get; set; }
    }

    public class Gameplaytag1
    {
        public string tagName { get; set; }
    }

    public class Linkedquest
    {
        public string questDefinition { get; set; }
        public Objectivestathandle4 objectiveStatHandle { get; set; }
    }

    public class Objectivestathandle4
    {
        public string dataTable { get; set; }
        public string rowName { get; set; }
    }

    public class Missionweightoverride
    {
        public float weight { get; set; }
        public string missionGenerator { get; set; }
    }

    public class Difficultyweightoverride
    {
        public float weight { get; set; }
        public Difficultyinfo difficultyInfo { get; set; }
    }

    public class Difficultyinfo
    {
        public string dataTable { get; set; }
        public string rowName { get; set; }
    }

    public class Region
    {
        public Displayname1 displayName { get; set; }
        public string uniqueId { get; set; }
        public Regiontags regionTags { get; set; }
        public int[] tileIndices { get; set; }
        public string regionThemeIcon { get; set; }
        public Missiondata missionData { get; set; }
        public Requirements2 requirements { get; set; }
        public Missionalertrequirement[] missionAlertRequirements { get; set; }
    }

    public class Displayname1
    {
        public string de { get; set; }
        public string ru { get; set; }
        public string ko { get; set; }
        public string zhhant { get; set; }
        public string ptbr { get; set; }
        public string en { get; set; }
        public string it { get; set; }
        public string fr { get; set; }
        public string zhcn { get; set; }
        public string es { get; set; }
        public string ar { get; set; }
        public string ja { get; set; }
        public string pl { get; set; }
        public string es419 { get; set; }
        public string tr { get; set; }
    }

    public class Regiontags
    {
        public Gameplaytag2[] gameplayTags { get; set; }
    }

    public class Gameplaytag2
    {
        public string tagName { get; set; }
    }

    public class Missiondata
    {
        public Missionweight[] missionWeights { get; set; }
        public Difficultyweight[] difficultyWeights { get; set; }
        public int numMissionsAvailable { get; set; }
        public int numMissionsToChange { get; set; }
        public float missionChangeFrequency { get; set; }
    }

    public class Missionweight
    {
        public float weight { get; set; }
        public string missionGenerator { get; set; }
    }

    public class Difficultyweight
    {
        public float weight { get; set; }
        public Difficultyinfo1 difficultyInfo { get; set; }
    }

    public class Difficultyinfo1
    {
        public string dataTable { get; set; }
        public string rowName { get; set; }
    }

    public class Requirements2
    {
        public int commanderLevel { get; set; }
        public int personalPowerRating { get; set; }
        public int maxPersonalPowerRating { get; set; }
        public int partyPowerRating { get; set; }
        public int maxPartyPowerRating { get; set; }
        public object[] activeQuestDefinitions { get; set; }
        public string questDefinition { get; set; }
        public Objectivestathandle5 objectiveStatHandle { get; set; }
        public string uncompletedQuestDefinition { get; set; }
        public string itemDefinition { get; set; }
        public string eventFlag { get; set; }
    }

    public class Objectivestathandle5
    {
        public string dataTable { get; set; }
        public string rowName { get; set; }
    }

    public class Missionalertrequirement
    {
        public string categoryName { get; set; }
        public Requirements3 requirements { get; set; }
    }

    public class Requirements3
    {
        public int commanderLevel { get; set; }
        public int personalPowerRating { get; set; }
        public int maxPersonalPowerRating { get; set; }
        public int partyPowerRating { get; set; }
        public int maxPartyPowerRating { get; set; }
        public object[] activeQuestDefinitions { get; set; }
        public string questDefinition { get; set; }
        public Objectivestathandle6 objectiveStatHandle { get; set; }
        public string uncompletedQuestDefinition { get; set; }
        public string itemDefinition { get; set; }
        public string eventFlag { get; set; }
    }

    public class Objectivestathandle6
    {
        public string dataTable { get; set; }
        public string rowName { get; set; }
    }

    public class Mission
    {
        public string theaterId { get; set; }
        public Availablemission[] availableMissions { get; set; }
        public DateTime nextRefresh { get; set; }
    }

    public class Availablemission
    {
        public string missionGuid { get; set; }
        public Missionrewards missionRewards { get; set; }
        public Overridemissionrewards overrideMissionRewards { get; set; }
        public string missionGenerator { get; set; }
        public Missiondifficultyinfo missionDifficultyInfo { get; set; }
        public int tileIndex { get; set; }
        public DateTime availableUntil { get; set; }
        public Bonusmissionrewards bonusMissionRewards { get; set; }
    }

    public class Missionrewards
    {
        public string tierGroupName { get; set; }
        public Item[] items { get; set; }
    }

    public class Overridemissionrewards
    {
        public Endurance Endurance { get; set; }
        public Wargames Wargames { get; set; }
    }

    public class Endurance
    {
        public string tierGroupName { get; set; }
        public Item[] items { get; set; }
    }

    public class Wargames
    {
        public string tierGroupName { get; set; }
        public Item[] items { get; set; }
    }

    public class Missiondifficultyinfo
    {
        public string dataTable { get; set; }
        public string rowName { get; set; }
    }

    public class Bonusmissionrewards
    {
        public string tierGroupName { get; set; }
        public Item[] items { get; set; }
    }

    public class MissionAlert
    {
        public string theaterId { get; set; }
        public AvailableMissionAlert[] availableMissionAlerts { get; set; }
        public DateTime nextRefresh { get; set; }
    }

    public class AvailableMissionAlert
    {
        public string name { get; set; }
        public string categoryName { get; set; }
        public string spreadDataName { get; set; }
        public string missionAlertGuid { get; set; }
        public int tileIndex { get; set; }
        public DateTime availableUntil { get; set; }
        public int totalSpreadRefreshes { get; set; }
        public MissionAlertRewards missionAlertRewards { get; set; }
        public MissionAlertModifiers missionAlertModifiers { get; set; }
    }

    public class MissionAlertRewards
    {
        public string tierGroupName { get; set; }
        public Item[] items { get; set; }
    }

    public class Item
    {
        public string itemType { get; set; }
        public int quantity { get; set; }
        public Attributes attributes { get; set; }
    }

    public class Attributes
    {
        public alteration Alteration { get; set; }

        public class alteration
        {
            public string LootTierGroup { get; set; }
            public int Tier { get; set; }
        }
    }

    public class MissionAlertModifiers
    {
        public string tierGroupName { get; set; }
        public Item[] items { get; set; }
    }
}

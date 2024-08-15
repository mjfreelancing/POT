//using System.Text.Json.Serialization;

//namespace Pot.Maui
//{
//    // material-theme-1.json
//    public class AppTheme
//    {
//        public Schemes Schemes { get; init; } = new();
//        public Palettes Palettes { get; init; } = new();
//    }

//    public class Dark
//    {
//        public string Primary { get; init; } = string.Empty;
//        public string SurfaceTint { get; init; } = string.Empty;
//        public string OnPrimary { get; init; } = string.Empty;
//        public string PrimaryContainer { get; init; } = string.Empty;
//        public string OnPrimaryContainer { get; init; } = string.Empty;
//        public string Secondary { get; init; } = string.Empty;
//        public string OnSecondary { get; init; } = string.Empty;
//        public string SecondaryContainer { get; init; } = string.Empty;
//        public string OnSecondaryContainer { get; init; } = string.Empty;
//        public string Tertiary { get; init; } = string.Empty;
//        public string OnTertiary { get; init; } = string.Empty;
//        public string TertiaryContainer { get; init; } = string.Empty;
//        public string OnTertiaryContainer { get; init; } = string.Empty;
//        public string Error { get; init; } = string.Empty;
//        public string OnError { get; init; } = string.Empty;
//        public string ErrorContainer { get; init; } = string.Empty;
//        public string OnErrorContainer { get; init; } = string.Empty;
//        public string Background { get; init; } = string.Empty;
//        public string OnBackground { get; init; } = string.Empty;
//        public string Surface { get; init; } = string.Empty;
//        public string OnSurface { get; init; } = string.Empty;
//        public string SurfaceVariant { get; init; } = string.Empty;
//        public string OnSurfaceVariant { get; init; } = string.Empty;
//        public string Outline { get; init; } = string.Empty;
//        public string OutlineVariant { get; init; } = string.Empty;
//        public string Shadow { get; init; } = string.Empty;
//        public string Scrim { get; init; } = string.Empty;
//        public string InverseSurface { get; init; } = string.Empty;
//        public string InverseOnSurface { get; init; } = string.Empty;
//        public string InversePrimary { get; init; } = string.Empty;
//        public string PrimaryFixed { get; init; } = string.Empty;
//        public string OnPrimaryFixed { get; init; } = string.Empty;
//        public string PrimaryFixedDim { get; init; } = string.Empty;
//        public string OnPrimaryFixedVariant { get; init; } = string.Empty;
//        public string SecondaryFixed { get; init; } = string.Empty;
//        public string OnSecondaryFixed { get; init; } = string.Empty;
//        public string SecondaryFixedDim { get; init; } = string.Empty;
//        public string OnSecondaryFixedVariant { get; init; } = string.Empty;
//        public string TertiaryFixed { get; init; } = string.Empty;
//        public string OnTertiaryFixed { get; init; } = string.Empty;
//        public string TertiaryFixedDim { get; init; } = string.Empty;
//        public string OnTertiaryFixedVariant { get; init; } = string.Empty;
//        public string SurfaceDim { get; init; } = string.Empty;
//        public string SurfaceBright { get; init; } = string.Empty;
//        public string SurfaceContainerLowest { get; init; } = string.Empty;
//        public string SurfaceContainerLow { get; init; } = string.Empty;
//        public string SurfaceContainer { get; init; } = string.Empty;
//        public string SurfaceContainerHigh { get; init; } = string.Empty;
//        public string SurfaceContainerHighest { get; init; } = string.Empty;
//    }

//    public class DarkHighContrast
//    {
//        public string Primary { get; init; } = string.Empty;
//        public string SurfaceTint { get; init; } = string.Empty;
//        public string OnPrimary { get; init; } = string.Empty;
//        public string PrimaryContainer { get; init; } = string.Empty;
//        public string OnPrimaryContainer { get; init; } = string.Empty;
//        public string Secondary { get; init; } = string.Empty;
//        public string OnSecondary { get; init; } = string.Empty;
//        public string SecondaryContainer { get; init; } = string.Empty;
//        public string OnSecondaryContainer { get; init; } = string.Empty;
//        public string Tertiary { get; init; } = string.Empty;
//        public string OnTertiary { get; init; } = string.Empty;
//        public string TertiaryContainer { get; init; } = string.Empty;
//        public string OnTertiaryContainer { get; init; } = string.Empty;
//        public string Error { get; init; } = string.Empty;
//        public string OnError { get; init; } = string.Empty;
//        public string ErrorContainer { get; init; } = string.Empty;
//        public string OnErrorContainer { get; init; } = string.Empty;
//        public string Background { get; init; } = string.Empty;
//        public string OnBackground { get; init; } = string.Empty;
//        public string Surface { get; init; } = string.Empty;
//        public string OnSurface { get; init; } = string.Empty;
//        public string SurfaceVariant { get; init; } = string.Empty;
//        public string OnSurfaceVariant { get; init; } = string.Empty;
//        public string Outline { get; init; } = string.Empty;
//        public string OutlineVariant { get; init; } = string.Empty;
//        public string Shadow { get; init; } = string.Empty;
//        public string Scrim { get; init; } = string.Empty;
//        public string InverseSurface { get; init; } = string.Empty;
//        public string InverseOnSurface { get; init; } = string.Empty;
//        public string InversePrimary { get; init; } = string.Empty;
//        public string PrimaryFixed { get; init; } = string.Empty;
//        public string OnPrimaryFixed { get; init; } = string.Empty;
//        public string PrimaryFixedDim { get; init; } = string.Empty;
//        public string OnPrimaryFixedVariant { get; init; } = string.Empty;
//        public string SecondaryFixed { get; init; } = string.Empty;
//        public string OnSecondaryFixed { get; init; } = string.Empty;
//        public string SecondaryFixedDim { get; init; } = string.Empty;
//        public string OnSecondaryFixedVariant { get; init; } = string.Empty;
//        public string TertiaryFixed { get; init; } = string.Empty;
//        public string OnTertiaryFixed { get; init; } = string.Empty;
//        public string TertiaryFixedDim { get; init; } = string.Empty;
//        public string OnTertiaryFixedVariant { get; init; } = string.Empty;
//        public string SurfaceDim { get; init; } = string.Empty;
//        public string SurfaceBright { get; init; } = string.Empty;
//        public string SurfaceContainerLowest { get; init; } = string.Empty;
//        public string SurfaceContainerLow { get; init; } = string.Empty;
//        public string SurfaceContainer { get; init; } = string.Empty;
//        public string SurfaceContainerHigh { get; init; } = string.Empty;
//        public string SurfaceContainerHighest { get; init; } = string.Empty;
//    }

//    public class DarkMediumContrast
//    {
//        public string Primary { get; init; } = string.Empty;
//        public string SurfaceTint { get; init; } = string.Empty;
//        public string OnPrimary { get; init; } = string.Empty;
//        public string PrimaryContainer { get; init; } = string.Empty;
//        public string OnPrimaryContainer { get; init; } = string.Empty;
//        public string Secondary { get; init; } = string.Empty;
//        public string OnSecondary { get; init; } = string.Empty;
//        public string SecondaryContainer { get; init; } = string.Empty;
//        public string OnSecondaryContainer { get; init; } = string.Empty;
//        public string Tertiary { get; init; } = string.Empty;
//        public string OnTertiary { get; init; } = string.Empty;
//        public string TertiaryContainer { get; init; } = string.Empty;
//        public string OnTertiaryContainer { get; init; } = string.Empty;
//        public string Error { get; init; } = string.Empty;
//        public string OnError { get; init; } = string.Empty;
//        public string ErrorContainer { get; init; } = string.Empty;
//        public string OnErrorContainer { get; init; } = string.Empty;
//        public string Background { get; init; } = string.Empty;
//        public string OnBackground { get; init; } = string.Empty;
//        public string Surface { get; init; } = string.Empty;
//        public string OnSurface { get; init; } = string.Empty;
//        public string SurfaceVariant { get; init; } = string.Empty;
//        public string OnSurfaceVariant { get; init; } = string.Empty;
//        public string Outline { get; init; } = string.Empty;
//        public string OutlineVariant { get; init; } = string.Empty;
//        public string Shadow { get; init; } = string.Empty;
//        public string Scrim { get; init; } = string.Empty;
//        public string InverseSurface { get; init; } = string.Empty;
//        public string InverseOnSurface { get; init; } = string.Empty;
//        public string InversePrimary { get; init; } = string.Empty;
//        public string PrimaryFixed { get; init; } = string.Empty;
//        public string OnPrimaryFixed { get; init; } = string.Empty;
//        public string PrimaryFixedDim { get; init; } = string.Empty;
//        public string OnPrimaryFixedVariant { get; init; } = string.Empty;
//        public string SecondaryFixed { get; init; } = string.Empty;
//        public string OnSecondaryFixed { get; init; } = string.Empty;
//        public string SecondaryFixedDim { get; init; } = string.Empty;
//        public string OnSecondaryFixedVariant { get; init; } = string.Empty;
//        public string TertiaryFixed { get; init; } = string.Empty;
//        public string OnTertiaryFixed { get; init; } = string.Empty;
//        public string TertiaryFixedDim { get; init; } = string.Empty;
//        public string OnTertiaryFixedVariant { get; init; } = string.Empty;
//        public string SurfaceDim { get; init; } = string.Empty;
//        public string SurfaceBright { get; init; } = string.Empty;
//        public string SurfaceContainerLowest { get; init; } = string.Empty;
//        public string SurfaceContainerLow { get; init; } = string.Empty;
//        public string SurfaceContainer { get; init; } = string.Empty;
//        public string SurfaceContainerHigh { get; init; } = string.Empty;
//        public string SurfaceContainerHighest { get; init; } = string.Empty;
//    }

//    public class Light
//    {
//        public string Primary { get; init; } = string.Empty;
//        public string SurfaceTint { get; init; } = string.Empty;
//        public string OnPrimary { get; init; } = string.Empty;
//        public string PrimaryContainer { get; init; } = string.Empty;
//        public string OnPrimaryContainer { get; init; } = string.Empty;
//        public string Secondary { get; init; } = string.Empty;
//        public string OnSecondary { get; init; } = string.Empty;
//        public string SecondaryContainer { get; init; } = string.Empty;
//        public string OnSecondaryContainer { get; init; } = string.Empty;
//        public string Tertiary { get; init; } = string.Empty;
//        public string OnTertiary { get; init; } = string.Empty;
//        public string TertiaryContainer { get; init; } = string.Empty;
//        public string OnTertiaryContainer { get; init; } = string.Empty;
//        public string Error { get; init; } = string.Empty;
//        public string OnError { get; init; } = string.Empty;
//        public string ErrorContainer { get; init; } = string.Empty;
//        public string OnErrorContainer { get; init; } = string.Empty;
//        public string Background { get; init; } = string.Empty;
//        public string OnBackground { get; init; } = string.Empty;
//        public string Surface { get; init; } = string.Empty;
//        public string OnSurface { get; init; } = string.Empty;
//        public string SurfaceVariant { get; init; } = string.Empty;
//        public string OnSurfaceVariant { get; init; } = string.Empty;
//        public string Outline { get; init; } = string.Empty;
//        public string OutlineVariant { get; init; } = string.Empty;
//        public string Shadow { get; init; } = string.Empty;
//        public string Scrim { get; init; } = string.Empty;
//        public string InverseSurface { get; init; } = string.Empty;
//        public string InverseOnSurface { get; init; } = string.Empty;
//        public string InversePrimary { get; init; } = string.Empty;
//        public string PrimaryFixed { get; init; } = string.Empty;
//        public string OnPrimaryFixed { get; init; } = string.Empty;
//        public string PrimaryFixedDim { get; init; } = string.Empty;
//        public string OnPrimaryFixedVariant { get; init; } = string.Empty;
//        public string SecondaryFixed { get; init; } = string.Empty;
//        public string OnSecondaryFixed { get; init; } = string.Empty;
//        public string SecondaryFixedDim { get; init; } = string.Empty;
//        public string OnSecondaryFixedVariant { get; init; } = string.Empty;
//        public string TertiaryFixed { get; init; } = string.Empty;
//        public string OnTertiaryFixed { get; init; } = string.Empty;
//        public string TertiaryFixedDim { get; init; } = string.Empty;
//        public string OnTertiaryFixedVariant { get; init; } = string.Empty;
//        public string SurfaceDim { get; init; } = string.Empty;
//        public string SurfaceBright { get; init; } = string.Empty;
//        public string SurfaceContainerLowest { get; init; } = string.Empty;
//        public string SurfaceContainerLow { get; init; } = string.Empty;
//        public string SurfaceContainer { get; init; } = string.Empty;
//        public string SurfaceContainerHigh { get; init; } = string.Empty;
//        public string SurfaceContainerHighest { get; init; } = string.Empty;
//    }

//    public class LightHighContrast
//    {
//        public string Primary { get; init; } = string.Empty;
//        public string SurfaceTint { get; init; } = string.Empty;
//        public string OnPrimary { get; init; } = string.Empty;
//        public string PrimaryContainer { get; init; } = string.Empty;
//        public string OnPrimaryContainer { get; init; } = string.Empty;
//        public string Secondary { get; init; } = string.Empty;
//        public string OnSecondary { get; init; } = string.Empty;
//        public string SecondaryContainer { get; init; } = string.Empty;
//        public string OnSecondaryContainer { get; init; } = string.Empty;
//        public string Tertiary { get; init; } = string.Empty;
//        public string OnTertiary { get; init; } = string.Empty;
//        public string TertiaryContainer { get; init; } = string.Empty;
//        public string OnTertiaryContainer { get; init; } = string.Empty;
//        public string Error { get; init; } = string.Empty;
//        public string OnError { get; init; } = string.Empty;
//        public string ErrorContainer { get; init; } = string.Empty;
//        public string OnErrorContainer { get; init; } = string.Empty;
//        public string Background { get; init; } = string.Empty;
//        public string OnBackground { get; init; } = string.Empty;
//        public string Surface { get; init; } = string.Empty;
//        public string OnSurface { get; init; } = string.Empty;
//        public string SurfaceVariant { get; init; } = string.Empty;
//        public string OnSurfaceVariant { get; init; } = string.Empty;
//        public string Outline { get; init; } = string.Empty;
//        public string OutlineVariant { get; init; } = string.Empty;
//        public string Shadow { get; init; } = string.Empty;
//        public string Scrim { get; init; } = string.Empty;
//        public string InverseSurface { get; init; } = string.Empty;
//        public string InverseOnSurface { get; init; } = string.Empty;
//        public string InversePrimary { get; init; } = string.Empty;
//        public string PrimaryFixed { get; init; } = string.Empty;
//        public string OnPrimaryFixed { get; init; } = string.Empty;
//        public string PrimaryFixedDim { get; init; } = string.Empty;
//        public string OnPrimaryFixedVariant { get; init; } = string.Empty;
//        public string SecondaryFixed { get; init; } = string.Empty;
//        public string OnSecondaryFixed { get; init; } = string.Empty;
//        public string SecondaryFixedDim { get; init; } = string.Empty;
//        public string OnSecondaryFixedVariant { get; init; } = string.Empty;
//        public string TertiaryFixed { get; init; } = string.Empty;
//        public string OnTertiaryFixed { get; init; } = string.Empty;
//        public string TertiaryFixedDim { get; init; } = string.Empty;
//        public string OnTertiaryFixedVariant { get; init; } = string.Empty;
//        public string SurfaceDim { get; init; } = string.Empty;
//        public string SurfaceBright { get; init; } = string.Empty;
//        public string SurfaceContainerLowest { get; init; } = string.Empty;
//        public string SurfaceContainerLow { get; init; } = string.Empty;
//        public string SurfaceContainer { get; init; } = string.Empty;
//        public string SurfaceContainerHigh { get; init; } = string.Empty;
//        public string SurfaceContainerHighest { get; init; } = string.Empty;
//    }

//    public class LightMediumContrast
//    {
//        public string Primary { get; init; } = string.Empty;
//        public string SurfaceTint { get; init; } = string.Empty;
//        public string OnPrimary { get; init; } = string.Empty;
//        public string PrimaryContainer { get; init; } = string.Empty;
//        public string OnPrimaryContainer { get; init; } = string.Empty;
//        public string Secondary { get; init; } = string.Empty;
//        public string OnSecondary { get; init; } = string.Empty;
//        public string SecondaryContainer { get; init; } = string.Empty;
//        public string OnSecondaryContainer { get; init; } = string.Empty;
//        public string Tertiary { get; init; } = string.Empty;
//        public string OnTertiary { get; init; } = string.Empty;
//        public string TertiaryContainer { get; init; } = string.Empty;
//        public string OnTertiaryContainer { get; init; } = string.Empty;
//        public string Error { get; init; } = string.Empty;
//        public string OnError { get; init; } = string.Empty;
//        public string ErrorContainer { get; init; } = string.Empty;
//        public string OnErrorContainer { get; init; } = string.Empty;
//        public string Background { get; init; } = string.Empty;
//        public string OnBackground { get; init; } = string.Empty;
//        public string Surface { get; init; } = string.Empty;
//        public string OnSurface { get; init; } = string.Empty;
//        public string SurfaceVariant { get; init; } = string.Empty;
//        public string OnSurfaceVariant { get; init; } = string.Empty;
//        public string Outline { get; init; } = string.Empty;
//        public string OutlineVariant { get; init; } = string.Empty;
//        public string Shadow { get; init; } = string.Empty;
//        public string Scrim { get; init; } = string.Empty;
//        public string InverseSurface { get; init; } = string.Empty;
//        public string InverseOnSurface { get; init; } = string.Empty;
//        public string InversePrimary { get; init; } = string.Empty;
//        public string PrimaryFixed { get; init; } = string.Empty;
//        public string OnPrimaryFixed { get; init; } = string.Empty;
//        public string PrimaryFixedDim { get; init; } = string.Empty;
//        public string OnPrimaryFixedVariant { get; init; } = string.Empty;
//        public string SecondaryFixed { get; init; } = string.Empty;
//        public string OnSecondaryFixed { get; init; } = string.Empty;
//        public string SecondaryFixedDim { get; init; } = string.Empty;
//        public string OnSecondaryFixedVariant { get; init; } = string.Empty;
//        public string TertiaryFixed { get; init; } = string.Empty;
//        public string OnTertiaryFixed { get; init; } = string.Empty;
//        public string TertiaryFixedDim { get; init; } = string.Empty;
//        public string OnTertiaryFixedVariant { get; init; } = string.Empty;
//        public string SurfaceDim { get; init; } = string.Empty;
//        public string SurfaceBright { get; init; } = string.Empty;
//        public string SurfaceContainerLowest { get; init; } = string.Empty;
//        public string SurfaceContainerLow { get; init; } = string.Empty;
//        public string SurfaceContainer { get; init; } = string.Empty;
//        public string SurfaceContainerHigh { get; init; } = string.Empty;
//        public string SurfaceContainerHighest { get; init; } = string.Empty;
//    }

//    public class Neutral
//    {
//        [JsonPropertyName("0")]
//        public string _0 { get; init; } = string.Empty;

//        [JsonPropertyName("5")]
//        public string _5 { get; init; } = string.Empty;

//        [JsonPropertyName("10")]
//        public string _10 { get; init; } = string.Empty;

//        [JsonPropertyName("15")]
//        public string _15 { get; init; } = string.Empty;

//        [JsonPropertyName("20")]
//        public string _20 { get; init; } = string.Empty;

//        [JsonPropertyName("25")]
//        public string _25 { get; init; } = string.Empty;

//        [JsonPropertyName("30")]
//        public string _30 { get; init; } = string.Empty;

//        [JsonPropertyName("35")]
//        public string _35 { get; init; } = string.Empty;

//        [JsonPropertyName("40")]
//        public string _40 { get; init; } = string.Empty;

//        [JsonPropertyName("50")]
//        public string _50 { get; init; } = string.Empty;

//        [JsonPropertyName("60")]
//        public string _60 { get; init; } = string.Empty;

//        [JsonPropertyName("70")]
//        public string _70 { get; init; } = string.Empty;

//        [JsonPropertyName("80")]
//        public string _80 { get; init; } = string.Empty;

//        [JsonPropertyName("90")]
//        public string _90 { get; init; } = string.Empty;

//        [JsonPropertyName("95")]
//        public string _95 { get; init; } = string.Empty;

//        [JsonPropertyName("98")]
//        public string _98 { get; init; } = string.Empty;

//        [JsonPropertyName("99")]
//        public string _99 { get; init; } = string.Empty;

//        [JsonPropertyName("100")]
//        public string _100 { get; init; } = string.Empty;
//    }

//    public class NeutralVariant
//    {
//        [JsonPropertyName("0")]
//        public string _0 { get; init; } = string.Empty;

//        [JsonPropertyName("5")]
//        public string _5 { get; init; } = string.Empty;

//        [JsonPropertyName("10")]
//        public string _10 { get; init; } = string.Empty;

//        [JsonPropertyName("15")]
//        public string _15 { get; init; } = string.Empty;

//        [JsonPropertyName("20")]
//        public string _20 { get; init; } = string.Empty;

//        [JsonPropertyName("25")]
//        public string _25 { get; init; } = string.Empty;

//        [JsonPropertyName("30")]
//        public string _30 { get; init; } = string.Empty;

//        [JsonPropertyName("35")]
//        public string _35 { get; init; } = string.Empty;

//        [JsonPropertyName("40")]
//        public string _40 { get; init; } = string.Empty;

//        [JsonPropertyName("50")]
//        public string _50 { get; init; } = string.Empty;

//        [JsonPropertyName("60")]
//        public string _60 { get; init; } = string.Empty;

//        [JsonPropertyName("70")]
//        public string _70 { get; init; } = string.Empty;

//        [JsonPropertyName("80")]
//        public string _80 { get; init; } = string.Empty;

//        [JsonPropertyName("90")]
//        public string _90 { get; init; } = string.Empty;

//        [JsonPropertyName("95")]
//        public string _95 { get; init; } = string.Empty;

//        [JsonPropertyName("98")]
//        public string _98 { get; init; } = string.Empty;

//        [JsonPropertyName("99")]
//        public string _99 { get; init; } = string.Empty;

//        [JsonPropertyName("100")]
//        public string _100 { get; init; } = string.Empty;
//    }

//    public class Palettes
//    {
//        public Primary Primary { get; init; } = new();
//        public Secondary Secondary { get; init; } = new();
//        public Tertiary Tertiary { get; init; } = new();
//        public Neutral Neutral { get; init; } = new();

//        [JsonPropertyName("neutral-variant")]
//        public NeutralVariant NeutralVariant { get; init; } = new();
//    }

//    public class Primary
//    {
//        [JsonPropertyName("0")]
//        public string _0 { get; init; } = string.Empty;

//        [JsonPropertyName("5")]
//        public string _5 { get; init; } = string.Empty;

//        [JsonPropertyName("10")]
//        public string _10 { get; init; } = string.Empty;

//        [JsonPropertyName("15")]
//        public string _15 { get; init; } = string.Empty;

//        [JsonPropertyName("20")]
//        public string _20 { get; init; } = string.Empty;

//        [JsonPropertyName("25")]
//        public string _25 { get; init; } = string.Empty;

//        [JsonPropertyName("30")]
//        public string _30 { get; init; } = string.Empty;

//        [JsonPropertyName("35")]
//        public string _35 { get; init; } = string.Empty;

//        [JsonPropertyName("40")]
//        public string _40 { get; init; } = string.Empty;

//        [JsonPropertyName("50")]
//        public string _50 { get; init; } = string.Empty;

//        [JsonPropertyName("60")]
//        public string _60 { get; init; } = string.Empty;

//        [JsonPropertyName("70")]
//        public string _70 { get; init; } = string.Empty;

//        [JsonPropertyName("80")]
//        public string _80 { get; init; } = string.Empty;

//        [JsonPropertyName("90")]
//        public string _90 { get; init; } = string.Empty;

//        [JsonPropertyName("95")]
//        public string _95 { get; init; } = string.Empty;

//        [JsonPropertyName("98")]
//        public string _98 { get; init; } = string.Empty;

//        [JsonPropertyName("99")]
//        public string _99 { get; init; } = string.Empty;

//        [JsonPropertyName("100")]
//        public string _100 { get; init; } = string.Empty;
//    }

//    public class Schemes
//    {
//        public Light Light { get; init; } = new();

//        [JsonPropertyName("light-medium-contrast")]
//        public LightMediumContrast LightMediumContrast { get; init; } = new();

//        [JsonPropertyName("light-high-contrast")]
//        public LightHighContrast LightHighContrast { get; init; } = new();
//        public Dark Dark { get; init; } = new();

//        [JsonPropertyName("dark-medium-contrast")]
//        public DarkMediumContrast DarkMediumContrast { get; init; } = new();

//        [JsonPropertyName("dark-high-contrast")]
//        public DarkHighContrast DarkHighContrast { get; init; } = new();
//    }

//    public class Secondary
//    {
//        [JsonPropertyName("0")]
//        public string _0 { get; init; } = string.Empty;

//        [JsonPropertyName("5")]
//        public string _5 { get; init; } = string.Empty;

//        [JsonPropertyName("10")]
//        public string _10 { get; init; } = string.Empty;

//        [JsonPropertyName("15")]
//        public string _15 { get; init; } = string.Empty;

//        [JsonPropertyName("20")]
//        public string _20 { get; init; } = string.Empty;

//        [JsonPropertyName("25")]
//        public string _25 { get; init; } = string.Empty;

//        [JsonPropertyName("30")]
//        public string _30 { get; init; } = string.Empty;

//        [JsonPropertyName("35")]
//        public string _35 { get; init; } = string.Empty;

//        [JsonPropertyName("40")]
//        public string _40 { get; init; } = string.Empty;

//        [JsonPropertyName("50")]
//        public string _50 { get; init; } = string.Empty;

//        [JsonPropertyName("60")]
//        public string _60 { get; init; } = string.Empty;

//        [JsonPropertyName("70")]
//        public string _70 { get; init; } = string.Empty;

//        [JsonPropertyName("80")]
//        public string _80 { get; init; } = string.Empty;

//        [JsonPropertyName("90")]
//        public string _90 { get; init; } = string.Empty;

//        [JsonPropertyName("95")]
//        public string _95 { get; init; } = string.Empty;

//        [JsonPropertyName("98")]
//        public string _98 { get; init; } = string.Empty;

//        [JsonPropertyName("99")]
//        public string _99 { get; init; } = string.Empty;

//        [JsonPropertyName("100")]
//        public string _100 { get; init; } = string.Empty;
//    }

//    public class Tertiary
//    {
//        [JsonPropertyName("0")]
//        public string _0 { get; init; } = string.Empty;

//        [JsonPropertyName("5")]
//        public string _5 { get; init; } = string.Empty;

//        [JsonPropertyName("10")]
//        public string _10 { get; init; } = string.Empty;

//        [JsonPropertyName("15")]
//        public string _15 { get; init; } = string.Empty;

//        [JsonPropertyName("20")]
//        public string _20 { get; init; } = string.Empty;

//        [JsonPropertyName("25")]
//        public string _25 { get; init; } = string.Empty;

//        [JsonPropertyName("30")]
//        public string _30 { get; init; } = string.Empty;

//        [JsonPropertyName("35")]
//        public string _35 { get; init; } = string.Empty;

//        [JsonPropertyName("40")]
//        public string _40 { get; init; } = string.Empty;

//        [JsonPropertyName("50")]
//        public string _50 { get; init; } = string.Empty;

//        [JsonPropertyName("60")]
//        public string _60 { get; init; } = string.Empty;

//        [JsonPropertyName("70")]
//        public string _70 { get; init; } = string.Empty;

//        [JsonPropertyName("80")]
//        public string _80 { get; init; } = string.Empty;

//        [JsonPropertyName("90")]
//        public string _90 { get; init; } = string.Empty;

//        [JsonPropertyName("95")]
//        public string _95 { get; init; } = string.Empty;

//        [JsonPropertyName("98")]
//        public string _98 { get; init; } = string.Empty;

//        [JsonPropertyName("99")]
//        public string _99 { get; init; } = string.Empty;

//        [JsonPropertyName("100")]
//        public string _100 { get; init; } = string.Empty;
//    }
//}

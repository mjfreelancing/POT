using TimeZoneConverter;

namespace Pot.App;

/*
EXPLANATION: TimeZoneOffset and Cross-Platform Time Zone Handling

- The TimeZoneOffset property is designed to provide the current offset from UTC for a given time zone.
- In this implementation, the IANA time zone ID (e.g., "Australia/Sydney") is used on Linux/macOS, and the Windows time zone ID (e.g., "AUS Eastern Standard Time") is used on Windows.
- This approach ensures compatibility across platforms, as .NET requires different time zone IDs depending on the OS.
- For best practice, the frontend and database will use IANA time zone IDs (like "Australia/Sydney").
- When the backend runs on Windows, the TimeZoneConverter (https://github.com/mj1856/TimeZoneConverter) package is used to map IANA IDs to Windows IDs as needed.
- This keeps your data portable and future-proof, and allows the backend to always calculate the correct offset regardless of the OS.

USAGE IN REACT / TYPESCRIPT APPLICATION:

- Use the browser API to detect the user's current IANA time zone:
    Intl.DateTimeFormat().resolvedOptions().timeZone
  This returns a string like "Australia/Sydney".

- To present a list of locations (time zones) to the user, use a package such as '@vvo/tzdb' to get a comprehensive, up-to-date list of IANA time zone names:
    npm install @vvo/tzdb

    import { timeZonesNames } from '@vvo/tzdb';
    // timeZonesNames is a string[] of IANA time zone IDs

- Default the user's selection to the detected time zone from the browser.

- When the user selects a location, send the IANA time zone ID to the backend.

- The backend will use this IANA ID for all time zone calculations, translating to a Windows ID only if required by the OS.

- This approach ensures the application is cross-platform, user-friendly, and future-proof.
*/

internal sealed class AppContext : IAppContext
{
    public TimeSpan TimeZoneOffset
    {
        // Recalculate each time to ensure it is always up-to-date
        get
        {
            var ianaTz = "Australia/Sydney";
            var windowsTz = TZConvert.IanaToWindows(ianaTz);
            var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(OperatingSystem.IsWindows() ? windowsTz : ianaTz);

            return tzInfo.GetUtcOffset(DateTime.UtcNow);
        }
    }
}

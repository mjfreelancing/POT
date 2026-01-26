# Clean previous results
Remove-Item -Path "TestResults" -Recurse -ErrorAction SilentlyContinue
Remove-Item -Path "CoverageReport" -Recurse -ErrorAction SilentlyContinue

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings

# Generate HTML report
reportgenerator `
    -reports:"**/TestResults/**/coverage.cobertura.xml" `
    -targetdir:"CoverageReport" `
    -reporttypes:"Html;HtmlSummary"

# Open in browser
Start-Process "CoverageReport/index.html"

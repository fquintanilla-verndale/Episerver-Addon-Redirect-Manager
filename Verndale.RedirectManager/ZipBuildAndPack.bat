@echo off
cls

echo Creating the module zip file..
powershell -command "Compress-Archive -Path .\Views\, .\module.config -DestinationPath .\modules\_protected\Verndale.RedirectManager\Verndale.RedirectManager.zip -CompressionLevel Optimal -Force"
echo.

echo Creating NuGet package..
echo.
nuget pack Verndale.RedirectManager.csproj -Build -Properties Configuration=Release -Verbosity detailed

echo.
echo.
echo Done
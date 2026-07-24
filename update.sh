dotnet tool uninstall -g tedium
dotnet pack
dotnet tool install -g --add-source bin/Release tedium

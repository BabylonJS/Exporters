build:
	dotnet publish -r win-x64 -o ./Release/Windows -c Release
	dotnet publish -r linux-x64 -o ./Release/Linux -c Release
	dotnet publish -r osx-x64 -o ./Release/MacOS -c Release
	make pack

pack:
	rm -rf ./Dist/Linux.tar.gz
	rm -rf ./Dist/Windows.tar.gz
	rm -rf ./Dist/MacOS.tar.gz
	tar -czf ./Dist/Linux.tar.gz Release/Linux/*
	tar -czf ./Dist/Windows.tar.gz Release/Windows/*
	tar -czf ./Dist/MacOS.tar.gz Release/MacOS/*


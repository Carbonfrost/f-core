#
# A standard system for building a dotnet assembly.  This should be included from the
# main Makefile
#
# Structure of the repository should be:
#
#    dotnet
#    ├── Build.props   -- (Optional) Shared build attributes controlling version, etc.
#    ├── solution.sln
#    ├── src
#    │   └── Project1
#    │       ├── Automation  -- (Optional) String resources, text templates, etc.
#    │       │   ├── SR.properties
#    │       │   ├── TextTemplates
#    │       │   └── TextTemplates
#    │       ├── Project1.csproj
#    │       └── (source files)
#    └── test
#        └── Project1.UnitTests
#            ├── Project1.UnitTests.csproj
#            ├── Content    -- (Optional) Fixture files
#            │   └── ...
#            └── (source files)
#
.PHONY: dotnet/restore dotnet/build dotnet/test dotnet/publish dotnet/push

# The configuration to build (probably "Debug" or "Release")
CONFIGURATION?=Release

# The location of the NuGet configuration file
NUGET_FILE?=./dotnet/nuget.config

## Restore package dependencies
dotnet/restore:
	@ dotnet restore ./dotnet

## Build the dotnet solution
dotnet/build: dotnet/restore -dotnet/build

## Pack the dotnet build into a NuGet package
dotnet/pack: dotnet/build -dotnet/pack

## Push the dotnet build into package repository
dotnet/push: dotnet/pack -dotnet/push

-dotnet/build:
	@ eval $(shell eng/build_env); \
		dotnet build --configuration $(CONFIGURATION) --no-restore ./dotnet

-dotnet/pack:
	@ eval $(shell eng/build_env); \
		dotnet pack --configuration $(CONFIGURATION) --no-build ./dotnet

-dotnet/push:
	@ echo "<configuration />" > $(NUGET_FILE)
	@ nuget source add -Name "GPR" \
		 -Source $(NUGET_SOURCE_URL) \
		 -UserName $(NUGET_USER_NAME) \
		 -Password $(NUGET_PASSWORD) \
		 -ConfigFile $(NUGET_FILE) \
		 -StorePasswordInClearText
	@ dotnet nuget push ./dotnet/src/*/bin/Release/*.nupkg --source $(NUGET_SOURCE_URL)

release/requirements:
	@ eng/release_requirements

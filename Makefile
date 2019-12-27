.PHONY: dotnet/build dotnet/test dotnet/publish dotnet/push dotnet/generate

CONFIGURATION ?= Release

## Generate generated code
dotnet/generate:
	srgen -c Carbonfrost.Commons.Core.Resources.SR \
		-r Carbonfrost.Commons.Core.Automation.SR \
		--resx \
		dotnet/src/Carbonfrost.Commons.Core/Automation/SR.properties

## Build the dotnet solution
dotnet/build:
	@ eval $(shell eng/build_env); \
		dotnet build --configuration $(CONFIGURATION) ./dotnet

## Execute dotnet unit tests
dotnet/test: dotnet/build
	fspec -i dotnet/test/Carbonfrost.UnitTests.Core/Content \
		dotnet/test/Carbonfrost.UnitTests.Core/bin/$(CONFIGURATION)/netcoreapp3.0/Carbonfrost.Commons.Core.dll \
		dotnet/test/Carbonfrost.UnitTests.Core/bin/$(CONFIGURATION)/netcoreapp3.0/Carbonfrost.UnitTests.Core.dll

## Pack the dotnet build into a NuGet package
dotnet/pack: release/requirements
	@ eval $(shell eng/build_env); \
		dotnet pack --configuration $(CONFIGURATION) ./dotnet

## Push the dotnet build into package repository
dotnet/push: dotnet/pack -dotnet/push

-dotnet/push:
	@ dotnet nuget push dotnet/src/*/bin/$(CONFIGURATION)/*.nupkg

release/requirements:
	@ eng/release_requirements

include .mk/*.mk

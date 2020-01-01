#
# Investigate `make help` to see a list of targets and their descriptions.
#
.PHONY: dotnet/generate

## Generate generated code
dotnet/generate:
	@ srgen -c Carbonfrost.Commons.Core.Resources.SR \
		-r Carbonfrost.Commons.Core.Automation.SR \
		--resx \
		dotnet/src/Carbonfrost.Commons.Core/Automation/SR.properties

## Execute dotnet unit tests
dotnet/test: dotnet/build -dotnet/test

-dotnet/test:
	@ fspec -i dotnet/test/Carbonfrost.UnitTests.Core/Content \
		dotnet/test/Carbonfrost.UnitTests.Core/bin/$(CONFIGURATION)/netcoreapp3.0/Carbonfrost.Commons.Core.dll \
		dotnet/test/Carbonfrost.UnitTests.Core/bin/$(CONFIGURATION)/netcoreapp3.0/Carbonfrost.UnitTests.Core.dll

include eng/.mk/*.mk

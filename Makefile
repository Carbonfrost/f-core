#
# Investigate `make help` to see a list of targets and their descriptions.
#
.PHONY: dotnet/generate

-include eng/.mk/*.mk

## Generate generated code
dotnet/generate:
	@ srgen -c Carbonfrost.Commons.Core.Resources.SR \
		-r Carbonfrost.Commons.Core.Automation.SR \
		--resx \
		dotnet/src/Carbonfrost.Commons.Core/Automation/SR.properties

## Execute dotnet unit tests
dotnet/test: dotnet/publish -dotnet/test

-dotnet/test:
	@ fspec $(FSPEC_OPTIONS) -t ~integration -i dotnet/test/Carbonfrost.UnitTests.Core/Content \
		dotnet/test/Carbonfrost.UnitTests.Core/bin/$(CONFIGURATION)/netcoreapp3.0/Carbonfrost.Commons.Core.dll \
		dotnet/test/Carbonfrost.UnitTests.Core/bin/$(CONFIGURATION)/netcoreapp3.0/Carbonfrost.UnitTests.Core.dll

## Run unit tests with code coverage
dotnet/cover: dotnet/publish -check-command-coverlet
	$(Q) coverlet \
		--target "make" \
		--targetargs "-- -dotnet/test" \
		--format lcov \
		--output lcov.info \
		--exclude-by-attribute 'Obsolete' \
		--exclude-by-attribute 'GeneratedCode' \
		--exclude-by-attribute 'CompilerGenerated' \
		dotnet/test/Carbonfrost.UnitTests.Core/bin/$(CONFIGURATION)/netcoreapp3.0/Carbonfrost.UnitTests.Core.dll

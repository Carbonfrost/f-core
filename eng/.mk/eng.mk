#
# Various commands for the Engineering platform itself
#
.PHONY: release/requirements eng/check eng/update

release/requirements:
	$(Q) eng/release_requirements

eng/check:
	$(Q) eng/check_csproj

eng_update_file:=$(shell mktemp)

eng/update: -download-eng-archive -check-command-curl
	$(Q) (tar -xvf "$(eng_update_file)" --strip-components=1 'eng-commons-dotnet-master/eng/*'; \
		tar -xvf "$(eng_update_file)" --strip-components=2 'eng-commons-dotnet-master/integration/*'; \
	)

ifeq ($(ENG_DEV_UPDATE), 1)
-download-eng-archive:
	$(Q) git archive --format=zip --prefix=eng-commons-dotnet-master/ --remote=file://$(HOME)/source/eng-commons-dotnet master -o $(eng_update_file)

else
-download-eng-archive: -check-command-curl
	$(Q) curl -o "$(eng_update_file)" -sL https://github.com/Carbonfrost/eng-commons-dotnet/archive/master.zip

endif
#
# Various commands for the Engineering platform itself
#

# Because these are used for installing eng itself, we can't depend on any of the utility targets
# or variables, so they are redefined here

.PHONY: \
	eng/check \
	eng/install \
	eng/update \
	release/requirements \

## Get started with eng, meant to be used in a new repo
eng/start: eng/update -eng/start-Makefile

release/requirements:
	$(Q) eng/release_requirements

eng/check:
	$(Q) eng/check_csproj

_ENG_UPDATE_FILE:=$(shell mktemp)

eng/update: -download-eng-archive -clean-eng-directory
	$(Q) (tar -xf "$(_ENG_UPDATE_FILE)" --strip-components=1 'eng-commons-dotnet-master/eng/*'; \
		tar -xf "$(_ENG_UPDATE_FILE)" --strip-components=2 'eng-commons-dotnet-master/integration/*'; \
	)

ifeq ($(ENG_DEV_UPDATE), 1)
-download-eng-archive: -check-eng-updates-requirements
	$(Q) git archive --format=zip --prefix=eng-commons-dotnet-master/ --remote=file://$(HOME)/source/eng-commons-dotnet master -o $(_ENG_UPDATE_FILE)

else
-download-eng-archive: -check-eng-updates-requirements
	$(Q) curl -o "$(_ENG_UPDATE_FILE)" -sL https://github.com/Carbonfrost/eng-commons-dotnet/archive/master.zip
endif


_RED = \x1b[31m
_RESET = \x1b[39m

-clean-eng-directory:
	$(Q) rm -rf eng

-check-eng-updates-requirements:
	@ if [ ! $(shell command -v curl ) ]; then \
		echo "$(_RED)fatal: $(_RESET)must have curl to download files"; \
		exit 1; \
	fi
	@ if [ ! $(shell command -v tar ) ]; then \
		echo "$(_RED)fatal: $(_RESET)must have tar to unarchive files"; \
		exit 1; \
	fi
	@ if [ ! $(shell command -v git ) ]; then \
		echo "$(_RED)fatal: $(_RESET)must have git to unarchive files"; \
		exit 1; \
	fi

-eng/start-Makefile:
	$(Q) printf -- "-include eng/.mk/*.mk\nstart:\n\t@ echo 'The Future awaits !'" > Makefile

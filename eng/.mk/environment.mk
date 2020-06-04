.PHONY: \
	env \
	-env-global \
	-env-enabled-dotnet \
	-env-enabled-frameworks \
	-env-enabled-python \
	-env-enabled-ruby

_DEV_MESSAGE=(Is direnv set up correctly?  Have you tried 'make init'?)
-check-command-%:
	@ if [ ! $(shell command -v "${*}" ) ]; then \
		echo "Command ${*} could not be found $(_DEV_MESSAGE)"; \
		exit 1; \
	fi

-check-env-%:
	@ if [ "${${*}}" = "" ]; then \
		echo "Environment variable ${*} not set $(_DEV_MESSAGE)"; \
		exit 1; \
	fi

env: -env-global -env-enabled-frameworks
	@ printf ""

-env-global:
	@ $(call _display_variables,ENG_GLOBAL_VARIABLES)

-env-enabled-frameworks: | -env-enabled-dotnet -env-enabled-python -env-enabled-ruby

-env-enabled-dotnet:
	@ $(call _runtime_status,.NET,$(_ENG_ACTUALLY_USING_DOTNET))
	@ $(call _display_variables,ENG_DOTNET_VARIABLES)

-env-enabled-python:
	@ $(call _runtime_status,Python,$(ENG_USING_PYTHON))

-env-enabled-ruby:
	@ $(call _runtime_status,Ruby,$(ENG_USING_RUBY))

define _runtime_status
    printf "$(_MAGENTA)%s$(_RESET) support is available and %b$(_RESET)\n" $(1) "$(if $(filter $(2),1),$(_GREEN)enabled,$(_RED)not enabled)"
endef

define _display_variables
    $(foreach var,$($(1)),printf "  $(_CYAN)%-22s$(_RESET) %s\n" $(var) "$($(var))";)
endef

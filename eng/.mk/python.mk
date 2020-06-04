ENG_AUTODETECT_USING_PYTHON = $(shell \
	[ ! -f requirements.txt ] && \
	[ ! -f Pipfile ] ; \
	echo $$? \
)
ENG_AVAILABLE_RUNTIMES += python

_VENV = . $(VIRTUAL_ENV_NAME)/bin/activate $(OUTPUT_HIDDEN)

.PHONY: \
	-hint-unsupported-python \
	-python/init \
	-use/python \
	-use/python-Pipfile \
	-use/python-version \
	python/init \
	use/python \

## Add support for python
use/python: | -python/init -use/python-version -use/python-Pipfile

# Enable the tasks if we are using python
ifeq (1, $(ENG_USING_PYTHON))

ENG_ENABLED_RUNTIMES += python

## Install python and project dependencies
python/init: -python/init
else
python/init: -hint-unsupported-python
endif

-python/init: -check-command-brew -python/init-python -python/init-venv -python/init-deps

-use/python-Pipfile: -check-command-Pipfile
	$(Q) [ -f Pipfile ] || pipenv init $(OUTPUT_HIDDEN)

-use/python-version:
	@    echo "Adding support for python to this project... "
	@(Q) brew bundle add $(PYTHON) $(OUTPUT_HIDDEN)

-hint-unsupported-python:
	@ echo $(_HIDDEN_IF_BOOTSTRAPPING) "$(_WARNING) Nothing to do" \
		"because $(_MAGENTA)python$(_RESET) is not enabled (Investigate $(_CYAN)\`make use/python\`$(_RESET))"

-python/init-python:
	@    echo "$(_GREEN)Installing Python and Python dependencies...$(_RESET)"
	$(Q) brew install $(PYTHON) $(OUTPUT_HIDDEN)

-python/init-venv: -check-command-$(PYTHON)
	$(Q) if [ "$(VIRTUAL_ENV)" != "$(shell pwd)/$(VIRTUAL_ENV_NAME)" ] || [ ! -d "$(VIRTUAL_ENV)" ]; then \
		$(PYTHON) -m venv $(VIRTUAL_ENV_NAME) $(OUTPUT_HIDDEN); \
	fi
	$(Q) ( \
		$(_VENV) && \
		$(PIP) install pipenv $(OUTPUT_HIDDEN) \
	)

-python/init-deps: -check-command-$(PYTHON)
	$(Q) if [ -f requirements.txt ]; then \
		$(_VENV) && pip install $(OUTPUT_HIDDEN); \
	fi
	$(Q) if [ -f Pipfile ]; then \
		$(_VENV) && command -v pipenv > /dev/null && pipenv install $(OUTPUT_HIDDEN); \
	fi

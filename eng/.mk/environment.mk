-check-command-%:
	@ if [ ! $(shell command -v "${*}" ) ]; then \
		echo "Command ${*} could not be found (Is direnv set up correctly?  Have you tried 'make init'?)"; \
		exit 1; \
	fi

-check-env-%:
	@ if [ "${${*}}" = "" ]; then \
		echo "Environment variable ${*} not set (Is direnv set up correctly?  Have you tried 'make init'?)"; \
		exit 1; \
	fi

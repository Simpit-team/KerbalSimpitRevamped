# Arduino makefile
# Uses the Arduino IDE in commandline mode to build/install sketches.
#
# Environment variables:
# ARDUINO_BOARD: Overrides the target board.
#	See https://github.com/arduino/Arduino/blob/ide-1.5.x/build/shared/manpage.adoc
#	for how to specify the board.
# ARDUINO_PORT: Overrides the serial port the Arduino is attached to.
# ARDUINO_PREFS: Optionally sets a custom preferences file for Arduino IDE
# ARDUINO_VERBOSE: If set, passes the debug flag to arduino env.
# ARDUINO_XVFB: If set, run arduino with the xvfb-run wrapper.
#
# This Makefile is in the public domain.
# Peter Hardy <peter@hardy.dropbear.id.au>

# Hardcoded options:
# This is for an Uno compatible board
BOARD=arduino:avr:uno
PORT=/dev/tty.usbmodem1421
ARDUINOCMD=arduino

# This emulates the Arduino IDE's requirement for
# putting a project in a folder of the same name
INO=$(notdir $(CURDIR)).ino

# Overrides
ifdef ARDUINO_BOARD
	BOARD=$(ARDUINO_BOARD)
endif

ifdef ARDUINO_PORT
	PORT=$(ARDUINO_PORT)
endif

FLAGS = --board $(BOARD) --port $(PORT)

ifdef ARDUINO_VERBOSE
	FLAGS += -v
endif

ifdef ARDUINO_PREFS
	FLAGS += --preferences-file $(ARDUINO_PREFS)
endif

ifdef ARDUINO_XVFB
	ARDUINOCMD := xvfb-run -a $(ARDUINOCMD)
endif

all:
	$(ARDUINOCMD) $(FLAGS) --verify $(PWD)/$(INO)

install:
	$(ARDUINOCMD) $(FLAGS) --upload $(PWD)/$(INO)

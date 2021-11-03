![Main branch building](https://github.com/Simpit-team/KerbalSimpitRevamped-Arduino/actions/workflows/cy-arduino.yml/badge.svg?branch=develop)
[![Documentation Status](https://readthedocs.org/projects/kerbalsimpitrevamped-arduino/badge/?version=latest)](https://kerbalsimpitrevamped-arduino.readthedocs.io/en/latest/?badge=latest)
![Documentation Status](https://raw.githubusercontent.com/KSP-CKAN/CKAN/master/assets/ckan-indexed.svg)

# Kerbal Simpit Revamped

This is the repository for the revamped version of the excellent KSP mod Kerbal Simpit, to try and bring it up to date with some of the recent changes to the game. This is a [Kerbal Space Program](https://kerbalspaceprogram.com/) plugin to enable communication with devices over a serial connection.

It works with an accompanying [Arduino library](https://github.com/Simpit-team/KerbalSimpitRevamped-Arduino) to make building hardware devices simpler.

We have a Discord Server! [Invite Link](https://discord.gg/ZwcPdNcaRN)
We have an [online documentation](https://kerbalsimpitrevamped-arduino.readthedocs.io) for using this mod.

Feel free to raise any issue or idea of improvement you have with us, either in Discord or through the GitHub Issues.

## How to install

This mod comes in two parts : the KSP mod and the Arduino lib.

To install the KSP mod, you can either :
 - install it through [CKAN](https://github.com/KSP-CKAN/CKAN) by installing Simpit (version 2.0.0 or after).
 - go the [release](https://github.com/Simpit-team/KerbalSimpitRevamped/releases) tab and dowload the last one. Copy the `KerbalSimpit` folder into the `GameData` folder of your KSP install

Don't forget to update your port name in the `KerbalSimpit\Settings.cfg` file ! Copy it from `Settings.cfg.sample` if it does not already exists. You can find the right port name by copying the port name you are using in the Arduino IDE.

To install the Arduino lib, you can go to the `KerbalSimpit` folder installed previously and copy the `KerbalSimpitRevamped-Arduino` into your Arduino library folder (usually under `Documents\Arduino\libraries`). Then you can open your Arduino IDE and you should find some Simpit examples in the example list. It should be included in the Arduino library manager shortly.



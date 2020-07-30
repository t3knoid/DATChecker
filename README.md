# DAT Checker
DAT Checker is a Windows form application that checks concordance DAT files.

## Requirements
Visual Studio 2017 is required to build the source. The installer is written using Nullsoft NSIS script.

## Download
Builds are dynamically created courtesy of Appveyor. Downloads are available int the [releases page](https://github.com/t3knoid/DATChecker/releases).

[![Build status](https://ci.appveyor.com/api/projects/status/0krc1vjswah5179s?svg=true)](https://ci.appveyor.com/project/t3knoid/datchecker) [![Latest build](https://img.shields.io/github/v/tag/t3knoid/DATChecker)](https://github.com/t3knoid/DATChecker/releases)

## Defaults
Currently DAT Checker assumes the following:
* Fields are delimied with the ASCII character 020
* Fields are qualified with the ASCII character 254

## Checks
DAT Checker verifies that a given concordance DAT file is valid by checking the following:
* Each row contains the expected number of fields based on the number columns dictated by the file header.
* Each field is bordered with the qualifier character


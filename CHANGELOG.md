# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 0.1.3 - Unreleased

### Changed

- Updated SDK

## 0.1.2 - 2023-07-03

### Added

- Added Program.cs to run and test the plugin locally
- Expose update frequency as settings

### Changed

- Updated SDK
- Do not publish .dll of SDK
- Set 'InvariantGlobalization' to true

## 0.1.1 - 2023-02-06

### Fixed

- Disk spaces (total, used, free) were off by one unit (GB instead of TB, MB instead of GB,...)

## 0.1.0 - 2023-01-23

### Changed

- Updated to new SDK
- Updated to .NET 7

## 0.0.3 - 2022-11-22

### Fixed

- Parsing for numerical values from shared memory

## 0.0.2 - 2022-11-21

### Changed

- Sensor readings as struct to reduce memory allocations
- Return metrics as type 'text' instead of 'miscellaneous'

### Fixed

- Correctly parse numeric metrics of no specific type as numeric type
- Conversion of dataflow metric values
- Numeric metric values parsed wrong due to cultural formatting
- Wrong type returned for some metrics

## 0.0.1 - 2022-10-17

### Added

- Initial release

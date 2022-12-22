# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Unreleased

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

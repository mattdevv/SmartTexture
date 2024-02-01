# Changelog
All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.3.0] - 2024-02-01
## Added 
 - Added alpha is transparency option to output texture settings
 - Allow mismatched texture resolutions as input, uses largest resolution per dimension as default
 - Allow setting exact resolution for output texture
## Changed
 - Packing textures is no longer available as an extension to Texture2D class
## Removed
 - CPU-side packing

## [0.2.0] - 2024-02-01
### Added
 - Ability to choose which channel each source will pull from
### Fixed
 - Correct srgb settings now used when creating textures

## [0.1.0] - 2020-05-16
### Added
 - Packing tool that supports GPU and CPU channel packing.

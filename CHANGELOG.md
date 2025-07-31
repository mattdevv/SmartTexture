# Changelog
All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.5.0] - 2025-07-31
### Added
 - HDR support, input textures are now detected as HDR, and output texture can now be set to be HDR
 - Force include alpha toggle, for when you want to include a solid 0 or 1 alpha channel by leaving the slot empty

## [0.4.3] - 2025-07-31
### Added
 - Multi editing support

## [0.4.2] - 2025-05-09
### Fixed
 - Asset import could fail after deleting library folder or changing build target

## [0.4.1] - 2025-02-28
### Added
 - Basic support for Android and iOS. Uses ASTC for basic compression, and ETC1/2 for crunch compression

## [0.4.0] - 2024-02-02
### Added
 - Output resolution modes
 - New better controls for texture compression settings
 - Inspector warnings about texture format (compression, srgb) to help prevent quality degradation
 - Utilities to recommend texture/graphics formats based on requirements
### Changed
 - Moved all code into namespaces
 - default output if all sources are invalid is now an empty 16x16 texture

## [0.3.0] - 2024-02-01
### Added 
 - Added alpha is transparency option to output texture settings
 - Allow mismatched texture resolutions as input, uses largest resolution per dimension as default
 - Allow setting exact resolution for output texture
### Changed
 - Packing textures is no longer available as an extension to Texture2D class
### Removed
 - CPU-side packing

## [0.2.0] - 2024-02-01
### Added
 - Ability to choose which channel each source will pull from
### Fixed
 - Correct srgb settings now used when creating textures

## [0.1.0] - 2020-05-16
### Added
 - Packing tool that supports GPU and CPU channel packing.

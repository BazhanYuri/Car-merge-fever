# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.1] - 2022-08-09
- Fixed parsing issues in some specific cultures

## [1.1.0] - 2022-07-27
- Fixed settings folder creation
- Improved reflection caching by explicitly declaring used classes
- **Breaking:** Introduced [DebuggableClass] attribute required on every debugged class

## [1.0.4] - 2022-06-03
- Fixed Canvas Sort Order
- Fixed native library clash for InGameDebugConsole

## [1.0.3] - 2022-04-22
- Fixed compile issues with the New Input System

## [1.0.2] - 2022-03-25
- Fixed color picker init
- Added documentation for events

## [1.0.1] - 2022-02-18
- Fixed SystemInfo display

## [1.0.0] - 2022-02-16
- Properties Ordering
- Properties grouping (paths can be declared now instead of a group)
- Foldout state saved
- Unity Objects now work out of the box

## [0.2.7] - 2022-01-26
- Resources.Load optimisation

## [0.2.6] - 2022-01-05
- Fixed DebuggableField for enum did not worl properly if the console was closed and opened again

## [0.2.5] - 2021-12-22
- Fixed DebuggableActions fired twice

## [0.2.4] - 2021-12-09
- Cached reflection types
- Fixed Slider View initial value
- Debug views now hidden in component menus
- Ensuring console canvas is always on top

## [0.2.3] - 2021-12-08
- Support for scenes without cameras

## [0.2.2] - 2021-11-18
- Creative camera init fix

## [0.2.1] - 2021-11-05
- Fixing creatives default debugs

## [0.2.0] - 2021-10-26
- Fixed static fields not interactable
- Enum fields support
- SafeAreaFitter fixed (better notch support)
- Hiding properties without instances
- Prevent activator from triggering twice
- Properties in groups now order from small to big to have a tighter layout
- Better looking and uniform UI for al views
- Improved value slider
- Slider support for floats

## [0.1.7] - 2021-10-07
- Fixed Fonts files conflicting GUIDs

## [0.1.6] - 2021-09-28
- Landscape support
- Inactive objects support
- Better missing instance display

## [0.1.5] - 2021-09-21
- Events for visibility changes
- Custom names for properties
- Better readonly fields
- Settings for activator

## [0.1.4] - 2021-08-25
- Prevent from pushing to store on android when using the console

## [0.1.3] - 2021-08-24
- Updated CI

## [0.1.2] - 2021-08-09
- Adding Ints support
- Suppressing warnings

## [0.1.1] - 2021-08-06
- Documentation misc fixes

## [0.1.0] - 2021-08-05
- Internal release



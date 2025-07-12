# Greenstack's Game Stats
A simple, lightweight library that contains a bunch of classes to manage game character stats.

## Installation
There are three ways to include GameStats in your project:
1. Install the [NuGet package](https://www.nuget.org/packages/Greenstack.GameStats/): `dotnet add package Greenstack.GameStats --version 1.0.0`
2. Clone the repository as a submodule into your project. This will let you have the most up-to-date sources. Note that the `main` branch may have breaking changes.
3. Copy `src/Stats.cs` to an appropriate location in your project. While you won't get updates, it does reduce the number of dlls that will be produced in a project build.

## Getting Started
The core stat types are `ResourceStat<T>` and `ModifiableStat<T>`. `T` is constrained to any type that implements the `INumber<T>` interface.

 - `ResourceStat<T>`: This stat is used to track a value that can increase or decrease in value. This is good for tracking things such as money, health, mana, or any other resource that can be depleted, refreshed, or otherwise.
 - `ModifiableStat<T>`: This status is used to track a stat that has a definite value, but can be modified in some way and reset to that core value. This class is valuable to act as a proxy over a value that should be preserved or reset to a regular value after a battle ends, as ModifiableStats don't alter its main value as modifications come an and out.

Each of these stats come with helper methods to query and operate on their internal values, as well as to compare them against raw values and other stats.
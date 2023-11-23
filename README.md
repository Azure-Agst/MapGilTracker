![icon](./MapGilTracker/images/icon.png)

# FFXIV FATE/Map Gil Tracker

[![Dynamic XML Badge](https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Agst%2FMapGilTracker%2Fmaster%2FMapGilTracker%2FMapGilTracker.csproj&query=%2F%2FProject%2FPropertyGroup%2FVersion&label=Version&color=blue)](https://github.com/Azure-Agst/MapGilTracker/releases/latest)
[![License](https://img.shields.io/github/license/Azure-Agst/MapGilTracker)](https://github.com/Azure-Agst/MapGilTracker/blob/master/LICENSE.md)
[![Dalamud Repo](https://img.shields.io/badge/Dalamud_Repo-Third_Party-992c31)][AzureDalamudRepo]


This plugin aims to simplify reward tracking for FCs who use FATEs/Maps as fundraising events! Simply install it, enable it, and it'll automatically track every chest or sack opened and calculate how much gil each person owes.

## Screenshots

<details>

![image1](./MapGilTracker/images/image1.png)
![image2](./MapGilTracker/images/image2.png)

</details>

## Installation

As of writing, this plugin is still in testing and not yet available on the general Dalamud plugin repo. That said, there are two ways you can install test builds onto your client.

### Dalamud Testing Repo - Public Betas

As of now, the most official method of installation is via Dalamud's testing channel. Versions distributed via this method are reviewed, tested, and approved by the Dalamud devs, but there still might be some bugs in your experience.

To install a public beta, open the Dalamud Plugin Installer's settings, and under the "Experimental" tab, enable testing builds. After saving, closing, then reopening the installer, the plugin should now be visible in the new "Testing Available" section of the sidebar.

### Third-Party Repo - Public Alphas

If you feel like living on the edge and would like to test new features the second they become available, you can install the public alpha via my third-party repo. Versions distributed via this method are generally tested by myself, but because they're only reviewed by one person, are bound to be buggy and may potentially crash your game. Tread lightly!

You can find the page for my repo, which contains instructions on how to use it, [here][AzureDalamudRepo] or by clicking on the repo card below.

[![Card](https://github-readme-stats.vercel.app/api/pin?username=azure-agst&repo=DalamudRepo&title_color=fff&icon_color=f9f9f9&theme=dark)][AzureDalamudRepo]

## Development

In order to set up a development environment to work on this plugin, perform the following steps:

0. Install Visual Studio Code 2022 with C# support
1. Fork this repo to your account, then clone it to your local machine
2. Open MapGilTracker.sln in VS2022 and build it
3. In Dalamud, open the Plugin Installer settings menu
4. Under the "Experimental" tab, add the full path to the compiled debug DLL under "Dev Plugin Locations"
5. Profit? (Heh.)

## Contributions

Sure, I guess? Simply open a PR and I can work with you to merge in whatever features/fixes you want to add.

[AzureDalamudRepo]: https://github.com/Azure-Agst/DalamudRepo

# S2HD

* A decompilation of Sonic 2 HD.

* Initial decomp done in one hour.

# Tools used

I used dotpeek to export the project and dnSpy to reference the scripts while fixing them, I also used [this](https://github.com/maybekoi/FSNSFix) to turn "namespace S2HD;" to "namespace S2HD {".

## How to build

Check [Building.md](Building.md)

## Changes

- **Film / H.264 cutscenes**: Accord.Video.FFMPEG doesn't support modern .NET so the end video that you'd get once you beat Hill Top Zone doesn't play.
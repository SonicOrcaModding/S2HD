# S2HD

* A decompilation of Sonic 2 HD.

* Initial decomp done in one hour.

# Tools used

I used dotpeek to export the project and dnSpy to reference the scripts while fixing them, I also used [this](https://github.com/maybekoi/FSNSFix) to turn "namespace S2HD;" to "namespace S2HD {".

## How to build

Check [Building.md](Building.md)

## Changes

- **Film / H.264 cutscenes**: Accord.Video.FFMPEG doesn't support modern .NET so the end video that you'd get once you beat Hill Top Zone doesn't play.

# Bugs

- Lava in Hill Top Zone Act 1 Not Visible + It doesn't do damage (In the first part/part with the 2 moving platforms).
- Rising Lava in Hill Top Act 2 doesn't even load/spawn so theres just a pit at the bottom. 
- Shake effect & sfx in Hill Top Act 1 & 2 persists after the rising lava section (You have to die for it to stop??). 
- Lava can unload its asset if the game thinks the cam is offscreen when it isnt.
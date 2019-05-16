SpaceEngineersScripts
---------------------

This is a collection of C# scripts that I have made and work on for Space Engineers. I thought it would be better to use a Github then the workshop, since it'll be easier for me to work on and use version control then that. Bleck.

The bare minimum that the grid needs is:
  * 1x Battery (either large or small)
  * 1x Cargo Container (any size)
  * 1x Piston
  * 1x Rotor
  * 1x Ship Drill

<!-- WIP -->
How to set up:
  * You need to have *at least* the blocks that are listed above
  * Place down a Programmable Block
      * You **DO NOT** need a timer block. This script auto runs by itself.
  * Copy - Paste this script in to it, `Check Code`, `Save`
  * Change either the values
  * Put down a standard size LCD and add `!MinerManagerOutput` to the name.
      * You can put multiple around the same grid if you'd like

These projects can be minified (if needed or wanted) using one of these tools:
    * https://codebeautify.org/csharpviewer
    * https://github.com/atifaziz/CSharpMinifier

Projects so far:
    * [Miner Manager](MinerManager)

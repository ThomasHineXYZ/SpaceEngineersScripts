Miner Manager
-------------
This is a script to manage a mining outpost

Inspired by [Kanajashi](https://www.youtube.com/user/Kanajashi)'s videos where he built offsite mining outposts ([Earth - Nickel](https://youtu.be/cPABaLSkRts), [Earth - Ice](https://youtu.be/6tvmT8C2ur0), [Moon - Gold](https://youtu.be/mCgou-HMmug))

<!-- Ended up (at least for now) not using calculations, just numbers in variables -->
<!-- [The math was from the `Earth - Ice` video, around the 18 minute mark](https://youtu.be/6tvmT8C2ur0?t=1093) -->

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

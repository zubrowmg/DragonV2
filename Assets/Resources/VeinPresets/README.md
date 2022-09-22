This README will detail how presets are to be created/formated.

Preset Core:
1. Need to leave 5 tiles of empty space to the top/bottom/right/left of the entire core
	a. This is so that we can install preset pieces
2. Try to keep the overall shape wide
3. Mark the start locations for the core preset(>,V) and the pieces(W,A,S,D)
	a. No need for left and up core start locations, since they just use the right and down start locations

Key
> = Right orientation start
V = Down orientation start
W = Up hallway start
A = Left hallway start
S = Down hallway start
D = Right hallway start
X = Room
B = PresetBossRoom, does not use the ILK system. B0 = boos with room id equaling 0
w,a,s,d,x - same as the capitol version, but has a way less chance of spawning
	L - Linked Pieces - Multiple pieces with the same "Link Id", where if one piece is choosen they are all choosen
	I - Inverted Pieces - Multiple pieces with the same "Inverted Id", where if one piece is choosen the rest are tossed out
	k - Inverted Linked Pieces - If you decide to have inverted pieces after an inverted piece you need to mark it as such
	Format = (w,a,s,d,x)L#I#K#
	Ex. WL0 - Means that pieces with link id of 0, will all be spawned/not spawned
	Ex. WI0 - Means that pieces with inverted id of 0, only one will spawn
	Ex. WL1I2 - Means that only one inverted piece will be choosed, and it's linked pieces will also be choosen
			The rejected inverted pieces will be tossed, along with their linked pieces

	Inverted Linked Pieces rules
		1. You don't mark the I root as K
		2. Mark the Linked leaves as K
		Ex. DI0L0 is the root, if this piece is choosen you can mark a Linked piece (L0) as K
			DI0L0 -> XL0K0
		3. Now mark leaves off of the K root as k, these k leaves should have matching I and different Links
				AL1I1K0
				  ^
				  |
			DI0L0 -> XL0K0
				  |
				  V
				SL2I1K0
		

Preset Pieces:
1. Smaller chunks, meant to fill gaps or extend the core
2. Only reuses > notation from the core
3. Need to make 2 versions of non sysmetrical designs
	a. We only rotate piece, do not flip them
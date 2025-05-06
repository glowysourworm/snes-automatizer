.include "hdr.asm"

.section ".rodata1" superfree

patterns:
.incbin "samus-styalized-invert.pic"
patterns_end:

.ends

.section ".rodata2" superfree
map:
.incbin "samus-styalized-invert.map"
map_end:

palette:
.incbin "samus-styalized-invert.pal"
palette_end:

.ends

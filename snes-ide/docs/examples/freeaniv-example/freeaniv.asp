.include "hdr.asm"
.accu 16
.index 16
.16bit
.define __main_locals 0

.SECTION ".maintext_0x0" SUPERFREE

main:
.ifgr __main_locals 0
tsa
sec
sbc #__main_locals
tas
.endif
lda.w #:patterns_end
sta.b tcc__r0h
lda.w #patterns_end + 0
sta.b tcc__r0
lda.w #:patterns
lda.w #patterns + 0
sta.b tcc__r1
sec
lda.b tcc__r0
sbc.b tcc__r1
sta.b tcc__r0
lda.w #:palette_end
lda.w #palette_end + 0
sta.b tcc__r1
lda.w #:palette
lda.w #palette + 0
sta.b tcc__r2
sec
lda.b tcc__r1
sbc.b tcc__r2
sta.b tcc__r1
pea.w 16384
pea.w 16
pei (tcc__r1)
pei (tcc__r0)
sep #$20
lda #0
pha
rep #$20
pea.w :palette
pea.w palette + 0
pea.w :patterns
pea.w patterns + 0
sep #$20
lda #0
pha
rep #$20
jsr.l bgInitTileSet
tsa
clc
adc #18
tas
lda.w #:map_end
lda.w #map_end + 0
sta.b tcc__r0
lda.w #:map
lda.w #map + 0
sta.b tcc__r1
sec
lda.b tcc__r0
sbc.b tcc__r1
sta.b tcc__r0
pea.w 0
sep #$20
lda #0
pha
rep #$20
pei (tcc__r0)
pea.w :map
pea.w map + 0
sep #$20
lda #0
pha
rep #$20
jsr.l bgInitMapSet
tsa
clc
adc #10
tas
pea.w (0 * 256 + 1)
sep #$20
rep #$20
jsr.l setMode
pla
sep #$20
lda #1
pha
rep #$20
jsr.l bgSetDisable
tsa
clc
adc #1
tas
sep #$20
lda #2
pha
rep #$20
jsr.l bgSetDisable
tsa
clc
adc #1
tas
jsr.l setScreenOn
__local_0:
jsr.l WaitForVBlank
bra __local_0
lda.w #0
sta.b tcc__r0
__local_1:
.ifgr __main_locals 0
tsa
clc
adc #__main_locals
tas
.endif
rtl
.ENDS
.RAMSECTION "ram{WLA_FILENAME}.data" APPENDTO "globram.data"

.ENDS

.SECTION "{WLA_FILENAME}.data" APPENDTO "glob.data"

.ENDS

.SECTION ".rodata" SUPERFREE

.ENDS

.RAMSECTION ".bss" BANK $7e SLOT 2
.ENDS

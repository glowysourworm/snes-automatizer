.include "hdr.asm"
.accu 16
.index 16
.16bit
.BASE $80
.define __main_locals 0
.SECTION ".maintext_0x0" SUPERFREE
main:
.ifgr __main_locals 0
tsa
sec
sbc #__main_locals
tas
.endif
pea.w 26624
jsr.l consoleSetTextVramBGAdr
pla
pea.w 12288
jsr.l consoleSetTextVramAdr
pla
pea.w 256
jsr.l consoleSetTextOffset
pla
pea.w :palfont
pea.w palfont + 0
pea.w :tilfont
pea.w tilfont + 0
pea.w (32 * 256 + 0)
sep #$20
rep #$20
jsr.l consoleInitText
tsa
clc
adc #10
tas
pea.w 8192
sep #$20
lda #0
pha
rep #$20
jsr.l bgSetGfxPtr
tsa
clc
adc #3
tas
sep #$20
lda #0
pha
rep #$20
pea.w 26624
sep #$20
lda #0
pha
rep #$20
jsr.l bgSetMapPtr
tsa
clc
adc #4
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
pea.w :tccs_{WLA_FILENAME}_L.{WLA_FILENAME}8
pea.w tccs_{WLA_FILENAME}_L.{WLA_FILENAME}8 + 0
pea.w 10
pea.w 10
jsr.l consoleDrawText
tsa
clc
adc #8
tas
pea.w :tccs_{WLA_FILENAME}_L.{WLA_FILENAME}9
pea.w tccs_{WLA_FILENAME}_L.{WLA_FILENAME}9 + 0
pea.w 14
pea.w 6
jsr.l consoleDrawText
tsa
clc
adc #8
tas
pea.w :tccs_{WLA_FILENAME}_L.{WLA_FILENAME}10
pea.w tccs_{WLA_FILENAME}_L.{WLA_FILENAME}10 + 0
pea.w 18
pea.w 3
jsr.l consoleDrawText
tsa
clc
adc #8
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
.BASE $00
.RAMSECTION "ram{WLA_FILENAME}.data" APPENDTO "globram.data"
.ENDS
.SECTION "{WLA_FILENAME}.data" APPENDTO "glob.data"
.ENDS
.SECTION ".rodata" SUPERFREE
tccs_{WLA_FILENAME}_L.{WLA_FILENAME}8: .db $48,$65,$6c,$6c,$6f,$20,$57,$6f,$72,$6c,$64,$20,$21,$0
tccs_{WLA_FILENAME}_L.{WLA_FILENAME}9: .db $57,$45,$4c,$43,$4f,$4d,$45,$20,$54,$4f,$20,$50,$56,$53,$4e,$45,$53,$4c,$49,$42,$0
tccs_{WLA_FILENAME}_L.{WLA_FILENAME}10: .db $48,$54,$54,$50,$53,$3a,$2f,$2f,$57,$57,$57,$2e,$50,$4f,$52,$54,$41,$42,$4c,$45,$44,$45,$56,$2e,$43,$4f,$4d,$0
.ENDS



.BASE $00
.RAMSECTION ".bss" BANK $7e SLOT 2
.ENDS

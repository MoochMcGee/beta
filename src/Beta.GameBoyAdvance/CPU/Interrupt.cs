namespace Beta.GameBoyAdvance.CPU
{
    public enum Interrupt
    {
        VBlank = 0x0001, // 0 - lcd v-blank
        HBlank = 0x0002, // 1 - lcd h-blank
        VCheck = 0x0004, // 2 - lcd v-counter match
        Timer0 = 0x0008, // 3 - timer 0 overflow
        Timer1 = 0x0010, // 4 - timer 1 overflow
        Timer2 = 0x0020, // 5 - timer 2 overflow
        Timer3 = 0x0040, // 6 - timer 3 overflow
        Serial = 0x0080, // 7 - serial communication
        Dma0   = 0x0100, // 8 - dma 0
        Dma1   = 0x0200, // 9 - dma 1
        Dma2   = 0x0400, // a - dma 2
        Dma3   = 0x0800, // b - dma 3
        Joypad = 0x1000, // c - keypad
        Cart   = 0x2000, // d - game pak
        Res0   = 0x4000, // e - not used
        Res1   = 0x8000  // f - not used
    }
}

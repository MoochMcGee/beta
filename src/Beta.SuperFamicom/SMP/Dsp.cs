using Beta.Platform.Audio;
using Beta.Platform.Core;

namespace Beta.SuperFamicom.SMP
{
    public sealed class Dsp : Processor
    {
        private const int MVOLL = 0x0c;
        private const int MVOLR = 0x1c;
        private const int EVOLL = 0x2c;
        private const int EVOLR = 0x3c;
        private const int KON = 0x4c;
        private const int KOFF = 0x5c;
        private const int FLG = 0x6c;
        private const int ENDX = 0x7c;
        private const int EFB = 0x0d;
        private const int PMON = 0x2d;
        private const int NON = 0x3d;
        private const int EON = 0x4d;
        private const int DIR = 0x5d;
        private const int ESA = 0x6d;
        private const int EDL = 0x7d;
        private const int FIR = 0x0f;
        private const int VOLL = 0x00;
        private const int VOLR = 0x01;
        private const int PITCHL = 0x02;
        private const int PITCHH = 0x03;
        private const int SRCN = 0x04;
        private const int ADSR0 = 0x05;
        private const int ADSR1 = 0x06;
        private const int GAIN = 0x07;
        private const int ENVX = 0x08;
        private const int OUTX = 0x09;

        private const int ENVELOPE_A = 1;
        private const int ENVELOPE_D = 2;
        private const int ENVELOPE_S = 3;
        private const int ENVELOPE_R = 0;

        // public constants
        private const int ECHO_HIST_SIZE = 8;

        private const int BRR_BUFFER_SIZE = 12;
        private const int BRR_BLOCK_SIZE = 9;
        private const int COUNTER_RANGE = 2048 * 5 * 3; // 30720 (0x7800)

        // gaussian
        private static short[] gaussianTable = new short[]
        {
               0,    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
               1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    2,    2,    2,    2,    2,
               2,    2,    3,    3,    3,    3,    3,    4,    4,    4,    4,    4,    5,    5,    5,    5,
               6,    6,    6,    6,    7,    7,    7,    8,    8,    8,    9,    9,    9,   10,   10,   10,
              11,   11,   11,   12,   12,   13,   13,   14,   14,   15,   15,   15,   16,   16,   17,   17,
              18,   19,   19,   20,   20,   21,   21,   22,   23,   23,   24,   24,   25,   26,   27,   27,
              28,   29,   29,   30,   31,   32,   32,   33,   34,   35,   36,   36,   37,   38,   39,   40,
              41,   42,   43,   44,   45,   46,   47,   48,   49,   50,   51,   52,   53,   54,   55,   56,
              58,   59,   60,   61,   62,   64,   65,   66,   67,   69,   70,   71,   73,   74,   76,   77,
              78,   80,   81,   83,   84,   86,   87,   89,   90,   92,   94,   95,   97,   99,  100,  102,
             104,  106,  107,  109,  111,  113,  115,  117,  118,  120,  122,  124,  126,  128,  130,  132,
             134,  137,  139,  141,  143,  145,  147,  150,  152,  154,  156,  159,  161,  163,  166,  168,
             171,  173,  175,  178,  180,  183,  186,  188,  191,  193,  196,  199,  201,  204,  207,  210,
             212,  215,  218,  221,  224,  227,  230,  233,  236,  239,  242,  245,  248,  251,  254,  257,
             260,  263,  267,  270,  273,  276,  280,  283,  286,  290,  293,  297,  300,  304,  307,  311,
             314,  318,  321,  325,  328,  332,  336,  339,  343,  347,  351,  354,  358,  362,  366,  370,
             374,  378,  381,  385,  389,  393,  397,  401,  405,  410,  414,  418,  422,  426,  430,  434,
             439,  443,  447,  451,  456,  460,  464,  469,  473,  477,  482,  486,  491,  495,  499,  504,
             508,  513,  517,  522,  527,  531,  536,  540,  545,  550,  554,  559,  563,  568,  573,  577,
             582,  587,  592,  596,  601,  606,  611,  615,  620,  625,  630,  635,  640,  644,  649,  654,
             659,  664,  669,  674,  678,  683,  688,  693,  698,  703,  708,  713,  718,  723,  728,  732,
             737,  742,  747,  752,  757,  762,  767,  772,  777,  782,  787,  792,  797,  802,  806,  811,
             816,  821,  826,  831,  836,  841,  846,  851,  855,  860,  865,  870,  875,  880,  884,  889,
             894,  899,  904,  908,  913,  918,  923,  927,  932,  937,  941,  946,  951,  955,  960,  965,
             969,  974,  978,  983,  988,  992,  997, 1001, 1005, 1010, 1014, 1019, 1023, 1027, 1032, 1036,
            1040, 1045, 1049, 1053, 1057, 1061, 1066, 1070, 1074, 1078, 1082, 1086, 1090, 1094, 1098, 1102,
            1106, 1109, 1113, 1117, 1121, 1125, 1128, 1132, 1136, 1139, 1143, 1146, 1150, 1153, 1157, 1160,
            1164, 1167, 1170, 1174, 1177, 1180, 1183, 1186, 1190, 1193, 1196, 1199, 1202, 1205, 1207, 1210,
            1213, 1216, 1219, 1221, 1224, 1227, 1229, 1232, 1234, 1237, 1239, 1241, 1244, 1246, 1248, 1251,
            1253, 1255, 1257, 1259, 1261, 1263, 1265, 1267, 1269, 1270, 1272, 1274, 1275, 1277, 1279, 1280,
            1282, 1283, 1284, 1286, 1287, 1288, 1290, 1291, 1292, 1293, 1294, 1295, 1296, 1297, 1297, 1298,
            1299, 1300, 1300, 1301, 1302, 1302, 1303, 1303, 1303, 1304, 1304, 1304, 1304, 1304, 1305, 1305
        };

        private static ushort[] counterRate = new ushort[]
        {
            0x000, 0x800, 0x600,
            0x500, 0x400, 0x300,
            0x280, 0x200, 0x180,
            0x140, 0x100, 0x0c0,
            0x0a0, 0x080, 0x060,
            0x050, 0x040, 0x030,
            0x028, 0x020, 0x018,
            0x014, 0x010, 0x00c,
            0x00a, 0x008, 0x006,
            0x005, 0x004, 0x003,
                   0x002,
                   0x001
        };

        private static ushort[] counterOffset = new ushort[]
        {
            0x000, 0x000, 0x410,
            0x218, 0x000, 0x410,
            0x218, 0x000, 0x410,
            0x218, 0x000, 0x410,
            0x218, 0x000, 0x410,
            0x218, 0x000, 0x410,
            0x218, 0x000, 0x410,
            0x218, 0x000, 0x410,
            0x218, 0x000, 0x410,
            0x218, 0x000, 0x410,
                   0x000,
                   0x000
        };

        private readonly IAudioBackend audio;
        private readonly PSRAM psram;

        private State state = new State();
        private Voice[] voice = new Voice[8];

        private int step;

        public Dsp(IAudioBackend audio, PSRAM psram)
        {
            Single = 1;

            this.audio = audio;
            this.psram = psram;

            state.Regs[FLG] = 0xe0;

            state.Noise = 0x4000;
            state.EveryOtherSample = true;

            for (var i = 0; i < 8; i++)
            {
                voice[i] = new Voice
                {
                    BrrOffset = 1,
                    Vbit = (1 << i),
                    Vidx = (i << 4),
                    EnvMode = ENVELOPE_R
                };
            }
        }

        public byte Peek()
        {
            int addr = psram.Read(0x00f2);
            return state.Regs[addr & 0x7f];
        }

        public void Poke(byte data)
        {
            int addr = psram.Read(0x00f2);

            if ((addr & 0x80) != 0) return;

            state.Regs[addr] = data;

            switch (addr & 0x0f)
            {
            case ENVX: state.EnvxBuf = data; break;
            case OUTX: state.OutxBuf = data; break;
            default:
                switch (addr)
                {
                case KON: state.NewKon = data; break;
                case ENDX:
                    state.EndxBuf = 0;
                    state.Regs[ENDX] = 0;
                    break;
                }
                break;
            }
        }

        public override void Update()
        {
            switch (step)
            {
            case 0x00: V5(voice[0]); V2(voice[1]); break;
            case 0x01: V6(voice[0]); V3(voice[1]); break;
            case 0x02: V7(voice[0]); V4(voice[1]); V1(voice[3]); break;
            case 0x03: V8(voice[0]); V5(voice[1]); V2(voice[2]); break;
            case 0x04: V9(voice[0]); V6(voice[1]); V3(voice[2]); break;
            case 0x05: V7(voice[1]); V4(voice[2]); V1(voice[4]); break;
            case 0x06: V8(voice[1]); V5(voice[2]); V2(voice[3]); break;
            case 0x07: V9(voice[1]); V6(voice[2]); V3(voice[3]); break;
            case 0x08: V7(voice[2]); V4(voice[3]); V1(voice[5]); break;
            case 0x09: V8(voice[2]); V5(voice[3]); V2(voice[4]); break;
            case 0x0a: V9(voice[2]); V6(voice[3]); V3(voice[4]); break;
            case 0x0b: V7(voice[3]); V4(voice[4]); V1(voice[6]); break;
            case 0x0c: V8(voice[3]); V5(voice[4]); V2(voice[5]); break;
            case 0x0d: V9(voice[3]); V6(voice[4]); V3(voice[5]); break;
            case 0x0e: V7(voice[4]); V4(voice[5]); V1(voice[7]); break;
            case 0x0f: V8(voice[4]); V5(voice[5]); V2(voice[6]); break;
            case 0x10: V9(voice[4]); V6(voice[5]); V3(voice[6]); break;
            case 0x11: V1(voice[0]); V7(voice[5]); V4(voice[6]); break;
            case 0x12: V8(voice[5]); V5(voice[6]); V2(voice[7]); break;
            case 0x13: V9(voice[5]); V6(voice[6]); V3(voice[7]); break;
            case 0x14: V1(voice[1]); V7(voice[6]); V4(voice[7]); break;
            case 0x15: V8(voice[6]); V5(voice[7]); V2(voice[0]); break;
            case 0x16: V3A(voice[0]); V9(voice[6]); V6(voice[7]); echo_22(); break;
            case 0x17: V7(voice[7]); echo_23(); break;
            case 0x18: V8(voice[7]); echo_24(); break;
            case 0x19: V3B(voice[0]); V9(voice[7]); echo_25(); break;
            case 0x1a: echo_26(); break;
            case 0x1b: misc_27(); echo_27(); break;
            case 0x1c: misc_28(); echo_28(); break;
            case 0x1d: misc_29(); echo_29(); break;
            case 0x1e: misc_30(); V3C(voice[0]); echo_30(); break;
            case 0x1f: V4(voice[0]); V1(voice[2]); break;
            }

            step = (step + 1) & 0x1f;
        }

        private int gaussian_interpolate(Voice v)
        {   //make pointers into gaussian table based on fractional position between samples
            var offset1 = (v.InterpPos >> 4) & 0xff;
            var offset2 = (v.InterpPos >> 12) + v.BufPos;

            int output = (gaussianTable[(255 - offset1) + 0] * v.Buffer.Read(offset2 + 0)) >> 11;
            output += (gaussianTable[(255 - offset1) + 256] * v.Buffer.Read(offset2 + 1)) >> 11;
            output += (gaussianTable[(0 + offset1) + 256] * v.Buffer.Read(offset2 + 2)) >> 11;
            output = (short)output;
            output += (gaussianTable[(0 + offset1) + 0] * v.Buffer.Read(offset2 + 3)) >> 11;

            return SignedClamp(output) & ~1;
        }

        private static int SignedClamp(int x)
        {
            if (x > +0x7fff) return +0x7fff;
            if (x < -0x8000) return -0x8000;

            return x;
        }

        //counter
        private void counter_tick()
        {
            state.Counter--;
            if (state.Counter < 0)
            {
                state.Counter = COUNTER_RANGE - 1;
            }
        }

        private bool counter_poll(uint rate)
        {
            if (rate == 0)
            {
                return false;
            }
            return (((uint)state.Counter + counterOffset[rate]) % counterRate[rate]) == 0;
        }

        //envelope
        private void envelope_run(Voice v)
        {
            var env = v.Env;

            if (v.EnvMode == ENVELOPE_R)
            { //60%
                env -= 0x8;
                if (env < 0)
                {
                    env = 0;
                }
                v.Env = env;
                return;
            }

            int rate;
            int envData = state.Regs[v.Vidx + ADSR1];
            if ((state.Adsr0 & 0x80) != 0)
            { //99% ADSR
                if (v.EnvMode >= ENVELOPE_D)
                { //99%
                    env--;
                    env -= env >> 8;
                    rate = envData & 0x1f;
                    if (v.EnvMode == ENVELOPE_D)
                    { //1%
                        rate = ((state.Adsr0 >> 3) & 0x0e) + 0x10;
                    }
                }
                else
                { //env_attack
                    rate = ((state.Adsr0 & 0x0f) << 1) + 1;
                    env += rate < 31 ? 0x20 : 0x400;
                }
            }
            else
            { //GAIN
                envData = state.Regs[v.Vidx + GAIN];
                var mode = envData >> 5;
                if (mode < 4)
                { //direct
                    env = envData << 4;
                    rate = 31;
                }
                else
                {
                    rate = envData & 0x1f;
                    if (mode == 4)
                    { //4: linear decrease
                        env -= 0x20;
                    }
                    else if (mode < 6)
                    { //5: exponential decrease
                        env--;
                        env -= env >> 8;
                    }
                    else
                    { //6, 7: linear increase
                        env += 0x20;
                        if (mode > 6 && (uint)v.HiddenEnv >= 0x600)
                        {
                            env += 0x8 - 0x20; //7: two-slope linear increase
                        }
                    }
                }
            }

            //sustain level
            if ((env >> 8) == (envData >> 5) && v.EnvMode == ENVELOPE_D)
            {
                v.EnvMode = ENVELOPE_S;
            }
            v.HiddenEnv = env;

            //unsigned cast because linear decrease underflowing also triggers this
            if ((uint)env > 0x7ff)
            {
                env = (env < 0 ? 0 : 0x7ff);
                if (v.EnvMode == ENVELOPE_A)
                {
                    v.EnvMode = ENVELOPE_D;
                }
            }

            if (counter_poll((uint)rate))
            {
                v.Env = env;
            }
        }

        //brr
        private void brr_decode(Voice v)
        {   //state.t_brr_byte = ram[v.brr_addr + v.brr_offset] cached from previous clock cycle
            var nybbles = (state.BrrByte << 8) + psram.Read((ushort)(v.BrrAddr + v.BrrOffset + 1));

            var filter = (state.BrrHeader >> 2) & 3;
            var scale = (state.BrrHeader >> 4);

            //decode four samples
            for (uint i = 0; i < 4; i++)
            {
                //bits 12-15 = current nybble; sign extend, then shift right to 4-bit precision
                //result: s = 4-bit sign-extended sample value
                var s = (short)nybbles >> 12;
                nybbles <<= 4; //slide nybble so that on next loop iteration, bits 12-15 = current nybble

                if (scale <= 12)
                {
                    s <<= scale;
                    s >>= 1;
                }
                else
                {
                    s &= ~0x7ff;
                }

                //apply IIR filter (2 is the most commonly used)
                var p1 = v.Buffer.Read(v.BufPos - 1);
                var p2 = v.Buffer.Read(v.BufPos - 2) >> 1;

                switch (filter)
                {
                case 0: break; //no filter
                case 1: //s += p1 * 0.46875
                    s += p1 >> 1;
                    s += (-p1) >> 5;
                    break;

                case 2: //s += p1 * 0.953125 - p2 * 0.46875
                    s += p1;
                    s -= p2;
                    s += p2 >> 4;
                    s += (p1 * -3) >> 6;
                    break;

                case 3: //s += p1 * 0.8984375 - p2 * 0.40625
                    s += p1;
                    s -= p2;
                    s += (p1 * -13) >> 7;
                    s += (p2 * 3) >> 4;
                    break;
                }

                //adjust and write sample
                s = SignedClamp(s);
                s = (short)(s << 1);
                v.Buffer.Write((uint)v.BufPos++, s);
                if (v.BufPos >= BRR_BUFFER_SIZE)
                {
                    v.BufPos = 0;
                }
            }
        }

        //misc
        private void misc_27()
        {
            state.Pmon = state.Regs[PMON] & ~1; //voice 0 doesn't support PMON
        }

        private void misc_28()
        {
            state.Non = state.Regs[NON];
            state.Eon = state.Regs[EON];
            state.Dir = state.Regs[DIR];
        }

        private void misc_29()
        {
            state.EveryOtherSample = !state.EveryOtherSample;

            if (state.EveryOtherSample)
            {
                state.NewKon &= ~state.Kon; //clears KON 63 clocks after it was last read
            }
        }

        private void misc_30()
        {
            if (state.EveryOtherSample)
            {
                state.Kon = state.NewKon;
                state.Koff = state.Regs[KOFF];
            }

            counter_tick();

            //noise
            if (counter_poll((uint)(state.Regs[FLG] & 0x1f)))
            {
                var feedback = (state.Noise << 13) ^ (state.Noise << 14);
                state.Noise = (feedback & 0x4000) ^ (state.Noise >> 1);
            }
        }

        //voice
        private void voice_output(Voice v, int channel)
        {   //apply left/right volume
            var amp = (state.Output * (sbyte)(state.Regs[v.Vidx + VOLL + channel])) >> 7;

            //add to output total
            state.MainOut[channel] += amp;
            state.MainOut[channel] = SignedClamp(state.MainOut[channel]);

            //optionally add to echo total
            if ((state.Eon & v.Vbit) != 0)
            {
                state.EchoOut[channel] += amp;
                state.EchoOut[channel] = SignedClamp(state.EchoOut[channel]);
            }
        }

        private void V1(Voice v)
        {
            state.DirAddr = (state.Dir << 8) + (state.Srcn << 2);
            state.Srcn = state.Regs[v.Vidx + SRCN];
        }

        private void V2(Voice v)
        {   //read sample pointer (ignored if not needed)
            var addr = (ushort)state.DirAddr;
            if (v.KonDelay == 0)
            {
                addr += 2;
            }
            var lo = psram.Read((ushort)(addr + 0));
            var hi = psram.Read((ushort)(addr + 1));
            state.BrrNextAddr = ((hi << 8) + lo);

            state.Adsr0 = state.Regs[v.Vidx + ADSR0];

            //read pitch, spread over two clocks
            state.Pitch = state.Regs[v.Vidx + PITCHL];
        }

        private void V3(Voice v)
        {
            V3A(v);
            V3B(v);
            V3C(v);
        }

        private void V3A(Voice v)
        {
            state.Pitch += (state.Regs[v.Vidx + PITCHH] & 0x3f) << 8;
        }

        private void V3B(Voice v)
        {
            state.BrrByte = psram.Read((ushort)(v.BrrAddr + v.BrrOffset));
            state.BrrHeader = psram.Read((ushort)(v.BrrAddr));
        }

        private void V3C(Voice v)
        {   //pitch modulation using previous voice's output
            if ((state.Pmon & v.Vbit) != 0)
            {
                state.Pitch += ((state.Output >> 5) * state.Pitch) >> 10;
            }

            if (v.KonDelay != 0)
            {
                //get ready to start BRR decoding on next sample
                if (v.KonDelay == 5)
                {
                    v.BrrAddr = state.BrrNextAddr;
                    v.BrrOffset = 1;
                    v.BufPos = 0;
                    state.BrrHeader = 0; //header is ignored on this sample
                }

                //envelope is never run during KON
                v.Env = 0;
                v.HiddenEnv = 0;

                //disable BRR decoding until last three samples
                v.InterpPos = 0;
                v.KonDelay--;

                if ((v.KonDelay & 3) != 0)
                {
                    v.InterpPos = 0x4000;
                }

                //pitch is never added during KON
                state.Pitch = 0;
            }

            //gaussian interpolation
            var output = gaussian_interpolate(v);

            //noise
            if ((state.Non & v.Vbit) != 0)
            {
                output = (short)(state.Noise << 1);
            }

            //apply envelope
            state.Output = ((output * v.Env) >> 11) & ~1;
            v.EnvxOut = v.Env >> 4;

            //immediate silence due to end of sample or soft reset
            if ((state.Regs[FLG] & 0x80) != 0 || (state.BrrHeader & 3) == 1)
            {
                v.EnvMode = ENVELOPE_R;
                v.Env = 0;
            }

            if (state.EveryOtherSample)
            {
                //KOFF
                if ((state.Koff & v.Vbit) != 0)
                {
                    v.EnvMode = ENVELOPE_R;
                }

                //KON
                if ((state.Kon & v.Vbit) != 0)
                {
                    v.KonDelay = 5;
                    v.EnvMode = ENVELOPE_A;
                }
            }

            //run envelope for next sample
            if (v.KonDelay == 0)
            {
                envelope_run(v);
            }
        }

        private void V4(Voice v)
        {   //decode BRR
            state.Looped = 0;
            if (v.InterpPos >= 0x4000)
            {
                brr_decode(v);
                v.BrrOffset += 2;
                if (v.BrrOffset >= 9)
                {
                    //start decoding next BRR block
                    v.BrrAddr = (ushort)(v.BrrAddr + 9);
                    if ((state.BrrHeader & 1) != 0)
                    {
                        v.BrrAddr = state.BrrNextAddr;
                        state.Looped = v.Vbit;
                    }
                    v.BrrOffset = 1;
                }
            }

            //apply pitch
            v.InterpPos = (v.InterpPos & 0x3fff) + state.Pitch;

            //keep from getting too far ahead (when using pitch modulation)
            if (v.InterpPos > 0x7fff)
            {
                v.InterpPos = 0x7fff;
            }

            //output left
            voice_output(v, 0);
        }

        private void V5(Voice v)
        {   //output right
            voice_output(v, 1);

            //ENDX, OUTX and ENVX won't update if you wrote to them 1-2 clocks earlier
            state.EndxBuf = state.Regs[ENDX] | state.Looped;

            //clear bit in ENDX if KON just began
            if (v.KonDelay == 5)
            {
                state.EndxBuf &= ~v.Vbit;
            }
        }

        private void V6(Voice v)
        {
            state.OutxBuf = state.Output >> 8;
        }

        private void V7(Voice v)
        {   //update ENDX
            state.Regs[ENDX] = (byte)state.EndxBuf;
            state.EnvxBuf = v.EnvxOut;
        }

        private void V8(Voice v)
        {   //update OUTX
            state.Regs[v.Vidx + OUTX] = (byte)state.OutxBuf;
        }

        private void V9(Voice v)
        {   //update ENVX
            state.Regs[v.Vidx + ENVX] = (byte)state.EnvxBuf;
        }

        //echo
        private int calc_fir(int i, int channel)
        {
            var s = state.EchoHist[channel].Read(state.EchoHistPos + i + 1);
            return (s * (sbyte)(state.Regs[FIR + i * 0x10])) >> 6;
        }

        private int echo_output(int channel)
        {
            var output =
                (short)((state.MainOut[channel] * (sbyte)(state.Regs[MVOLL + channel * 0x10])) >> 7) +
                (short)((state.EchoIn[channel] * (sbyte)(state.Regs[EVOLL + channel * 0x10])) >> 7);

            return SignedClamp(output);
        }

        private void echo_read(int channel)
        {
            var addr = (uint)(state.EchoPtr + channel * 2);
            var lo = psram.Read((ushort)(addr + 0));
            var hi = psram.Read((ushort)(addr + 1));
            int s = (short)((hi << 8) + lo);
            state.EchoHist[channel].Write((uint)state.EchoHistPos, s >> 1);
        }

        private void echo_write(int channel)
        {
            if ((state.EchoDisabled & 0x20) == 0)
            {
                var addr = (uint)(state.EchoPtr + channel * 2);
                var s = state.EchoOut[channel];
                psram.Write((ushort)(addr + 0), (byte)(s >> 0));
                psram.Write((ushort)(addr + 1), (byte)(s >> 8));
            }

            state.EchoOut[channel] = 0;
        }

        private void echo_22()
        {   //history
            state.EchoHistPos++;
            if (state.EchoHistPos >= ECHO_HIST_SIZE)
            {
                state.EchoHistPos = 0;
            }

            state.EchoPtr = (ushort)((state.Esa << 8) + state.EchoOffset);
            echo_read(0);

            //FIR
            var l = calc_fir(0, 0);
            var r = calc_fir(0, 1);

            state.EchoIn[0] = l;
            state.EchoIn[1] = r;
        }

        private void echo_23()
        {
            var l = calc_fir(1, 0) + calc_fir(2, 0);
            var r = calc_fir(1, 1) + calc_fir(2, 1);

            state.EchoIn[0] += l;
            state.EchoIn[1] += r;

            echo_read(1);
        }

        private void echo_24()
        {
            var l = calc_fir(3, 0) + calc_fir(4, 0) + calc_fir(5, 0);
            var r = calc_fir(3, 1) + calc_fir(4, 1) + calc_fir(5, 1);

            state.EchoIn[0] += l;
            state.EchoIn[1] += r;
        }

        private void echo_25()
        {
            var l = state.EchoIn[0] + calc_fir(6, 0);
            var r = state.EchoIn[1] + calc_fir(6, 1);

            l = (short)l;
            r = (short)r;

            l += (short)calc_fir(7, 0);
            r += (short)calc_fir(7, 1);

            state.EchoIn[0] = SignedClamp(l) & ~1;
            state.EchoIn[1] = SignedClamp(r) & ~1;
        }

        private void echo_26()
        {   //left output volumes
            //(save sample for next clock so we can output both together)
            state.MainOut[0] = echo_output(0);

            //echo feedback
            var l = state.EchoOut[0] + (short)((state.EchoIn[0] * (sbyte)state.Regs[EFB]) >> 7);
            var r = state.EchoOut[1] + (short)((state.EchoIn[1] * (sbyte)state.Regs[EFB]) >> 7);

            state.EchoOut[0] = SignedClamp(l) & ~1;
            state.EchoOut[1] = SignedClamp(r) & ~1;
        }

        private void echo_27()
        {   //output
            var outl = state.MainOut[0];
            var outr = echo_output(1);
            state.MainOut[0] = 0;
            state.MainOut[1] = 0;

            //global muting isn't this simple
            //(turns DAC on and off or something, causing small ~37-sample pulse when first muted)
            if ((state.Regs[FLG] & 0x40) != 0)
            {
                outl = 0;
                outr = 0;
            }

            //output sample to DAC
            audio.Render(outl);
            audio.Render(outr);
        }

        private void echo_28()
        {
            state.EchoDisabled = state.Regs[FLG];
        }

        private void echo_29()
        {
            state.Esa = state.Regs[ESA];

            if (state.EchoOffset == 0)
            {
                state.EchoLength = (state.Regs[EDL] & 0x0f) << 11;
            }

            state.EchoOffset += 4;
            if (state.EchoOffset >= state.EchoLength)
            {
                state.EchoOffset = 0;
            }

            //write left echo
            echo_write(0);

            state.EchoDisabled = state.Regs[FLG];
        }

        private void echo_30()
        {   //write right echo
            echo_write(1);
        }

        private class State
        {
            public byte[] Regs = new byte[128];

            public ModuloArray[] EchoHist = new[] // echo history keeps most recent 8 samples
            {
                new ModuloArray(ECHO_HIST_SIZE),
                new ModuloArray(ECHO_HIST_SIZE)
            };

            public int EchoHistPos;

            public bool EveryOtherSample;  //toggles every sample
            public int Kon;                  //KON value when last checked
            public int Noise;
            public int Counter;
            public int EchoOffset;          //offset from ESA in echo buffer
            public int EchoLength;          //number of bytes that echo_offset will stop at

            //hidden registers also written to when main register is written to
            public int NewKon;

            public int EndxBuf;
            public int EnvxBuf;
            public int OutxBuf;

            //temporary state between clocks

            //read once per sample
            public int Pmon;

            public int Non;
            public int Eon;
            public int Dir;
            public int Koff;

            //read a few clocks ahead before used
            public int BrrNextAddr;

            public int Adsr0;
            public int BrrHeader;
            public int BrrByte;
            public int Srcn;
            public int Esa;
            public int EchoDisabled;

            //public state that is recalculated every sample
            public int DirAddr;

            public int Pitch;
            public int Output;
            public int Looped;
            public int EchoPtr;

            //left/right sums
            public int[] MainOut = new int[2];

            public int[] EchoOut = new int[2];
            public int[] EchoIn = new int[2];
        }

        private class Voice
        {
            public ModuloArray Buffer = new ModuloArray(BRR_BUFFER_SIZE); //decoded samples
            public int BufPos;     //place in buffer where next samples will be decoded
            public int InterpPos;  //relative fractional position in sample (0x1000 = 1.0)
            public int BrrAddr;    //address of current BRR block
            public int BrrOffset;  //current decoding offset in BRR block
            public int Vbit;        //bitmask for voice: 0x01 for voice 0, 0x02 for voice 1, etc
            public int Vidx;        //voice channel register index: 0x00 for voice 0, 0x10 for voice 1, etc
            public int KonDelay;   //KON delay/current setup phase
            public int EnvMode;
            public int Env;         //current envelope level
            public int EnvxOut;
            public int HiddenEnv;  //used by GAIN mode 7, very obscure quirk
        }
    }

    public class ModuloArray
    {
        private int size;
        private int[] buffer;

        public ModuloArray(int size)
        {
            this.size = size;
            buffer = new int[size * 3];
        }

        public int Read(int index)
        {
            return buffer[size + index];
        }

        public void Write(uint index, int value)
        {
            buffer[index] = buffer[index + size] = buffer[index + size + size] = value;
        }
    }
}

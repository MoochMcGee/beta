using System;
using Beta.Platform.Exceptions;

namespace Beta.Platform.Processors.ARM7
{
    public abstract partial class Core
    {
        private Action[] armv4Codes;
        private Action[] thumbCodes;
        private Flags cpsr = new Flags();
        private Flags spsr;
        private Flags spsrAbt = new Flags();
        private Flags spsrFiq = new Flags();
        private Flags spsrIrq = new Flags();
        private Flags spsrSvc = new Flags();
        private Flags spsrUnd = new Flags();
        private Pipeline pipeline = new Pipeline();
        private Register sp;
        private Register lr;
        private Register pc;
        private Register[] registersAbt = new Register[2];
        private Register[] registersFiq = new Register[7];
        private Register[] registersIrq = new Register[2];
        private Register[] registersSvc = new Register[2];
        private Register[] registersUnd = new Register[2];
        private Register[] registersUsr = new Register[7];
        private Register[] registers = new Register[16];
        private uint code;

        public int cycles;
        public bool halt;
        public bool interrupt;

        protected Core()
        {
            registersAbt.Initialize(() => new Register());
            registersFiq.Initialize(() => new Register());
            registersIrq.Initialize(() => new Register());
            registersSvc.Initialize(() => new Register());
            registersUnd.Initialize(() => new Register());
            registersUsr.Initialize(() => new Register());

            registers[0] = new Register();
            registers[1] = new Register();
            registers[2] = new Register();
            registers[3] = new Register();
            registers[4] = new Register();
            registers[5] = new Register();
            registers[6] = new Register();
            registers[7] = new Register();
            registers[15] = new Register();

            Isr(Mode.SVC, Vector.RST);
        }

        private void ChangeMode(uint mode)
        {
            ChangeRegisters(mode);

            if (spsr != null)
                spsr.Load(cpsr.Save());
        }

        private void ChangeRegisters(uint mode)
        {
            Register[] smallBank = null;
            Register[] largeBank = mode == Mode.FIQ
                ? registersFiq
                : registersUsr;

            registers[8] = largeBank[6];
            registers[9] = largeBank[5];
            registers[10] = largeBank[4];
            registers[11] = largeBank[3];
            registers[12] = largeBank[2];

            switch (mode)
            {
            case Mode.ABT: smallBank = registersAbt; spsr = spsrAbt; break;
            case Mode.FIQ: smallBank = registersFiq; spsr = spsrFiq; break;
            case Mode.IRQ: smallBank = registersIrq; spsr = spsrIrq; break;
            case Mode.SVC: smallBank = registersSvc; spsr = spsrSvc; break;
            case Mode.SYS: smallBank = registersUsr; spsr = null; break;
            case Mode.UND: smallBank = registersUnd; spsr = spsrUnd; break;
            case Mode.USR: smallBank = registersUsr; spsr = null; break;
            }

            sp = registers[13] = smallBank[1];
            lr = registers[14] = smallBank[0];
            pc = registers[15];
        }

        private bool GetCondition(uint condition)
        {
            switch (condition & 15)
            {
            case 0x0: /* EQ */ return cpsr.z != 0;
            case 0x1: /* NE */ return cpsr.z == 0;
            case 0x2: /* CS */ return cpsr.c != 0;
            case 0x3: /* CC */ return cpsr.c == 0;
            case 0x4: /* MI */ return cpsr.n != 0;
            case 0x5: /* PL */ return cpsr.n == 0;
            case 0x6: /* VS */ return cpsr.v != 0;
            case 0x7: /* VC */ return cpsr.v == 0;
            case 0x8: /* HI */ return cpsr.c != 0 && cpsr.z == 0;
            case 0x9: /* LS */ return cpsr.c == 0 || cpsr.z != 0;
            case 0xa: /* GE */ return cpsr.n == cpsr.v;
            case 0xb: /* LT */ return cpsr.n != cpsr.v;
            case 0xc: /* GT */ return cpsr.n == cpsr.v && cpsr.z == 0;
            case 0xd: /* LE */ return cpsr.n != cpsr.v || cpsr.z != 0;
            case 0xe: /* AL */ return true;
            case 0xf: /* NV */ return false;
            }

            throw new CompilerPleasingException();
        }

        private void Isr(uint mode, uint vector)
        {
            ChangeMode(mode);

            if (vector == Vector.FIQ ||
                vector == Vector.RST)
            {
                cpsr.f = 1;
            }

            cpsr.t = 0;
            cpsr.i = 1;
            cpsr.m = mode;

            lr.value = pipeline.decode.address;
            pc.value = vector;
            pipeline.refresh = true;
        }

        private uint LoadWord(uint address)
        {
            var data = Read(2, address & ~3u);

            switch (address & 3)
            {
            case 0: return (data >> 0) | (data << 32);
            case 1: return (data >> 8) | (data << 24);
            case 2: return (data >> 16) | (data << 16);
            case 3: return (data >> 24) | (data << 8);
            }

            throw new CompilerPleasingException();
        }

        protected abstract void Dispatch();

        protected abstract uint Read(int size, uint address);

        protected abstract void Write(int size, uint address, uint data);

        public virtual void Initialize()
        {
            Armv4Initialize();
            ThumbInitialize();
        }

        public virtual void Update()
        {
            if (halt)
            {
                cycles += 1;
            }
            else
            {
                if (cpsr.t == 0) { Armv4Execute(); }
                if (cpsr.t == 1) { ThumbExecute(); }
            }

            Dispatch();
            cycles = 0;
        }

        public uint GetProgramCursor()
        {
            return pc.value;
        }
    }
}

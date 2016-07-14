namespace Beta.Platform.Processors.RP65816
{
    public partial class Core
    {
        private void am_abs_w()
        {
            regs.aal = Read(regs.pcb, regs.pc++);
            regs.aah = Read(regs.pcb, regs.pc++);
            regs.aab = db;
        }

        private void am_abx_w()
        {
            regs.ial = Read(regs.pcb, regs.pc++);
            regs.iah = Read(regs.pcb, regs.pc++);
            regs.iab = db;
            regs.aa24 = regs.ia24 + regs.x;

            if (p.x == false || (regs.aah != regs.iah) || (code == 0x9d || code == 0x9e))
            {
                InternalOperation();
            }
        }

        private void am_abx_l()
        {
            regs.aal = Read(regs.pcb, regs.pc++);
            regs.aah = Read(regs.pcb, regs.pc++);
            regs.aab = Read(regs.pcb, regs.pc++);

            regs.aa24 += regs.x;
        }

        private void am_aby_w()
        {
            regs.ial = Read(regs.pcb, regs.pc++);
            regs.iah = Read(regs.pcb, regs.pc++);
            regs.iab = db;
            regs.aa24 = regs.ia24 + regs.y;

            if (p.x == false || (regs.aah != regs.iah) || (code == 0x99))
            {
                InternalOperation();
            }
        }

        private void am_abs_l()
        {
            regs.aal = Read(regs.pcb, regs.pc++);
            regs.aah = Read(regs.pcb, regs.pc++);
            regs.aab = Read(regs.pcb, regs.pc++);
        }

        private void am_dpg_w()
        {
            regs.aal = Read(regs.pcb, regs.pc++);
            regs.aah = regs.dph;
            regs.aab = 0x00;

            if (regs.dpl != 0)
            {
                InternalOperation();
                regs.aa += regs.dpl;
            }
        }

        private void am_dpx_w()
        {
            am_dpg_w();

            InternalOperation();
            regs.aa += regs.x;
        }

        private void am_dpy_w()
        {
            am_dpg_w();

            InternalOperation();
            regs.aa += regs.y;
        }

        private void am_imp_w()
        {
            LastCycle();
            InternalOperation();
        }

        private void am_ind_w()
        {
            am_dpg_w();

            regs.ial = Read(regs.aab, regs.aa); regs.aa24++;
            regs.iah = Read(regs.aab, regs.aa);
            regs.iab = db;
            regs.aa24 = regs.ia24;
        }

        private void am_ind_l()
        {
            am_dpg_w();

            regs.ial = Read(regs.aab, regs.aa); regs.aa24++;
            regs.iah = Read(regs.aab, regs.aa); regs.aa24++;
            regs.iab = Read(regs.aab, regs.aa);
            regs.aa24 = regs.ia24;
        }

        private void am_inx_w()
        {
            am_dpx_w();

            regs.ial = Read(regs.aab, regs.aa++);
            regs.iah = Read(regs.aab, regs.aa++);
            regs.iab = db;
            regs.aa24 = regs.ia24;
        }

        private void am_iny_w()
        {
            am_dpg_w();

            regs.ial = Read(regs.aab, regs.aa); regs.aa24++;
            regs.iah = Read(regs.aab, regs.aa);
            regs.iab = db;
            regs.aa24 = regs.ia24 + regs.y;

            if (p.x == false || (regs.aah != regs.iah) || (code == 0x91))
            {
                InternalOperation();
            }
        }

        private void am_iny_l()
        {
            am_dpg_w();

            regs.ial = Read(regs.aab, regs.aa); regs.aa24++;
            regs.iah = Read(regs.aab, regs.aa); regs.aa24++;
            regs.iab = Read(regs.aab, regs.aa);
            regs.aa24 = regs.ia24 + regs.y;
        }

        private void am_spr_w()
        {
            regs.aal = Read(regs.pcb, regs.pc++);
            regs.aah = 0x00;
            regs.aab = 0x00;

            InternalOperation();
            regs.aa += regs.sp;
        }

        private void am_spy_w()
        {
            am_spr_w();

            regs.ial = Read(regs.aab, regs.aa); regs.aa24++;
            regs.iah = Read(regs.aab, regs.aa);
            regs.ia24 = db;

            InternalOperation();
            regs.aa24 = regs.ia24 + regs.y;
        }
    }
}

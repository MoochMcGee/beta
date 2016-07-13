namespace Beta.Platform.Processors.RP65816
{
    public partial class Core
    {
        private void am_abs_w()
        {
            aa.l = Read(pc.b, pc.w++);
            aa.h = Read(pc.b, pc.w++);
            aa.b = db;
        }

        private void am_abx_w()
        {
            ia.l = Read(pc.b, pc.w++);
            ia.h = Read(pc.b, pc.w++);
            ia.b = db;
            aa.d = ia.d + x.w;

            if (p.x == false || (aa.h != ia.h) || (code == 0x9d || code == 0x9e))
            {
                InternalOperation();
            }
        }

        private void am_abx_l()
        {
            aa.l = Read(pc.b, pc.w++);
            aa.h = Read(pc.b, pc.w++);
            aa.b = Read(pc.b, pc.w++);

            aa.d += x.w;
        }

        private void am_aby_w()
        {
            ia.l = Read(pc.b, pc.w++);
            ia.h = Read(pc.b, pc.w++);
            ia.b = db;
            aa.d = ia.d + y.w;

            if (p.x == false || (aa.h != ia.h) || (code == 0x99))
            {
                InternalOperation();
            }
        }

        private void am_abs_l()
        {
            aa.l = Read(pc.b, pc.w++);
            aa.h = Read(pc.b, pc.w++);
            aa.b = Read(pc.b, pc.w++);
        }

        private void am_dpg_w()
        {
            aa.l = Read(pc.b, pc.w++);
            aa.h = dp.h;
            aa.b = 0x00;

            if (dp.l != 0)
            {
                InternalOperation();
                aa.w += dp.l;
            }
        }

        private void am_dpx_w()
        {
            am_dpg_w();

            InternalOperation();
            aa.w += x.w;
        }

        private void am_dpy_w()
        {
            am_dpg_w();

            InternalOperation();
            aa.w += y.w;
        }

        private void am_imp_w()
        {
            LastCycle();
            InternalOperation();
        }

        private void am_ind_w()
        {
            am_dpg_w();

            ia.l = Read(aa.b, aa.w); aa.d++;
            ia.h = Read(aa.b, aa.w);
            ia.b = db;
            aa.d = ia.d;
        }

        private void am_ind_l()
        {
            am_dpg_w();

            ia.l = Read(aa.b, aa.w); aa.d++;
            ia.h = Read(aa.b, aa.w); aa.d++;
            ia.b = Read(aa.b, aa.w);
            aa.d = ia.d;
        }

        private void am_inx_w()
        {
            am_dpx_w();

            ia.l = Read(aa.b, aa.w++);
            ia.h = Read(aa.b, aa.w++);
            ia.b = db;
            aa.d = ia.d;
        }

        private void am_iny_w()
        {
            am_dpg_w();

            ia.l = Read(aa.b, aa.w); aa.d++;
            ia.h = Read(aa.b, aa.w);
            ia.b = db;
            aa.d = ia.d + y.w;

            if (p.x == false || (aa.h != ia.h) || (code == 0x91))
            {
                InternalOperation();
            }
        }

        private void am_iny_l()
        {
            am_dpg_w();

            ia.l = Read(aa.b, aa.w); aa.d++;
            ia.h = Read(aa.b, aa.w); aa.d++;
            ia.b = Read(aa.b, aa.w);
            aa.d = ia.d + y.w;
        }

        private void am_spr_w()
        {
            aa.l = Read(pc.b, pc.w++);
            aa.h = 0x00;
            aa.b = 0x00;

            InternalOperation();
            aa.w += sp.w;
        }

        private void am_spy_w()
        {
            am_spr_w();

            ia.l = Read(aa.b, aa.w); aa.d++;
            ia.h = Read(aa.b, aa.w);
            ia.d = db;

            InternalOperation();
            aa.d = ia.d + y.w;
        }
    }
}

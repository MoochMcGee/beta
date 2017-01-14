using System;

namespace Beta.Platform.Processors.RP65816
{
    public abstract partial class Core
    {
        private Registers regs;
        private Status p;
        private byte code;
        private byte db;

        private bool interrupt;
        private bool irq;
        private bool nmi;

        public virtual void Initialize()
        {
            regs.pcl = Read(0, 0xfffc);
            regs.pch = Read(0, 0xfffd);
            regs.pcb = 0;
            regs.sp = 0x1ff;

            p.e = true;
            p.m = true;
            p.x = true;
            p.d = false;
            p.i = true;
        }

        public virtual void Update()
        {
            switch (code = Read(regs.pcb, regs.pc++))
            {
            case 0x00: goto default; /* BRK #$nn */
            case 0x01: am_inx_w(); op_ora_m(); break; /* ORA ($nn,x) */
            case 0x02: goto default; /* COP #$nn */
            case 0x03: am_spr_w(); op_ora_m(); break; /* ORA $nn,s */
            case 0x04: am_dpg_w(); op_tsb_m(); break; /* TSB $nn */
            case 0x05: am_dpg_w(); op_ora_m(); break; /* ORA $nn */
            case 0x06: am_dpg_w(); op_asl_m(); break; /* ASL $nn */
            case 0x07: am_ind_l(); op_ora_m(); break; /* ORA [$nn] */
            case 0x08: /*       */ op_php_i(); break; /* PHP */

            case 0x09: /* ORA #$nnnn */
                if (p.m || p.e)
                {
                    LastCycle();
                    regs.rdl = Read(regs.pcb, regs.pc++);
                    op_ora_b();
                }
                else
                {
                    regs.rdl = Read(regs.pcb, regs.pc++);
                    LastCycle();
                    regs.rdh = Read(regs.pcb, regs.pc++);
                    op_ora_w();
                }
                break;

            case 0x0a: am_imp_w(); op_asl_a(); break; /* ASL A */
            case 0x0b: /*       */ op_phd_i(); break; /* PHD */
            case 0x0c: am_abs_w(); op_tsb_m(); break; /* TSB $nnnn */
            case 0x0d: am_abs_w(); op_ora_m(); break; /* ORA $nnnn */
            case 0x0e: am_abs_w(); op_asl_m(); break; /* ASL $nnnn */
            case 0x0f: am_abs_l(); op_ora_m(); break; /* ORA $nn:nnnn */
            case 0x10: Branch(p.n == false); break; /* BPL #$nn */
            case 0x11: am_iny_w(); op_ora_m(); break; /* ORA ($nn),y */
            case 0x12: am_ind_w(); op_ora_m(); break; /* ORA ($nn) */
            case 0x13: am_spy_w(); op_ora_m(); break; /* ORA ($nn,s),y */
            case 0x14: am_dpg_w(); op_trb_m(); break; /* TRB $nn */
            case 0x15: am_dpx_w(); op_ora_m(); break; /* ORA $nn,x */
            case 0x16: am_dpx_w(); op_asl_m(); break; /* ASL $nn,x */
            case 0x17: am_iny_l(); op_ora_m(); break; /* ORA [$nn],y */
            case 0x18: am_imp_w(); op_clc_i(); break; /* CLC */
            case 0x19: am_aby_w(); op_ora_m(); break; /* ORA $nnnn,y */
            case 0x1a: am_imp_w(); op_inc_a(); break; /* INC A */
            case 0x1b: am_imp_w(); op_tcs_i(); break; /* TCS */
            case 0x1c: am_abs_w(); op_trb_m(); break; /* TRB $nnnn */
            case 0x1d: am_abx_w(); op_ora_m(); break; /* ORA $nnnn,x */
            case 0x1e: am_abx_w(); op_asl_m(); break; /* ASL $nnnn,x */
            case 0x1f: am_abx_l(); op_ora_m(); break; /* ORA $nn:nnnn,x */

            case 0x20: /* JSR $nnnn */
                regs.aal = Read(regs.pcb, regs.pc); regs.pc++;
                regs.aah = Read(regs.pcb, regs.pc);
                InternalOperation();
                Write(0, regs.sp--, regs.pch); if (p.e) { regs.sph = 1; }
                LastCycle();
                Write(0, regs.sp--, regs.pcl); if (p.e) { regs.sph = 1; }
                regs.pc = regs.aa;
                break;

            case 0x21: am_inx_w(); op_and_m(); break; /* AND ($nn,x) */

            case 0x22: /* JSR $nn:nnnn */
                regs.aal = Read(regs.pcb, regs.pc++);
                regs.aah = Read(regs.pcb, regs.pc++);
                Write(0, regs.sp--, regs.pcb);
                InternalOperation();
                regs.pcb = Read(regs.pcb, regs.pc);
                Write(0, regs.sp--, regs.pch);
                LastCycle();
                Write(0, regs.sp--, regs.pcl);
                regs.pc = regs.aa;

                if (p.e)
                {
                    regs.sph = 1;
                }
                break;

            case 0x23: am_spr_w(); op_and_m(); break; /* AND $nn,s */
            case 0x24: am_dpg_w(); op_bit_m(); break; /* BIT $nn */
            case 0x25: am_dpg_w(); op_and_m(); break; /* AND $nn */
            case 0x26: am_dpg_w(); op_rol_m(); break; /* ROL $nn */
            case 0x27: am_ind_l(); op_and_m(); break; /* AND [$nn] */
            case 0x28: /*       */ op_plp_i(); break; /* PLP */

            case 0x29: /* AND #$nnnn */
                if (p.m || p.e)
                {
                    LastCycle();
                    regs.rdl = Read(regs.pcb, regs.pc++);
                    op_and_b();
                }
                else
                {
                    regs.rdl = Read(regs.pcb, regs.pc++);
                    LastCycle();
                    regs.rdh = Read(regs.pcb, regs.pc++);
                    op_and_w();
                }
                break;

            case 0x2a: am_imp_w(); op_rol_a(); break; /* ROL A */
            case 0x2b: /*       */ op_pld_i(); break; /* PLD */
            case 0x2c: am_abs_w(); op_bit_m(); break; /* BIT $nnnn */
            case 0x2d: am_abs_w(); op_and_m(); break; /* AND $nnnn */
            case 0x2e: am_abs_w(); op_rol_m(); break; /* ROL $nnnn */
            case 0x2f: am_abs_l(); op_and_m(); break; /* AND $nn:nnnn */
            case 0x30: Branch(p.n == true); break; /* BMI #$nn */
            case 0x31: am_iny_w(); op_and_m(); break; /* AND ($nn),y */
            case 0x32: am_ind_w(); op_and_m(); break; /* AND ($nn) */
            case 0x33: am_spy_w(); op_and_m(); break; /* AND ($nn,s),y */
            case 0x34: am_dpx_w(); op_bit_m(); break; /* BIT $nn,x */
            case 0x35: am_dpx_w(); op_and_m(); break; /* AND $nn,x */
            case 0x36: am_dpx_w(); op_rol_m(); break; /* ROL $nn,x */
            case 0x37: am_iny_l(); op_and_m(); break; /* AND [$nn],y */
            case 0x38: am_imp_w(); op_sec_i(); break; /* SEC */
            case 0x39: am_aby_w(); op_and_m(); break; /* AND $nnnn,y */
            case 0x3a: am_imp_w(); op_dec_a(); break; /* DEC A */
            case 0x3b: am_imp_w(); op_tsc_i(); break; /* TSC */
            case 0x3c: am_abx_w(); op_bit_m(); break; /* BIT $nnnn,x */
            case 0x3d: am_abx_w(); op_and_m(); break; /* AND $nnnn,x */
            case 0x3e: am_abx_w(); op_rol_m(); break; /* ROL $nnnn,x */
            case 0x3f: am_abx_l(); op_and_m(); break; /* AND $nn:nnnn,x */
            case 0x40: /*       */ op_rti_i(); break; /* RTI */
            case 0x41: am_inx_w(); op_eor_m(); break; /* EOR ($nn,x) */
            case 0x42: am_imp_w(); break; /* WDM */
            case 0x43: am_spr_w(); op_eor_m(); break; /* EOR $nn,s */
            case 0x44: op_move(-1); break; /* MVP #$nn, #$nn */
            case 0x45: am_dpg_w(); op_eor_m(); break; /* EOR $nn */
            case 0x46: am_dpg_w(); op_lsr_m(); break; /* LSR $nn */
            case 0x47: am_ind_l(); op_eor_m(); break; /* EOR [$nn] */
            case 0x48: /*       */ op_pha_i(); break; /* PHA */

            case 0x49: /* EOR #$nnnn */
                if (p.m || p.e)
                {
                    LastCycle();
                    regs.rdl = Read(regs.pcb, regs.pc++);
                    op_eor_b();
                }
                else
                {
                    regs.rdl = Read(regs.pcb, regs.pc++);
                    LastCycle();
                    regs.rdh = Read(regs.pcb, regs.pc++);
                    op_eor_w();
                }
                break;

            case 0x4a: am_imp_w(); op_lsr_a(); break; /* LSR A */
            case 0x4b: /*       */ op_phk_i(); break; /* PHK */

            case 0x4c: /* JMP $nnnn */
                regs.aal = Read(regs.pcb, regs.pc++);
                LastCycle();
                regs.aah = Read(regs.pcb, regs.pc++);
                regs.pc = regs.aa;
                break;

            case 0x4d: am_abs_w(); op_eor_m(); break; /* EOR $nnnn */
            case 0x4e: am_abs_w(); op_lsr_m(); break; /* LSR $nnnn */
            case 0x4f: am_abs_l(); op_eor_m(); break; /* EOR $nn:nnnn */
            case 0x50: Branch(p.v == false); break; /* BVC #$nn */
            case 0x51: am_iny_w(); op_eor_m(); break; /* EOR ($nn),y */
            case 0x52: am_ind_w(); op_eor_m(); break; /* EOR ($nn) */
            case 0x53: am_spy_w(); op_eor_m(); break; /* EOR ($nn,s),y */
            case 0x54: op_move(+1); break; /* MVN #$nn, #$nn */
            case 0x55: am_dpx_w(); op_eor_m(); break; /* EOR $nn,x */
            case 0x56: am_dpx_w(); op_lsr_m(); break; /* LSR $nn,x */
            case 0x57: am_iny_l(); op_eor_m(); break; /* EOR [$nn],y */
            case 0x58: am_imp_w(); op_cli_i(); break; /* CLI */
            case 0x59: am_aby_w(); op_eor_m(); break; /* EOR $nnnn,y */
            case 0x5a: /*       */ op_phy_i(); break; /* PHY */
            case 0x5b: am_imp_w(); op_tcd_i(); break; /* TCD */

            case 0x5c: /* JMP $nn:nnnn */
                regs.aal = Read(regs.pcb, regs.pc++);
                regs.aah = Read(regs.pcb, regs.pc++);
                LastCycle();
                regs.aab = Read(regs.pcb, regs.pc++);

                regs.pc24 = regs.aa24;
                break;

            case 0x5d: am_abx_w(); op_eor_m(); break; /* EOR $nnnn,x */
            case 0x5e: am_abx_w(); op_lsr_m(); break; /* LSR $nnnn,x */
            case 0x5f: am_abx_l(); op_eor_m(); break; /* EOR $nn:nnnn,x */
            case 0x60: /*       */ op_rts_i(); break; /* RTS */
            case 0x61: am_inx_w(); op_adc_m(); break; /* ADC ($nn,x) */
            case 0x62: /*       */ op_per_i(); break; /* PER #$nnnn */
            case 0x63: am_spr_w(); op_adc_m(); break; /* ADC $nn,s */
            case 0x64: am_dpg_w(); op_stz_m(); break; /* STZ $nn */
            case 0x65: am_dpg_w(); op_adc_m(); break; /* ADC $nn */
            case 0x66: am_dpg_w(); op_ror_m(); break; /* ROR $nn */
            case 0x67: am_ind_l(); op_adc_m(); break; /* ADC [$nn] */
            case 0x68: /*       */ op_pla_i(); break; /* PLA */

            case 0x69: /* ADC #$nnnn */
                if (p.m || p.e)
                {
                    LastCycle();
                    regs.rdl = Read(regs.pcb, regs.pc++);
                    op_adc_b();
                }
                else
                {
                    regs.rdl = Read(regs.pcb, regs.pc++);
                    LastCycle();
                    regs.rdh = Read(regs.pcb, regs.pc++);
                    op_adc_w();
                }
                break;

            case 0x6a: am_imp_w(); op_ror_a(); break; /* ROR A */
            case 0x6b: /*       */ op_rtl_i(); break; /* RTL */

            case 0x6c: /* JMP ($nnnn) */
                regs.aal = Read(regs.pcb, regs.pc++);
                regs.aah = Read(regs.pcb, regs.pc++);
                regs.pcl = Read(0x00, regs.aa++);
                regs.pch = Read(0x00, regs.aa++);
                break;

            case 0x6d: am_abs_w(); op_adc_m(); break; /* ADC $nnnn */
            case 0x6e: am_abs_w(); op_ror_m(); break; /* ROR $nnnn */
            case 0x6f: am_abs_l(); op_adc_m(); break; /* ADC $nn:nnnn */
            case 0x70: Branch(p.v == true); break; /* BVS #$nn */
            case 0x71: am_iny_w(); op_adc_m(); break; /* ADC ($nn),y */
            case 0x72: am_ind_w(); op_adc_m(); break; /* ADC ($nn) */
            case 0x73: am_spy_w(); op_adc_m(); break; /* ADC ($nn,s),y */
            case 0x74: am_dpx_w(); op_stz_m(); break; /* STZ $nn,x */
            case 0x75: am_dpx_w(); op_adc_m(); break; /* ADC $nn,x */
            case 0x76: am_dpx_w(); op_ror_m(); break; /* ROR $nn,x */
            case 0x77: am_iny_l(); op_adc_m(); break; /* ADC [$nn],y */
            case 0x78: am_imp_w(); op_sei_i(); break; /* SEI */
            case 0x79: am_aby_w(); op_adc_m(); break; /* ADC $nnnn,y */
            case 0x7a: /*       */ op_ply_i(); break; /* PLY */
            case 0x7b: am_imp_w(); op_tdc_i(); break; /* TDC */

            case 0x7c: /* JMP ($nnnn,x) */
                regs.aal = Read(regs.pcb, regs.pc++);
                regs.aah = Read(regs.pcb, regs.pc++);
                regs.aab = regs.pcb;

                InternalOperation();
                regs.aa += regs.x;

                regs.pcl = Read(regs.aab, regs.aa++);
                LastCycle();
                regs.pch = Read(regs.aab, regs.aa++);
                break;

            case 0x7d: am_abx_w(); op_adc_m(); break; /* ADC $nnnn,x */
            case 0x7e: am_abx_w(); op_ror_m(); break; /* ROR $nnnn,x */
            case 0x7f: am_abx_l(); op_adc_m(); break; /* ADC $nn:nnnn,x */
            case 0x80: Branch(true); break; /* BRA #$nn */
            case 0x81: am_inx_w(); op_sta_m(); break; /* STA ($nn,x) */

            case 0x82: /* BRL #$nnnn */
                regs.rdl = Read(regs.pcb, regs.pc++);
                regs.rdh = Read(regs.pcb, regs.pc++);

                InternalOperation();
                regs.pc += regs.rd;
                break;

            case 0x83: am_spr_w(); op_sta_m(); break; /* STA $nn,s */
            case 0x84: am_dpg_w(); op_sty_x(); break; /* STY $nn */
            case 0x85: am_dpg_w(); op_sta_m(); break; /* STA $nn */
            case 0x86: am_dpg_w(); op_stx_x(); break; /* STX $nn */
            case 0x87: am_ind_l(); op_sta_m(); break; /* STA [$nn] */
            case 0x88: am_imp_w(); op_dey_i(); break; /* DEY */

            case 0x89: /* BIT #$nnnn */
                if (p.m || p.e)
                {
                    LastCycle();
                    regs.rdl = Read(regs.pcb, regs.pc++);
                    p.z = (regs.rdl & regs.al) == 0;
                }
                else
                {
                    regs.rdl = Read(regs.pcb, regs.pc++);
                    LastCycle();
                    regs.rdh = Read(regs.pcb, regs.pc++);
                    p.z = (regs.rd & regs.a) == 0;
                }
                break;

            case 0x8a: am_imp_w(); op_txa_i(); break; /* TXA */
            case 0x8b: /*       */ op_phb_i(); break; /* PHB */
            case 0x8c: am_abs_w(); op_sty_x(); break; /* STY $nnnn */
            case 0x8d: am_abs_w(); op_sta_m(); break; /* STA $nnnn */
            case 0x8e: am_abs_w(); op_stx_x(); break; /* STX $nnnn */
            case 0x8f: am_abs_l(); op_sta_m(); break; /* STA $nn:nnnn */
            case 0x90: Branch(p.c == false); break; /* BCC */
            case 0x91: am_iny_w(); op_sta_m(); break; /* STA ($nn),y */
            case 0x92: am_ind_w(); op_sta_m(); break; /* STA ($nn) */
            case 0x93: am_spy_w(); op_sta_m(); break; /* STA ($nn,s),y */
            case 0x94: am_dpx_w(); op_sty_x(); break; /* STY $nn,x */
            case 0x95: am_dpx_w(); op_sta_m(); break; /* STA $nn,x */
            case 0x96: am_dpy_w(); op_stx_x(); break; /* STX $nn,y */
            case 0x97: am_iny_l(); op_sta_m(); break; /* STA [$nn],y */
            case 0x98: am_imp_w(); op_tya_i(); break; /* TYA */
            case 0x99: am_aby_w(); op_sta_m(); break; /* STA $nnnn,y */
            case 0x9a: am_imp_w(); op_txs_i(); break; /* TXS */
            case 0x9b: am_imp_w(); op_txy_i(); break; /* TXY */
            case 0x9c: am_abs_w(); op_stz_m(); break; /* STZ $nnnn */
            case 0x9d: am_abx_w(); op_sta_m(); break; /* STA $nnnn,x */
            case 0x9e: am_abx_w(); op_stz_m(); break; /* STZ $nnnn,x */
            case 0x9f: am_abx_l(); op_sta_m(); break; /* STA $nn:nnnn,x */

            case 0xa0: /* LDY #$nnnn */
                if (p.x || p.e)
                {
                    LastCycle();
                    regs.yl = Read(regs.pcb, regs.pc++);
                    p.n = regs.yl >= 0x80;
                    p.z = regs.yl == 0x00;
                }
                else
                {
                    regs.yl = Read(regs.pcb, regs.pc++);
                    LastCycle();
                    regs.yh = Read(regs.pcb, regs.pc++);
                    p.n = regs.y >= 0x8000;
                    p.z = regs.y == 0x0000;
                }
                break;

            case 0xa1: am_inx_w(); op_lda_m(); break; /* LDA ($nn,x) */

            case 0xa2: /* LDX #$nnnn */
                if (p.x || p.e)
                {
                    LastCycle();
                    regs.xl = Read(regs.pcb, regs.pc++);
                    p.n = regs.xl >= 0x80;
                    p.z = regs.xl == 0x00;
                }
                else
                {
                    regs.xl = Read(regs.pcb, regs.pc++);
                    LastCycle();
                    regs.xh = Read(regs.pcb, regs.pc++);
                    p.n = regs.x >= 0x8000;
                    p.z = regs.x == 0x0000;
                }
                break;

            case 0xa3: am_spr_w(); op_lda_m(); break; /* LDA $nn,s */
            case 0xa4: am_dpg_w(); op_ldy_x(); break; /* LDY $nn */
            case 0xa5: am_dpg_w(); op_lda_m(); break; /* LDA $nn */
            case 0xa6: am_dpg_w(); op_ldx_x(); break; /* LDX $nn */
            case 0xa7: am_ind_l(); op_lda_m(); break; /* LDA [$nn] */
            case 0xa8: am_imp_w(); op_tay_i(); break; /* TAY */

            case 0xa9: /* LDA #$nnnn */
                if (p.m || p.e)
                {
                    LastCycle();
                    regs.al = Read(regs.pcb, regs.pc++);
                    p.n = regs.al >= 0x80;
                    p.z = regs.al == 0x00;
                }
                else
                {
                    regs.al = Read(regs.pcb, regs.pc++);
                    LastCycle();
                    regs.ah = Read(regs.pcb, regs.pc++);
                    p.n = regs.a >= 0x8000;
                    p.z = regs.a == 0x0000;
                }
                break;

            case 0xaa: am_imp_w(); op_tax_i(); break; /* TAX */
            case 0xab: /*       */ op_plb_i(); break; /* PLB */
            case 0xac: am_abs_w(); op_ldy_x(); break; /* LDA $nnnn */
            case 0xad: am_abs_w(); op_lda_m(); break; /* LDA $nnnn */
            case 0xae: am_abs_w(); op_ldx_x(); break; /* LDX $nnnn */
            case 0xaf: am_abs_l(); op_lda_m(); break; /* LDA $nn:nnnn */
            case 0xb0: Branch(p.c == true); break; /* BCS #$nn */
            case 0xb1: am_iny_w(); op_lda_m(); break; /* LDA ($nn),y */
            case 0xb2: am_ind_w(); op_lda_m(); break; /* LDA ($nn) */
            case 0xb3: am_spy_w(); op_lda_m(); break; /* LDA ($nn,s),y */
            case 0xb4: am_dpx_w(); op_ldy_x(); break; /* LDY $nn,x */
            case 0xb5: am_dpx_w(); op_lda_m(); break; /* LDA $nn,x */
            case 0xb6: am_dpy_w(); op_ldx_x(); break; /* LDX $nn,y */
            case 0xb7: am_iny_l(); op_lda_m(); break; /* LDA [$nn],y */
            case 0xb8: am_imp_w(); op_clv_i(); break; /* CLV */
            case 0xb9: am_aby_w(); op_lda_m(); break; /* LDA $nnnn,y */
            case 0xba: am_imp_w(); op_tsx_i(); break; /* TSX */
            case 0xbb: am_imp_w(); op_tyx_i(); break; /* TYX */
            case 0xbc: am_abx_w(); op_ldy_x(); break; /* LDY $nnnn,x */
            case 0xbd: am_abx_w(); op_lda_m(); break; /* LDA $nnnn,x */
            case 0xbe: am_aby_w(); op_ldx_x(); break; /* LDX $nnnn,y */
            case 0xbf: am_abx_l(); op_lda_m(); break; /* LDA $nn:nnnn,x */

            case 0xc0: /* CPY #$nnnn */
                if (p.x || p.e)
                {
                    LastCycle();
                    regs.rdl = Read(regs.pcb, regs.pc++);
                    op_cpy_b();
                }
                else
                {
                    regs.rdl = Read(regs.pcb, regs.pc++);
                    LastCycle();
                    regs.rdh = Read(regs.pcb, regs.pc++);
                    op_cpy_w();
                }
                break;

            case 0xc1: am_inx_w(); op_cmp_m(); break; /* CMP ($nn,x) */
            case 0xc2: /*       */ op_rep_i(); break; /* REP #$nn */
            case 0xc3: am_spr_w(); op_cmp_m(); break; /* CMP $nn,s */
            case 0xc4: am_dpg_w(); op_cpy_x(); break; /* CPY $nn */
            case 0xc5: am_dpg_w(); op_cmp_m(); break; /* CMP $nn */
            case 0xc6: am_dpg_w(); op_dec_m(); break; /* DEC $nn */
            case 0xc7: am_ind_l(); op_cmp_m(); break; /* CMP [$nn] */
            case 0xc8: am_imp_w(); op_iny_i(); break; /* INY */

            case 0xc9: /* CMP #$nnnn */
                if (p.m || p.e)
                {
                    LastCycle();
                    regs.rdl = Read(regs.pcb, regs.pc++);
                    op_cmp_b();
                }
                else
                {
                    regs.rdl = Read(regs.pcb, regs.pc++);
                    LastCycle();
                    regs.rdh = Read(regs.pcb, regs.pc++);
                    op_cmp_w();
                }
                break;

            case 0xca: am_imp_w(); op_dex_i(); break; /* DEX */
            case 0xcb: goto default; /* WAI */
            case 0xcc: am_abs_w(); op_cpy_x(); break; /* CPY $nnnn */
            case 0xcd: am_abs_w(); op_cmp_m(); break; /* CMP $nnnn */
            case 0xce: am_abs_w(); op_dec_m(); break; /* DEC $nnnn */
            case 0xcf: am_abs_l(); op_cmp_m(); break; /* CMP $nn:nnnn */
            case 0xd0: Branch(p.z == false); break; /* BNE #$nn */
            case 0xd1: am_iny_w(); op_cmp_m(); break; /* CMP ($nn),y */
            case 0xd2: am_ind_w(); op_cmp_m(); break; /* CMP ($nn) */
            case 0xd3: am_spy_w(); op_cmp_m(); break; /* CMP ($nn,s),y */
            case 0xd4: /*       */ op_pei_i(); break; /* PEI */
            case 0xd5: am_dpx_w(); op_cmp_m(); break; /* CMP $nn,x */
            case 0xd6: am_dpx_w(); op_dec_m(); break; /* DEC $nn,x */
            case 0xd7: am_iny_l(); op_cmp_m(); break; /* CMP [$nn],y */
            case 0xd8: am_imp_w(); op_cld_i(); break; /* CLD */
            case 0xd9: am_aby_w(); op_cmp_m(); break; /* CMP $nnnn,y */
            case 0xda: /*       */ op_phx_i(); break; /* PHX */
            case 0xdb: goto default; /* STP */

            case 0xdc: /* JMP [$nnnn] */
                regs.aal = Read(regs.pcb, regs.pc++);
                regs.aah = Read(regs.pcb, regs.pc++);
                regs.aab = 0;
                regs.pcl = Read(regs.aab, regs.aa++);
                regs.pch = Read(regs.aab, regs.aa++);
                regs.pcb = Read(regs.aab, regs.aa++);
                break;

            case 0xdd: am_abx_w(); op_cmp_m(); break; /* CMP $nnnn,x */
            case 0xde: am_abx_w(); op_dec_m(); break; /* DEC $nnnn,x */
            case 0xdf: am_abx_l(); op_cmp_m(); break; /* CMP $nn:nnnn,x */

            case 0xe0: /* CPX #$nnnn */
                if (p.x || p.e)
                {
                    LastCycle();
                    regs.rdl = Read(regs.pcb, regs.pc++);
                    op_cpx_b();
                }
                else
                {
                    regs.rdl = Read(regs.pcb, regs.pc++);
                    LastCycle();
                    regs.rdh = Read(regs.pcb, regs.pc++);
                    op_cpx_w();
                }
                break;

            case 0xe1: am_inx_w(); op_sbc_m(); break; /* SBC ($nn,x) */
            case 0xe2: /*       */ op_sep_i(); break; /* SEP #$nn */
            case 0xe3: am_spr_w(); op_sbc_m(); break; /* SBC $nn,s */
            case 0xe4: am_dpg_w(); op_cpx_x(); break; /* CPX $nn */
            case 0xe5: am_dpg_w(); op_sbc_m(); break; /* SBC $nn */
            case 0xe6: am_dpg_w(); op_inc_m(); break; /* INC $nn */
            case 0xe7: am_ind_l(); op_sbc_m(); break; /* SBC [$nn] */
            case 0xe8: am_imp_w(); op_inx_i(); break; /* INX */

            case 0xe9: /* SBC #$nnnn */
                if (p.m || p.e)
                {
                    LastCycle();
                    regs.rdl = Read(regs.pcb, regs.pc++);
                    op_sbc_b();
                }
                else
                {
                    regs.rdl = Read(regs.pcb, regs.pc++);
                    LastCycle();
                    regs.rdh = Read(regs.pcb, regs.pc++);
                    op_sbc_w();
                }
                break;

            case 0xea: am_imp_w(); /*       */ break; /* NOP */
            case 0xeb: /*       */ op_xba_i(); break; /* XBA */
            case 0xec: am_abs_w(); op_cpx_x(); break; /* CPX $nnnn */
            case 0xed: am_abs_w(); op_sbc_m(); break; /* SBC $nnnn */
            case 0xee: am_abs_w(); op_inc_m(); break; /* INC $nnnn */
            case 0xef: am_abs_l(); op_sbc_m(); break; /* SBC $nn:nnnn */
            case 0xf0: Branch(p.z == true); break; /* BEQ #$nn */
            case 0xf1: am_iny_w(); op_sbc_m(); break; /* SBC ($nn),y */
            case 0xf2: am_ind_w(); op_sbc_m(); break; /* SBC ($nn) */
            case 0xf3: am_spy_w(); op_sbc_m(); break; /* SBC ($nn,s),y */
            case 0xf4: /*       */ op_pea_i(); break; /* PEA $nnnn */
            case 0xf5: am_dpx_w(); op_sbc_m(); break; /* SBC $nn,x */
            case 0xf6: am_dpx_w(); op_inc_m(); break; /* INC $nn,x */
            case 0xf7: am_iny_l(); op_sbc_m(); break; /* SBC [$nn],y */
            case 0xf8: am_imp_w(); op_sed_i(); break; /* SED */
            case 0xf9: am_aby_w(); op_sbc_m(); break; /* SBC $nnnn,y */
            case 0xfa: /*       */ op_plx_i(); break; /* PLX */
            case 0xfb: am_imp_w(); op_xce_i(); break; /* XCE */

            case 0xfc: /* JSR ($nnnn,x) */
                regs.aal = Read(regs.pcb, regs.pc++);
                Write(0, regs.sp--, regs.pch);
                Write(0, regs.sp--, regs.pcl);
                regs.aah = Read(regs.pcb, regs.pc++);

                InternalOperation();
                regs.aa += regs.x;

                regs.pcl = Read(regs.pcb, regs.aa++);
                LastCycle();
                regs.pch = Read(regs.pcb, regs.aa++);
                break;

            case 0xfd: am_abx_w(); op_sbc_m(); break; /* SBC $nnnn,x */
            case 0xfe: am_abx_w(); op_inc_m(); break; /* INC $nnnn,x */
            case 0xff: am_abx_l(); op_sbc_m(); break; /* SBC $nn:nnnn,x */

            default:
                throw new NotImplementedException($"Instruction \"{code:x2}\" isn't implemented.");
            }

            if (interrupt)
            {
                const ushort NMI_E = 0xfffa, NMI_N = 0xffea;
                const ushort IRQ_E = 0xfffe, IRQ_N = 0xffee;

                if (nmi)
                {
                    nmi = false;
                    Isr(p.e ? NMI_E : NMI_N);
                    return;
                }

                if (irq && !p.i)
                {
                    irq = false;
                    Isr(p.e ? IRQ_E : IRQ_N);
                    return;
                }
            }
        }

        protected abstract void InternalOperation();

        protected abstract byte Read(byte bank, ushort address);

        protected abstract void Write(byte bank, ushort address, byte data);

        private void Branch(bool flag)
        {
            if (flag == false)
            {
                LastCycle();
                regs.rdl = Read(regs.pcb, regs.pc++);
            }
            else
            {
                regs.rdl = Read(regs.pcb, regs.pc++);
                regs.aa = regs.pc;
                regs.pc += (ushort)(sbyte)regs.rdl;

                if (p.e && regs.pch != regs.aah)
                {
                    InternalOperation();
                }

                LastCycle();
                InternalOperation();
            }
        }

        private void LastCycle()
        {
            interrupt = nmi || (irq && !p.i);
        }

        private void Isr(ushort vector)
        {
            Read(regs.pcb, regs.pc);
            Read(regs.pcb, regs.pc);

            if (p.e == false)
            {
                Write(0, regs.sp--, regs.pcb);
            }

            var flag = (byte)(p.e ? p.Pack() & ~0x10 : p.Pack());
            p.d = false;
            p.i = true;

            Write(0, regs.sp--, regs.pch); if (p.e) { regs.sph = 1; }
            Write(0, regs.sp--, regs.pcl); if (p.e) { regs.sph = 1; }
            Write(0, regs.sp--, flag); if (p.e) { regs.sph = 1; }

            regs.pcl = Read(0, vector++);
            regs.pch = Read(0, vector++);
            regs.pcb = 0;
        }

        public void Irq()
        {
            this.irq = true;
        }

        public void Nmi()
        {
            this.nmi = true;
        }
    }
}

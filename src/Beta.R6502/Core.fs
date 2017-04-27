namespace Beta.R6502


module Core =

  open Common
  open Codes
  open Modes


  let private map =
    //           0x00             0x01             0x02             0x03             0x04             0x05             0x06             0x07             0x08             0x09             0x0a             0x0b             0x0c             0x0d             0x0e             0x0f
    [          op_brk; am_inx_r op_ora;              [];              [];              []; am_zpg_r op_ora; am_zpg_m op_asl;              [];          op_php; am_imm_r op_ora; am_acc_r op_asl;              [];              []; am_abs_r op_ora; am_abs_m op_asl;              []; // 0x00
               op_bpl; am_iny_r op_ora;              [];              [];              []; am_zpx_r op_ora; am_zpx_m op_asl;              []; am_imp_r op_clc; am_aby_r op_ora;              [];              [];              []; am_abx_r op_ora; am_abx_m op_asl;              []; // 0x10
               op_jsr; am_inx_r op_and;              [];              []; am_zpg_r op_bit; am_zpg_r op_and; am_zpg_m op_rol;              [];          op_plp; am_imm_r op_and; am_acc_r op_rol;              []; am_abs_r op_bit; am_abs_r op_and; am_abs_m op_rol;              []; // 0x20
               op_bmi; am_iny_r op_and;              [];              [];              []; am_zpx_r op_and; am_zpx_m op_rol;              []; am_imp_r op_sec; am_aby_r op_and;              [];              [];              []; am_abx_r op_and; am_abx_m op_rol;              []; // 0x30
               op_rti; am_inx_r op_eor;              [];              [];              []; am_zpg_r op_eor; am_zpg_m op_lsr;              [];          op_pha; am_imm_r op_eor; am_acc_r op_lsr;              [];          op_jmp; am_abs_r op_eor; am_abs_m op_lsr;              []; // 0x40
               op_bvc; am_iny_r op_eor;              [];              [];              []; am_zpx_r op_eor; am_zpx_m op_lsr;              []; am_imp_r op_cli; am_aby_r op_eor;              [];              [];              []; am_abx_r op_eor; am_abx_m op_lsr;              []; // 0x50
               op_rts; am_inx_r op_adc;              [];              [];              []; am_zpg_r op_adc; am_zpg_m op_ror;              [];          op_pla; am_imm_r op_adc; am_acc_r op_ror;              [];          op_jmi; am_abs_r op_adc; am_abs_m op_ror;              []; // 0x60
               op_bvs; am_iny_r op_adc;              [];              [];              []; am_zpx_r op_adc; am_zpx_m op_ror;              []; am_imp_r op_sei; am_aby_r op_adc;              [];              [];              []; am_abx_r op_adc; am_abx_m op_ror;              []; // 0x70
                   []; am_inx_w op_sta;              [];              []; am_zpg_w op_sty; am_zpg_w op_sta; am_zpg_w op_stx;              []; am_imp_r op_dey;              []; am_imp_r op_txa;              []; am_abs_w op_sty; am_abs_w op_sta; am_abs_w op_stx;              []; // 0x80
               op_bcc; am_iny_w op_sta;              [];              []; am_zpx_w op_sty; am_zpx_w op_sta; am_zpy_w op_stx;              []; am_imp_r op_tya; am_aby_w op_sta; am_imp_r op_txs;              [];              []; am_abx_w op_sta;              [];              []; // 0x90
      am_imm_r op_ldy; am_inx_r op_lda; am_imm_r op_ldx;              []; am_zpg_r op_ldy; am_zpg_r op_lda; am_zpg_r op_ldx;              []; am_imp_r op_tay; am_imm_r op_lda; am_imp_r op_tax;              []; am_abs_r op_ldy; am_abs_r op_lda; am_abs_r op_ldx;              []; // 0xa0
               op_bcs; am_iny_r op_lda;              [];              []; am_zpx_r op_ldy; am_zpx_r op_lda; am_zpy_r op_ldx;              []; am_imp_r op_clv; am_aby_r op_lda; am_imp_r op_tsx;              []; am_abx_r op_ldy; am_abx_r op_lda; am_aby_r op_ldx;              []; // 0xb0
      am_imm_r op_cpy; am_inx_r op_cmp;              [];              []; am_zpg_r op_cpy; am_zpg_r op_cmp; am_zpg_m op_dec;              []; am_imp_r op_iny; am_imm_r op_cmp; am_imp_r op_dex;              []; am_abs_r op_cpy; am_abs_r op_cmp; am_abs_m op_dec;              []; // 0xc0
               op_bne; am_iny_r op_cmp;              [];              [];              []; am_zpx_r op_cmp; am_zpx_m op_dec;              []; am_imp_r op_cld; am_aby_r op_cmp;              [];              [];              []; am_abx_r op_cmp; am_abx_m op_dec;              []; // 0xd0
      am_imm_r op_cpx; am_inx_r op_sbc;              [];              []; am_zpg_r op_cpx; am_zpg_r op_sbc; am_zpg_m op_inc;              []; am_imp_r op_inx; am_imm_r op_sbc; am_imp_r op_nop;              []; am_abs_r op_cpx; am_abs_r op_sbc; am_abs_m op_inc;              []; // 0xe0
               op_beq; am_iny_r op_sbc;              [];              [];              []; am_zpx_r op_sbc; am_zpx_m op_inc;              []; am_imp_r op_sed; am_aby_r op_sbc;              [];              [];              []; am_abx_r op_sbc; am_abx_m op_inc;              []  // 0xf0
    ]
    |> List.map List.toArray
    |> List.toArray


  let tick e =
    e

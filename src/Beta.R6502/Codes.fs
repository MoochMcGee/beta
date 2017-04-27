namespace Beta.R6502


module Codes =

  open Common


  let private add e data =
    let next = e.a + data + e.flag.c in

    let z = next = 0uy

    let v =
      ((e.a ^^^ data) &&& 0x80uy) = 0x00uy &&
      ((e.a ^^^ next) &&& 0x80uy) = 0x80uy

    let c =
      ((e.a &&& data) &&& ~~~next &&& 0x80uy) = 0x80uy ||
      ((e.a ^^^ data) &&& ~~~next &&& 0x80uy) = 0x80uy

    { e with
        a = next;

        flag =
          { e.flag with
              n = (next >>> 7) &&& 1uy;
              v = if v then 1uy else 0uy;
              z = if z then 1uy else 0uy;
              c = if c then 1uy else 0uy;
          }
    }


  let private branch flag =
    [ read_from_pc >> inc_pc
      move_data_to_temp >> step_time_by_4_if_flag flag
      read_from_pc
      id
      read_from_pc
      id
      read_from_pc >> inc_pc
      move_data_to_code
    ]


  let private compare e reg =
    let next = reg - e.temp in
    let temp = e.temp in

    { e with
        flag =
          { e.flag with
              n = (next >>> 7) &&& 1uy;
              z = if next = 0uy then 1uy else 0uy;
              c = if temp > reg then 1uy else 0uy;
          }
    }


  let op_adc e =
    add e e.temp


  let op_and e =
    set_a_register e (e.a &&& e.temp)


  let op_asl e =
    let next = e.temp <<< 1 in
    let n = (next >>> 7) &&& 1uy in
    let z = if next = 0uy then 1uy else 0uy in
    let c = (e.temp >>> 7) &&& 1uy in

    { e with
        temp = next;
        flag =
          { e.flag with
              n = n;
              z = z;
              c = c;
          };
    }


  let op_bcc =
    branch (fun e -> e.flag.c = 0uy)


  let op_bcs =
    branch (fun e -> e.flag.c = 1uy)


  let op_beq =
    branch (fun e -> e.flag.z = 1uy)


  let op_bit e =
    let n = (e.temp >>> 7) &&& 1uy in
    let v = (e.temp >>> 6) &&& 1uy in
    let z = (e.temp &&& e.a) = 0uy in

    { e with
        flag =
          { e.flag with
              n = n;
              v = v;
              z = if z then 1uy else 0uy;
          };
    }


  let op_bmi =
    branch (fun e -> e.flag.n = 1uy)


  let op_bne =
    branch (fun e -> e.flag.z = 0uy)


  let op_bpl =
    branch (fun e -> e.flag.n = 0uy)


  let op_brk =
    [ read_from_pc >> inc_pc
      id

      write_pch_to_sp >> dec_sp
      id

      write_pcl_to_sp >> dec_sp
      id

      write_p_to_sp >> dec_sp >> vector
      id

      read_from_ea >> inc_ea
      move_data_to_pcl

      read_from_ea
      move_data_to_pch

      read_from_pc >> inc_pc
      move_data_to_code
    ]


  let op_bvc =
    branch (fun e -> e.flag.c = 1uy)


  let op_bvs =
    branch (fun e -> e.flag.v = 1uy)


  let op_clc e =
    { e with flag = { e.flag with c = 0uy; } }


  let op_cld e =
    { e with flag = { e.flag with d = 0uy; } }


  let op_cli e =
    { e with flag = { e.flag with i = 0uy; } }


  let op_clv e =
    { e with flag = { e.flag with v = 0uy; } }


  let op_cmp e =
    compare e e.a


  let op_cpx e =
    compare e e.x


  let op_cpy e =
    compare e e.y


  let op_dec e =
    let next = e.temp - 1uy in

    { e with
        temp = next;
        flag = Flags.nz e.flag next;
    }


  let op_dex e =
    set_x_register e (e.x - 1uy)


  let op_dey e =
    set_y_register e (e.y - 1uy)


  let op_eor e =
    set_a_register e (e.a ^^^ e.temp)


  let op_inc e =
    let next = e.temp + 1uy in

    { e with
        temp = next;
        flag = Flags.nz e.flag next;
    }


  let op_inx e =
    set_x_register e (e.x + 1uy)


  let op_iny e =
    set_y_register e (e.y + 1uy)


  let op_jmi =
    [ read_from_pc >> inc_pc
      move_data_to_eal

      read_from_pc
      move_data_to_eah

      read_from_ea >> inc_ea
      move_data_to_pcl

      read_from_ea
      move_data_to_pch

      read_from_pc >> inc_pc
      move_data_to_code
    ]


  let op_jmp =
    [ read_from_pc >> inc_pc
      move_data_to_temp

      read_from_pc
      move_data_to_pch >> move_temp_to_pcl

      read_from_pc >> inc_pc
      move_data_to_code
    ]


  let op_jsr =
    [ read_from_pc >> inc_pc
      move_data_to_temp

      read_from_sp
      id

      write_pch_to_sp >> dec_sp
      id

      write_pcl_to_sp >> dec_sp
      id

      read_from_pc
      move_data_to_pch >> move_temp_to_pcl
    ]


  let op_lda e =
    set_a_register e e.temp


  let op_ldx e =
    set_x_register e e.temp


  let op_ldy e =
    set_y_register e e.temp


  let op_lsr e =
    let next = e.temp >>> 1 in
    let n = (next >>> 7) &&& 1uy in
    let z = if next = 0uy then 1uy else 0uy in
    let c = e.temp &&& 1uy in

    { e with
        temp = next;
        flag =
          { e.flag with
              n = n;
              z = z;
              c = c;
          };
    }


  let op_ora e =
    set_a_register e (e.a ||| e.temp)


  let op_nop e =
    e


  let op_pha =
    [ read_from_pc
      id

      write_a_to_sp >> dec_sp
      id

      read_from_pc >> inc_pc
      move_data_to_code
    ]


  let op_php =
    [ read_from_pc
      id

      write_p_to_sp >> dec_sp
      id

      read_from_pc >> inc_pc
      move_data_to_code
    ]


  let op_pla =
    [ read_from_pc
      id

      read_from_sp >> inc_sp
      id

      read_from_sp
      move_data_to_a

      read_from_pc >> inc_pc
      move_data_to_code
    ]


  let op_plp =
    [ read_from_pc
      id

      read_from_sp >> inc_sp
      id

      read_from_sp
      move_data_to_p

      read_from_pc >> inc_pc
      move_data_to_code
    ]


  let op_rol e =
    let next = (e.temp <<< 1) ||| (e.flag.c >>> 0) in
    let n = (next >>> 7) &&& 1uy in
    let z = if next = 0uy then 1uy else 0uy in
    let c = (e.temp >>> 7) &&& 1uy in

    { e with
        temp = next;
        flag =
          { e.flag with
              n = n;
              z = z;
              c = c;
          };
    }


  let op_ror e =
    let next = (e.temp >>> 1) ||| (e.flag.c <<< 7) in
    let n = (next >>> 7) &&& 1uy in
    let z = if next = 0uy then 1uy else 0uy in
    let c = e.temp &&& 1uy in

    { e with
        temp = next;
        flag =
          { e.flag with
              n = n;
              z = z;
              c = c;
          };
    }


  let op_rti =
    [ read_from_pc
      id

      read_from_sp >> inc_sp
      id

      read_from_sp >> inc_sp
      move_data_to_p

      read_from_sp >> inc_sp
      move_data_to_pcl

      read_from_sp
      move_data_to_pch

      read_from_pc >> inc_pc
      move_data_to_code
    ]


  let op_rts =
    [ read_from_pc
      id

      read_from_sp >> inc_sp
      id

      read_from_sp >> inc_sp
      move_data_to_pcl

      read_from_sp
      move_data_to_pch

      read_from_pc >> inc_pc
      id

      read_from_pc >> inc_pc
      move_data_to_code
    ]


  let op_sbc e =
    add e (~~~e.temp)


  let op_sec e =
    { e with flag = { e.flag with c = 1uy; } }


  let op_sed e =
    { e with flag = { e.flag with d = 1uy; } }


  let op_sei e =
    { e with flag = { e.flag with i = 1uy; } }


  let op_sta e =
    write e e.ea e.a


  let op_stx e =
    write e e.ea e.x


  let op_sty e =
    write e e.ea e.y


  let op_tax e =
    set_x_register e e.a


  let op_tay e =
    set_y_register e e.a


  let op_tsx e =
    set_x_register e (uint8 e.sp)


  let op_txa e =
    set_a_register e e.x


  let op_txs e =
    { e with
        sp = 0x0100us ||| (uint16 e.x);
    }


  let op_tya e =
    set_a_register e e.y

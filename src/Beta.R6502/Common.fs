namespace Beta.R6502


module Common =

  let read e address =
    { e with
        address = address;
        read = true;
    }


  let write e address data =
    { e with
        address = address;
        data = data;
        read = false;
    }


  let set_a_register e value =
    { e with
        a = value;
        flag = Flags.nz e.flag value;
    }


  let set_x_register e value =
    { e with
        x = value;
        flag = Flags.nz e.flag value;
    }


  let set_y_register e value =
    { e with
        y = value;
        flag = Flags.nz e.flag value;
    }


  let move_a_to_temp e =
    { e with temp = e.a; }


  let move_temp_to_a e =
    { e with a = e.temp; }


  let move_temp_to_eal e =
    { e with
        ea = (uint16 e.temp);
    }


  let move_data_to_temp e =
    { e with
        temp = e.data;
    }


  let move_data_to_code e =
    let code =
      if e.ints.int_available = 1uy
      then 0x00uy
      else e.data
    in
      { e with
          code = code;
          time = 0uy;
      }


  let move_data_to_eal e =
    { e with
        ea = uint16 e.data;
    }


  let move_data_to_eah e =
    { e with
        ea = ((uint16 e.data) <<< 8) ||| (e.ea &&& 0x00ffus);
    }


  let move_data_to_pcl e =
    { e with
        pc = (e.pc &&& 0xff00us) ||| (uint16 e.data);
    }


  let move_data_to_pch e =
    { e with
        pc = (e.pc &&& 0x00ffus) ||| ((uint16 e.data) <<< 8);
    }


  let move_temp_to_pcl e =
    { e with
        pc = (e.pc &&& 0xff00us) ||| (uint16 e.data);
    }


  let move_data_to_a e =
    set_a_register e e.data


  let move_data_to_p e =
    { e with
        flag = Flags.unpack e.data
    }


  let step_time_by_2_if_c_is_0 e =
    if e.alu_c = 0uy then
      { e with
          time = e.time + 2uy;
      }
    else
      e


  let step_time_by_4_if_flag flag e =
    if flag e then
      { e with
          time = e.time + 4uy;
      }
    else
      e


  let dec_sp e =
    { e with
        sp = 0x0100us ||| ((e.sp - 1us) &&& 0xffus);
    }


  let inc_ea e =
    let lower = e.ea &&& 0xff00us in
    let upper = e.ea &&& 0x00ffus in
    let value = uint8 lower + 1uy in

    { e with
        ea = upper ||| (uint16 value);
    }


  let inc_pc e =
    { e with
        pc = e.pc + 1us;
    }


  let inc_sp e =
    { e with
        sp = 0x0100us ||| ((e.sp + 1us) &&& 0xffus);
    }


  let inc_time e =
    { e with
        time = e.time + 1uy;
    }


  let read_from_ea e =
    read e e.ea


  let read_from_pc e =
    read e e.pc


  let read_from_sp e =
    read e e.sp


  let write_to_ea e =
    write e e.ea e.temp


  let write_a_to_sp e =
    write e e.sp e.a


  let write_p_to_sp e =
    write e e.sp (Flags.pack e.flag)


  let write_pch_to_sp e =
    write e e.sp (uint8 (e.pc >>> 8))


  let write_pcl_to_sp e =
    write e e.sp (uint8 (e.pc >>> 0))


  let step_eah_by_c e =
    if e.alu_c = 1uy then
      { e with
          ea = (e.ea + 0x0100us);
      }
    else
      e


  let private step_eal_by_register e reg =
    let lower = e.ea &&& 0x00ffus in
    let upper = e.ea &&& 0xff00us in
    let value = reg + uint8 lower in

    let
      carry =
        if value < reg
        then 1uy
        else 0uy
    in
      { e with
          ea = upper ||| (uint16 value);
          alu_c = carry;
      }


  let step_eal_by_x e =
    step_eal_by_register e e.x


  let step_eal_by_y e =
    step_eal_by_register e e.y


  let vector e =
    let ints, address = Interrupt.get_vector e.ints in

    { e with
        ea = address;

        ints = ints;
    }

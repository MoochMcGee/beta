namespace Beta.R6502


module State =

  let init =
    { code = 0uy;
      time = 0uy;

      ea = 0x0000us;
      pc = 0x0000us;
      sp = 0x0100us;

      a = 0x00uy;
      x = 0x00uy;
      y = 0x00uy;

      flag = Flags.init;
      ints = Interrupt.init;

      temp = 0uy;

      alu_c = 0uy;
      alu_v = 0uy;

      address = 0x0000us
      data = 0x00uy
      read = false
      ready = true
    }

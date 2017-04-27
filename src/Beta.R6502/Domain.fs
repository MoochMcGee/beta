namespace Beta.R6502


[<AutoOpen>]
module Domain =

  type Flags =
    { n : uint8;
      v : uint8;
      d : uint8;
      i : uint8;
      z : uint8;
      c : uint8;
    }


  type Interrupts =
    { int_available : uint8;
      irq : uint8;
      nmi : uint8;
      nmi_latch : uint8;
      res : uint8;
    }


  type State =
    { code : uint8;
      time : uint8;

      ea : uint16;
      pc : uint16;
      sp : uint16;

      a : uint8;
      x : uint8;
      y : uint8;

      flag : Flags;
      ints : Interrupts;

      temp : uint8;

      alu_c : uint8;
      alu_v : uint8;

      address : uint16;
      data : uint8;
      read : bool;
      ready : bool;
    }

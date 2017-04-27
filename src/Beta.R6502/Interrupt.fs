namespace Beta.R6502


module Interrupt =

  let init =
    { int_available = 1uy;
      irq = 0uy;
      nmi = 0uy;
      nmi_latch = 0uy;
      res = 1uy;
    }


  let get_vector e =
    if e.res = 1uy then
      ({ e with res = 0uy }, 0xfffcus)
    elif e.nmi = 1uy then
      ({ e with nmi = 0uy }, 0xfffeus)
    else
      ({ e with irq = 0uy }, 0xfffaus)


  let irq e value =
    { e with
        irq = value;
    }


  let nmi e value =
    let edge =
      if e.nmi_latch < value
      then 1uy
      else 0uy
    in
      { e with
          nmi = e.nmi ||| edge
          nmi_latch = value
      }


  let poll e i =
    { e with
        int_available = e.res ||| e.nmi ||| (e.irq &&& ~~~i);
    }

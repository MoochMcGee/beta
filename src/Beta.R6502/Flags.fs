namespace Beta.R6502


module Flags =

  let init =
    { n = 0uy;
      v = 0uy;
      d = 0uy;
      i = 0uy;
      z = 0uy;
      c = 0uy;
    }


  let pack e =
    (e.n <<< 7) |||
    (e.v <<< 6) |||
    (1uy <<< 5) |||
    (1uy <<< 4) |||
    (e.d <<< 3) |||
    (e.i <<< 2) |||
    (e.z <<< 1) |||
    (e.c <<< 0)


  let unpack data =
    { n = (data >>> 7) &&& 1uy;
      v = (data >>> 6) &&& 1uy;
      d = (data >>> 3) &&& 1uy;
      i = (data >>> 2) &&& 1uy;
      z = (data >>> 1) &&& 1uy;
      c = (data >>> 0) &&& 1uy;
    }


  let nz e data =
    { e with
        n = (data >>> 7) &&& 1uy;
        z = if data = 0uy then 1uy else 0uy;
    }

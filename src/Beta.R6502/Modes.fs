namespace Beta.R6502


module Modes =

  open Common


  let private merge' t f i e =
    if i = t
    then e >> f
    else e


  let private merge t f =
    List.mapi (merge' t f)


  let private r code =
    [ read_from_ea
      move_data_to_temp

      read_from_pc >> inc_pc >> code
      move_data_to_code
    ]


  let private m code =
    [ read_from_ea
      move_data_to_temp

      write_to_ea >> code
      id

      write_to_ea
      id

      read_from_pc >> inc_pc
      move_data_to_code
    ]


  let private w code =
    [ code
      id

      read_from_pc >> inc_pc
      move_data_to_code
    ]


  let private am_abs =
    [ read_from_pc >> inc_pc
      move_data_to_eal

      read_from_pc >> inc_pc
      move_data_to_eah
    ]


  let am_abs_r code = List.concat [ am_abs; r code ]
  let am_abs_m code = List.concat [ am_abs; m code ]
  let am_abs_w code = List.concat [ am_abs; w code ]


  let private am_abs_indexed f =
    [ read_from_pc >> inc_pc
      move_data_to_eal

      read_from_pc >> inc_pc
      move_data_to_eah >> f

      read_from_ea
      step_eah_by_c
    ]


  let am_abx_r code = List.concat [ merge 3 step_time_by_2_if_c_is_0 (am_abs_indexed step_eal_by_x); r code ]
  let am_abx_m code = List.concat [ am_abs_indexed step_eal_by_x; m code ]
  let am_abx_w code = List.concat [ am_abs_indexed step_eal_by_x; w code ]


  let am_aby_r code = List.concat [ merge 3 step_time_by_2_if_c_is_0 (am_abs_indexed step_eal_by_y); r code ]
  let am_aby_m code = List.concat [ am_abs_indexed step_eal_by_y; m code ]
  let am_aby_w code = List.concat [ am_abs_indexed step_eal_by_y; w code ]


  let am_acc_r code =
    [ read_from_pc >> move_a_to_temp >> code >> move_temp_to_a
      id

      read_from_pc >> inc_pc
      move_data_to_code
    ]


  let am_imm_r code =
    [ read_from_pc >> inc_pc
      move_data_to_temp

      read_from_pc >> inc_pc >> code
      move_data_to_code
    ]


  let am_imp_r code =
    [ read_from_pc >> code
      id

      read_from_pc >> inc_pc
      move_data_to_code
    ]


  let private am_inx =
    [ read_from_pc >> inc_pc
      move_data_to_eal

      read_from_ea
      step_eal_by_x

      read_from_ea >> inc_ea
      move_data_to_temp

      read_from_ea >> move_temp_to_eal
      move_data_to_eah
    ]


  let am_inx_r code = List.concat [ am_inx; r code ]
  let am_inx_m code = List.concat [ am_inx; m code ]
  let am_inx_w code = List.concat [ am_inx; w code ]


  let private am_iny =
    [ read_from_pc >> inc_pc
      move_data_to_eal

      read_from_ea >> inc_ea
      move_data_to_temp

      read_from_ea >> move_temp_to_eal
      move_data_to_eah >> step_eal_by_y

      read_from_ea
      step_eah_by_c
    ]


  let am_iny_r code = List.concat [ merge 5 step_time_by_2_if_c_is_0 am_iny; r code ]
  let am_iny_m code = List.concat [ am_iny; m code ]
  let am_iny_w code = List.concat [ am_iny; w code ]


  let private am_zpg =
    [ read_from_pc >> inc_pc
      move_data_to_eal
    ]


  let am_zpg_r code = List.concat [ am_zpg; r code ]
  let am_zpg_m code = List.concat [ am_zpg; m code ]
  let am_zpg_w code = List.concat [ am_zpg; w code ]


  let private am_zpg_indexed f =
    [ read_from_pc >> inc_pc
      move_data_to_eal

      read_from_ea
      f
    ]


  let am_zpx_r code = List.concat [ am_zpg_indexed step_eal_by_x; r code ]
  let am_zpx_m code = List.concat [ am_zpg_indexed step_eal_by_x; m code ]
  let am_zpx_w code = List.concat [ am_zpg_indexed step_eal_by_x; w code ]


  let am_zpy_r code = List.concat [ am_zpg_indexed step_eal_by_y; r code ]
  let am_zpy_m code = List.concat [ am_zpg_indexed step_eal_by_y; m code ]
  let am_zpy_w code = List.concat [ am_zpg_indexed step_eal_by_y; w code ]

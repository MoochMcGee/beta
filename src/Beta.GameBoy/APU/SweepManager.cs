namespace Beta.GameBoy.APU
{
    public static class SweepManager
    {
        public static void Tick(Sq1State state)
        {
            if (state.sweep_timer != 0 && --state.sweep_timer == 0)
            {
                if (state.sweep_enabled == false || state.sweep_period == 0)
                {
                    return;
                }

                state.sweep_timer = state.sweep_period;

                OverflowCheck(state);
            }
        }

        public static void OverflowCheck(Sq1State state)
        {
            var period = state.sweep_direction == 0
                                ? state.period + (state.period >> state.sweep_shift)
                                : state.period - (state.period >> state.sweep_shift)
                                ;

            if (period > 0x7ff)
            {
                state.enabled = false;
            }
            else
            {
                if (period < 0)
                {
                    period = 0;
                }

                state.period = period;
            }
        }
    }
}

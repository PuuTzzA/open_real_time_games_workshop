using System;


public class SubRoutine
{
    private int tick_index;
    private int tick_count;

    private Func<int, bool> each_tick;

    public SubRoutine(int tick_count, Func<int, bool> each_tick)
    {
        this.tick_index = 0;
        this.tick_count = tick_count;
        this.each_tick = each_tick;
    }

    public bool tick()
    {
        if(tick_index < tick_count)
        {
            return each_tick(tick_index++);
        }
        return false;
    }

    public void start()
    {
        tick_index = 0;
    }
}

using System;


public class SubRoutine
{
    private int tick_index;
    private int tick_count;

    private Action<int> each_tick;

    public SubRoutine(int tick_count, Action<int> each_tick)
    {
        this.tick_index = 0;
        this.tick_count = tick_count;
        this.each_tick = each_tick;
    }

    public bool tick()
    {
        if(tick_index < tick_count)
        {
            each_tick(tick_index++);
            return true;
        }
        return false;
    }

    public void start()
    {
        tick_index = 0;
    }
}

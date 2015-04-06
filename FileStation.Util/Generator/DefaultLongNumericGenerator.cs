using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace FileStation.Util
{
    public class DefaultIntNumericGenerator
    {

        private static int MAX_STRING_LENGTH = int.MaxValue.ToString().Length;
   
    private static  int MIN_STRING_LENGTH = 1;

    private AtomicInteger count;



    public DefaultIntNumericGenerator(int initialValue)
    {
        this.count = new AtomicInteger(initialValue);
    }

    public long getNextLong() {
        return this.getNextValue();
    }

    public String getNextNumberAsString() {
        return getNextValue().ToString();
    }

    public int maxLength() {
        return DefaultIntNumericGenerator.MAX_STRING_LENGTH;
    }

    public int minLength() {
        return DefaultIntNumericGenerator.MIN_STRING_LENGTH;
    }

    protected long getNextValue() {
        if (this.count.CompareAndSet(int.MaxValue, 0)) {
            return int.MaxValue;
        }
        return this.count.GetAndIncrement();
    }
    }
    public class AtomicInteger
    {
        private int value;

        public AtomicInteger(int initialValue)
        {
            value = initialValue;
        }

        public AtomicInteger()
            : this(0)
        {
        }

        public int Get()
        {
            return value;
        }

        public void Set(int newValue)
        {
            value = newValue;
        }

        public int GetAndSet(int newValue)
        {
            for (; ; )
            {
                int current = Get();
                if (CompareAndSet(current, newValue))
                    return current;
            }
        }

        public bool CompareAndSet(int expect, int update)
        {
            return Interlocked.CompareExchange(ref value, update, expect) == expect;
        }

        public int GetAndIncrement()
        {
            for (; ; )
            {
                int current = Get();
                int next = current + 1;
                if (CompareAndSet(current, next))
                    return current;
            }
        }

        public int GetAndDecrement()
        {
            for (; ; )
            {
                int current = Get();
                int next = current - 1;
                if (CompareAndSet(current, next))
                    return current;
            }
        }

        public int GetAndAdd(int delta)
        {
            for (; ; )
            {
                int current = Get();
                int next = current + delta;
                if (CompareAndSet(current, next))
                    return current;
            }
        }

        public int IncrementAndGet()
        {
            for (; ; )
            {
                int current = Get();
                int next = current + 1;
                if (CompareAndSet(current, next))
                    return next;
            }
        }

        public int DecrementAndGet()
        {
            for (; ; )
            {
                int current = Get();
                int next = current - 1;
                if (CompareAndSet(current, next))
                    return next;
            }
        }

        public int AddAndGet(int delta)
        {
            for (; ; )
            {
                int current = Get();
                int next = current + delta;
                if (CompareAndSet(current, next))
                    return next;
            }
        }

        public override String ToString()
        {
            return Convert.ToString(Get());
        }
    }
}
using Sharpen;

namespace org.apache.hadoop.util
{
	/// <summary>Utility methods for getting the time and computing intervals.</summary>
	public sealed class Time
	{
		/// <summary>Current system time.</summary>
		/// <remarks>
		/// Current system time.  Do not use this to calculate a duration or interval
		/// to sleep, because it will be broken by settimeofday.  Instead, use
		/// monotonicNow.
		/// </remarks>
		/// <returns>current time in msec.</returns>
		public static long now()
		{
			return Sharpen.Runtime.currentTimeMillis();
		}

		/// <summary>
		/// Current time from some arbitrary time base in the past, counting in
		/// milliseconds, and not affected by settimeofday or similar system clock
		/// changes.
		/// </summary>
		/// <remarks>
		/// Current time from some arbitrary time base in the past, counting in
		/// milliseconds, and not affected by settimeofday or similar system clock
		/// changes.  This is appropriate to use when computing how much longer to
		/// wait for an interval to expire.
		/// This function can return a negative value and it must be handled correctly
		/// by callers. See the documentation of System#nanoTime for caveats.
		/// </remarks>
		/// <returns>a monotonic clock that counts in milliseconds.</returns>
		public static long monotonicNow()
		{
			long NANOSECONDS_PER_MILLISECOND = 1000000;
			return Sharpen.Runtime.nanoTime() / NANOSECONDS_PER_MILLISECOND;
		}
	}
}

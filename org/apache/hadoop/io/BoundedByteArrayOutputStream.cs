using Sharpen;

namespace org.apache.hadoop.io
{
	/// <summary>A byte array backed output stream with a limit.</summary>
	/// <remarks>
	/// A byte array backed output stream with a limit. The limit should be smaller
	/// than the buffer capacity. The object can be reused through <code>reset</code>
	/// API and choose different limits in each round.
	/// </remarks>
	public class BoundedByteArrayOutputStream : java.io.OutputStream
	{
		private byte[] buffer;

		private int startOffset;

		private int limit;

		private int currentPointer;

		/// <summary>
		/// Create a BoundedByteArrayOutputStream with the specified
		/// capacity
		/// </summary>
		/// <param name="capacity">The capacity of the underlying byte array</param>
		public BoundedByteArrayOutputStream(int capacity)
			: this(capacity, capacity)
		{
		}

		/// <summary>
		/// Create a BoundedByteArrayOutputStream with the specified
		/// capacity and limit.
		/// </summary>
		/// <param name="capacity">The capacity of the underlying byte array</param>
		/// <param name="limit">The maximum limit upto which data can be written</param>
		public BoundedByteArrayOutputStream(int capacity, int limit)
			: this(new byte[capacity], 0, limit)
		{
		}

		protected internal BoundedByteArrayOutputStream(byte[] buf, int offset, int limit
			)
		{
			resetBuffer(buf, offset, limit);
		}

		protected internal virtual void resetBuffer(byte[] buf, int offset, int limit)
		{
			int capacity = buf.Length - offset;
			if ((capacity < limit) || (capacity | limit) < 0)
			{
				throw new System.ArgumentException("Invalid capacity/limit");
			}
			this.buffer = buf;
			this.startOffset = offset;
			this.currentPointer = offset;
			this.limit = offset + limit;
		}

		/// <exception cref="System.IO.IOException"/>
		public override void write(int b)
		{
			if (currentPointer >= limit)
			{
				throw new java.io.EOFException("Reaching the limit of the buffer.");
			}
			buffer[currentPointer++] = unchecked((byte)b);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void write(byte[] b, int off, int len)
		{
			if ((off < 0) || (off > b.Length) || (len < 0) || ((off + len) > b.Length) || ((off
				 + len) < 0))
			{
				throw new System.IndexOutOfRangeException();
			}
			else
			{
				if (len == 0)
				{
					return;
				}
			}
			if (currentPointer + len > limit)
			{
				throw new java.io.EOFException("Reach the limit of the buffer");
			}
			System.Array.Copy(b, off, buffer, currentPointer, len);
			currentPointer += len;
		}

		/// <summary>Reset the limit</summary>
		/// <param name="newlim">New Limit</param>
		public virtual void reset(int newlim)
		{
			if (newlim > (buffer.Length - startOffset))
			{
				throw new System.IndexOutOfRangeException("Limit exceeds buffer size");
			}
			this.limit = newlim;
			this.currentPointer = startOffset;
		}

		/// <summary>Reset the buffer</summary>
		public virtual void reset()
		{
			this.limit = buffer.Length - startOffset;
			this.currentPointer = startOffset;
		}

		/// <summary>Return the current limit</summary>
		public virtual int getLimit()
		{
			return limit;
		}

		/// <summary>Returns the underlying buffer.</summary>
		/// <remarks>
		/// Returns the underlying buffer.
		/// Data is only valid to
		/// <see cref="size()"/>
		/// .
		/// </remarks>
		public virtual byte[] getBuffer()
		{
			return buffer;
		}

		/// <summary>
		/// Returns the length of the valid data
		/// currently in the buffer.
		/// </summary>
		public virtual int size()
		{
			return currentPointer - startOffset;
		}

		public virtual int available()
		{
			return limit - currentPointer;
		}
	}
}
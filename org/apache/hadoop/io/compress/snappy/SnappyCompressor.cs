/*
* Licensed to the Apache Software Foundation (ASF) under one
* or more contributor license agreements.  See the NOTICE file
* distributed with this work for additional information
* regarding copyright ownership.  The ASF licenses this file
* to you under the Apache License, Version 2.0 (the
* "License"); you may not use this file except in compliance
* with the License.  You may obtain a copy of the License at
*
*     http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
using Sharpen;

namespace org.apache.hadoop.io.compress.snappy
{
	/// <summary>
	/// A
	/// <see cref="org.apache.hadoop.io.compress.Compressor"/>
	/// based on the snappy compression algorithm.
	/// http://code.google.com/p/snappy/
	/// </summary>
	public class SnappyCompressor : org.apache.hadoop.io.compress.Compressor
	{
		private static readonly org.apache.commons.logging.Log LOG = org.apache.commons.logging.LogFactory
			.getLog(Sharpen.Runtime.getClassForType(typeof(org.apache.hadoop.io.compress.snappy.SnappyCompressor
			)).getName());

		private const int DEFAULT_DIRECT_BUFFER_SIZE = 64 * 1024;

		private static java.lang.Class clazz = Sharpen.Runtime.getClassForType(typeof(org.apache.hadoop.io.compress.snappy.SnappyCompressor
			));

		private int directBufferSize;

		private java.nio.Buffer compressedDirectBuf = null;

		private int uncompressedDirectBufLen;

		private java.nio.Buffer uncompressedDirectBuf = null;

		private byte[] userBuf = null;

		private int userBufOff = 0;

		private int userBufLen = 0;

		private bool finish;

		private bool finished;

		private long bytesRead = 0L;

		private long bytesWritten = 0L;

		private static bool nativeSnappyLoaded = false;

		static SnappyCompressor()
		{
			// HACK - Use this as a global lock in the JNI layer
			if (org.apache.hadoop.util.NativeCodeLoader.isNativeCodeLoaded() && org.apache.hadoop.util.NativeCodeLoader
				.buildSupportsSnappy())
			{
				try
				{
					initIDs();
					nativeSnappyLoaded = true;
				}
				catch (System.Exception t)
				{
					LOG.error("failed to load SnappyCompressor", t);
				}
			}
		}

		public static bool isNativeCodeLoaded()
		{
			return nativeSnappyLoaded;
		}

		/// <summary>Creates a new compressor.</summary>
		/// <param name="directBufferSize">size of the direct buffer to be used.</param>
		public SnappyCompressor(int directBufferSize)
		{
			this.directBufferSize = directBufferSize;
			uncompressedDirectBuf = java.nio.ByteBuffer.allocateDirect(directBufferSize);
			compressedDirectBuf = java.nio.ByteBuffer.allocateDirect(directBufferSize);
			compressedDirectBuf.position(directBufferSize);
		}

		/// <summary>Creates a new compressor with the default buffer size.</summary>
		public SnappyCompressor()
			: this(DEFAULT_DIRECT_BUFFER_SIZE)
		{
		}

		/// <summary>Sets input data for compression.</summary>
		/// <remarks>
		/// Sets input data for compression.
		/// This should be called whenever #needsInput() returns
		/// <code>true</code> indicating that more input data is required.
		/// </remarks>
		/// <param name="b">Input data</param>
		/// <param name="off">Start offset</param>
		/// <param name="len">Length</param>
		public virtual void setInput(byte[] b, int off, int len)
		{
			if (b == null)
			{
				throw new System.ArgumentNullException();
			}
			if (off < 0 || len < 0 || off > b.Length - len)
			{
				throw new System.IndexOutOfRangeException();
			}
			finished = false;
			if (len > uncompressedDirectBuf.remaining())
			{
				// save data; now !needsInput
				this.userBuf = b;
				this.userBufOff = off;
				this.userBufLen = len;
			}
			else
			{
				((java.nio.ByteBuffer)uncompressedDirectBuf).put(b, off, len);
				uncompressedDirectBufLen = uncompressedDirectBuf.position();
			}
			bytesRead += len;
		}

		/// <summary>
		/// If a write would exceed the capacity of the direct buffers, it is set
		/// aside to be loaded by this function while the compressed data are
		/// consumed.
		/// </summary>
		internal virtual void setInputFromSavedData()
		{
			if (0 >= userBufLen)
			{
				return;
			}
			finished = false;
			uncompressedDirectBufLen = System.Math.min(userBufLen, directBufferSize);
			((java.nio.ByteBuffer)uncompressedDirectBuf).put(userBuf, userBufOff, uncompressedDirectBufLen
				);
			// Note how much data is being fed to snappy
			userBufOff += uncompressedDirectBufLen;
			userBufLen -= uncompressedDirectBufLen;
		}

		/// <summary>Does nothing.</summary>
		public virtual void setDictionary(byte[] b, int off, int len)
		{
		}

		// do nothing
		/// <summary>
		/// Returns true if the input data buffer is empty and
		/// #setInput() should be called to provide more input.
		/// </summary>
		/// <returns>
		/// <code>true</code> if the input data buffer is empty and
		/// #setInput() should be called in order to provide more input.
		/// </returns>
		public virtual bool needsInput()
		{
			return !(compressedDirectBuf.remaining() > 0 || uncompressedDirectBuf.remaining()
				 == 0 || userBufLen > 0);
		}

		/// <summary>
		/// When called, indicates that compression should end
		/// with the current contents of the input buffer.
		/// </summary>
		public virtual void finish()
		{
			finish = true;
		}

		/// <summary>
		/// Returns true if the end of the compressed
		/// data output stream has been reached.
		/// </summary>
		/// <returns>
		/// <code>true</code> if the end of the compressed
		/// data output stream has been reached.
		/// </returns>
		public virtual bool finished()
		{
			// Check if all uncompressed data has been consumed
			return (finish && finished && compressedDirectBuf.remaining() == 0);
		}

		/// <summary>Fills specified buffer with compressed data.</summary>
		/// <remarks>
		/// Fills specified buffer with compressed data. Returns actual number
		/// of bytes of compressed data. A return value of 0 indicates that
		/// needsInput() should be called in order to determine if more input
		/// data is required.
		/// </remarks>
		/// <param name="b">Buffer for the compressed data</param>
		/// <param name="off">Start offset of the data</param>
		/// <param name="len">Size of the buffer</param>
		/// <returns>The actual number of bytes of compressed data.</returns>
		/// <exception cref="System.IO.IOException"/>
		public virtual int compress(byte[] b, int off, int len)
		{
			if (b == null)
			{
				throw new System.ArgumentNullException();
			}
			if (off < 0 || len < 0 || off > b.Length - len)
			{
				throw new System.IndexOutOfRangeException();
			}
			// Check if there is compressed data
			int n = compressedDirectBuf.remaining();
			if (n > 0)
			{
				n = System.Math.min(n, len);
				((java.nio.ByteBuffer)compressedDirectBuf).get(b, off, n);
				bytesWritten += n;
				return n;
			}
			// Re-initialize the snappy's output direct-buffer
			compressedDirectBuf.clear();
			compressedDirectBuf.limit(0);
			if (0 == uncompressedDirectBuf.position())
			{
				// No compressed data, so we should have !needsInput or !finished
				setInputFromSavedData();
				if (0 == uncompressedDirectBuf.position())
				{
					// Called without data; write nothing
					finished = true;
					return 0;
				}
			}
			// Compress data
			n = compressBytesDirect();
			compressedDirectBuf.limit(n);
			uncompressedDirectBuf.clear();
			// snappy consumes all buffer input
			// Set 'finished' if snapy has consumed all user-data
			if (0 == userBufLen)
			{
				finished = true;
			}
			// Get atmost 'len' bytes
			n = System.Math.min(n, len);
			bytesWritten += n;
			((java.nio.ByteBuffer)compressedDirectBuf).get(b, off, n);
			return n;
		}

		/// <summary>Resets compressor so that a new set of input data can be processed.</summary>
		public virtual void reset()
		{
			finish = false;
			finished = false;
			uncompressedDirectBuf.clear();
			uncompressedDirectBufLen = 0;
			compressedDirectBuf.clear();
			compressedDirectBuf.limit(0);
			userBufOff = userBufLen = 0;
			bytesRead = bytesWritten = 0L;
		}

		/// <summary>
		/// Prepare the compressor to be used in a new stream with settings defined in
		/// the given Configuration
		/// </summary>
		/// <param name="conf">Configuration from which new setting are fetched</param>
		public virtual void reinit(org.apache.hadoop.conf.Configuration conf)
		{
			reset();
		}

		/// <summary>Return number of bytes given to this compressor since last reset.</summary>
		public virtual long getBytesRead()
		{
			return bytesRead;
		}

		/// <summary>Return number of bytes consumed by callers of compress since last reset.
		/// 	</summary>
		public virtual long getBytesWritten()
		{
			return bytesWritten;
		}

		/// <summary>Closes the compressor and discards any unprocessed input.</summary>
		public virtual void end()
		{
		}

		private static void initIDs()
		{
		}

		private int compressBytesDirect()
		{
		}

		public static string getLibraryName()
		{
		}
	}
}
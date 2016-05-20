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

namespace org.apache.hadoop.util
{
	/// <summary>An implementation of the core algorithm of MergeSort.</summary>
	public class MergeSort
	{
		internal org.apache.hadoop.io.IntWritable I = new org.apache.hadoop.io.IntWritable
			(0);

		internal org.apache.hadoop.io.IntWritable J = new org.apache.hadoop.io.IntWritable
			(0);

		private java.util.Comparator<org.apache.hadoop.io.IntWritable> comparator;

		public MergeSort(java.util.Comparator<org.apache.hadoop.io.IntWritable> comparator
			)
		{
			//Reusable IntWritables
			//the comparator that the algo should use
			this.comparator = comparator;
		}

		public virtual void mergeSort(int[] src, int[] dest, int low, int high)
		{
			int length = high - low;
			// Insertion sort on smallest arrays
			if (length < 7)
			{
				for (int i = low; i < high; i++)
				{
					for (int j = i; j > low; j--)
					{
						I.set(dest[j - 1]);
						J.set(dest[j]);
						if (comparator.compare(I, J) > 0)
						{
							swap(dest, j, j - 1);
						}
					}
				}
				return;
			}
			// Recursively sort halves of dest into src
			int mid = (int)(((uint)(low + high)) >> 1);
			mergeSort(dest, src, low, mid);
			mergeSort(dest, src, mid, high);
			I.set(src[mid - 1]);
			J.set(src[mid]);
			// If list is already sorted, just copy from src to dest.  This is an
			// optimization that results in faster sorts for nearly ordered lists.
			if (comparator.compare(I, J) <= 0)
			{
				System.Array.Copy(src, low, dest, low, length);
				return;
			}
			// Merge sorted halves (now in src) into dest
			for (int i_1 = low; i_1 < high; i_1++)
			{
				if (q < high && p < mid)
				{
					I.set(src[p]);
					J.set(src[q]);
				}
				if (q >= high || p < mid && comparator.compare(I, J) <= 0)
				{
					dest[i_1] = src[p++];
				}
				else
				{
					dest[i_1] = src[q++];
				}
			}
		}

		private void swap(int[] x, int a, int b)
		{
			int t = x[a];
			x[a] = x[b];
			x[b] = t;
		}
	}
}
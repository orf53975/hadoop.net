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
using System;
using System.IO;


namespace Org.Apache.Hadoop.Util
{
	public class ClassUtil
	{
		/// <summary>Find a jar that contains a class of the same name, if any.</summary>
		/// <remarks>
		/// Find a jar that contains a class of the same name, if any.
		/// It will return a jar file, even if that is not the first thing
		/// on the class path that has a class with the same name.
		/// </remarks>
		/// <param name="clazz">the class to find.</param>
		/// <returns>a jar file that contains the class, or null.</returns>
		/// <exception cref="System.IO.IOException"/>
		public static string FindContainingJar(Type clazz)
		{
			ClassLoader loader = clazz.GetClassLoader();
			string classFile = clazz.FullName.ReplaceAll("\\.", "/") + ".class";
			try
			{
				for (Enumeration<Uri> itr = loader.GetResources(classFile); itr.MoveNext(); )
				{
					Uri url = itr.Current;
					if ("jar".Equals(url.Scheme))
					{
						string toReturn = url.AbsolutePath;
						if (toReturn.StartsWith("file:"))
						{
							toReturn = Runtime.Substring(toReturn, "file:".Length);
						}
						toReturn = URLDecoder.Decode(toReturn, "UTF-8");
						return toReturn.ReplaceAll("!.*$", string.Empty);
					}
				}
			}
			catch (IOException e)
			{
				throw new RuntimeException(e);
			}
			return null;
		}
	}
}

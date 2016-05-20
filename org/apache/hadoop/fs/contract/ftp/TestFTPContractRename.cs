/*
* Licensed to the Apache Software Foundation (ASF) under one
*  or more contributor license agreements.  See the NOTICE file
*  distributed with this work for additional information
*  regarding copyright ownership.  The ASF licenses this file
*  to you under the Apache License, Version 2.0 (the
*  "License"); you may not use this file except in compliance
*  with the License.  You may obtain a copy of the License at
*
*       http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" BASIS,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*  See the License for the specific language governing permissions and
*  limitations under the License.
*/
using Sharpen;

namespace org.apache.hadoop.fs.contract.ftp
{
	public class TestFTPContractRename : org.apache.hadoop.fs.contract.AbstractContractRenameTest
	{
		protected internal override org.apache.hadoop.fs.contract.AbstractFSContract createContract
			(org.apache.hadoop.conf.Configuration conf)
		{
			return new org.apache.hadoop.fs.contract.ftp.FTPContract(conf);
		}

		/// <summary>
		/// Check the exception was about cross-directory renames
		/// -if not, rethrow it.
		/// </summary>
		/// <param name="e">exception raised</param>
		/// <exception cref="System.IO.IOException"/>
		private void verifyUnsupportedDirRenameException(System.IO.IOException e)
		{
			if (!e.ToString().contains(org.apache.hadoop.fs.ftp.FTPFileSystem.E_SAME_DIRECTORY_ONLY
				))
			{
				throw e;
			}
		}

		/// <exception cref="System.Exception"/>
		public override void testRenameDirIntoExistingDir()
		{
			try
			{
				base.testRenameDirIntoExistingDir();
				NUnit.Framework.Assert.Fail("Expected a failure");
			}
			catch (System.IO.IOException e)
			{
				verifyUnsupportedDirRenameException(e);
			}
		}

		/// <exception cref="System.Exception"/>
		public override void testRenameFileNonexistentDir()
		{
			try
			{
				base.testRenameFileNonexistentDir();
				NUnit.Framework.Assert.Fail("Expected a failure");
			}
			catch (System.IO.IOException e)
			{
				verifyUnsupportedDirRenameException(e);
			}
		}
	}
}

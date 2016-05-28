using System.IO;
using Com.Google.Common.Collect;
using NUnit.Framework;
using NUnit.Framework.Rules;
using Org.Apache.Commons.IO;
using Org.Apache.Commons.Logging;
using Org.Apache.Hadoop.Hdfs;
using Org.Apache.Hadoop.Hdfs.Server.Namenode;
using Org.Apache.Hadoop.Test;
using Sharpen;

namespace Org.Apache.Hadoop.Hdfs.Tools.OfflineEditsViewer
{
	public class TestOfflineEditsViewer
	{
		private static readonly Log Log = LogFactory.GetLog(typeof(TestOfflineEditsViewer
			));

		private static readonly string buildDir = PathUtils.GetTestDirName(typeof(TestOfflineEditsViewer
			));

		private static readonly OfflineEditsViewerHelper nnHelper = new OfflineEditsViewerHelper
			();

		private static readonly ImmutableSet<FSEditLogOpCodes> skippedOps = SkippedOps();

		// to create edits and get edits filename
		private static ImmutableSet<FSEditLogOpCodes> SkippedOps()
		{
			ImmutableSet.Builder<FSEditLogOpCodes> b = ImmutableSet.Builder();
			// Deprecated opcodes
			((ImmutableSet.Builder<FSEditLogOpCodes>)((ImmutableSet.Builder<FSEditLogOpCodes>
				)((ImmutableSet.Builder<FSEditLogOpCodes>)((ImmutableSet.Builder<FSEditLogOpCodes
				>)b.Add(FSEditLogOpCodes.OpDatanodeAdd)).Add(FSEditLogOpCodes.OpDatanodeRemove))
				.Add(FSEditLogOpCodes.OpSetNsQuota)).Add(FSEditLogOpCodes.OpClearNsQuota)).Add(FSEditLogOpCodes
				.OpSetGenstampV1);
			// Cannot test delegation token related code in insecure set up
			((ImmutableSet.Builder<FSEditLogOpCodes>)((ImmutableSet.Builder<FSEditLogOpCodes>
				)b.Add(FSEditLogOpCodes.OpGetDelegationToken)).Add(FSEditLogOpCodes.OpRenewDelegationToken
				)).Add(FSEditLogOpCodes.OpCancelDelegationToken);
			// Skip invalid opcode
			b.Add(FSEditLogOpCodes.OpInvalid);
			return ((ImmutableSet<FSEditLogOpCodes>)b.Build());
		}

		[Rule]
		public readonly TemporaryFolder folder = new TemporaryFolder();

		/// <exception cref="System.IO.IOException"/>
		[NUnit.Framework.SetUp]
		public virtual void SetUp()
		{
			nnHelper.StartCluster(buildDir + "/dfs/");
		}

		/// <exception cref="System.IO.IOException"/>
		[NUnit.Framework.TearDown]
		public virtual void TearDown()
		{
			nnHelper.ShutdownCluster();
		}

		/// <summary>Test the OfflineEditsViewer</summary>
		/// <exception cref="System.IO.IOException"/>
		[NUnit.Framework.Test]
		public virtual void TestGenerated()
		{
			// edits generated by nnHelper (MiniDFSCluster), should have all op codes
			// binary, XML, reparsed binary
			string edits = nnHelper.GenerateEdits();
			Log.Info("Generated edits=" + edits);
			string editsParsedXml = folder.NewFile("editsParsed.xml").GetAbsolutePath();
			string editsReparsed = folder.NewFile("editsParsed").GetAbsolutePath();
			// parse to XML then back to binary
			NUnit.Framework.Assert.AreEqual(0, RunOev(edits, editsParsedXml, "xml", false));
			NUnit.Framework.Assert.AreEqual(0, RunOev(editsParsedXml, editsReparsed, "binary"
				, false));
			// judgment time
			NUnit.Framework.Assert.IsTrue("Edits " + edits + " should have all op codes", HasAllOpCodes
				(edits));
			Log.Info("Comparing generated file " + editsReparsed + " with reference file " + 
				edits);
			NUnit.Framework.Assert.IsTrue("Generated edits and reparsed (bin to XML to bin) should be same"
				, FilesEqualIgnoreTrailingZeros(edits, editsReparsed));
		}

		/// <exception cref="System.IO.IOException"/>
		[NUnit.Framework.Test]
		public virtual void TestRecoveryMode()
		{
			// edits generated by nnHelper (MiniDFSCluster), should have all op codes
			// binary, XML, reparsed binary
			string edits = nnHelper.GenerateEdits();
			FileOutputStream os = new FileOutputStream(edits, true);
			// Corrupt the file by truncating the end
			FileChannel editsFile = os.GetChannel();
			editsFile.Truncate(editsFile.Size() - 5);
			string editsParsedXml = folder.NewFile("editsRecoveredParsed.xml").GetAbsolutePath
				();
			string editsReparsed = folder.NewFile("editsRecoveredReparsed").GetAbsolutePath();
			string editsParsedXml2 = folder.NewFile("editsRecoveredParsed2.xml").GetAbsolutePath
				();
			// Can't read the corrupted file without recovery mode
			NUnit.Framework.Assert.AreEqual(-1, RunOev(edits, editsParsedXml, "xml", false));
			// parse to XML then back to binary
			NUnit.Framework.Assert.AreEqual(0, RunOev(edits, editsParsedXml, "xml", true));
			NUnit.Framework.Assert.AreEqual(0, RunOev(editsParsedXml, editsReparsed, "binary"
				, false));
			NUnit.Framework.Assert.AreEqual(0, RunOev(editsReparsed, editsParsedXml2, "xml", 
				false));
			// judgment time
			NUnit.Framework.Assert.IsTrue("Test round trip", FileUtils.ContentEqualsIgnoreEOL
				(new FilePath(editsParsedXml), new FilePath(editsParsedXml2), "UTF-8"));
			os.Close();
		}

		/// <exception cref="System.IO.IOException"/>
		[NUnit.Framework.Test]
		public virtual void TestStored()
		{
			// reference edits stored with source code (see build.xml)
			string cacheDir = Runtime.GetProperty("test.cache.data", "build/test/cache");
			// binary, XML, reparsed binary
			string editsStored = cacheDir + "/editsStored";
			string editsStoredParsedXml = cacheDir + "/editsStoredParsed.xml";
			string editsStoredReparsed = cacheDir + "/editsStoredReparsed";
			// reference XML version of editsStored (see build.xml)
			string editsStoredXml = cacheDir + "/editsStored.xml";
			// parse to XML then back to binary
			NUnit.Framework.Assert.AreEqual(0, RunOev(editsStored, editsStoredParsedXml, "xml"
				, false));
			NUnit.Framework.Assert.AreEqual(0, RunOev(editsStoredParsedXml, editsStoredReparsed
				, "binary", false));
			// judgement time
			NUnit.Framework.Assert.IsTrue("Edits " + editsStored + " should have all op codes"
				, HasAllOpCodes(editsStored));
			NUnit.Framework.Assert.IsTrue("Reference XML edits and parsed to XML should be same"
				, FileUtils.ContentEqualsIgnoreEOL(new FilePath(editsStoredXml), new FilePath(editsStoredParsedXml
				), "UTF-8"));
			NUnit.Framework.Assert.IsTrue("Reference edits and reparsed (bin to XML to bin) should be same"
				, FilesEqualIgnoreTrailingZeros(editsStored, editsStoredReparsed));
		}

		/// <summary>Run OfflineEditsViewer</summary>
		/// <param name="inFilename">input edits filename</param>
		/// <param name="outFilename">oputput edits filename</param>
		/// <exception cref="System.IO.IOException"/>
		private int RunOev(string inFilename, string outFilename, string processor, bool 
			recovery)
		{
			Log.Info("Running oev [" + inFilename + "] [" + outFilename + "]");
			Org.Apache.Hadoop.Hdfs.Tools.OfflineEditsViewer.OfflineEditsViewer oev = new Org.Apache.Hadoop.Hdfs.Tools.OfflineEditsViewer.OfflineEditsViewer
				();
			OfflineEditsViewer.Flags flags = new OfflineEditsViewer.Flags();
			flags.SetPrintToScreen();
			if (recovery)
			{
				flags.SetRecoveryMode();
			}
			return oev.Go(inFilename, outFilename, processor, flags, null);
		}

		/// <summary>Checks that the edits file has all opCodes</summary>
		/// <param name="filename">edits file</param>
		/// <returns>true is edits (filename) has all opCodes</returns>
		/// <exception cref="System.IO.IOException"/>
		private bool HasAllOpCodes(string inFilename)
		{
			string outFilename = inFilename + ".stats";
			FileOutputStream fout = new FileOutputStream(outFilename);
			StatisticsEditsVisitor visitor = new StatisticsEditsVisitor(fout);
			Org.Apache.Hadoop.Hdfs.Tools.OfflineEditsViewer.OfflineEditsViewer oev = new Org.Apache.Hadoop.Hdfs.Tools.OfflineEditsViewer.OfflineEditsViewer
				();
			if (oev.Go(inFilename, outFilename, "stats", new OfflineEditsViewer.Flags(), visitor
				) != 0)
			{
				return false;
			}
			Log.Info("Statistics for " + inFilename + "\n" + visitor.GetStatisticsString());
			bool hasAllOpCodes = true;
			foreach (FSEditLogOpCodes opCode in FSEditLogOpCodes.Values())
			{
				// don't need to test obsolete opCodes
				if (skippedOps.Contains(opCode))
				{
					continue;
				}
				long count = visitor.GetStatistics()[opCode];
				if ((count == null) || (count == 0))
				{
					hasAllOpCodes = false;
					Log.Info("Opcode " + opCode + " not tested in " + inFilename);
				}
			}
			return hasAllOpCodes;
		}

		/// <summary>
		/// Compare two files, ignore trailing zeros at the end, for edits log the
		/// trailing zeros do not make any difference, throw exception is the files are
		/// not same
		/// </summary>
		/// <param name="filenameSmall">first file to compare (doesn't have to be smaller)</param>
		/// <param name="filenameLarge">second file to compare (doesn't have to be larger)</param>
		/// <exception cref="System.IO.IOException"/>
		private bool FilesEqualIgnoreTrailingZeros(string filenameSmall, string filenameLarge
			)
		{
			ByteBuffer small = ByteBuffer.Wrap(DFSTestUtil.LoadFile(filenameSmall));
			ByteBuffer large = ByteBuffer.Wrap(DFSTestUtil.LoadFile(filenameLarge));
			// OEV outputs with the latest layout version, so tweak the old file's
			// contents to have latest version so checkedin binary files don't
			// require frequent updates
			small.Put(3, unchecked((byte)NameNodeLayoutVersion.CurrentLayoutVersion));
			// now correct if it's otherwise
			if (small.Capacity() > large.Capacity())
			{
				ByteBuffer tmpByteBuffer = small;
				small = large;
				large = tmpByteBuffer;
				string tmpFilename = filenameSmall;
				filenameSmall = filenameLarge;
				filenameLarge = tmpFilename;
			}
			// compare from 0 to capacity of small
			// the rest of the large should be all zeros
			small.Position(0);
			small.Limit(small.Capacity());
			large.Position(0);
			large.Limit(small.Capacity());
			// compares position to limit
			if (!small.Equals(large))
			{
				return false;
			}
			// everything after limit should be 0xFF
			int i = large.Limit();
			large.Clear();
			for (; i < large.Capacity(); i++)
			{
				if (large.Get(i) != FSEditLogOpCodes.OpInvalid.GetOpCode())
				{
					return false;
				}
			}
			return true;
		}
	}
}

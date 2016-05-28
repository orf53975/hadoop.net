using System;
using System.IO;
using NUnit.Framework;
using Org.Apache.Hadoop.Conf;
using Org.Apache.Hadoop.FS;
using Org.Apache.Hadoop.Hdfs;
using Org.Apache.Hadoop.Test;
using Org.Mortbay.Jetty.Webapp;
using Sharpen;

namespace Org.Apache.Hadoop.FS.Http.Server
{
	/// <summary>
	/// This test class ensures that everything works as expected when ACL
	/// support is turned off HDFS.
	/// </summary>
	/// <remarks>
	/// This test class ensures that everything works as expected when ACL
	/// support is turned off HDFS.  This is the default configuration.  The other
	/// tests operate with ACL support turned on.
	/// </remarks>
	public class TestHttpFSServerNoACLs : HTestCase
	{
		private MiniDFSCluster miniDfs;

		private Configuration nnConf;

		/// <summary>Fire up our own hand-rolled MiniDFSCluster.</summary>
		/// <remarks>
		/// Fire up our own hand-rolled MiniDFSCluster.  We do this here instead
		/// of relying on TestHdfsHelper because we don't want to turn on ACL
		/// support.
		/// </remarks>
		/// <exception cref="System.Exception"/>
		private void StartMiniDFS()
		{
			FilePath testDirRoot = TestDirHelper.GetTestDir();
			if (Runtime.GetProperty("hadoop.log.dir") == null)
			{
				Runtime.SetProperty("hadoop.log.dir", new FilePath(testDirRoot, "hadoop-log").GetAbsolutePath
					());
			}
			if (Runtime.GetProperty("test.build.data") == null)
			{
				Runtime.SetProperty("test.build.data", new FilePath(testDirRoot, "hadoop-data").GetAbsolutePath
					());
			}
			Configuration conf = HadoopUsersConfTestHelper.GetBaseConf();
			HadoopUsersConfTestHelper.AddUserConf(conf);
			conf.Set("fs.hdfs.impl.disable.cache", "true");
			conf.Set("dfs.block.access.token.enable", "false");
			conf.Set("dfs.permissions", "true");
			conf.Set("hadoop.security.authentication", "simple");
			// Explicitly turn off ACL support
			conf.SetBoolean(DFSConfigKeys.DfsNamenodeAclsEnabledKey, false);
			MiniDFSCluster.Builder builder = new MiniDFSCluster.Builder(conf);
			builder.NumDataNodes(2);
			miniDfs = builder.Build();
			nnConf = miniDfs.GetConfiguration(0);
		}

		/// <summary>Create an HttpFS Server to talk to the MiniDFSCluster we created.</summary>
		/// <exception cref="System.Exception"/>
		private void CreateHttpFSServer()
		{
			FilePath homeDir = TestDirHelper.GetTestDir();
			NUnit.Framework.Assert.IsTrue(new FilePath(homeDir, "conf").Mkdir());
			NUnit.Framework.Assert.IsTrue(new FilePath(homeDir, "log").Mkdir());
			NUnit.Framework.Assert.IsTrue(new FilePath(homeDir, "temp").Mkdir());
			HttpFSServerWebApp.SetHomeDirForCurrentThread(homeDir.GetAbsolutePath());
			FilePath secretFile = new FilePath(new FilePath(homeDir, "conf"), "secret");
			TextWriter w = new FileWriter(secretFile);
			w.Write("secret");
			w.Close();
			// HDFS configuration
			FilePath hadoopConfDir = new FilePath(new FilePath(homeDir, "conf"), "hadoop-conf"
				);
			if (!hadoopConfDir.Mkdirs())
			{
				throw new IOException();
			}
			string fsDefaultName = nnConf.Get(CommonConfigurationKeysPublic.FsDefaultNameKey);
			Configuration conf = new Configuration(false);
			conf.Set(CommonConfigurationKeysPublic.FsDefaultNameKey, fsDefaultName);
			// Explicitly turn off ACLs, just in case the default becomes true later
			conf.SetBoolean(DFSConfigKeys.DfsNamenodeAclsEnabledKey, false);
			FilePath hdfsSite = new FilePath(hadoopConfDir, "hdfs-site.xml");
			OutputStream os = new FileOutputStream(hdfsSite);
			conf.WriteXml(os);
			os.Close();
			// HTTPFS configuration
			conf = new Configuration(false);
			conf.Set("httpfs.hadoop.config.dir", hadoopConfDir.ToString());
			conf.Set("httpfs.proxyuser." + HadoopUsersConfTestHelper.GetHadoopProxyUser() + ".groups"
				, HadoopUsersConfTestHelper.GetHadoopProxyUserGroups());
			conf.Set("httpfs.proxyuser." + HadoopUsersConfTestHelper.GetHadoopProxyUser() + ".hosts"
				, HadoopUsersConfTestHelper.GetHadoopProxyUserHosts());
			conf.Set("httpfs.authentication.signature.secret.file", secretFile.GetAbsolutePath
				());
			FilePath httpfsSite = new FilePath(new FilePath(homeDir, "conf"), "httpfs-site.xml"
				);
			os = new FileOutputStream(httpfsSite);
			conf.WriteXml(os);
			os.Close();
			ClassLoader cl = Sharpen.Thread.CurrentThread().GetContextClassLoader();
			Uri url = cl.GetResource("webapp");
			if (url == null)
			{
				throw new IOException();
			}
			WebAppContext context = new WebAppContext(url.AbsolutePath, "/webhdfs");
			Org.Mortbay.Jetty.Server server = TestJettyHelper.GetJettyServer();
			server.AddHandler(context);
			server.Start();
		}

		/// <summary>
		/// Talks to the http interface to get the json output of a *STATUS command
		/// on the given file.
		/// </summary>
		/// <param name="filename">The file to query.</param>
		/// <param name="command">Either GETFILESTATUS, LISTSTATUS, or ACLSTATUS</param>
		/// <param name="expectOK">Is this operation expected to succeed?</param>
		/// <exception cref="System.Exception"/>
		private void GetStatus(string filename, string command, bool expectOK)
		{
			string user = HadoopUsersConfTestHelper.GetHadoopUsers()[0];
			// Remove leading / from filename
			if (filename[0] == '/')
			{
				filename = Sharpen.Runtime.Substring(filename, 1);
			}
			string pathOps = MessageFormat.Format("/webhdfs/v1/{0}?user.name={1}&op={2}", filename
				, user, command);
			Uri url = new Uri(TestJettyHelper.GetJettyURL(), pathOps);
			HttpURLConnection conn = (HttpURLConnection)url.OpenConnection();
			conn.Connect();
			int resp = conn.GetResponseCode();
			BufferedReader reader;
			if (expectOK)
			{
				NUnit.Framework.Assert.AreEqual(HttpURLConnection.HttpOk, resp);
				reader = new BufferedReader(new InputStreamReader(conn.GetInputStream()));
				string res = reader.ReadLine();
				NUnit.Framework.Assert.IsTrue(!res.Contains("aclBit"));
				NUnit.Framework.Assert.IsTrue(res.Contains("owner"));
			}
			else
			{
				// basic sanity check
				NUnit.Framework.Assert.AreEqual(HttpURLConnection.HttpInternalError, resp);
				reader = new BufferedReader(new InputStreamReader(conn.GetErrorStream()));
				string res = reader.ReadLine();
				NUnit.Framework.Assert.IsTrue(res.Contains("AclException"));
				NUnit.Framework.Assert.IsTrue(res.Contains("Support for ACLs has been disabled"));
			}
		}

		/// <summary>General-purpose http PUT command to the httpfs server.</summary>
		/// <param name="filename">The file to operate upon</param>
		/// <param name="command">The command to perform (SETACL, etc)</param>
		/// <param name="params">Parameters, like "aclspec=..."</param>
		/// <exception cref="System.Exception"/>
		private void PutCmd(string filename, string command, string @params, bool expectOK
			)
		{
			string user = HadoopUsersConfTestHelper.GetHadoopUsers()[0];
			// Remove leading / from filename
			if (filename[0] == '/')
			{
				filename = Sharpen.Runtime.Substring(filename, 1);
			}
			string pathOps = MessageFormat.Format("/webhdfs/v1/{0}?user.name={1}{2}{3}&op={4}"
				, filename, user, (@params == null) ? string.Empty : "&", (@params == null) ? string.Empty
				 : @params, command);
			Uri url = new Uri(TestJettyHelper.GetJettyURL(), pathOps);
			HttpURLConnection conn = (HttpURLConnection)url.OpenConnection();
			conn.SetRequestMethod("PUT");
			conn.Connect();
			int resp = conn.GetResponseCode();
			if (expectOK)
			{
				NUnit.Framework.Assert.AreEqual(HttpURLConnection.HttpOk, resp);
			}
			else
			{
				NUnit.Framework.Assert.AreEqual(HttpURLConnection.HttpInternalError, resp);
				BufferedReader reader;
				reader = new BufferedReader(new InputStreamReader(conn.GetErrorStream()));
				string err = reader.ReadLine();
				NUnit.Framework.Assert.IsTrue(err.Contains("AclException"));
				NUnit.Framework.Assert.IsTrue(err.Contains("Support for ACLs has been disabled"));
			}
		}

		/// <summary>
		/// Ensure that
		/// <ol>
		/// <li>GETFILESTATUS and LISTSTATUS work happily</li>
		/// <li>ACLSTATUS throws an exception</li>
		/// <li>The ACL SET, REMOVE, etc calls all fail</li>
		/// </ol>
		/// </summary>
		/// <exception cref="System.Exception"/>
		[NUnit.Framework.Test]
		[TestDir]
		[TestJetty]
		public virtual void TestWithNoAcls()
		{
			string aclUser1 = "user:foo:rw-";
			string aclUser2 = "user:bar:r--";
			string aclGroup1 = "group::r--";
			string aclSpec = "aclspec=user::rwx," + aclUser1 + "," + aclGroup1 + ",other::---";
			string modAclSpec = "aclspec=" + aclUser2;
			string remAclSpec = "aclspec=" + aclUser1;
			string defUser1 = "default:user:glarch:r-x";
			string defSpec1 = "aclspec=" + defUser1;
			string dir = "/noACLs";
			string path = dir + "/foo";
			StartMiniDFS();
			CreateHttpFSServer();
			FileSystem fs = FileSystem.Get(nnConf);
			fs.Mkdirs(new Path(dir));
			OutputStream os = fs.Create(new Path(path));
			os.Write(1);
			os.Close();
			/* The normal status calls work as expected; GETACLSTATUS fails */
			GetStatus(path, "GETFILESTATUS", true);
			GetStatus(dir, "LISTSTATUS", true);
			GetStatus(path, "GETACLSTATUS", false);
			/* All the ACL-based PUT commands fail with ACL exceptions */
			PutCmd(path, "SETACL", aclSpec, false);
			PutCmd(path, "MODIFYACLENTRIES", modAclSpec, false);
			PutCmd(path, "REMOVEACLENTRIES", remAclSpec, false);
			PutCmd(path, "REMOVEACL", null, false);
			PutCmd(dir, "SETACL", defSpec1, false);
			PutCmd(dir, "REMOVEDEFAULTACL", null, false);
			miniDfs.Shutdown();
		}
	}
}

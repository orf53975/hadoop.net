using System.Collections.Generic;
using System.IO;
using Org.Apache.Commons.Logging;
using Org.Apache.Hadoop.Classification;
using Org.Apache.Hadoop.Conf;
using Org.Apache.Hadoop.Security.Token;
using Org.Apache.Hadoop.Yarn.Api.Records;
using Org.Apache.Hadoop.Yarn.Security;
using Org.Apache.Hadoop.Yarn.Server.Api.Records;
using Org.Apache.Hadoop.Yarn.Server.Nodemanager.Recovery;
using Org.Apache.Hadoop.Yarn.Server.Security;
using Sharpen;

namespace Org.Apache.Hadoop.Yarn.Server.Nodemanager.Security
{
	/// <summary>The NM maintains only two master-keys.</summary>
	/// <remarks>
	/// The NM maintains only two master-keys. The current key that RM knows and the
	/// key from the previous rolling-interval.
	/// </remarks>
	public class NMContainerTokenSecretManager : BaseContainerTokenSecretManager
	{
		private static readonly Log Log = LogFactory.GetLog(typeof(Org.Apache.Hadoop.Yarn.Server.Nodemanager.Security.NMContainerTokenSecretManager
			));

		private MasterKeyData previousMasterKey;

		private readonly SortedDictionary<long, IList<ContainerId>> recentlyStartedContainerTracker;

		private readonly NMStateStoreService stateStore;

		private string nodeHostAddr;

		public NMContainerTokenSecretManager(Configuration conf)
			: this(conf, new NMNullStateStoreService())
		{
		}

		public NMContainerTokenSecretManager(Configuration conf, NMStateStoreService stateStore
			)
			: base(conf)
		{
			recentlyStartedContainerTracker = new SortedDictionary<long, IList<ContainerId>>(
				);
			this.stateStore = stateStore;
		}

		/// <exception cref="System.IO.IOException"/>
		public virtual void Recover()
		{
			lock (this)
			{
				NMStateStoreService.RecoveredContainerTokensState state = stateStore.LoadContainerTokensState
					();
				MasterKey key = state.GetCurrentMasterKey();
				if (key != null)
				{
					base.currentMasterKey = new MasterKeyData(key, CreateSecretKey(((byte[])key.GetBytes
						().Array())));
				}
				key = state.GetPreviousMasterKey();
				if (key != null)
				{
					previousMasterKey = new MasterKeyData(key, CreateSecretKey(((byte[])key.GetBytes(
						).Array())));
				}
				// restore the serial number from the current master key
				if (base.currentMasterKey != null)
				{
					base.serialNo = base.currentMasterKey.GetMasterKey().GetKeyId() + 1;
				}
				foreach (KeyValuePair<ContainerId, long> entry in state.GetActiveTokens())
				{
					ContainerId containerId = entry.Key;
					long expTime = entry.Value;
					IList<ContainerId> containerList = recentlyStartedContainerTracker[expTime];
					if (containerList == null)
					{
						containerList = new AList<ContainerId>();
						recentlyStartedContainerTracker[expTime] = containerList;
					}
					if (!containerList.Contains(containerId))
					{
						containerList.AddItem(containerId);
					}
				}
			}
		}

		private void UpdateCurrentMasterKey(MasterKeyData key)
		{
			base.currentMasterKey = key;
			try
			{
				stateStore.StoreContainerTokenCurrentMasterKey(key.GetMasterKey());
			}
			catch (IOException e)
			{
				Log.Error("Unable to update current master key in state store", e);
			}
		}

		private void UpdatePreviousMasterKey(MasterKeyData key)
		{
			previousMasterKey = key;
			try
			{
				stateStore.StoreContainerTokenPreviousMasterKey(key.GetMasterKey());
			}
			catch (IOException e)
			{
				Log.Error("Unable to update previous master key in state store", e);
			}
		}

		/// <summary>
		/// Used by NodeManagers to create a token-secret-manager with the key obtained
		/// from the RM.
		/// </summary>
		/// <remarks>
		/// Used by NodeManagers to create a token-secret-manager with the key obtained
		/// from the RM. This can happen during registration or when the RM rolls the
		/// master-key and signals the NM.
		/// </remarks>
		/// <param name="masterKeyRecord"/>
		[InterfaceAudience.Private]
		public virtual void SetMasterKey(MasterKey masterKeyRecord)
		{
			lock (this)
			{
				// Update keys only if the key has changed.
				if (base.currentMasterKey == null || base.currentMasterKey.GetMasterKey().GetKeyId
					() != masterKeyRecord.GetKeyId())
				{
					Log.Info("Rolling master-key for container-tokens, got key with id " + masterKeyRecord
						.GetKeyId());
					if (base.currentMasterKey != null)
					{
						UpdatePreviousMasterKey(base.currentMasterKey);
					}
					UpdateCurrentMasterKey(new MasterKeyData(masterKeyRecord, CreateSecretKey(((byte[]
						)masterKeyRecord.GetBytes().Array()))));
				}
			}
		}

		/// <summary>
		/// Override of this is to validate ContainerTokens generated by using
		/// different
		/// <see cref="Org.Apache.Hadoop.Yarn.Server.Api.Records.MasterKey"/>
		/// s.
		/// </summary>
		/// <exception cref="Org.Apache.Hadoop.Security.Token.SecretManager.InvalidToken"/>
		public override byte[] RetrievePassword(ContainerTokenIdentifier identifier)
		{
			lock (this)
			{
				int keyId = identifier.GetMasterKeyId();
				MasterKeyData masterKeyToUse = null;
				if (this.previousMasterKey != null && keyId == this.previousMasterKey.GetMasterKey
					().GetKeyId())
				{
					// A container-launch has come in with a token generated off the last
					// master-key
					masterKeyToUse = this.previousMasterKey;
				}
				else
				{
					if (keyId == base.currentMasterKey.GetMasterKey().GetKeyId())
					{
						// A container-launch has come in with a token generated off the current
						// master-key
						masterKeyToUse = base.currentMasterKey;
					}
				}
				if (nodeHostAddr != null && !identifier.GetNmHostAddress().Equals(nodeHostAddr))
				{
					// Valid container token used for incorrect node.
					throw new SecretManager.InvalidToken("Given Container " + identifier.GetContainerID
						().ToString() + " identifier is not valid for current Node manager. Expected : "
						 + nodeHostAddr + " Found : " + identifier.GetNmHostAddress());
				}
				if (masterKeyToUse != null)
				{
					return RetrievePasswordInternal(identifier, masterKeyToUse);
				}
				// Invalid request. Like startContainer() with token generated off
				// old-master-keys.
				throw new SecretManager.InvalidToken("Given Container " + identifier.GetContainerID
					().ToString() + " seems to have an illegally generated token.");
			}
		}

		/// <summary>Container start has gone through.</summary>
		/// <remarks>
		/// Container start has gone through. We need to store the containerId in order
		/// to block future container start requests with same container token. This
		/// container token needs to be saved till its container token expires.
		/// </remarks>
		public virtual void StartContainerSuccessful(ContainerTokenIdentifier tokenId)
		{
			lock (this)
			{
				RemoveAnyContainerTokenIfExpired();
				ContainerId containerId = tokenId.GetContainerID();
				long expTime = tokenId.GetExpiryTimeStamp();
				// We might have multiple containers with same expiration time.
				if (!recentlyStartedContainerTracker.Contains(expTime))
				{
					recentlyStartedContainerTracker[expTime] = new AList<ContainerId>();
				}
				recentlyStartedContainerTracker[expTime].AddItem(containerId);
				try
				{
					stateStore.StoreContainerToken(containerId, expTime);
				}
				catch (IOException e)
				{
					Log.Error("Unable to store token for container " + containerId, e);
				}
			}
		}

		protected internal virtual void RemoveAnyContainerTokenIfExpired()
		{
			lock (this)
			{
				// Trying to remove any container if its container token has expired.
				IEnumerator<KeyValuePair<long, IList<ContainerId>>> containersI = this.recentlyStartedContainerTracker
					.GetEnumerator();
				long currTime = Runtime.CurrentTimeMillis();
				while (containersI.HasNext())
				{
					KeyValuePair<long, IList<ContainerId>> containerEntry = containersI.Next();
					if (containerEntry.Key < currTime)
					{
						foreach (ContainerId container in containerEntry.Value)
						{
							try
							{
								stateStore.RemoveContainerToken(container);
							}
							catch (IOException e)
							{
								Log.Error("Unable to remove token for container " + container, e);
							}
						}
						containersI.Remove();
					}
					else
					{
						break;
					}
				}
			}
		}

		/// <summary>
		/// Container will be remembered based on expiration time of the container
		/// token used for starting the container.
		/// </summary>
		/// <remarks>
		/// Container will be remembered based on expiration time of the container
		/// token used for starting the container. It is safe to use expiration time
		/// as there is one to many mapping between expiration time and containerId.
		/// </remarks>
		/// <returns>true if the current token identifier is not present in cache.</returns>
		public virtual bool IsValidStartContainerRequest(ContainerTokenIdentifier containerTokenIdentifier
			)
		{
			lock (this)
			{
				RemoveAnyContainerTokenIfExpired();
				long expTime = containerTokenIdentifier.GetExpiryTimeStamp();
				IList<ContainerId> containers = this.recentlyStartedContainerTracker[expTime];
				if (containers == null || !containers.Contains(containerTokenIdentifier.GetContainerID
					()))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public virtual void SetNodeId(NodeId nodeId)
		{
			lock (this)
			{
				nodeHostAddr = nodeId.ToString();
				Log.Info("Updating node address : " + nodeHostAddr);
			}
		}
	}
}

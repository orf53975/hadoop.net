using Com.Google.Protobuf;
using Org.Apache.Hadoop.Security.Proto;
using Org.Apache.Hadoop.Yarn.Api.Protocolrecords;
using Org.Apache.Hadoop.Yarn.Api.Records;
using Org.Apache.Hadoop.Yarn.Api.Records.Impl.PB;
using Sharpen;

namespace Org.Apache.Hadoop.Yarn.Api.Protocolrecords.Impl.PB
{
	public class RenewDelegationTokenRequestPBImpl : RenewDelegationTokenRequest
	{
		internal SecurityProtos.RenewDelegationTokenRequestProto proto = SecurityProtos.RenewDelegationTokenRequestProto
			.GetDefaultInstance();

		internal SecurityProtos.RenewDelegationTokenRequestProto.Builder builder = null;

		internal bool viaProto = false;

		public RenewDelegationTokenRequestPBImpl()
		{
			builder = SecurityProtos.RenewDelegationTokenRequestProto.NewBuilder();
		}

		public RenewDelegationTokenRequestPBImpl(SecurityProtos.RenewDelegationTokenRequestProto
			 proto)
		{
			this.proto = proto;
			this.viaProto = true;
		}

		internal Token token;

		public override Token GetDelegationToken()
		{
			SecurityProtos.RenewDelegationTokenRequestProtoOrBuilder p = viaProto ? proto : builder;
			if (this.token != null)
			{
				return this.token;
			}
			this.token = ConvertFromProtoFormat(p.GetToken());
			return this.token;
		}

		public override void SetDelegationToken(Token token)
		{
			MaybeInitBuilder();
			if (token == null)
			{
				builder.ClearToken();
			}
			this.token = token;
		}

		public virtual SecurityProtos.RenewDelegationTokenRequestProto GetProto()
		{
			MergeLocalToProto();
			proto = viaProto ? proto : ((SecurityProtos.RenewDelegationTokenRequestProto)builder
				.Build());
			viaProto = true;
			return proto;
		}

		public override int GetHashCode()
		{
			return GetProto().GetHashCode();
		}

		public override bool Equals(object other)
		{
			if (other == null)
			{
				return false;
			}
			if (other.GetType().IsAssignableFrom(this.GetType()))
			{
				return this.GetProto().Equals(this.GetType().Cast(other).GetProto());
			}
			return false;
		}

		public override string ToString()
		{
			return TextFormat.ShortDebugString(GetProto());
		}

		private void MergeLocalToBuilder()
		{
			if (token != null)
			{
				builder.SetToken(ConvertToProtoFormat(this.token));
			}
		}

		private void MergeLocalToProto()
		{
			if (viaProto)
			{
				MaybeInitBuilder();
			}
			MergeLocalToBuilder();
			proto = ((SecurityProtos.RenewDelegationTokenRequestProto)builder.Build());
			viaProto = true;
		}

		private void MaybeInitBuilder()
		{
			if (viaProto || builder == null)
			{
				builder = SecurityProtos.RenewDelegationTokenRequestProto.NewBuilder(proto);
			}
			viaProto = false;
		}

		private TokenPBImpl ConvertFromProtoFormat(SecurityProtos.TokenProto p)
		{
			return new TokenPBImpl(p);
		}

		private SecurityProtos.TokenProto ConvertToProtoFormat(Token t)
		{
			return ((TokenPBImpl)t).GetProto();
		}
	}
}

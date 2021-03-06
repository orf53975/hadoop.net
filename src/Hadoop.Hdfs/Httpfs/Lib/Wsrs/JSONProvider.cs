using System;
using System.IO;
using Javax.WS.RS.Core;
using Javax.WS.RS.Ext;
using Org.Apache.Commons.IO;
using Org.Json.Simple;
using Sharpen;
using Sharpen.Reflect;

namespace Org.Apache.Hadoop.Lib.Wsrs
{
	public class JSONProvider : MessageBodyWriter<JSONStreamAware>
	{
		private static readonly string Enter = Runtime.GetProperty("line.separator");

		public virtual bool IsWriteable(Type aClass, Type type, Sharpen.Annotation.Annotation
			[] annotations, MediaType mediaType)
		{
			return typeof(JSONStreamAware).IsAssignableFrom(aClass);
		}

		public virtual long GetSize(JSONStreamAware jsonStreamAware, Type aClass, Type type
			, Sharpen.Annotation.Annotation[] annotations, MediaType mediaType)
		{
			return -1;
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="Javax.WS.RS.WebApplicationException"/>
		public virtual void WriteTo(JSONStreamAware jsonStreamAware, Type aClass, Type type
			, Sharpen.Annotation.Annotation[] annotations, MediaType mediaType, MultivaluedMap
			<string, object> stringObjectMultivaluedMap, OutputStream outputStream)
		{
			TextWriter writer = new OutputStreamWriter(outputStream, Charsets.Utf8);
			jsonStreamAware.WriteJSONString(writer);
			writer.Write(Enter);
			writer.Flush();
		}
	}
}

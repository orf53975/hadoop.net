using Sharpen;

namespace org.apache.hadoop.metrics2
{
	/// <summary>The metrics collector interface</summary>
	public interface MetricsCollector
	{
		/// <summary>Add a metrics record</summary>
		/// <param name="name">of the record</param>
		/// <returns>a metrics record builder for the record</returns>
		org.apache.hadoop.metrics2.MetricsRecordBuilder addRecord(string name);

		/// <summary>Add a metrics record</summary>
		/// <param name="info">of the record</param>
		/// <returns>a metrics record builder for the record</returns>
		org.apache.hadoop.metrics2.MetricsRecordBuilder addRecord(org.apache.hadoop.metrics2.MetricsInfo
			 info);
	}
}

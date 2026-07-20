using System;
using System.Text;

[Serializable]
public class VolumeCloudsWeatherLayerConfig
{
	public VolumeCloudsNoiseLayerConfig Instability = new VolumeCloudsNoiseLayerConfig();

	public VolumeCloudsNoiseLayerConfig CoverageBase = new VolumeCloudsNoiseLayerConfig();

	public VolumeCloudsNoiseLayerConfig CoverageDetailPerlin = new VolumeCloudsNoiseLayerConfig();

	public VolumeCloudsNoiseLayerConfig CoverageDetailWorley = new VolumeCloudsNoiseLayerConfig();

	public VolumeCloudsCurlNoiseConfig Curl = new VolumeCloudsCurlNoiseConfig();

	public void CopyFrom(VolumeCloudsWeatherLayerConfig copy)
	{
		Instability.CopyFrom(copy.Instability);
		CoverageBase.CopyFrom(copy.CoverageBase);
		CoverageDetailPerlin.CopyFrom(copy.CoverageDetailPerlin);
		CoverageDetailWorley.CopyFrom(copy.CoverageDetailWorley);
		Curl.CopyFrom(copy.Curl);
	}

	public void Output(StringBuilder sb, VolumeClouds.NoiseOffsets ofs)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		sb.AppendLine("Cov base:");
		CoverageBase.Output(sb, ofs.CoverageBase);
		sb.AppendLine("Cov dp:");
		CoverageDetailPerlin.Output(sb, ofs.CoverageDetailPerlin);
		sb.AppendLine("Cov dw:");
		CoverageDetailWorley.Output(sb, ofs.CoverageDetailWorley);
	}
}

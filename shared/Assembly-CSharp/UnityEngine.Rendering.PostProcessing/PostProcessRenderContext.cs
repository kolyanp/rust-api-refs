using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityEngine.Rendering.PostProcessing;

public class PostProcessRenderContext
{
	public enum StereoRenderingMode
	{
		MultiPass,
		SinglePass,
		SinglePassInstanced,
		SinglePassMultiview
	}

	public bool dlssEnabled;

	private Camera m_Camera;

	internal PropertySheet uberSheet;

	internal Texture autoExposureTexture;

	internal LogHistogram logHistogram;

	internal Texture logLut;

	internal AutoExposure autoExposure;

	internal int bloomBufferNameID;

	internal bool physicalCamera;

	private RenderTextureDescriptor m_sourceDescriptor;

	public Camera camera
	{
		get
		{
			return m_Camera;
		}
		set
		{
			m_Camera = value;
			if (!m_Camera.stereoEnabled)
			{
				width = m_Camera.pixelWidth;
				height = m_Camera.pixelHeight;
				m_sourceDescriptor.width = width;
				m_sourceDescriptor.height = height;
				screenWidth = width;
				screenHeight = height;
				stereoActive = false;
				numberOfEyes = 1;
			}
		}
	}

	public CommandBuffer command { get; set; }

	public RenderTargetIdentifier source
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public RenderTargetIdentifier destination
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public RenderTextureFormat sourceFormat
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public bool flip { get; set; }

	public PostProcessResources resources { get; internal set; }

	public PropertySheetFactory propertySheets { get; internal set; }

	public Dictionary<string, object> userData { get; private set; }

	public PostProcessDebugLayer debugLayer { get; internal set; }

	public int width { get; set; }

	public int height { get; set; }

	public bool stereoActive { get; private set; }

	public int xrActiveEye { get; private set; }

	public int numberOfEyes { get; private set; }

	public StereoRenderingMode stereoRenderingMode { get; private set; }

	public int screenWidth { get; set; }

	public int screenHeight { get; set; }

	public bool isSceneView { get; internal set; }

	public PostProcessLayer.Antialiasing antialiasing { get; internal set; }

	public TemporalAntialiasing temporalAntialiasing { get; internal set; }

	public void Resize(int width, int height, bool dlssEnabled)
	{
		int num = (screenWidth = width);
		this.width = num;
		num = (screenHeight = height);
		this.height = num;
		this.dlssEnabled = dlssEnabled;
		m_sourceDescriptor.width = width;
		m_sourceDescriptor.height = height;
	}

	public void Reset()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		m_Camera = null;
		width = 0;
		height = 0;
		dlssEnabled = false;
		m_sourceDescriptor = new RenderTextureDescriptor(0, 0);
		physicalCamera = false;
		stereoActive = false;
		xrActiveEye = 0;
		screenWidth = 0;
		screenHeight = 0;
		command = null;
		source = RenderTargetIdentifier.op_Implicit(0);
		destination = RenderTargetIdentifier.op_Implicit(0);
		sourceFormat = (RenderTextureFormat)0;
		flip = false;
		resources = null;
		propertySheets = null;
		debugLayer = null;
		isSceneView = false;
		antialiasing = PostProcessLayer.Antialiasing.None;
		temporalAntialiasing = null;
		uberSheet = null;
		autoExposureTexture = null;
		logLut = null;
		autoExposure = null;
		bloomBufferNameID = -1;
		if (userData == null)
		{
			userData = new Dictionary<string, object>();
		}
		userData.Clear();
	}

	public bool IsTemporalAntialiasingActive()
	{
		return false;
	}

	public bool IsDebugOverlayEnabled(DebugOverlay overlay)
	{
		return debugLayer.debugOverlay == overlay;
	}

	public void PushDebugOverlay(CommandBuffer cmd, RenderTargetIdentifier source, PropertySheet sheet, int pass)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		debugLayer.PushDebugOverlay(cmd, source, sheet, pass);
	}

	private RenderTextureDescriptor GetDescriptor(int depthBufferBits = 0, RenderTextureFormat colorFormat = (RenderTextureFormat)7, RenderTextureReadWrite readWrite = (RenderTextureReadWrite)0)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Invalid comparison between Unknown and I4
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Invalid comparison between Unknown and I4
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Invalid comparison between Unknown and I4
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Invalid comparison between Unknown and I4
		RenderTextureDescriptor result = new RenderTextureDescriptor(m_sourceDescriptor.width, m_sourceDescriptor.height, m_sourceDescriptor.colorFormat, depthBufferBits);
		result.dimension = m_sourceDescriptor.dimension;
		result.volumeDepth = m_sourceDescriptor.volumeDepth;
		result.vrUsage = m_sourceDescriptor.vrUsage;
		result.msaaSamples = m_sourceDescriptor.msaaSamples;
		result.memoryless = m_sourceDescriptor.memoryless;
		result.useMipMap = m_sourceDescriptor.useMipMap;
		result.autoGenerateMips = m_sourceDescriptor.autoGenerateMips;
		result.enableRandomWrite = m_sourceDescriptor.enableRandomWrite;
		result.shadowSamplingMode = m_sourceDescriptor.shadowSamplingMode;
		if ((int)colorFormat != 7)
		{
			result.colorFormat = colorFormat;
		}
		if ((int)readWrite == 2)
		{
			result.sRGB = true;
		}
		else if ((int)readWrite == 1)
		{
			result.sRGB = false;
		}
		else if ((int)readWrite == 0)
		{
			result.sRGB = (int)QualitySettings.activeColorSpace > 0;
		}
		return result;
	}

	public void GetScreenSpaceTemporaryRT(CommandBuffer cmd, int nameID, int depthBufferBits = 0, RenderTextureFormat colorFormat = (RenderTextureFormat)7, RenderTextureReadWrite readWrite = (RenderTextureReadWrite)0, FilterMode filter = (FilterMode)1, int widthOverride = 0, int heightOverride = 0)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		RenderTextureDescriptor descriptor = GetDescriptor(depthBufferBits, colorFormat, readWrite);
		if (widthOverride > 0)
		{
			descriptor.width = widthOverride;
		}
		if (heightOverride > 0)
		{
			descriptor.height = heightOverride;
		}
		if (stereoActive && (int)descriptor.dimension == 5)
		{
			descriptor.dimension = (TextureDimension)2;
		}
		cmd.GetTemporaryRT(nameID, descriptor, filter);
	}

	public RenderTexture GetScreenSpaceTemporaryRT(int depthBufferBits = 0, RenderTextureFormat colorFormat = (RenderTextureFormat)7, RenderTextureReadWrite readWrite = (RenderTextureReadWrite)0, int widthOverride = 0, int heightOverride = 0)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		RenderTextureDescriptor descriptor = GetDescriptor(depthBufferBits, colorFormat, readWrite);
		if (widthOverride > 0)
		{
			descriptor.width = widthOverride;
		}
		if (heightOverride > 0)
		{
			descriptor.height = heightOverride;
		}
		return RenderTexture.GetTemporary(descriptor);
	}
}

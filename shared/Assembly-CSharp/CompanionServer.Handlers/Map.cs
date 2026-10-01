using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConVar;
using Facepunch;
using ProtoBuf;
using UnityEngine;

namespace CompanionServer.Handlers;

public class Map : BasePlayerHandler<AppEmpty>
{
	private static int _width;

	private static int _height;

	private static byte[] _imageData;

	private static string _background;

	public static byte[] ImageData => _imageData;

	protected override double TokenCost => 5.0;

	public override ValueTask Execute()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		if (_imageData == null || !ConVar.Server.mapenabled || ConVar.Server.fogofwar)
		{
			SendError("no_map");
			return default;
		}
		AppMap val = Pool.Get<AppMap>();
		val.width = (uint)_width;
		val.height = (uint)_height;
		val.oceanMargin = 500;
		val.jpgImage = _imageData;
		val.background = _background;
		val.monuments = Pool.Get<List<Monument>>();
		if ((Object)(object)TerrainMeta.Path != (Object)null && TerrainMeta.Path.Landmarks != null)
		{
			foreach (LandmarkInfo landmark in TerrainMeta.Path.Landmarks)
			{
				if (landmark.shouldDisplayOnMap)
				{
					Vector2 val2 = Util.WorldToMap(((Component)landmark).transform.position);
					Monument val3 = Pool.Get<Monument>();
					val3.x = val2.x;
					val3.y = val2.y;
					if (landmark.displayPhrase.IsValid())
					{
						val3.token = landmark.displayPhrase.token;
						val3.isCustomName = false;
					}
					else
					{
						val3.token = landmark.GetFallbackName();
						val3.isCustomName = true;
					}
					val.monuments.Add(val3);
				}
			}
		}
		AppResponse val4 = Pool.Get<AppResponse>();
		val4.map = val;
		Send(val4);
		return default;
	}

	public static void PopulateCache()
	{
		RenderToCache();
	}

	private static void RenderToCache()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_imageData = null;
		_width = 0;
		_height = 0;
		try
		{
			_imageData = MapImageRenderer.Render(out _width, out _height, out var background);
			_background = "#" + ColorUtility.ToHtmlStringRGB(background);
		}
		catch (Exception arg)
		{
			Debug.LogError((object)$"Exception thrown when rendering map for the app: {arg}");
		}
		if (_imageData == null)
		{
			Debug.LogError((object)"Map image is null! App users will not be able to see the map.");
		}
	}
}

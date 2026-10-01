using System.Threading.Tasks;
using CompanionServer.Cameras;
using Facepunch;
using ProtoBuf;
using UnityEngine;

namespace CompanionServer.Handlers;

public class CameraSubscribe : BasePlayerHandler<AppCameraSubscribe>
{
	public override ValueTask Execute()
	{
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		if (!CameraRenderer.enabled)
		{
			SendError("not_enabled");
			return default;
		}
		CameraRendererManager instance = SingletonComponent<CameraRendererManager>.Instance;
		if ((Object)(object)instance == (Object)null)
		{
			SendError("server_error");
			return default;
		}
		if (string.IsNullOrEmpty(Proto.cameraId))
		{
			Client.EndViewing();
			SendError("invalid_id");
			return default;
		}
		bool flag = CameraRenderer.developerPermissions && DeveloperList.Contains(UserId);
		if (!Player.IsValid())
		{
			Client.EndViewing();
			SendError("no_player");
			return default;
		}
		if (!flag && Player.IsConnected)
		{
			Client.EndViewing();
			SendError("player_online");
			return default;
		}
		IRemoteControllable remoteControllable = RemoteControlEntity.FindByID(Proto.cameraId);
		if (remoteControllable == null || !remoteControllable.CanControl(UserId))
		{
			Client.EndViewing();
			SendError("not_found");
			return default;
		}
		if (!flag && remoteControllable is CCTV_RC cCTV_RC && cCTV_RC.IsStatic())
		{
			Client.EndViewing();
			SendError("access_denied");
			return default;
		}
		BaseEntity ent = remoteControllable.GetEnt();
		if (!ent.IsValid())
		{
			Client.EndViewing();
			SendError("not_found");
			return default;
		}
		float num = Vector3.Distance(((Component)Player).transform.position, ((Component)ent).transform.position);
		if (!flag && num >= remoteControllable.MaxRange)
		{
			Client.EndViewing();
			SendError("not_found");
			return default;
		}
		if (!Client.BeginViewing(remoteControllable))
		{
			Client.EndViewing();
			SendError("not_found");
			return default;
		}
		instance.StartRendering(remoteControllable);
		AppResponse val = Pool.Get<AppResponse>();
		AppCameraInfo val2 = Pool.Get<AppCameraInfo>();
		val2.width = CameraRenderer.width;
		val2.height = CameraRenderer.height;
		val2.nearPlane = CameraRenderer.nearPlane;
		val2.farPlane = CameraRenderer.farPlane;
		val2.controlFlags = (int)(Client.IsControllingCamera ? remoteControllable.RequiredControls : RemoteControllableControls.None);
		val.cameraSubscribeInfo = val2;
		Send(val);
		return default;
	}
}

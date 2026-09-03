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
			return default(ValueTask);
		}
		CameraRendererManager instance = SingletonComponent<CameraRendererManager>.Instance;
		if ((Object)(object)instance == (Object)null)
		{
			SendError("server_error");
			return default(ValueTask);
		}
		if (string.IsNullOrEmpty(base.Proto.cameraId))
		{
			base.Client.EndViewing();
			SendError("invalid_id");
			return default(ValueTask);
		}
		bool flag = CameraRenderer.developerPermissions && DeveloperList.Contains(base.UserId);
		if (!base.Player.IsValid())
		{
			base.Client.EndViewing();
			SendError("no_player");
			return default(ValueTask);
		}
		if (!flag && base.Player.IsConnected)
		{
			base.Client.EndViewing();
			SendError("player_online");
			return default(ValueTask);
		}
		IRemoteControllable remoteControllable = RemoteControlEntity.FindByID(base.Proto.cameraId);
		if (remoteControllable == null || !remoteControllable.CanControl(base.UserId))
		{
			base.Client.EndViewing();
			SendError("not_found");
			return default(ValueTask);
		}
		if (!flag && remoteControllable is CCTV_RC cCTV_RC && cCTV_RC.IsStatic())
		{
			base.Client.EndViewing();
			SendError("access_denied");
			return default(ValueTask);
		}
		BaseEntity ent = remoteControllable.GetEnt();
		if (!ent.IsValid())
		{
			base.Client.EndViewing();
			SendError("not_found");
			return default(ValueTask);
		}
		float num = Vector3.Distance(((Component)base.Player).transform.position, ((Component)ent).transform.position);
		if (!flag && num >= remoteControllable.MaxRange)
		{
			base.Client.EndViewing();
			SendError("not_found");
			return default(ValueTask);
		}
		if (!base.Client.BeginViewing(remoteControllable))
		{
			base.Client.EndViewing();
			SendError("not_found");
			return default(ValueTask);
		}
		instance.StartRendering(remoteControllable);
		AppResponse val = Pool.Get<AppResponse>();
		AppCameraInfo val2 = Pool.Get<AppCameraInfo>();
		val2.width = CameraRenderer.width;
		val2.height = CameraRenderer.height;
		val2.nearPlane = CameraRenderer.nearPlane;
		val2.farPlane = CameraRenderer.farPlane;
		val2.controlFlags = (int)(base.Client.IsControllingCamera ? remoteControllable.RequiredControls : RemoteControllableControls.None);
		val.cameraSubscribeInfo = val2;
		Send(val);
		return default(ValueTask);
	}
}

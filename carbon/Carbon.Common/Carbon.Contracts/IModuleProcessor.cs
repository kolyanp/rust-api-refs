using System;
using System.Collections.Generic;
using Carbon.Base;
using Carbon.Base.Interfaces;

namespace Carbon.Contracts;

public interface IModuleProcessor : IDisposable
{
	List<BaseHookable> Modules { get; }

	void Init();

	void OnServerInit();

	void OnServerSave();

	void Setup(BaseHookable hookable);

	void Build(params Type[] types);

	void Uninstall(IModule module);
}

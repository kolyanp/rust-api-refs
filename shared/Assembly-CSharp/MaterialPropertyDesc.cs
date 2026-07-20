using System;
using UnityEngine;

public struct MaterialPropertyDesc(string name, Type type)
{
	public int nameID = Shader.PropertyToID(name);

	public Type type = type;
}

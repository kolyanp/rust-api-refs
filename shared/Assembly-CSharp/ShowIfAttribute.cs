using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class ShowIfAttribute : PropertyAttribute
{
	public string field;

	public int value;

	public ShowIfAttribute(string field, object value)
	{
		this.field = field;
		this.value = Convert.ToInt32(value);
		((PropertyAttribute)this).order = -100;
	}
}

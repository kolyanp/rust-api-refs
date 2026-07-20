using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MySqlConnector;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
public sealed class MySqlAttributeCollection : IEnumerable<MySqlAttribute>, IEnumerable
{
	private readonly List<MySqlAttribute> m_attributes;

	public int Count => m_attributes.Count;

	public MySqlAttribute this[int index] => m_attributes[index];

	public void Add(MySqlAttribute attribute)
	{
		if (attribute == null)
		{
			throw new ArgumentNullException("attribute");
		}
		if (string.IsNullOrEmpty(attribute.AttributeName))
		{
			throw new ArgumentException("Attribute name must not be empty", "attribute");
		}
		foreach (MySqlAttribute attribute2 in m_attributes)
		{
			if (attribute2.AttributeName == attribute.AttributeName)
			{
				throw new ArgumentException("An attribute with the name " + attribute.AttributeName + " already exists in the collection", "attribute");
			}
		}
		m_attributes.Add(attribute);
	}

	public void SetAttribute(string attributeName, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] object value)
	{
		if (string.IsNullOrEmpty(attributeName))
		{
			throw new ArgumentException("Attribute name must not be empty", "attributeName");
		}
		for (int i = 0; i < m_attributes.Count; i++)
		{
			if (m_attributes[i].AttributeName == attributeName)
			{
				m_attributes[i] = new MySqlAttribute(attributeName, value);
				return;
			}
		}
		m_attributes.Add(new MySqlAttribute(attributeName, value));
	}

	public void Clear()
	{
		m_attributes.Clear();
	}

	public IEnumerator<MySqlAttribute> GetEnumerator()
	{
		return m_attributes.GetEnumerator();
	}

	public bool Remove(MySqlAttribute attribute)
	{
		return m_attributes.Remove(attribute);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	internal MySqlAttributeCollection()
	{
		m_attributes = new List<MySqlAttribute>();
	}
}

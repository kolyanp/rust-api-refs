using System;
using UnityEngine;

[Serializable]
public class ItemAmountRandom
{
	[ItemSelector]
	public ItemDefinition itemDef;

	public AnimationCurve amount;

	public int RandomAmount()
	{
		return Mathf.RoundToInt(amount.Evaluate(Random.Range(0f, 1f)));
	}

	public ItemAmountRandom()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		amount = new AnimationCurve((Keyframe[])(object)new Keyframe[2]
		{
			new Keyframe(0f, 0f),
			new Keyframe(1f, 1f)
		});
		base._002Ector();
	}
}
